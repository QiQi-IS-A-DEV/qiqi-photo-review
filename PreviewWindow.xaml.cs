using System.Windows;
using System.Windows.Input;
using PhotoFileFilter.Core;
using PhotoFileFilter.Services;

namespace PhotoFileFilter;

public partial class PreviewWindow : Window
{
    private readonly PhotoFile _file;
    private readonly CancellationTokenSource _cancellation = new();
    public PreviewWindow(PhotoFile file)
    {
        InitializeComponent(); _file = file; FileTitle.Text = file.Name;
        Width = Math.Min(Width, SystemParameters.WorkArea.Width);
        Height = Math.Min(Height, SystemParameters.WorkArea.Height);
        Loaded += async (_, _) =>
        {
            var token = _cancellation.Token;
            try
            {
                var result = await Task.Run(() => new PreviewService().Load(file.FullPath, token), token);
                if (token.IsCancellationRequested) return;
                PreviewImage.Source = result.Image; PreviewDescription.Text = result.Description;
            }
            catch (OperationCanceledException) { }
            catch (Exception e) { if (!token.IsCancellationRequested) PreviewDescription.Text = "Could not load the photo: " + e.Message; }
        };
        Closed += (_, _) => { _cancellation.Cancel(); _cancellation.Dispose(); };
    }
    private void OnPreviewKeyDown(object sender, KeyEventArgs e) { if (e.Key is Key.Escape or Key.Space) { e.Handled = true; Close(); } }
    private void OnReveal(object sender, RoutedEventArgs e)
    {
        try { new DialogService().RevealFile(_file.FullPath); }
        catch (Exception error) { PreviewDescription.Text = error.Message; }
    }
}
