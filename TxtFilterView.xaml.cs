using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using PhotoFileFilter.Services;
using PhotoFileFilter.ViewModels;

namespace PhotoFileFilter;

public partial class TxtFilterView : UserControl
{
    private DropHighlightAdorner? _dropHighlight;
    public event EventHandler? ReviewRequested;
    public TxtFilterView()
    {
        InitializeComponent(); DataContext = new MainViewModel(new DialogService(), new SettingsService());
        Loaded += (_, _) => LanguageService.Apply(this);
        Unloaded += (_, _) => ClearDropHighlight();
    }
    private void OnFileDragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.None;
        if (DataContext is MainViewModel { Busy: false } && e.Data.GetData(DataFormats.FileDrop) is string[] paths && sender is FrameworkElement { Tag: string target })
            if (paths.Any(p => target == "txt" ? System.IO.File.Exists(p) && string.Equals(System.IO.Path.GetExtension(p), ".txt", StringComparison.OrdinalIgnoreCase) : System.IO.Directory.Exists(p)))
                e.Effects = DragDropEffects.Copy;
        ClearDropHighlight();
        if (e.Effects == DragDropEffects.Copy && sender is FrameworkElement element && AdornerLayer.GetAdornerLayer(element) is { } layer)
        {
            _dropHighlight = new DropHighlightAdorner(element);
            layer.Add(_dropHighlight);
        }
        e.Handled = true;
    }
    private void OnFileDrop(object sender, DragEventArgs e)
    {
        ClearDropHighlight();
        if (DataContext is MainViewModel vm && e.Data.GetData(DataFormats.FileDrop) is string[] paths && sender is FrameworkElement { Tag: string target })
            vm.ApplyDroppedPaths(paths, target);
        e.Handled = true;
    }
    private void OnFileDragLeave(object sender, DragEventArgs e) => ClearDropHighlight();
    private void ClearDropHighlight()
    {
        if (_dropHighlight != null) AdornerLayer.GetAdornerLayer(_dropHighlight.AdornedElement)?.Remove(_dropHighlight);
        _dropHighlight = null;
    }
    private void OnResultsRightClick(object sender, MouseButtonEventArgs e)
    {
        if (ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) is DataGridRow row) row.IsSelected = true;
        else if (DataContext is MainViewModel vm) vm.SelectedFile = null;
    }
    private void OnShowOutput(object sender, RoutedEventArgs e) => OutputSettings.BringIntoView();
    private void OnHelp(object sender, RoutedEventArgs e) => ShowHelp();
    private void OnCloseFilterHelp(object sender, RoutedEventArgs e) => FilterHelpOverlay.Visibility = Visibility.Collapsed;
    private void OnMainKeyDown(object sender, KeyEventArgs e)
    {
        if (FilterHelpOverlay.Visibility == Visibility.Visible && (e.Key == Key.F1 || e.Key == Key.Escape))
        {
            FilterHelpOverlay.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }
        else if (e.Key == Key.F1)
        {
            ShowHelp();
            e.Handled = true;
        }
        else if (e.Key == Key.D1 && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            OnOpenReview(this, new RoutedEventArgs());
            e.Handled = true;
        }
    }
    private void ShowHelp()
    {
        FilterHelpOverlay.Visibility = FilterHelpOverlay.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }
    public void ShowHelpPopup() => FilterHelpOverlay.Visibility = Visibility.Visible;
    public void HideHelpPopup() => FilterHelpOverlay.Visibility = Visibility.Collapsed;
    private void OnClearFilterSession(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel { Busy: false } vm) return;
        var owner = Window.GetWindow(ReviewNavigationButton) ?? Application.Current.MainWindow;
        var confirmed = MessageBox.Show(owner,
            LanguageService.IsVietnamese ? "Xóa phiên Lọc TXT hiện tại?\n\nDanh sách TXT, đường dẫn nguồn/đích và kết quả quét sẽ được xóa khỏi app. Ảnh gốc và file TXT không bị thay đổi." : "Clear the current TXT Filter session?\n\nThe TXT list, source and destination paths, and scan results will be removed from the app. Original photos and TXT files will not be changed.",
            LanguageService.IsVietnamese ? "Xóa phiên Lọc TXT" : "Clear TXT Filter Session", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
        if (confirmed == MessageBoxResult.OK) vm.ClearSession();
    }
    private void OnOpenReview(object sender, RoutedEventArgs e)
    {
        if (ReviewRequested != null)
        {
            ReviewRequested(this, EventArgs.Empty);
            return;
        }
        var source = (DataContext as MainViewModel)?.SourceFolder;
        new ReviewWindow(source) { Owner = Window.GetWindow(this) }.ShowDialog();
    }
    public void ConfigureForReview()
    {
        ReviewNavigationButton.Content = "←  Back to Review  ·  Ctrl+1";
        ReviewNavigationButton.ToolTip = "Return to the Import & Review workspace";
        ThemeToggle.Visibility = Visibility.Collapsed;
    }
    public void SaveSettings() => (DataContext as MainViewModel)?.SaveSettings();
    private void OnResultsDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) is DataGridRow && DataContext is MainViewModel vm && vm.PreviewCommand.CanExecute(null)) vm.PreviewCommand.Execute(null);
    }
    private void OnResultsKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space && DataContext is MainViewModel vm && vm.PreviewCommand.CanExecute(null)) { e.Handled = true; vm.PreviewCommand.Execute(null); }
    }
}
