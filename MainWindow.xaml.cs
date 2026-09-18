using System.ComponentModel;
using System.Windows;
using PhotoFileFilter.ViewModels;

namespace PhotoFileFilter;

// Optional standalone host. Workspace UI and input bindings belong to TxtFilterView.
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = FilterView.DataContext;
        DataContextChanged += (_, _) => FilterView.DataContext = DataContext;
        MinWidth = Math.Min(MinWidth, SystemParameters.WorkArea.Width);
        MinHeight = Math.Min(MinHeight, SystemParameters.WorkArea.Height);
        Width = Math.Min(Width, SystemParameters.WorkArea.Width);
        Height = Math.Min(Height, SystemParameters.WorkArea.Height);
    }
    protected override void OnClosing(CancelEventArgs e)
    {
        if (DataContext is MainViewModel { Busy: true })
        {
            e.Cancel = true;
            MessageBox.Show(this, "An operation is still running. Select Cancel and wait for it to stop before closing the app.", "Photo File Filter", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else FilterView.SaveSettings();
        base.OnClosing(e);
    }
}
