using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using PhotoFileFilter.Core;
using PhotoFileFilter.Features.TxtFilter.Models;
using PhotoFileFilter.Features.TxtFilter.Services;
using PhotoFileFilter.Shared.Services;
using PhotoFileFilter.Shared.ViewModels;

namespace PhotoFileFilter.Features.TxtFilter.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly IDialogService _dialogs;
    private readonly SettingsService? _settings;
    private readonly TxtParserService _parser = new();
    private readonly PhotoScannerService _scanner = new();
    private readonly FileCopyService _copier = new();
    private CancellationTokenSource? _cancellation;
    private CopyPauseToken? _copyPause;
    private ScanResult? _scan;
    private CopyResult? _copy;
    private string? _lastDestination;
    private string _txtPath = "", _sourceFolder = "", _outputFolder = "", _subfolderName = "Selected";
    private bool _comma = true, _space = true, _newLine = true, _ignoreExtension = true, _recursive = true;
    private bool _alongside = true, _subfolder = true, _busy, _scanning;
    private int _policy, _duplicates;
    private double _progress;
    private bool _darkMode = true, _completionSound = true, _updatingSelection;
    private bool _isPaused;
    private PhotoFile? _selectedFile;
    private string _searchText = "";
    private IReadOnlyList<PhotoFile> _visibleFiles = [];
    private IReadOnlyList<string> _visibleMissing = [];
    private string _status = LanguageService.Text("Ready"), _detail = LanguageService.Text("Choose a TXT filename list and a source photo folder to begin."), _currentFile = "";

    public MainViewModel(IDialogService dialogs, SettingsService? settings = null)
    {
        _dialogs = dialogs;
        _settings = settings;
        Extensions = new(new[] { "JPG", "JPEG", "PNG", "TIF", "TIFF", "HEIC", "ARW", "CR2", "CR3", "NEF", "RAF", "ORF", "RW2", "DNG" }
            .Select(name => new ExtensionOption(name, name is "JPG" or "JPEG" or "ARW" or "CR2" or "CR3")));
        foreach (var extension in Extensions) extension.PropertyChanged += (_, _) => InvalidateScan();
        BrowseTxtCommand = new(() => Guard(() => { if (_dialogs.SelectTextFile() is { } path) TxtPath = path; }), () => !Busy);
        BrowseSourceCommand = new(() => Guard(() => { if (_dialogs.SelectFolder("Choose the source photo folder") is { } path) SourceFolder = path; }), () => !Busy);
        BrowseOutputCommand = new(() => Guard(() => { if (_dialogs.SelectFolder("Choose the destination folder") is { } path) OutputFolder = path; }), () => !Busy);
        ScanCommand = new(async () => await ScanAsync(), () => !Busy);
        CopyCommand = new(async () => await CopyAsync(), () => !Busy && CopyCount > 0);
        CancelCommand = new(() => { _cancellation?.Cancel(); Status = "Canceling…"; }, () => Busy);
        PauseCommand = new(PauseCopy, () => CanPauseCopy);
        ResumeCommand = new(ResumeCopy, () => CanResumeCopy);
        ExportCommand = new(() => Guard(ExportReport), () => !Busy && _scan != null);
        OpenDestinationCommand = new(() => Guard(() => _dialogs.OpenFolder(_lastDestination!)), () => !Busy && _lastDestination != null && Directory.Exists(_lastDestination));
        SelectRawCommand = new(() => { foreach (var item in Extensions) item.Selected = item.Name is "ARW" or "CR2" or "CR3" or "NEF" or "RAF" or "ORF" or "RW2" or "DNG"; }, () => !Busy);
        SelectAllCommand = new(() => { foreach (var item in Extensions) item.Selected = true; }, () => !Busy);
        SelectJpgCommand = new(() => { foreach (var item in Extensions) item.Selected = item.Name is "JPG" or "JPEG"; }, () => !Busy);
        ClearSearchCommand = new(() => SearchText = "");
        CopyMissingCommand = new(() => Guard(() => { _dialogs.CopyText(string.Join(Environment.NewLine, Missing)); Status = "Copied missing names"; Detail = $"{MissingCount:N0} missing names were copied to the clipboard."; }), () => !Busy && MissingCount > 0);
        CopyPathCommand = new(() => Guard(() => _dialogs.CopyText(SelectedFile!.FullPath)), () => SelectedFile != null);
        RevealFileCommand = new(() => Guard(() => _dialogs.RevealFile(SelectedFile!.FullPath)), () => SelectedFile != null);
        PreviewCommand = new(() => Guard(() => _dialogs.ShowPreview(SelectedFile!, VisibleFiles.ToArray())), () => !Busy && SelectedFile != null);
        ToggleIncludedCommand = new(() => SelectedFile!.IncludeInCopy = !SelectedFile.IncludeInCopy, () => !Busy && SelectedFile != null);
        IncludeAllCommand = new(() =>
        {
            _updatingSelection = true;
            try { foreach (var file in Files) file.IncludeInCopy = true; }
            finally { _updatingSelection = false; NotifySelection(); }
        }, () => !Busy && Files.Count > 0);
        if (_settings?.Load() is { } saved) RestoreSettings(saved); else ThemeService.Apply(_darkMode);
    }
    public ObservableCollection<ExtensionOption> Extensions { get; }
    public RelayCommand BrowseTxtCommand { get; }
    public RelayCommand BrowseSourceCommand { get; }
    public RelayCommand BrowseOutputCommand { get; }
    public RelayCommand ScanCommand { get; }
    public RelayCommand CopyCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand PauseCommand { get; }
    public RelayCommand ResumeCommand { get; }
    public RelayCommand ExportCommand { get; }
    public RelayCommand OpenDestinationCommand { get; }
    public RelayCommand SelectRawCommand { get; }
    public RelayCommand SelectAllCommand { get; }
    public RelayCommand SelectJpgCommand { get; }
    public RelayCommand ClearSearchCommand { get; }
    public RelayCommand CopyMissingCommand { get; }
    public RelayCommand CopyPathCommand { get; }
    public RelayCommand RevealFileCommand { get; }
    public RelayCommand PreviewCommand { get; }
    public RelayCommand ToggleIncludedCommand { get; }
    public RelayCommand IncludeAllCommand { get; }
    public PhotoFile? SelectedFile { get => _selectedFile; set { if (Set(ref _selectedFile, value)) { RefreshCommands(); Notify(nameof(ToggleIncludedLabel)); } } }
    public string ToggleIncludedLabel => LanguageService.Text(SelectedFile?.IncludeInCopy == true ? "Exclude from copy" : "Include in copy");
    public int CopyCount => Files.Count(f => f.IncludeInCopy);
    public bool DarkMode { get => _darkMode; set { if (Set(ref _darkMode, value)) { ThemeService.Apply(value); SaveSettings(); } } }
    public bool CompletionSound { get => _completionSound; set => Set(ref _completionSound, value); }
    public string FormatBreakdown => string.Join("  ·  ", Files.GroupBy(f => f.FileType).OrderBy(g => g.Key).Select(g => $"{g.Count():N0} {g.Key}"));
    public string SearchText { get => _searchText; set { if (Set(ref _searchText, value)) RefreshFilter(); } }
    public IReadOnlyList<PhotoFile> VisibleFiles => _visibleFiles;
    public IReadOnlyList<string> VisibleMissing => _visibleMissing;
    public string FilterSummary => LanguageService.IsVietnamese ? $"Đang hiện {VisibleFiles.Count:N0}/{MatchedCount:N0} file · {CopyCount:N0} file được chọn để sao chép. Tìm kiếm không thay đổi lựa chọn." : $"Showing {VisibleFiles.Count:N0}/{MatchedCount:N0} files · {CopyCount:N0} selected for copying. Search does not change your selection.";
    public string TxtFileName => string.IsNullOrWhiteSpace(TxtPath) ? LanguageService.Text("Choose or drop a .txt file") : Path.GetFileName(TxtPath);
    public string TxtFileHint => string.IsNullOrWhiteSpace(TxtPath) ? LanguageService.Text("Filename list selected by the client") : TxtPath;
    public string SourceHint => string.IsNullOrWhiteSpace(SourceFolder) ? LanguageService.Text("Choose or drop the source photo folder") : SourceFolder;
    public string TotalSize => FormatSize(Files.Where(f => f.IncludeInCopy).Sum(f => f.Size));
    public string SelectionSummary => LanguageService.IsVietnamese ? $"Đã chọn {CopyCount:N0}/{MatchedCount:N0} file · {TotalSize}" : $"Selected {CopyCount:N0}/{MatchedCount:N0} files · {TotalSize}";
    public string Readiness => _scan == null ? "Waiting to scan" : CopyCount == 0 ? "No files selected for copying" : "Ready to copy";
    private static string FormatSize(long size) => size >= 1073741824 ? $"{size / 1073741824d:N2} GB" : size >= 1048576 ? $"{size / 1048576d:N1} MB" : $"{size / 1024d:N1} KB";
    public string TxtPath { get => _txtPath; set { if (Set(ref _txtPath, value)) { InvalidateScan(); Notify(nameof(TxtFileName)); Notify(nameof(TxtFileHint)); Notify(nameof(WorkflowHint)); } } }
    public string SourceFolder { get => _sourceFolder; set { if (Set(ref _sourceFolder, value)) { InvalidateScan(); Notify(nameof(DestinationPreview)); Notify(nameof(SourceHint)); Notify(nameof(WorkflowHint)); } } }
    public bool Comma { get => _comma; set { if (Set(ref _comma, value)) InvalidateScan(); } }
    public bool Space { get => _space; set { if (Set(ref _space, value)) InvalidateScan(); } }
    public bool NewLine { get => _newLine; set { if (Set(ref _newLine, value)) InvalidateScan(); } }
    public bool IgnoreExtension { get => _ignoreExtension; set { if (Set(ref _ignoreExtension, value)) InvalidateScan(); } }
    public bool Recursive { get => _recursive; set { if (Set(ref _recursive, value)) InvalidateScan(); } }
    public bool Alongside { get => _alongside; set { if (Set(ref _alongside, value)) { Notify(nameof(SpecificFolder)); OutputChanged(); } } }
    public bool SpecificFolder { get => !_alongside; set => Alongside = !value; }
    public string OutputFolder { get => _outputFolder; set { if (Set(ref _outputFolder, value)) OutputChanged(); } }
    public bool Subfolder { get => _subfolder; set { if (Set(ref _subfolder, value)) OutputChanged(); } }
    public bool NeedsSubfolder => Alongside || Subfolder;
    public bool UseSubfolder { get => NeedsSubfolder; set => Subfolder = value; }
    public string SubfolderName { get => _subfolderName; set { if (Set(ref _subfolderName, value)) OutputChanged(); } }
    public int Policy { get => _policy; set => Set(ref _policy, value); }
    public string[] Policies => LanguageService.Texts("Rename — keep both", "Skip existing files", "Replace destination files");
    public bool Busy { get => _busy; private set { Set(ref _busy, value); Notify(nameof(Idle)); NotifyPauseState(); RefreshCommands(); } }
    public bool Idle => !Busy;
    public bool Scanning { get => _scanning; private set { if (Set(ref _scanning, value)) NotifyPauseState(); } }
    public bool IsPaused { get => _isPaused; private set { if (Set(ref _isPaused, value)) NotifyPauseState(); } }
    public bool CanPauseCopy => Busy && !Scanning && !IsPaused;
    public bool CanResumeCopy => Busy && !Scanning && IsPaused;
    public double Progress { get => _progress; private set => Set(ref _progress, value); }
    public string Status { get => _status; private set => Set(ref _status, value); }
    public string Detail { get => _detail; private set => Set(ref _detail, value); }
    public string CurrentFile { get => _currentFile; private set => Set(ref _currentFile, value); }
    public IReadOnlyList<PhotoFile> Files => _scan?.Files ?? [];
    public IReadOnlyList<string> Missing => _scan?.Missing ?? [];
    public IReadOnlyList<string> Issues => (_scan?.Warnings ?? []).Concat(_copy?.Errors.Select(e => $"{e.File}: {e.Reason}") ?? []).ToArray();
    public int RequestedCount => _scan?.RequestedNames.Count ?? 0;
    public int MatchedCount => Files.Count;
    public int MissingCount => Missing.Count;
    public string FilesTab => LanguageService.IsVietnamese ? $"File tìm thấy ({MatchedCount:N0})" : $"Files Found ({MatchedCount:N0})";
    public string MissingTab => LanguageService.IsVietnamese ? $"Không tìm thấy ({MissingCount:N0})" : $"Not Found ({MissingCount:N0})";
    public string IssuesTab => LanguageService.IsVietnamese ? $"Ghi chú / Lỗi ({Issues.Count:N0})" : $"Notes / Errors ({Issues.Count:N0})";
    public bool HasNoResults => _scan == null;
    public string ResultSummary => _scan == null ? "Results will appear after scanning." :
        $"Scanned {_scan.ExaminedCount:N0} files · removed {_duplicates:N0} duplicate names · {Files.Sum(f => f.Size) / 1073741824d:N2} GB";
    public string CopyLabel => LanguageService.IsVietnamese ? (CopyCount > 0 ? $"Sao chép {CopyCount:N0} file  →" : "Sao chép ảnh  →") : CopyCount > 0 ? $"Copy {CopyCount:N0} Files  →" : "Copy Photos  →";
    public string WorkflowHint
    {
        get
        {
            if (string.IsNullOrWhiteSpace(TxtPath)) return LanguageService.IsVietnamese ? "Bước 1/4 · Thêm danh sách tên TXT" : "Step 1/4 · Add a TXT filename list";
            if (string.IsNullOrWhiteSpace(SourceFolder)) return LanguageService.IsVietnamese ? "Bước 2/4 · Chọn thư mục ảnh nguồn" : "Step 2/4 · Choose the source photo folder";
            if (_scan == null) return LanguageService.IsVietnamese ? "Bước 3/4 · Quét và kiểm tra kết quả" : "Step 3/4 · Scan and review the matches";
            if (_copy == null) return LanguageService.IsVietnamese ? $"Bước 4/4 · Chọn nơi lưu và chép {CopyCount:N0} file" : $"Step 4/4 · Choose a destination and copy {CopyCount:N0} files";
            return LanguageService.IsVietnamese ? "Hoàn tất · Kiểm tra thư mục đích hoặc bắt đầu phiên mới" : "Complete · Review the destination or start a new session";
        }
    }
    public string DestinationPreview
    {
        get { try { return LanguageService.Text(ResolveDestination()); } catch (Exception e) when (e is ArgumentException or IOException or UnauthorizedAccessException or NotSupportedException) { return LanguageService.Text(e.Message); } }
    }
    private string ResolveDestination() => PathSafety.Destination(SourceFolder, Alongside, OutputFolder, Subfolder, SubfolderName);
    private void OutputChanged() { Notify(nameof(DestinationPreview)); Notify(nameof(NeedsSubfolder)); Notify(nameof(UseSubfolder)); }
    private void RefreshCommands()
    {
        foreach (var command in new[] { BrowseTxtCommand, BrowseSourceCommand, BrowseOutputCommand, ScanCommand, CopyCommand, CancelCommand, PauseCommand, ResumeCommand, ExportCommand, OpenDestinationCommand, SelectRawCommand, SelectAllCommand, SelectJpgCommand, CopyMissingCommand, CopyPathCommand, RevealFileCommand, PreviewCommand, ToggleIncludedCommand, IncludeAllCommand }) command?.Refresh();
    }
    private void NotifyPauseState()
    {
        Notify(nameof(CanPauseCopy)); Notify(nameof(CanResumeCopy));
        PauseCommand?.Refresh(); ResumeCommand?.Refresh();
    }
    private void PauseCopy()
    {
        if (!CanPauseCopy) return;
        _copyPause?.Pause(); IsPaused = true; Status = "Copy paused"; Detail = "Completed files are kept. Select Resume to continue or Cancel to stop.";
    }
    private void ResumeCopy()
    {
        if (!CanResumeCopy) return;
        _copyPause?.Resume(); IsPaused = false; Status = "Copying photos…";
    }
    private void NotifyResults()
    {
        foreach (var name in new[] { nameof(Files), nameof(Missing), nameof(Issues), nameof(RequestedCount), nameof(MatchedCount), nameof(MissingCount), nameof(FilesTab), nameof(MissingTab), nameof(IssuesTab), nameof(HasNoResults), nameof(ResultSummary), nameof(CopyLabel), nameof(TotalSize), nameof(SelectionSummary), nameof(Readiness), nameof(WorkflowHint) }) Notify(name);
        RefreshFilter();
        NotifySelection();
        Notify(nameof(FormatBreakdown));
        RefreshCommands();
    }
    private void NotifySelection()
    {
        if (_updatingSelection) return;
        foreach (var name in new[] { nameof(CopyCount), nameof(CopyLabel), nameof(SelectionSummary), nameof(TotalSize), nameof(Readiness), nameof(ToggleIncludedLabel), nameof(FilterSummary), nameof(WorkflowHint) }) Notify(name);
        RefreshCommands();
    }
    public void RefreshLanguage()
    {
        Status = LanguageService.Text(Status);
        Detail = LanguageService.Text(Detail);
        foreach (var name in new[] { nameof(Policies), nameof(ToggleIncludedLabel), nameof(FilterSummary), nameof(TxtFileName), nameof(TxtFileHint), nameof(SourceHint), nameof(SelectionSummary), nameof(Readiness), nameof(FilesTab), nameof(MissingTab), nameof(IssuesTab), nameof(ResultSummary), nameof(CopyLabel), nameof(WorkflowHint), nameof(DestinationPreview) }) Notify(name);
    }
    private void InvalidateScan()
    {
        if (_scan == null) return;
        _scan = null; _copy = null; SelectedFile = null; Progress = 0;
        Status = "Rescan required"; Detail = "The matching options changed. Scan again to refresh the results."; NotifyResults();
    }
    private void StartOperation(string status, bool scanning)
    {
        SaveSettings();
        _cancellation = new(); Busy = true; Scanning = scanning; Progress = 0; CurrentFile = ""; Status = status;
    }
    private void RefreshFilter()
    {
        var query = SearchText.Trim();
        _visibleFiles = query.Length == 0 ? Files : Files.Where(f => f.RelativePath.Contains(query, StringComparison.OrdinalIgnoreCase)).ToArray();
        _visibleMissing = query.Length == 0 ? Missing : Missing.Where(n => n.Contains(query, StringComparison.OrdinalIgnoreCase)).ToArray();
        Notify(nameof(VisibleFiles)); Notify(nameof(VisibleMissing)); Notify(nameof(FilterSummary));
    }

    public bool ApplyDroppedPaths(IEnumerable<string> paths, string target)
    {
        if (Busy) return false;
        var path = paths.FirstOrDefault(p => target == "txt" ? File.Exists(p) && string.Equals(Path.GetExtension(p), ".txt", StringComparison.OrdinalIgnoreCase) : Directory.Exists(p));
        if (path == null) return false;
        switch (target)
        {
            case "txt": TxtPath = path; break;
            case "source": SourceFolder = path; break;
            case "output": SpecificFolder = true; OutputFolder = path; break;
            default: return false;
        }
        return true;
    }

    public void SaveSettings() => _settings?.Save(new UserSettings
    {
        TxtPath = TxtPath, SourceFolder = SourceFolder, OutputFolder = OutputFolder, SubfolderName = SubfolderName,
        Comma = Comma, Space = Space, NewLine = NewLine, IgnoreExtension = IgnoreExtension, Recursive = Recursive,
        Alongside = Alongside, Subfolder = Subfolder, Policy = Policy, DarkMode = DarkMode, CompletionSound = CompletionSound, Extensions = Extensions.Where(e => e.Selected).Select(e => e.Name).ToArray()
    });

    public void ClearSession()
    {
        if (Busy) return;
        TxtPath = "";
        SourceFolder = "";
        OutputFolder = "";
        SubfolderName = "Selected";
        Comma = true;
        Space = true;
        NewLine = true;
        IgnoreExtension = true;
        Recursive = true;
        Alongside = true;
        Subfolder = true;
        Policy = 0;
        SearchText = "";
        _scan = null;
        _copy = null;
        _lastDestination = null;
        SelectedFile = null;
        _duplicates = 0;
        Progress = 0;
        CurrentFile = "";
        Status = "Ready";
        Detail = "Choose a TXT filename list and a source photo folder to begin.";
        foreach (var extension in Extensions)
            extension.Selected = extension.Name is "JPG" or "JPEG" or "ARW" or "CR2" or "CR3";
        Notify(nameof(DestinationPreview));
        Notify(nameof(TxtFileName));
        Notify(nameof(TxtFileHint));
        Notify(nameof(SourceHint));
        NotifyResults();
        SaveSettings();
    }

    private void RestoreSettings(UserSettings saved)
    {
        TxtPath = saved.TxtPath ?? ""; SourceFolder = saved.SourceFolder ?? ""; OutputFolder = saved.OutputFolder ?? "";
        SubfolderName = saved.SubfolderName ?? "Selected"; Comma = saved.Comma; Space = saved.Space; NewLine = saved.NewLine;
        IgnoreExtension = saved.IgnoreExtension; Recursive = saved.Recursive; Alongside = saved.Alongside; Subfolder = saved.Subfolder;
        Policy = Math.Clamp(saved.Policy, 0, Policies.Length - 1);
        _darkMode = saved.DarkMode; _completionSound = saved.CompletionSound;
        ThemeService.Apply(_darkMode);
        if (saved.Extensions != null)
            foreach (var extension in Extensions) extension.Selected = saved.Extensions.Contains(extension.Name, StringComparer.OrdinalIgnoreCase);
    }
    private void EndOperation() { Scanning = false; Busy = false; _cancellation?.Dispose(); _cancellation = null; }
    private void Guard(Action action)
    {
        try { action(); }
        catch (Exception e) { Status = "Could not complete the action"; Detail = e.Message; _dialogs.ShowError(e.Message); }
    }
    public async Task ScanAsync()
    {
        if (Busy) return;
        try
        {
            if (!File.Exists(TxtPath)) throw new ArgumentException("Choose a TXT filename list before scanning.");
            if (!Directory.Exists(SourceFolder)) throw new ArgumentException("Choose a valid source photo folder before scanning.");
            var extensions = Extensions.Where(e => e.Selected).Select(e => e.Name).ToArray();
            if (extensions.Length == 0) throw new ArgumentException("Select at least one file type to find.");
            var parsing = new ParseOptions(Comma, Space, NewLine, IgnoreExtension);
            _scan = null; _copy = null; SelectedFile = null; NotifyResults();
            StartOperation("Scanning photos…", true); Detail = "Reading the list and matching filenames.";
            var token = _cancellation!.Token;
            var parsed = await Task.Run(() => _parser.ReadAsync(TxtPath, parsing, token), token);
            _duplicates = parsed.DuplicateCount;
            string? excluded = null;
            try { excluded = ResolveDestination(); } catch (Exception e) when (e is ArgumentException or IOException or UnauthorizedAccessException or NotSupportedException) { /* Output can be configured after scanning. */ }
            var options = new ScanOptions(SourceFolder, parsed.Names, extensions, IgnoreExtension, Recursive, excluded);
            var progress = new Progress<OperationProgress>(p => { CurrentFile = p.CurrentFile; Detail = $"Scanned {p.Completed:N0} files…"; });
            _scan = await Task.Run(() => _scanner.Scan(options, progress, token), token);
            foreach (var file in _scan.Files) file.PropertyChanged += (_, _) => NotifySelection();
            Status = _scan.Warnings.Count > 0 ? "Scan completed with notes" : "Scan complete";
            Detail = $"Matched {MatchedCount:N0} files · {MissingCount:N0} names not found. Review the results and choose a destination.";
            CurrentFile = ""; Progress = 100; NotifyResults();
        }
        catch (OperationCanceledException) { Status = "Scan canceled"; Detail = "No files were copied. You can scan again."; }
        catch (Exception e) { Status = "Could not scan"; Detail = e.Message; _dialogs.ShowError(e.Message); }
        finally { EndOperation(); }
    }
    public async Task CopyAsync()
    {
        if (Busy) return;
        try
        {
            var scan = _scan;
            if (scan == null || CopyCount == 0) throw new ArgumentException("Scan and select photos before copying.");
            var included = scan.Files.Where(f => f.IncludeInCopy).Select(f => f.FullPath).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var destination = ResolveDestination();
            if (!_dialogs.ConfirmCopy(included.Count, scan.SourceFolder, destination, Policies[Policy])) return;
            _copy = null; StartOperation("Copying photos…", false); Detail = $"0 / {included.Count:N0} file";
            _copyPause = new(); IsPaused = false;
            _lastDestination = destination;
            var progress = new Progress<OperationProgress>(p =>
            {
                Progress = p.TotalBytes > 0 ? 100d * p.ProcessedBytes / p.TotalBytes : p.Total == 0 ? 0 : 100d * p.Completed / p.Total;
                var eta = p.RemainingSeconds is { } seconds && double.IsFinite(seconds)
                    ? $" · {(LanguageService.IsVietnamese ? "Còn khoảng" : "ETA")} {Math.Ceiling(seconds / 60):N0} min" : "";
                CurrentFile = p.CurrentFile;
                Detail = $"{p.Completed:N0} / {p.Total:N0} file · {Progress:N0}% · {p.BytesPerSecond / 1048576d:N1} MiB/s{eta}";
            });
            var options = new CopyOptions(destination, (CollisionPolicy)Policy, included);
            var token = _cancellation!.Token;
            _copy = await Task.Run(() => _copier.CopyAsync(scan, options, progress, token, _copyPause), token);
            Status = _copy.Cancelled ? "Copy canceled" : _copy.Errors.Count > 0 ? "Copy completed with errors" : "Copy complete";
            Detail = $"{_copy.Copied:N0} copied · {_copy.Skipped:N0} skipped · {_copy.Errors.Count:N0} errors" + (_copy.Cancelled ? ". Files already copied were kept." : ".");
            CurrentFile = ""; NotifyResults();
            if (!_copy.Cancelled && CompletionSound) _dialogs.PlayCompletionSound();
        }
        catch (OperationCanceledException) { Status = "Copy canceled"; Detail = "The operation was canceled."; }
        catch (Exception e) { Status = "Could not copy files"; Detail = e.Message; _dialogs.ShowError(e.Message); }
        finally { _copyPause?.Resume(); _copyPause = null; IsPaused = false; EndOperation(); }
    }
    private void ExportReport()
    {
        if (_scan == null || _dialogs.SaveReport() is not { } path) return;
        var text = new StringBuilder().AppendLine("PHOTO FILE FILTER — REPORT").AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            .AppendLine($"Source: {_scan.SourceFolder}").AppendLine($"Requested: {RequestedCount} | Files matched: {MatchedCount} | Not found: {MissingCount}")
            .AppendLine(ResultSummary).AppendLine().AppendLine("NOT FOUND");
        foreach (var name in Missing) text.AppendLine(name);
        text.AppendLine().AppendLine("FILES FOUND");
        foreach (var file in Files) text.AppendLine(file.FullPath);
        text.AppendLine().AppendLine("EXCLUDED FROM COPY");
        foreach (var file in Files.Where(f => !f.IncludeInCopy)) text.AppendLine(file.FullPath);
        if (_copy != null) text.AppendLine().AppendLine($"COPY → {_lastDestination}").AppendLine($"Copied: {_copy.Copied} | Skipped: {_copy.Skipped} | Errors: {_copy.Errors.Count} | Canceled: {_copy.Cancelled}");
        text.AppendLine().AppendLine("NOTES / ERRORS");
        foreach (var issue in Issues) text.AppendLine(issue);
        File.WriteAllText(path, text.ToString(), new UTF8Encoding(true));
        Status = "Report saved"; Detail = path;
    }
}
