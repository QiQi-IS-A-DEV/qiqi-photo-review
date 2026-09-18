using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using PhotoFileFilter.Review;
using PhotoFileFilter.Services;

namespace PhotoFileFilter;

public partial class ReviewWindow : Window
{
    private readonly ReviewViewModel _viewModel;
    private readonly string? _initialFolder;
    private TxtFilterView? _filterView;
    private Point? _panStart;
    private Point _panOrigin;
    private bool _isPanning;
    private Grid? _panViewport;
    private bool _zenMode;
    private bool _languageReady;
    private GridLength _navigatorWidth = new(240), _ratingWidth = new(255);

    public ReviewWindow() : this(null, null) { }

    public ReviewWindow(string? initialFolder) : this(initialFolder, null) { }

    public ReviewWindow(ReviewViewModel viewModel) : this(null, viewModel) { }

    private ReviewWindow(string? initialFolder, ReviewViewModel? viewModel)
    {
        InitializeComponent();
        ThemeService.Apply(true);
        _initialFolder = Directory.Exists(initialFolder) ? initialFolder : null;
        _viewModel = viewModel ?? new(new DialogService()); DataContext = _viewModel;
        LanguageCombo.SelectedValue = LanguageService.CurrentLanguage;
        MinWidth = Math.Min(MinWidth, SystemParameters.WorkArea.Width);
        MinHeight = Math.Min(MinHeight, SystemParameters.WorkArea.Height);
        Width = Math.Min(Width, SystemParameters.WorkArea.Width); Height = Math.Min(Height, SystemParameters.WorkArea.Height);
        Loaded += async (_, _) =>
        {
            LanguageService.Apply(this); _languageReady = true;
            if (_initialFolder != null) await _viewModel.ImportAsync(_initialFolder);
            else await _viewModel.RestoreSessionAsync();
            if (_viewModel.StartWithPanelsHidden && !_zenMode) ToggleZenMode();
        };
        Closed += (_, _) =>
        {
            _viewModel.Dispose();
            _filterView?.SaveSettings();
        };
    }

    private void OnReviewKeyDown(object sender, KeyEventArgs e)
    {
        if (SettingsOverlay.Visibility == Visibility.Visible)
        {
            if (e.Key == Key.Escape) { HideSettingsPopup(); e.Handled = true; }
            return;
        }
        if (HelpOverlay.Visibility == Visibility.Visible)
        {
            if (e.Key == Key.Escape || MatchesShortcut(e, _viewModel.HelpShortcut)) HideHelpPopup();
            e.Handled = true; return;
        }
        var control = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        if (control && e.Key == Key.D1)
        {
            if (FilterHost.Visibility == Visibility.Visible) ShowReviewScreen();
            e.Handled = true; return;
        }
        if (control && e.Key == Key.D2)
        {
            if (FilterHost.Visibility != Visibility.Visible) ShowFilterScreen();
            e.Handled = true; return;
        }
        if (FilterHost.Visibility == Visibility.Visible && (e.Key == Key.F1 || MatchesShortcut(e, _viewModel.HelpShortcut)))
        {
            _filterView?.ShowHelpPopup();
            e.Handled = true; return;
        }
        if (MatchesShortcut(e, _viewModel.HelpShortcut)) { ShowHelpPopup(); e.Handled = true; return; }
        if (FilterHost.Visibility != Visibility.Visible && MatchesShortcut(e, _viewModel.ZenShortcut))
        { ToggleZenMode(); e.Handled = true; return; }
        if (FilterHost.Visibility == Visibility.Visible)
        {
            // Let the focused workspace handle its own routed keyboard commands.
            return;
        }
        if (e.OriginalSource is TextBox or ComboBox) return;
        if (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.None)
        {
            if (!e.IsRepeat) _viewModel.ViewMode = _viewModel.IsGrid ? 1 : 0;
            e.Handled = true;
            return;
        }
        if (control && e.Key == Key.F) { _viewModel.ShowFilmstrip = !_viewModel.ShowFilmstrip; e.Handled = true; return; }
        if (MatchesShortcut(e, _viewModel.UndoShortcut)) { _viewModel.Undo(); e.Handled = true; return; }
        if (control && e.Key == Key.OemOpenBrackets) { _viewModel.Rotate(ReviewTargets(), -90); e.Handled = true; return; }
        if (control && e.Key == Key.OemCloseBrackets) { _viewModel.Rotate(ReviewTargets(), 90); e.Handled = true; return; }
        if (control && e.Key is Key.OemPlus or Key.Add) { _viewModel.ZoomBy(1); e.Handled = true; return; }
        if (control && e.Key is Key.OemMinus or Key.Subtract) { _viewModel.ZoomBy(-1); e.Handled = true; return; }
        if (MatchesShortcut(e, _viewModel.ResetZoomShortcut)) { _viewModel.ResetZoom(); e.Handled = true; return; }
        if (MatchesShortcut(e, _viewModel.GridShortcut)) { _viewModel.ViewMode = 0; e.Handled = true; return; }
        if (MatchesShortcut(e, _viewModel.LoupeShortcut)) { _viewModel.ViewMode = 1; e.Handled = true; return; }
        if (e.Key == Key.Multiply || (e.Key == Key.D8 && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)))
        { _viewModel.SetColor(ReviewTargets(), ReviewColor.None); e.Handled = true; return; }
        switch (e.Key)
        {
            case Key.Left: _viewModel.Previous(); break;
            case Key.Right: _viewModel.Next(); break;
            case Key.Up when _viewModel.IsGrid: _viewModel.MoveGrid(-GridColumnCount()); break;
            case Key.Down when _viewModel.IsGrid: _viewModel.MoveGrid(GridColumnCount()); break;
            case Key.D0: case Key.NumPad0: _viewModel.SetRating(ReviewTargets(), 0); break;
            case Key.D1: case Key.NumPad1: _viewModel.SetRating(ReviewTargets(), 1); break;
            case Key.D2: case Key.NumPad2: _viewModel.SetRating(ReviewTargets(), 2); break;
            case Key.D3: case Key.NumPad3: _viewModel.SetRating(ReviewTargets(), 3); break;
            case Key.D4: case Key.NumPad4: _viewModel.SetRating(ReviewTargets(), 4); break;
            case Key.D5: case Key.NumPad5: _viewModel.SetRating(ReviewTargets(), 5); break;
            case Key.D6: case Key.NumPad6: _viewModel.SetColor(ReviewTargets(), ReviewColor.Red); break;
            case Key.D7: case Key.NumPad7: _viewModel.SetColor(ReviewTargets(), ReviewColor.Yellow); break;
            case Key.D8: case Key.NumPad8: _viewModel.SetColor(ReviewTargets(), ReviewColor.Green); break;
            case Key.D9: case Key.NumPad9: _viewModel.SetColor(ReviewTargets(), ReviewColor.Blue); break;
            case Key.T when Keyboard.Modifiers == ModifierKeys.None: _viewModel.SetColor(ReviewTargets(), ReviewColor.Purple); break;
            case Key.P: _viewModel.SetFlag(ReviewTargets(), ReviewFlag.Pick); break;
            case Key.X: _viewModel.SetFlag(ReviewTargets(), ReviewFlag.Reject); break;
            case Key.U: _viewModel.SetFlag(ReviewTargets(), ReviewFlag.None); break;
            default: return;
        }
        e.Handled = true;
    }
    private void OnRatingHover(object sender, MouseEventArgs e) { if (sender is FrameworkElement { Tag: string value } && int.TryParse(value, out var rating)) _viewModel.PreviewRating(rating); }
    private void OnRatingLeave(object sender, MouseEventArgs e) => _viewModel.PreviewRating(null);
    private void OnFilmstripResize(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e) => _viewModel.FilmstripHeight -= (int)Math.Round(e.VerticalChange);
    private void OnFilmstripResizeCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) => _viewModel.SaveFilmstripSize();
    private void OnCopyFolder(object sender, RoutedEventArgs e) => _viewModel.CopySourceFolder();
    private void OnOpenSourceFolder(object sender, RoutedEventArgs e) => _viewModel.OpenSourceFolder();
    private void OnRateClick(object sender, RoutedEventArgs e) { if (sender is FrameworkElement { Tag: string value } && int.TryParse(value, out var rating)) _viewModel.SetRating(ReviewTargets(), rating); }
    private void OnPick(object sender, RoutedEventArgs e) => _viewModel.SetFlag(ReviewTargets(), ReviewFlag.Pick);
    private void OnReject(object sender, RoutedEventArgs e) => _viewModel.SetFlag(ReviewTargets(), ReviewFlag.Reject);
    private void OnUnflag(object sender, RoutedEventArgs e) => _viewModel.SetFlag(ReviewTargets(), ReviewFlag.None);
    private void OnColorClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string value } && Enum.TryParse<ReviewColor>(value, out var color)) _viewModel.SetColor(ReviewTargets(), color);
    }
    private void OnRotateLeft(object sender, RoutedEventArgs e) => _viewModel.Rotate(ReviewTargets(), -90);
    private void OnRotateRight(object sender, RoutedEventArgs e) => _viewModel.Rotate(ReviewTargets(), 90);
    private void OnZoomIn(object sender, RoutedEventArgs e) => _viewModel.ZoomBy(1);
    private void OnZoomOut(object sender, RoutedEventArgs e) => _viewModel.ZoomBy(-1);
    private void OnResetZoom(object sender, RoutedEventArgs e) => _viewModel.ResetZoom();
    private void OnLoupeMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not Grid viewport) return;
        var pointer = e.GetPosition(viewport);
        _viewModel.ZoomAt(e.Delta > 0 ? 1 : -1, pointer.X - viewport.ActualWidth / 2, pointer.Y - viewport.ActualHeight / 2);
        e.Handled = true;
    }
    private void OnLoupeMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || !_viewModel.HasPreview || IsInsideButton(e.OriginalSource as DependencyObject)) return;
        if (sender is not Grid viewport) return;
        _panViewport = viewport;
        _panStart = e.GetPosition(viewport); _panOrigin = new(_viewModel.PanX, _viewModel.PanY);
        _isPanning = false;
        viewport.CaptureMouse(); viewport.Cursor = _viewModel.Zoom > 1 ? Cursors.SizeAll : Cursors.Hand; e.Handled = true;
    }
    private void OnLoupeMouseMove(object sender, MouseEventArgs e)
    {
        if (_panViewport == null || _panStart is not { } start || e.LeftButton != MouseButtonState.Pressed) return;
        var current = e.GetPosition(_panViewport);
        if (!_isPanning && Math.Abs(current.X - start.X) + Math.Abs(current.Y - start.Y) < 5) return;
        if (_viewModel.Zoom <= 1) return;
        _isPanning = true;
        _viewModel.PanTo(_panOrigin.X + current.X - start.X, _panOrigin.Y + current.Y - start.Y);
        e.Handled = true;
    }
    private void OnLoupeMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_panViewport is not { } viewport || _panStart is not { } start) return;
        var wasPanning = _isPanning;
        _panStart = null; _isPanning = false; viewport.ReleaseMouseCapture(); viewport.Cursor = Cursors.Arrow;
        if (!wasPanning)
        {
            if (_viewModel.Zoom > 1) _viewModel.ResetZoom();
            else _viewModel.ZoomTo(_viewModel.ClickZoomPercent / 100d, start.X - viewport.ActualWidth / 2, start.Y - viewport.ActualHeight / 2);
        }
        e.Handled = true;
    }
    private void OnLoupeLostMouseCapture(object sender, MouseEventArgs e) { _panStart = null; _isPanning = false; if (_panViewport != null) _panViewport.Cursor = Cursors.Arrow; _panViewport = null; }
    private static bool IsInsideButton(DependencyObject? element)
    {
        while (element != null)
        {
            if (element is Button) return true;
            element = System.Windows.Media.VisualTreeHelper.GetParent(element);
        }
        return false;
    }
    private ReviewPhoto[] ReviewTargets() => _viewModel.IsGrid && GridPhotos.SelectedItems.Count > 0 ? GridPhotos.SelectedItems.Cast<ReviewPhoto>().ToArray() : _viewModel.CurrentPhoto == null ? [] : [_viewModel.CurrentPhoto];
    private void OnGridSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _viewModel.UpdateSelectionCount(GridPhotos.SelectedItems.Count);
        if (e.AddedItems.Count > 0) GridPhotos.ScrollIntoView(e.AddedItems[e.AddedItems.Count - 1]);
    }
    private void OnGridRightClick(object sender, MouseButtonEventArgs e)
    {
        if (ItemsControl.ContainerFromElement(GridPhotos, e.OriginalSource as DependencyObject) is not ListBoxItem item) return;
        if (!item.IsSelected)
        {
            GridPhotos.SelectedItems.Clear();
            item.IsSelected = true;
        }
        _viewModel.CurrentPhoto = item.DataContext as ReviewPhoto;
    }
    private async void OnContextExport(object sender, RoutedEventArgs e) => await _viewModel.ExportSelectedAsync(ReviewTargets());
    private void OnContextExportNames(object sender, RoutedEventArgs e) => _viewModel.ExportNamesWithDialog(ReviewTargets());
    private void OnContextSendToFilter(object sender, RoutedEventArgs e) => SendToFilter(ReviewTargets());
    private void OnContextRating(object sender, RoutedEventArgs e) { if (sender is FrameworkElement { Tag: string value } && int.TryParse(value, out var rating)) _viewModel.SetRating(ReviewTargets(), rating); }
    private void OnContextColor(object sender, RoutedEventArgs e) { if (sender is FrameworkElement { Tag: string value } && Enum.TryParse<ReviewColor>(value, out var color)) _viewModel.SetColor(ReviewTargets(), color); }
    private void OnContextFlag(object sender, RoutedEventArgs e) { if (sender is FrameworkElement { Tag: string value } && Enum.TryParse<ReviewFlag>(value, out var flag)) _viewModel.SetFlag(ReviewTargets(), flag); }
    private void OnContextLoupe(object sender, RoutedEventArgs e) => _viewModel.SelectAndLoupe(_viewModel.CurrentPhoto);
    private void OnContextReveal(object sender, RoutedEventArgs e) { if (_viewModel.RevealCommand.CanExecute(null)) _viewModel.RevealCommand.Execute(null); }
    private int GridColumnCount() => FindPhotoPanel(GridPhotos)?.Columns ?? Math.Max(1, (int)(GridPhotos.ActualWidth / _viewModel.TileWidth));
    private static VirtualizingPhotoPanel? FindPhotoPanel(DependencyObject root)
    {
        if (root is VirtualizingPhotoPanel panel) return panel;
        for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); i++)
            if (FindPhotoPanel(System.Windows.Media.VisualTreeHelper.GetChild(root, i)) is { } found) return found;
        return null;
    }
    private void OnOpenFilter(object sender, RoutedEventArgs e)
        => ShowFilterScreen();
    private void OnSendFilteredToFilter(object sender, RoutedEventArgs e)
        => SendToFilter(_viewModel.FilteredPhotos);
    private void SendToFilter(IEnumerable<ReviewPhoto> photos)
    {
        if (_viewModel.CreateTransferList(photos) is { } path) ShowFilterScreen(path);
    }
    private void ShowFilterScreen(string? textListPath = null)
    {
        if (_filterView == null)
        {
            _filterView = new TxtFilterView();
            _filterView.ConfigureForReview();
            _filterView.ReviewRequested += (_, _) => ShowReviewScreen();
            FilterHost.Content = _filterView;
        }
        if (_filterView.DataContext is ViewModels.MainViewModel filter && string.IsNullOrWhiteSpace(filter.SourceFolder) && Directory.Exists(_viewModel.Folder))
            filter.SourceFolder = _viewModel.Folder;
        if (_filterView.DataContext is ViewModels.MainViewModel targetFilter && !string.IsNullOrWhiteSpace(textListPath))
        {
            targetFilter.TxtPath = textListPath;
            if (targetFilter.SelectRawCommand.CanExecute(null)) targetFilter.SelectRawCommand.Execute(null);
        }
        if (_filterView.DataContext is ViewModels.MainViewModel filterTheme) filterTheme.DarkMode = true;
        ReviewScreen.Visibility = Visibility.Collapsed;
        FilterHost.Visibility = Visibility.Visible;
        Title = AppInfo.FilterTitle;
        _filterView.Focusable = true;
        _filterView.Focus();
    }
    private void ShowReviewScreen()
    {
        FilterHost.Visibility = Visibility.Collapsed;
        ReviewScreen.Visibility = Visibility.Visible;
        Title = AppInfo.ReviewTitle;
        Focus();
    }
    private void OnClose(object sender, RoutedEventArgs e) => Close();
    private void OnToggleZen(object sender, RoutedEventArgs e) => ToggleZenMode();
    private void ToggleZenMode()
    {
        if (!_zenMode)
        {
            if (NavigatorColumn.ActualWidth > 0) _navigatorWidth = NavigatorColumn.Width;
            if (RatingColumn.ActualWidth > 0) _ratingWidth = RatingColumn.Width;
            NavigatorColumn.Width = new(0); LeftSplitterColumn.Width = new(0);
            RightSplitterColumn.Width = new(0); RatingColumn.Width = new(0);
            _zenMode = true; ZenButton.Content = "Show panels";
        }
        else
        {
            NavigatorColumn.Width = _navigatorWidth; LeftSplitterColumn.Width = new(5);
            RightSplitterColumn.Width = new(5); RatingColumn.Width = _ratingWidth;
            _zenMode = false; ZenButton.Content = "Hide panels";
        }
    }
    private void OnShowHelp(object sender, RoutedEventArgs e) => ShowHelpPopup();
    private void OnCloseHelp(object sender, RoutedEventArgs e) => HideHelpPopup();
    public void ShowHelpPopup() { SettingsOverlay.Visibility = Visibility.Collapsed; HelpOverlay.Visibility = Visibility.Visible; HelpOverlay.Focus(); }
    private void HideHelpPopup() { HelpOverlay.Visibility = Visibility.Collapsed; Focus(); }
    private void OnShowSettings(object sender, RoutedEventArgs e) { HelpOverlay.Visibility = Visibility.Collapsed; SettingsOverlay.Visibility = Visibility.Visible; SettingsOverlay.Focus(); }
    private void OnCloseSettings(object sender, RoutedEventArgs e) => HideSettingsPopup();
    private void OnLanguageChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_languageReady || LanguageCombo.SelectedValue is not string language || language == LanguageService.CurrentLanguage) return;
        var vietnamese = language == LanguageService.Vietnamese;
        var message = vietnamese
            ? "Chuyển giao diện sang Tiếng Việt?\n\nỨng dụng sẽ lưu phiên hiện tại và tự khởi động lại để áp dụng ngôn ngữ."
            : "Switch the interface to English?\n\nThe app will save the current session and restart to apply the language.";
        var title = vietnamese ? "Xác nhận đổi ngôn ngữ" : "Confirm Language Change";
        if (MessageBox.Show(this, message, title, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK)
        {
            _viewModel.SaveSessionNow();
            _filterView?.SaveSettings();
            LanguageService.Set(language); LanguageService.Restart();
        }
        else
        {
            _languageReady = false; LanguageCombo.SelectedValue = LanguageService.CurrentLanguage; _languageReady = true;
        }
    }
    private void OnResetSettings(object sender, RoutedEventArgs e) => _viewModel.ResetPreferences();
    private void HideSettingsPopup() { SettingsOverlay.Visibility = Visibility.Collapsed; Focus(); }
    private static bool MatchesShortcut(KeyEventArgs e, string shortcut)
    {
        var ctrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        var plain = Keyboard.Modifiers == ModifierKeys.None;
        return shortcut switch
        {
            "F1" => e.Key == Key.F1 && plain,
            "Ctrl+H" => e.Key == Key.H && ctrl,
            "Tab" => e.Key == Key.Tab && plain,
            "F" => e.Key == Key.F && plain,
            "Ctrl+Z" => e.Key == Key.Z && ctrl,
            "Ctrl+Backspace" => e.Key == Key.Back && ctrl,
            "` + Ctrl+0" => (e.Key == Key.Oem3 && plain) || (ctrl && e.Key is Key.D0 or Key.NumPad0),
            "`" => e.Key == Key.Oem3 && plain,
            "Ctrl+0" => ctrl && e.Key is Key.D0 or Key.NumPad0,
            "G" => e.Key == Key.G && plain,
            "Ctrl+G" => e.Key == Key.G && ctrl,
            "E" => e.Key == Key.E && plain,
            "Enter" => e.Key == Key.Enter && plain,
            _ => false
        };
    }
    private async void OnPhotoTileLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: ReviewPhoto photo }) await _viewModel.RequestThumbnailAsync(photo);
    }
    private void OnGridDoubleClick(object sender, MouseButtonEventArgs e) { if (GridPhotos.SelectedItem is ReviewPhoto photo) _viewModel.SelectAndLoupe(photo); }
    private void OnFilmstripSelectionChanged(object sender, SelectionChangedEventArgs e) { if (Filmstrip.SelectedItem != null) Filmstrip.ScrollIntoView(Filmstrip.SelectedItem); }
    private void OnReviewDragOver(object sender, DragEventArgs e)
    {
        if (FilterHost.Visibility == Visibility.Visible) { e.Handled = false; return; }
        e.Effects = e.Data.GetData(DataFormats.FileDrop) is string[] paths && paths.Any(path => Directory.Exists(path) || ReviewImportService.IsSupportedFile(path)) && !_viewModel.Busy ? DragDropEffects.Copy : DragDropEffects.None; e.Handled = true;
    }
    private async void OnReviewDrop(object sender, DragEventArgs e)
    {
        if (FilterHost.Visibility == Visibility.Visible) return;
        if (e.Data.GetData(DataFormats.FileDrop) is string[] paths && paths.Any(path => Directory.Exists(path) || ReviewImportService.IsSupportedFile(path))) await _viewModel.ImportPathsAsync(paths);
        e.Handled = true;
    }
    protected override void OnClosing(CancelEventArgs e)
    {
        var filterBusy = _filterView?.DataContext is ViewModels.MainViewModel { Busy: true };
        if (_viewModel.Busy || filterBusy)
        {
            e.Cancel = true;
            MessageBox.Show(this, "An operation is still running. Select Cancel and wait for it to stop before closing the app.", "QiQi Studio", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        base.OnClosing(e);
    }
}
