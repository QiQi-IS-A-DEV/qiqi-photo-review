using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using PhotoFileFilter.Core;
using PhotoFileFilter.Services;
using PhotoFileFilter.ViewModels;

namespace PhotoFileFilter.Review;

public sealed record PreviewQualityOption(int MaxEdge, string Label);

public sealed class ReviewViewModel : ObservableObject, IDisposable
{
    private readonly IDialogService _dialogs;
    private readonly ReviewCatalogService _catalog;
    private readonly ReviewSessionService _session;
    private readonly ReviewPreferencesService _preferencesService;
    private readonly ReviewImportService _importer = new();
    private readonly PreviewService _preview = new();
    private readonly SemaphoreSlim _thumbnailGate = new(2, 2);
    private readonly HashSet<ReviewPhoto> _pendingThumbnails = [];
    private readonly Queue<ReviewPhoto> _thumbnailOrder = new();
    private readonly HistogramService _histogram = new();
    private readonly SemaphoreSlim _previewDecodeGate = new(1, 1);
    private ReviewPhoto? _requestedPreviewPhoto;
    private int? _requestedPreviewEdge;
    private bool _isPreviewLoading;
    private CancellationTokenSource? _importCancellation;
    private CancellationTokenSource? _previewCancellation;
    private CancellationTokenSource? _sessionSaveCancellation;
    private CancellationTokenSource? _overlayCancellation;
    private ReviewPhoto? _current;
    private BitmapSource? _previewImage;
    private BitmapSource? _histogramImage;
    private string _folder = "", _status = LanguageService.IsVietnamese ? "Chọn thư mục để bắt đầu Review ảnh." : "Choose a folder to begin reviewing photos.", _previewMessage = "";
    private bool _busy, _recursive = true;
    private int _filterIndex, _viewMode, _exportPolicy, _selectedCount = 1;
    private string? _lastExportFolder;
    private string[] _sourceInputs = [];
    private bool _restoringSession;
    private bool _disposed;
    private double _zoom = 1;
    private double _panX, _panY;
    private string _overlayMessage = "";
    private string _histogramSummary = LanguageService.Text("No histogram data"), _histogramAssessment = LanguageService.Text("Select a photo to analyze its tonal range.");
    private bool _overlayVisible;
    private int _previewMaxEdge = 2400, _zoomStepPercent = 25, _clickZoomPercent = 200, _overlayDurationMs = 950, _overlayPosition, _defaultView;
    private bool _startWithPanelsHidden, _histogramEnabled = true, _fullResolutionOnZoom = true;
    private string _helpShortcut = "F1", _zenShortcut = "Tab", _undoShortcut = "Ctrl+Z", _resetZoomShortcut = "` + Ctrl+0", _gridShortcut = "G", _loupeShortcut = "E";
    private readonly List<UndoEntry> _undo = [];

    private sealed record PhotoState(ReviewPhoto Photo, int Rating, ReviewFlag Flag, ReviewColor Color, int Rotation);
    private sealed record UndoEntry(PhotoState[] States, string Description, ReviewPhoto? Focus);

    public ReviewViewModel(IDialogService dialogs, ReviewCatalogService? catalog = null, ReviewSessionService? session = null, ReviewPreferencesService? preferences = null)
    {
        _dialogs = dialogs; _catalog = catalog ?? new(); _session = session ?? new();
        _preferencesService = preferences ?? (session != null ? new(Path.Combine(Path.GetDirectoryName(session.FilePath)!, "review-preferences.json")) : new());
        ApplyPreferences(_preferencesService.Load());
        ImportCommand = new(async () =>
        {
            if (_dialogs.SelectReviewSources(Directory.Exists(Folder) ? Folder : null) is not { Length: > 0 } paths) return;
            if (paths.Length == 1 && Directory.Exists(paths[0])) await ImportAsync(paths[0]);
            else await ImportPathsAsync(paths);
        }, () => !Busy);
        PreviousCommand = new(Previous, () => CurrentPhoto != null && FilteredPhotos.Count > 1);
        NextCommand = new(Next, () => CurrentPhoto != null && FilteredPhotos.Count > 1);
        RevealCommand = new(() => Guard(() => _dialogs.RevealFile(CurrentPhoto!.FullPath)), () => CurrentPhoto != null);
        CopyNamesCommand = new(() => Guard(() => _dialogs.CopyText(string.Join(Environment.NewLine, FilteredPhotos.Select(p => Path.GetFileNameWithoutExtension(p.Name))))), () => FilteredPhotos.Count > 0);
        ExportNamesCommand = new(() => ExportNamesWithDialog(FilteredPhotos), () => FilteredPhotos.Count > 0);
        ExportRatedCommand = new(async () => { if (_dialogs.SelectFolder("Choose a folder for the filtered rated photos") is { } path) await ExportRatedAsync(path); }, () => !Busy && FilteredPhotos.Any(photo => photo.Rating > 0));
        OpenExportCommand = new(() => Guard(() => _dialogs.OpenFolder(_lastExportFolder!)), () => !Busy && _lastExportFolder != null && Directory.Exists(_lastExportFolder));
        GridCommand = new(() => ViewMode = 0);
        LoupeCommand = new(() => ViewMode = 1);
        CancelCommand = new(() => _importCancellation?.Cancel(), () => Busy);
        ClearPreviewCacheCommand = new(ClearPreviewCache, () => Photos.Count > 0 && !Busy);
        ClearSessionCommand = new(ClearSession, () => Photos.Count > 0 && !Busy);
        WindowsCacheCleanupCommand = new(() => Guard(_dialogs.OpenWindowsThumbnailCleanup), () => !Busy);
        UndoCommand = new(Undo, () => _undo.Count > 0 && !Busy);
        ShowAllCommand = new(() => FilterIndex = 0, () => !Busy && FilterIndex != 0);
    }

    public ObservableCollection<ReviewPhoto> Photos { get; } = [];
    public ObservableCollection<ReviewPhoto> FilteredPhotos { get; } = [];
    public string[] Filters { get; } = LanguageService.Texts("All Photos", "Pick", "≥ 1 Star", "≥ 3 Stars", "5 Stars", "Reject", "Unrated", "Red Label", "Yellow Label", "Green Label", "Blue Label");
    public string[] ExportPolicies { get; } = LanguageService.Texts("Rename — keep both", "Skip existing files", "Replace destination files");
    public int[] PreviewSizeOptions { get; } = [1200, 1800, 2400, 3600, 4800, 0];
    public PreviewQualityOption[] PreviewQualityOptions { get; } =
    [
        new(1200, LanguageService.Text("Fast · 1,200 px")),
        new(1800, LanguageService.Text("Balanced · 1,800 px")),
        new(2400, LanguageService.Text("Detailed · 2,400 px")),
        new(3600, LanguageService.Text("High · 3,600 px")),
        new(4800, LanguageService.Text("Ultra · 4,800 px")),
        new(0, LanguageService.Text("Original · full resolution"))
    ];
    public int[] ZoomStepOptions { get; } = [10, 25, 50];
    public int[] ClickZoomOptions { get; } = [125, 150, 200, 300, 400, 800];
    public int[] OverlayDurationOptions { get; } = [600, 950, 1500];
    public string[] OverlayPositionOptions { get; } = LanguageService.Texts("Bottom of photo", "Center of photo");
    public string[] DefaultViewOptions { get; } = ["Grid", "Loupe"];
    public string[] HelpShortcutOptions { get; } = ["F1", "Ctrl+H"];
    public string[] ZenShortcutOptions { get; } = ["Tab", "F"];
    public string[] UndoShortcutOptions { get; } = ["Ctrl+Z", "Ctrl+Backspace"];
    public string[] ResetZoomShortcutOptions { get; } = ["` + Ctrl+0", "`", "Ctrl+0"];
    public string[] GridShortcutOptions { get; } = ["G", "Ctrl+G"];
    public string[] LoupeShortcutOptions { get; } = ["E", "Enter"];
    public RelayCommand ImportCommand { get; }
    public RelayCommand PreviousCommand { get; }
    public RelayCommand NextCommand { get; }
    public RelayCommand RevealCommand { get; }
    public RelayCommand CopyNamesCommand { get; }
    public RelayCommand ExportNamesCommand { get; }
    public RelayCommand ExportRatedCommand { get; }
    public RelayCommand OpenExportCommand { get; }
    public RelayCommand GridCommand { get; }
    public RelayCommand LoupeCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand ClearPreviewCacheCommand { get; }
    public RelayCommand ClearSessionCommand { get; }
    public RelayCommand WindowsCacheCleanupCommand { get; }
    public RelayCommand UndoCommand { get; }
    public RelayCommand ShowAllCommand { get; }
    public string Folder { get => _folder; private set { Set(ref _folder, value); Notify(nameof(FolderName)); } }
    public string FolderName => string.IsNullOrWhiteSpace(Folder) ? LanguageService.Text("No folder imported") : Path.GetFileName(Path.TrimEndingDirectorySeparator(Folder));
    public bool Recursive { get => _recursive; set { if (Set(ref _recursive, value)) QueueSessionSave(); } }
    public bool Busy { get => _busy; private set { Set(ref _busy, value); RefreshCommands(); } }
    public string Status { get => _status; private set => Set(ref _status, value); }
    public string PreviewMessage { get => _previewMessage; private set => Set(ref _previewMessage, value); }
    public BitmapSource? PreviewImage { get => _previewImage; private set { Set(ref _previewImage, value); Notify(nameof(HasPreview)); } }
    public bool HasPreview => PreviewImage != null;
    public bool IsPreviewLoading { get => _isPreviewLoading; private set => Set(ref _isPreviewLoading, value); }
    public BitmapSource? HistogramImage { get => _histogramImage; private set => Set(ref _histogramImage, value); }
    public string HistogramSummary { get => _histogramSummary; private set => Set(ref _histogramSummary, value); }
    public string HistogramAssessment { get => _histogramAssessment; private set => Set(ref _histogramAssessment, value); }
    public string OverlayMessage { get => _overlayMessage; private set => Set(ref _overlayMessage, value); }
    public bool OverlayVisible { get => _overlayVisible; private set => Set(ref _overlayVisible, value); }
    public int PreviewMaxEdge { get => _previewMaxEdge; set { if (Set(ref _previewMaxEdge, PreviewSizeOptions.Contains(value) ? value : 2400)) { SavePreferences(); _ = LoadCurrentPreviewAsync(); } } }
    public bool FullResolutionOnZoom { get => _fullResolutionOnZoom; set { if (Set(ref _fullResolutionOnZoom, value)) { SavePreferences(); _ = LoadCurrentPreviewAsync(); } } }
    public int ZoomStepPercent { get => _zoomStepPercent; set { if (Set(ref _zoomStepPercent, ZoomStepOptions.Contains(value) ? value : 25)) SavePreferences(); } }
    public int ClickZoomPercent { get => _clickZoomPercent; set { if (Set(ref _clickZoomPercent, ClickZoomOptions.Contains(value) ? value : 200)) SavePreferences(); } }
    public int OverlayDurationMs { get => _overlayDurationMs; set { if (Set(ref _overlayDurationMs, OverlayDurationOptions.Contains(value) ? value : 950)) SavePreferences(); } }
    public int OverlayPosition { get => _overlayPosition; set { if (Set(ref _overlayPosition, Math.Clamp(value, 0, 1))) SavePreferences(); } }
    public int DefaultView { get => _defaultView; set { if (Set(ref _defaultView, Math.Clamp(value, 0, 1))) SavePreferences(); } }
    public bool StartWithPanelsHidden { get => _startWithPanelsHidden; set { if (Set(ref _startWithPanelsHidden, value)) SavePreferences(); } }
    public bool HistogramEnabled { get => _histogramEnabled; set { if (Set(ref _histogramEnabled, value)) { if (!value) { HistogramImage = null; HistogramSummary = LanguageService.Text("Histogram disabled"); HistogramAssessment = ""; } else _ = LoadCurrentPreviewAsync(force: true); SavePreferences(); } } }
    public string HelpShortcut { get => _helpShortcut; set { if (Set(ref _helpShortcut, Choice(value, HelpShortcutOptions, "F1"))) SavePreferences(); } }
    public string ZenShortcut { get => _zenShortcut; set { if (Set(ref _zenShortcut, Choice(value, ZenShortcutOptions, "Tab"))) SavePreferences(); } }
    public string UndoShortcut { get => _undoShortcut; set { if (Set(ref _undoShortcut, Choice(value, UndoShortcutOptions, "Ctrl+Z"))) SavePreferences(); } }
    public string ResetZoomShortcut { get => _resetZoomShortcut; set { if (Set(ref _resetZoomShortcut, Choice(value, ResetZoomShortcutOptions, "` + Ctrl+0"))) SavePreferences(); } }
    public string GridShortcut { get => _gridShortcut; set { if (Set(ref _gridShortcut, Choice(value, GridShortcutOptions, "G"))) SavePreferences(); } }
    public string LoupeShortcut { get => _loupeShortcut; set { if (Set(ref _loupeShortcut, Choice(value, LoupeShortcutOptions, "E"))) SavePreferences(); } }
    public int FilterIndex { get => _filterIndex; set { if (Set(ref _filterIndex, Math.Clamp(value, 0, Filters.Length - 1))) { ApplyFilter(); QueueSessionSave(); } } }
    public int ViewMode
    {
        get => _viewMode;
        set
        {
            if (!Set(ref _viewMode, Math.Clamp(value, 0, 1))) return;
            Notify(nameof(IsGrid)); Notify(nameof(IsLoupe));
            QueueSessionSave(); _ = LoadCurrentPreviewAsync();
        }
    }
    public int ExportPolicy { get => _exportPolicy; set { if (Set(ref _exportPolicy, Math.Clamp(value, 0, ExportPolicies.Length - 1))) QueueSessionSave(); } }
    private int _thumbnailSize = 166;
    public int ThumbnailSize
    {
        get => _thumbnailSize;
        set
        {
            if (!Set(ref _thumbnailSize, Math.Clamp(value, 100, 400))) return;
            Notify(nameof(ThumbnailHeight)); Notify(nameof(TileWidth)); Notify(nameof(TileHeight)); SavePreferences();
        }
    }
    public double ThumbnailHeight => ThumbnailSize * 0.65 + 37;
    public double TileWidth => ThumbnailSize + 12;
    public double TileHeight => ThumbnailHeight + 12;
    public string ThumbnailSizeLabel => LanguageService.IsVietnamese ? "Cỡ ảnh" : "Thumbnails";
    public bool IsGrid => ViewMode == 0;
    public bool IsLoupe => ViewMode == 1;
    public bool HasNoPhotos => Photos.Count == 0;
    public bool HasNoFilteredPhotos => Photos.Count > 0 && FilteredPhotos.Count == 0;
    public string VisibleScopeSummary => LanguageService.IsVietnamese
        ? $"Đang hiện {FilteredPhotos.Count:N0}/{Photos.Count:N0} ảnh · {FilteredPhotos.Count(photo => photo.Rating > 0):N0} ảnh đã Rating"
        : $"Showing {FilteredPhotos.Count:N0}/{Photos.Count:N0} photos · {FilteredPhotos.Count(photo => photo.Rating > 0):N0} rated";
    public string SendVisibleLabel => LanguageService.IsVietnamese ? $"Gửi {FilteredPhotos.Count:N0} tên đang hiện sang Lọc TXT" : $"Send {FilteredPhotos.Count:N0} visible names to TXT Filter";
    public string ExportVisibleNamesLabel => LanguageService.IsVietnamese ? $"Xuất {FilteredPhotos.Count:N0} tên đang hiện…" : $"Export {FilteredPhotos.Count:N0} visible filenames…";
    public string CopyVisibleRatedLabel
    {
        get
        {
            var count = FilteredPhotos.Count(photo => photo.Rating > 0);
            return LanguageService.IsVietnamese ? $"Chép {count:N0} ảnh đã Rating đang hiện…" : $"Copy {count:N0} visible rated photos…";
        }
    }
    public ReviewPhoto? CurrentPhoto
    {
        get => _current;
        set
        {
            if (!Set(ref _current, value)) return;
            ResetZoom();
            HistogramImage = null; HistogramSummary = value == null ? "No histogram data" : "Analyzing tones…"; HistogramAssessment = "";
            NotifyCurrent();
            _ = LoadCurrentPreviewAsync();
            QueueSessionSave();
        }
    }
    public int CurrentPosition => CurrentPhoto == null ? 0 : FilteredPhotos.IndexOf(CurrentPhoto) + 1;
    public string PositionLabel => SelectedCount > 1 ? (LanguageService.IsVietnamese ? $"{CurrentPosition:N0} / {FilteredPhotos.Count:N0}  ·  đã chọn {SelectedCount:N0}" : $"{CurrentPosition:N0} / {FilteredPhotos.Count:N0}  ·  {SelectedCount:N0} selected") : $"{CurrentPosition:N0} / {FilteredPhotos.Count:N0}";
    public int SelectedCount { get => _selectedCount; private set { if (Set(ref _selectedCount, value)) Notify(nameof(PositionLabel)); } }
    public string RatingStars => CurrentPhoto?.Stars ?? "—";
    public string CurrentName => CurrentPhoto?.Name ?? LanguageService.Text("No photo selected");
    public string CurrentPath => CurrentPhoto?.RelativePath ?? "";
    public string CurrentDetails => CurrentPhoto == null ? "" : $"{CurrentPhoto.Extension}  ·  {FormatSize(CurrentPhoto.Size)}  ·  {CurrentPhoto.LastWriteUtc.ToLocalTime():dd/MM/yyyy HH:mm}";
    public int PickCount => Photos.Count(p => p.Flag == ReviewFlag.Pick);
    public int RejectCount => Photos.Count(p => p.Flag == ReviewFlag.Reject);
    public int RatedCount => Photos.Count(p => p.Rating > 0);
    public int ColorCount => Photos.Count(p => p.ColorLabel != ReviewColor.None);
    public string CatalogSummary => LanguageService.IsVietnamese ? $"{Photos.Count:N0} ảnh  ·  {PickCount:N0} Pick  ·  {RatedCount:N0} đã chấm  ·  {ColorCount:N0} nhãn màu" : $"{Photos.Count:N0} photos  ·  {PickCount:N0} picks  ·  {RatedCount:N0} rated  ·  {ColorCount:N0} color labels";

    public async Task ImportAsync(string folder)
    {
        var fullFolder = Path.GetFullPath(folder);
        await ImportCoreAsync(fullFolder, [fullFolder], token => _importer.Import(fullFolder, Recursive, new Progress<int>(count => Status = $"Checked {count:N0} files…"), token));
    }

    public async Task ImportPathsAsync(IEnumerable<string> paths)
    {
        var inputs = paths.Where(path => Directory.Exists(path) || (File.Exists(path) && ReviewImportService.IsSupportedFile(path)))
            .Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (inputs.Length == 0) return;
        var displayFolder = SuggestedFolder(inputs);
        await ImportCoreAsync(displayFolder, inputs, token => _importer.ImportPaths(inputs, Recursive, new Progress<int>(count => Status = $"Checked {count:N0} files…"), token));
    }

    private async Task ImportCoreAsync(string displayFolder, string[] sourceInputs, Func<CancellationToken, ReviewImportResult> import)
    {
        if (Busy) return;
        _importCancellation?.Cancel(); _importCancellation?.Dispose(); _importCancellation = new();
        _thumbnailOrder.Clear();
        Busy = true; Status = "Importing photos…"; PreviewImage = null; CurrentPhoto = null;
        var token = _importCancellation.Token;
        try
        {
            var result = await Task.Run(() => import(token), token);
            _undo.Clear(); UndoCommand.Refresh();
            Photos.Clear();
            foreach (var photo in result.Photos)
            {
                if (_catalog.Get(photo.FullPath) is { } mark && mark.Size == photo.Size && mark.LastWriteUtc == photo.LastWriteUtc)
                { photo.Rating = mark.Rating; photo.Flag = mark.Flag; photo.ColorLabel = mark.ColorLabel; photo.Rotation = mark.Rotation; }
                Photos.Add(photo);
            }
            // A new import is a new browsing context. A previous label/rating filter must not hide it.
            // Session restoration applies the saved filter again after import.
            _filterIndex = 0; Notify(nameof(FilterIndex));
            Folder = displayFolder; _sourceInputs = sourceInputs; ApplyFilter();
            Status = result.Warnings.Count == 0 ? $"Imported {Photos.Count:N0} photos." : $"Imported {Photos.Count:N0} photos · {result.Warnings.Count:N0} items could not be read.";
            SaveSessionNow();
        }
        catch (OperationCanceledException) { Status = "Import canceled."; }
        catch (Exception e) { Status = e.Message; _dialogs.ShowError(e.Message); }
        finally { Busy = false; }
    }

    public async Task RestoreSessionAsync()
    {
        if (Busy || _session.Load() is not { } saved) return;
        var sources = saved.Sources.Where(path => Directory.Exists(path) || (File.Exists(path) && ReviewImportService.IsSupportedFile(path)))
            .Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (sources.Length == 0) return;
        _restoringSession = true;
        try
        {
            _recursive = saved.Recursive;
            _exportPolicy = Math.Clamp(saved.ExportPolicy, 0, ExportPolicies.Length - 1);
            Notify(nameof(Recursive)); Notify(nameof(ExportPolicy));
            if (sources.Length == 1 && Directory.Exists(sources[0])) await ImportAsync(sources[0]);
            else await ImportPathsAsync(sources);
            FilterIndex = Math.Clamp(saved.FilterIndex, 0, Filters.Length - 1);
            ViewMode = Math.Clamp(saved.ViewMode, 0, 1);
            if (!string.IsNullOrWhiteSpace(saved.CurrentPath))
            {
                var restored = FilteredPhotos.FirstOrDefault(photo => string.Equals(photo.FullPath, saved.CurrentPath, StringComparison.OrdinalIgnoreCase));
                if (restored != null) CurrentPhoto = restored;
            }
            Status = $"Restored the previous review session · {Photos.Count:N0} photos.";
        }
        finally
        {
            _restoringSession = false;
            SaveSessionNow();
        }
    }

    private ReviewSession? CreateSessionSnapshot()
        => _sourceInputs.Length == 0 ? null : new(_sourceInputs, Recursive, FilterIndex, ViewMode, ExportPolicy, CurrentPhoto?.FullPath);

    private void QueueSessionSave()
    {
        if (_disposed || _restoringSession || CreateSessionSnapshot() is not { } snapshot) return;
        _sessionSaveCancellation?.Cancel(); _sessionSaveCancellation?.Dispose();
        var cancellation = _sessionSaveCancellation = new();
        _ = Task.Run(async () =>
        {
            try { await Task.Delay(350, cancellation.Token); if (!cancellation.IsCancellationRequested) _session.Save(snapshot); }
            catch (OperationCanceledException) { }
        });
    }

    public void SaveSessionNow()
    {
        if (_disposed || _restoringSession || CreateSessionSnapshot() is not { } snapshot) return;
        _sessionSaveCancellation?.Cancel();
        _session.Save(snapshot);
    }

    private void ApplyPreferences(ReviewPreferences value)
    {
        _thumbnailSize = Math.Clamp(value.ThumbnailSize, 100, 400);
        _previewMaxEdge = PreviewSizeOptions.Contains(value.PreviewMaxEdge) ? value.PreviewMaxEdge : 2400;
        _fullResolutionOnZoom = value.FullResolutionOnZoom;
        _zoomStepPercent = ZoomStepOptions.Contains(value.ZoomStepPercent) ? value.ZoomStepPercent : 25;
        _clickZoomPercent = ClickZoomOptions.Contains(value.ClickZoomPercent) ? value.ClickZoomPercent : 200;
        _overlayDurationMs = OverlayDurationOptions.Contains(value.OverlayDurationMs) ? value.OverlayDurationMs : 950;
        _overlayPosition = Math.Clamp(value.OverlayPosition, 0, 1); _defaultView = Math.Clamp(value.DefaultView, 0, 1);
        _startWithPanelsHidden = value.StartWithPanelsHidden; _histogramEnabled = value.HistogramEnabled;
        _helpShortcut = Choice(value.HelpShortcut, HelpShortcutOptions, "F1"); _zenShortcut = Choice(value.ZenShortcut, ZenShortcutOptions, "Tab");
        _undoShortcut = Choice(value.UndoShortcut, UndoShortcutOptions, "Ctrl+Z"); _resetZoomShortcut = Choice(value.ResetZoomShortcut, ResetZoomShortcutOptions, "` + Ctrl+0");
        _gridShortcut = Choice(value.GridShortcut, GridShortcutOptions, "G"); _loupeShortcut = Choice(value.LoupeShortcut, LoupeShortcutOptions, "E");
        _viewMode = _defaultView;
    }

    private static string Choice(string? value, IReadOnlyList<string> choices, string fallback)
        => value != null && choices.Contains(value, StringComparer.Ordinal) ? value : fallback;

    private void SavePreferences()
    {
        _preferencesService.Save(new(PreviewMaxEdge, ZoomStepPercent, OverlayDurationMs, OverlayPosition, DefaultView, StartWithPanelsHidden, HistogramEnabled,
            HelpShortcut, ZenShortcut, UndoShortcut, ResetZoomShortcut, GridShortcut, LoupeShortcut, ClickZoomPercent, FullResolutionOnZoom, ThumbnailSize));
    }

    public void ResetPreferences()
    {
        ApplyPreferences(new());
        foreach (var name in new[] { nameof(ThumbnailSize), nameof(ThumbnailHeight), nameof(TileWidth), nameof(TileHeight), nameof(PreviewMaxEdge), nameof(FullResolutionOnZoom), nameof(ZoomStepPercent), nameof(ClickZoomPercent), nameof(OverlayDurationMs), nameof(OverlayPosition), nameof(DefaultView), nameof(StartWithPanelsHidden), nameof(HistogramEnabled), nameof(HelpShortcut), nameof(ZenShortcut), nameof(UndoShortcut), nameof(ResetZoomShortcut), nameof(GridShortcut), nameof(LoupeShortcut), nameof(ViewMode), nameof(IsGrid), nameof(IsLoupe) }) Notify(name);
        _ = LoadCurrentPreviewAsync(force: true);
        SavePreferences(); Status = "Restored the default Photo Review settings.";
    }

    private static string SuggestedFolder(IReadOnlyList<string> paths)
    {
        var folders = paths.Select(path => Directory.Exists(path) ? Path.GetFullPath(path) : File.Exists(path) ? Path.GetDirectoryName(Path.GetFullPath(path)) : null)
            .Where(path => path != null).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        return folders.Length == 1 ? folders[0]! : "Multiple locations";
    }

    public void SetRating(int rating)
        => SetRating(CurrentPhoto == null ? [] : [CurrentPhoto], rating);
    public void SetRating(IEnumerable<ReviewPhoto> photos, int rating)
    {
        var targets = ValidTargets(photos);
        rating = Math.Clamp(rating, 0, 5);
        if (targets.Length == 0 || targets.All(photo => photo.Rating == rating)) return;
        PushUndo(targets, "rating change");
        foreach (var photo in targets) photo.Rating = rating;
        Persist(targets);
        ShowOverlay(rating == 0 ? WithCount("RATING CLEARED", targets.Length) : WithCount(new string('★', rating), targets.Length));
    }
    public void SetFlag(ReviewFlag flag)
        => SetFlag(CurrentPhoto == null ? [] : [CurrentPhoto], flag);
    public void SetFlag(IEnumerable<ReviewPhoto> photos, ReviewFlag flag)
    {
        var targets = ValidTargets(photos);
        if (targets.Length == 0 || targets.All(photo => photo.Flag == flag)) return;
        PushUndo(targets, "flag change");
        foreach (var photo in targets) photo.Flag = flag;
        Persist(targets);
        ShowOverlay(WithCount(flag switch { ReviewFlag.Pick => "PICK", ReviewFlag.Reject => "REJECT", _ => "FLAG REMOVED" }, targets.Length));
    }
    public void SetColor(ReviewColor color)
        => SetColor(CurrentPhoto == null ? [] : [CurrentPhoto], color);
    public void SetColor(IEnumerable<ReviewPhoto> photos, ReviewColor color)
    {
        var targets = ValidTargets(photos);
        if (targets.Length == 0) return;
        var targetColor = targets.All(photo => photo.ColorLabel == color) ? ReviewColor.None : color;
        if (targets.All(photo => photo.ColorLabel == targetColor)) return;
        PushUndo(targets, "color label change");
        foreach (var photo in targets) photo.ColorLabel = targetColor;
        Persist(targets);
        ShowOverlay(WithCount(targetColor switch { ReviewColor.Red => "RED LABEL", ReviewColor.Yellow => "YELLOW LABEL", ReviewColor.Green => "GREEN LABEL", ReviewColor.Blue => "BLUE LABEL", _ => "COLOR LABEL CLEARED" }, targets.Length));
    }
    public double Zoom { get => _zoom; private set { if (Set(ref _zoom, Math.Clamp(value, 0.25, 8))) Notify(nameof(ZoomLabel)); } }
    public double PanX { get => _panX; private set => Set(ref _panX, Math.Clamp(value, -10000, 10000)); }
    public double PanY { get => _panY; private set => Set(ref _panY, Math.Clamp(value, -10000, 10000)); }
    public string ZoomLabel => Zoom == 1 ? "Fit" : $"{Zoom * 100:0}% Fit";
    public void ZoomBy(int direction)
    {
        var factor = 1 + ZoomStepPercent / 100d;
        ZoomTo(direction > 0 ? Math.Min(8, Zoom * factor) : Math.Max(0.25, Zoom / factor), 0, 0);
    }
    public void ZoomAt(int direction, double anchorX, double anchorY)
    {
        var factor = 1 + ZoomStepPercent / 100d;
        ZoomTo(direction > 0 ? Math.Min(8, Zoom * factor) : Math.Max(0.25, Zoom / factor), anchorX, anchorY);
    }
    public void ZoomTo(double targetZoom, double anchorX, double anchorY)
    {
        var oldZoom = Zoom;
        var newZoom = Math.Clamp(targetZoom, 0.25, 8);
        if (newZoom <= 1) { ResetZoom(); return; }
        var ratio = newZoom / oldZoom;
        var nextPanX = anchorX - (anchorX - PanX) * ratio;
        var nextPanY = anchorY - (anchorY - PanY) * ratio;
        Zoom = newZoom;
        PanTo(nextPanX, nextPanY);
        _ = LoadCurrentPreviewAsync();
    }
    public void PanTo(double x, double y)
    {
        if (Zoom <= 1) { ResetPan(); return; }
        PanX = x; PanY = y;
    }
    public void ResetPan() { PanX = 0; PanY = 0; }
    public void ResetZoom() { Zoom = 1; ResetPan(); _ = LoadCurrentPreviewAsync(); }
    public void Rotate(IEnumerable<ReviewPhoto> photos, int degrees)
    {
        var targets = ValidTargets(photos);
        if (targets.Length == 0) return;
        PushUndo(targets, "rotation");
        foreach (var photo in targets) photo.Rotation += degrees;
        Persist(targets);
        Status = $"Rotated {targets.Length:N0} photos {Math.Abs(degrees)}° {(degrees < 0 ? "left" : "right")}. Original files remain unchanged.";
        ShowOverlay(WithCount(degrees < 0 ? "ROTATED LEFT 90°" : "ROTATED RIGHT 90°", targets.Length));
    }

    private void PushUndo(IReadOnlyList<ReviewPhoto> photos, string description)
    {
        _undo.Add(new(photos.Select(photo => new PhotoState(photo, photo.Rating, photo.Flag, photo.ColorLabel, photo.Rotation)).ToArray(), description, CurrentPhoto));
        if (_undo.Count > 50) _undo.RemoveAt(0);
        UndoCommand.Refresh();
    }

    public void Undo()
    {
        if (_undo.Count == 0 || Busy) return;
        var entry = _undo[^1]; _undo.RemoveAt(_undo.Count - 1);
        var targets = entry.States.Where(state => Photos.Contains(state.Photo)).ToArray();
        foreach (var state in targets)
        {
            state.Photo.Rating = state.Rating; state.Photo.Flag = state.Flag;
            state.Photo.ColorLabel = state.Color; state.Photo.Rotation = state.Rotation;
        }
        if (targets.Length > 0)
        {
            try { _catalog.SetMany(targets.Select(state => state.Photo)); }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException) { _dialogs.ShowError("Could not save the undo operation: " + e.Message); }
            ApplyFilter(entry.Focus); NotifyCurrent(); NotifyCounts();
            Status = $"Undid {entry.Description} for {targets.Length:N0} photos.";
            ShowOverlay(WithCount("UNDONE", targets.Length));
        }
        UndoCommand.Refresh();
    }

    private static string WithCount(string message, int count) => count > 1 ? $"{message}\n{count:N0} PHOTOS" : message;
    private void ShowOverlay(string message)
    {
        OverlayMessage = message; OverlayVisible = true;
        _overlayCancellation?.Cancel(); _overlayCancellation?.Dispose();
        var cancellation = _overlayCancellation = new();
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(OverlayDurationMs, cancellation.Token);
                if (!cancellation.IsCancellationRequested) System.Windows.Application.Current.Dispatcher.Invoke(() => OverlayVisible = false);
            }
            catch (OperationCanceledException) { }
        });
    }
    public void UpdateSelectionCount(int count) => SelectedCount = Math.Max(1, count);
    public async Task ExportRatedAsync(string destination)
        => await ExportPhotosAsync(FilteredPhotos.Where(photo => photo.Rating > 0), destination, "filtered rated photos");

    public async Task ExportSelectedAsync(IEnumerable<ReviewPhoto> photos)
    {
        var targets = ValidTargets(photos);
        if (targets.Length == 0) { Status = "No photos are selected for export."; return; }
        if (_dialogs.SelectFolder("Choose a folder for the selected photos") is { } destination)
            await ExportPhotosAsync(targets, destination, "selected photos");
    }

    public async Task ExportSelectedAsync(IEnumerable<ReviewPhoto> photos, string destination)
        => await ExportPhotosAsync(ValidTargets(photos), destination, "selected photos");

    public bool ExportNamesToText(IEnumerable<ReviewPhoto> photos, string path)
    {
        var names = NameStems(photos);
        if (names.Length == 0) { Status = "There are no filenames to export."; return false; }
        try
        {
            path = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllLines(path, names, new UTF8Encoding(true));
            Status = $"Exported {names.Length:N0} filenames: {path}";
            return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            Status = "Could not export the TXT list: " + e.Message; _dialogs.ShowError(Status); return false;
        }
    }

    public void ExportNamesWithDialog(IEnumerable<ReviewPhoto> photos)
    {
        var targets = ValidTargets(photos);
        if (targets.Length > 0 && _dialogs.SaveReviewNameList("QiQi-review-selection.txt") is { } path) ExportNamesToText(targets, path);
    }

    public string? CreateTransferList(IEnumerable<ReviewPhoto> photos)
        => ExportNamesToText(photos, _session.TransferFilePath) ? _session.TransferFilePath : null;

    private static string[] NameStems(IEnumerable<ReviewPhoto> photos)
        => photos.Select(photo => Path.GetFileNameWithoutExtension(photo.Name)).Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    private void ClearPreviewCache()
    {
        _previewCancellation?.Cancel(); _requestedPreviewEdge = null; IsPreviewLoading = false;
        _preview.ClearCache();
        _thumbnailOrder.Clear();
        Zoom = 1; ResetPan(); PreviewImage = null; HistogramImage = null; HistogramSummary = "No histogram data"; HistogramAssessment = "Preview memory released."; PreviewMessage = "Preview memory released. Select a photo to load it again.";
        foreach (var photo in Photos) photo.Thumbnail = null;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false);
        Status = "Released the app preview cache from memory.";
    }

    private void ClearSession()
    {
        if (!_dialogs.ConfirmClearReviewSession()) return;
        _restoringSession = true;
        try
        {
            _sessionSaveCancellation?.Cancel(); _previewCancellation?.Cancel();
            _preview.ClearCache();
            _thumbnailOrder.Clear();
            PreviewImage = null; CurrentPhoto = null; FilteredPhotos.Clear(); Photos.Clear();
            _undo.Clear(); UndoCommand.Refresh();
            _sourceInputs = []; Folder = ""; _filterIndex = 0; _viewMode = 0;
            Notify(nameof(FilterIndex)); Notify(nameof(ViewMode)); Notify(nameof(IsGrid)); Notify(nameof(IsLoupe)); NotifyCounts();
            Status = _session.Delete() ? "Cleared the current review session. Ratings and original files were preserved." : "Could not delete the review session file.";
        }
        finally { _restoringSession = false; RefreshCommands(); }
    }

    private async Task ExportPhotosAsync(IEnumerable<ReviewPhoto> selection, string destination, string label)
    {
        if (Busy) return;
        var rated = selection.Distinct().ToArray();
        if (rated.Length == 0) { Status = "No photos match the current filter and rating requirements."; return; }
        destination = Path.GetFullPath(destination);
        var source = Directory.Exists(Folder) ? Folder : Path.GetDirectoryName(rated[0].FullPath)!;
        var policy = (CollisionPolicy)ExportPolicy;
        if (!_dialogs.ConfirmCopy(rated.Length, source, destination, ExportPolicies[ExportPolicy])) return;
        _importCancellation?.Dispose(); _importCancellation = new();
        Busy = true; Status = $"Copying {rated.Length:N0} {label}…";
        try
        {
            var files = rated.Select(photo => new PhotoFile(photo.FullPath, photo.RelativePath, photo.Name, photo.Size, photo.LastWriteUtc)).ToArray();
            var scan = new ScanResult(source, files.Select(file => Path.GetFileNameWithoutExtension(file.Name)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(), files, [], [], files.Length);
            var progress = new Progress<OperationProgress>(value => Status = $"Copying {value.Completed:N0}/{value.Total:N0} · {value.CurrentFile}");
            var result = await new FileCopyService().CopyAsync(scan, new(destination, policy), progress, _importCancellation.Token);
            _lastExportFolder = destination;
            Status = result.Cancelled ? $"Canceled after copying {result.Copied:N0} photos." : $"Copied {result.Copied:N0} photos · skipped {result.Skipped:N0} · {result.Errors.Count:N0} errors.";
            if (result.Errors.Count > 0) _dialogs.ShowError(string.Join(Environment.NewLine, result.Errors.Take(8).Select(error => $"{error.File}: {error.Reason}")));
        }
        catch (OperationCanceledException) { Status = "Export canceled."; }
        catch (Exception e) { Status = "Could not export photos: " + e.Message; _dialogs.ShowError(Status); }
        finally { Busy = false; }
    }
    public void Next() => Move(1);
    public void Previous() => Move(-1);
    public void MoveBy(int offset) => Move(offset);
    public void MoveGrid(int offset)
    {
        if (FilteredPhotos.Count == 0) return;
        var index = CurrentPhoto == null ? 0 : Math.Max(0, FilteredPhotos.IndexOf(CurrentPhoto));
        CurrentPhoto = FilteredPhotos[Math.Clamp(index + offset, 0, FilteredPhotos.Count - 1)];
    }
    public void SelectAndLoupe(ReviewPhoto? photo) { if (photo == null) return; CurrentPhoto = photo; ViewMode = 1; }

    private void Move(int direction)
    {
        if (FilteredPhotos.Count == 0) return;
        var index = CurrentPhoto == null ? 0 : FilteredPhotos.IndexOf(CurrentPhoto);
        index = (index + direction + FilteredPhotos.Count) % FilteredPhotos.Count;
        CurrentPhoto = FilteredPhotos[index];
    }
    private ReviewPhoto[] ValidTargets(IEnumerable<ReviewPhoto> photos) => photos.Distinct().Where(Photos.Contains).ToArray();
    private void Persist(IReadOnlyList<ReviewPhoto> photos)
    {
        try { _catalog.SetMany(photos); }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Status = "Could not save review metadata: " + e.Message;
            _dialogs.ShowError(Status);
        }
        NotifyCurrent(); NotifyCounts();
        // When filtering on the edited attribute, remove non-matching images and advance naturally.
        var current = CurrentPhoto;
        if (photos.Any(photo => !MatchesFilter(photo))) ApplyFilter(current);
    }
    private void ApplyFilter(ReviewPhoto? preferred = null)
    {
        var previous = preferred ?? CurrentPhoto;
        FilteredPhotos.Clear();
        foreach (var photo in Photos.Where(MatchesFilter)) FilteredPhotos.Add(photo);
        CurrentPhoto = previous != null && FilteredPhotos.Contains(previous) ? previous : FilteredPhotos.FirstOrDefault();
        NotifyCounts();
        _ = WarmThumbnailsAsync(FilteredPhotos.Take(24).ToArray(), _importCancellation?.Token ?? default);
    }
    private bool MatchesFilter(ReviewPhoto photo) => FilterIndex switch
    {
        1 => photo.Flag == ReviewFlag.Pick, 2 => photo.Rating >= 1, 3 => photo.Rating >= 3,
        4 => photo.Rating == 5, 5 => photo.Flag == ReviewFlag.Reject, 6 => photo.Rating == 0,
        7 => photo.ColorLabel == ReviewColor.Red, 8 => photo.ColorLabel == ReviewColor.Yellow,
        9 => photo.ColorLabel == ReviewColor.Green, 10 => photo.ColorLabel == ReviewColor.Blue, _ => true
    };
    private async Task LoadCurrentPreviewAsync(bool force = false)
    {
        if (_disposed) return;
        var photo = CurrentPhoto;
        var maxEdge = FullResolutionOnZoom && IsLoupe && Zoom > 1 ? 0 : PreviewMaxEdge;
        if (!force && photo == _requestedPreviewPhoto && maxEdge == _requestedPreviewEdge) return;
        _previewCancellation?.Cancel(); _previewCancellation?.Dispose(); _previewCancellation = new();
        var token = _previewCancellation.Token;
        if (photo != _requestedPreviewPhoto || PreviewImage == null) PreviewImage = photo?.Thumbnail;
        _requestedPreviewPhoto = photo; _requestedPreviewEdge = maxEdge;
        PreviewMessage = photo == null ? "" : LanguageService.Text(maxEdge == 0 ? "Loading full-resolution preview…" : "Loading preview…");
        IsPreviewLoading = photo != null;
        if (photo == null) { PreviewImage = null; return; }
        try
        {
            var calculateHistogram = HistogramEnabled;
            var decoded = await Task.Run(async () =>
            {
                // A canceled codec may still finish its synchronous decode. Serialize these jobs
                // so fast navigation cannot queue several full-resolution buffers at once.
                await _previewDecodeGate.WaitAsync(token);
                try
                {
                    token.ThrowIfCancellationRequested();
                    var result = _preview.Load(photo.FullPath, token, maxEdge);
                    token.ThrowIfCancellationRequested();
                    var thumbnail = result.Image == null ? null : PreviewService.CreateThumbnail(result.Image);
                    var histogram = result.Image != null && calculateHistogram ? _histogram.Calculate(result.Image, token) : null;
                    return (result, thumbnail, histogram);
                }
                finally { _previewDecodeGate.Release(); }
            }, token);
            if (token.IsCancellationRequested || CurrentPhoto != photo) return;
            var result = decoded.result;
            PreviewImage = result.Image; PreviewMessage = result.Description;
            if (decoded.thumbnail != null) photo.Thumbnail = decoded.thumbnail;
            if (HistogramEnabled && decoded.histogram is { } histogram)
            {
                HistogramImage = histogram.Chart; HistogramSummary = histogram.Summary; HistogramAssessment = histogram.Assessment;
            }
            else { HistogramImage = null; HistogramSummary = LanguageService.Text(HistogramEnabled ? "No data" : "Histogram disabled"); HistogramAssessment = result.Image == null ? LanguageService.Text("The current preview could not be decoded.") : ""; }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            if (token.IsCancellationRequested || CurrentPhoto != photo) return;
            _requestedPreviewEdge = null;
            PreviewMessage = LanguageService.IsVietnamese ? "Không thể tải preview. Thử giảm chất lượng trong Cài đặt. " + e.Message : "Could not load the preview. Try a lower quality in Settings. " + e.Message;
        }
        finally { if (!token.IsCancellationRequested) IsPreviewLoading = false; }
    }
    private async Task WarmThumbnailsAsync(IReadOnlyList<ReviewPhoto> photos, CancellationToken token)
    {
        foreach (var photo in photos)
        {
            if (token.IsCancellationRequested || _disposed) return;
            await RequestThumbnailAsync(photo);
        }
    }
    public async Task RequestThumbnailAsync(ReviewPhoto photo)
    {
        if (_disposed || photo.Thumbnail != null || !_pendingThumbnails.Add(photo)) return;
        var token = _importCancellation?.Token ?? default;
        try
        {
            await _thumbnailGate.WaitAsync(token);
            try
            {
                if (_disposed || !Photos.Contains(photo)) return;
                var result = await Task.Run(() => _preview.Load(photo.FullPath, token, 240), token);
                if (!token.IsCancellationRequested && !_disposed && Photos.Contains(photo))
                {
                    photo.Thumbnail = result.Image;
                    if (result.Image != null)
                    {
                        _thumbnailOrder.Enqueue(photo);
                        while (_thumbnailOrder.Count > 512)
                        {
                            var oldest = _thumbnailOrder.Dequeue();
                            if (oldest != CurrentPhoto) oldest.Thumbnail = null;
                        }
                    }
                }
            }
            finally { _thumbnailGate.Release(); }
        }
        catch (Exception e) when (e is OperationCanceledException or IOException or NotSupportedException or UnauthorizedAccessException) { }
        finally { _pendingThumbnails.Remove(photo); }
    }
    private void NotifyCurrent()
    {
        foreach (var name in new[] { nameof(CurrentPosition), nameof(PositionLabel), nameof(RatingStars), nameof(CurrentName), nameof(CurrentPath), nameof(CurrentDetails) }) Notify(name);
        RefreshCommands();
    }
    private void NotifyCounts()
    {
        foreach (var name in new[] { nameof(PickCount), nameof(RejectCount), nameof(RatedCount), nameof(ColorCount), nameof(CatalogSummary), nameof(PositionLabel), nameof(HasNoPhotos), nameof(HasNoFilteredPhotos), nameof(VisibleScopeSummary), nameof(SendVisibleLabel), nameof(ExportVisibleNamesLabel), nameof(CopyVisibleRatedLabel) }) Notify(name);
        RefreshCommands();
    }
    private void RefreshCommands() { foreach (var command in new[] { ImportCommand, PreviousCommand, NextCommand, RevealCommand, CopyNamesCommand, ExportNamesCommand, ExportRatedCommand, OpenExportCommand, CancelCommand, ClearPreviewCacheCommand, ClearSessionCommand, WindowsCacheCleanupCommand, UndoCommand, ShowAllCommand }) command.Refresh(); }
    private void Guard(Action action) { try { action(); } catch (Exception e) { Status = e.Message; _dialogs.ShowError(e.Message); } }
    private static string FormatSize(long size) => size >= 1073741824 ? $"{size / 1073741824d:N2} GB" : size >= 1048576 ? $"{size / 1048576d:N1} MB" : $"{size / 1024d:N1} KB";
    public void Dispose()
    {
        if (_disposed) return;
        SaveSessionNow(); _disposed = true;
        _thumbnailOrder.Clear();
        _preview.ClearCache();
        _importCancellation?.Cancel(); _previewCancellation?.Cancel(); _sessionSaveCancellation?.Cancel(); _overlayCancellation?.Cancel();
        _importCancellation?.Dispose(); _previewCancellation?.Dispose(); _sessionSaveCancellation?.Dispose(); _overlayCancellation?.Dispose();
    }
}
