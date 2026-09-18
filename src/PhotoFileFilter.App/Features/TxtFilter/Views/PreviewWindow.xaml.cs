using System.Windows;
using System.Windows.Input;
using PhotoFileFilter.Core;
using PhotoFileFilter.Shared.Services;

namespace PhotoFileFilter.Features.TxtFilter.Views;

public partial class PreviewWindow : Window
{
    private PhotoFile[] _files = [];
    private int _index;
    private bool _closed;
    private CancellationTokenSource? _cancellation;
    private readonly PreviewService _preview = new();
    private readonly SemaphoreSlim _decodeGate = new(1, 1);
    private Point? _panStart;
    private Point _panOrigin;
    public PhotoFile CurrentFile => _files[_index];
    public double Zoom => ImageScale.ScaleX;
    public double Rotation => ImageRotation.Angle;

    public PreviewWindow(PhotoFile file, IReadOnlyList<PhotoFile>? files = null)
    {
        InitializeComponent();
        Width = Math.Min(Width, SystemParameters.WorkArea.Width);
        Height = Math.Min(Height, SystemParameters.WorkArea.Height);
        SetFiles(file, files);
        Loaded += async (_, _) => await LoadAsync();
        Closed += (_, _) => { _closed = true; _cancellation?.Cancel(); _cancellation?.Dispose(); _preview.ClearCache(); };
    }
    public void SetFiles(PhotoFile file, IReadOnlyList<PhotoFile>? files)
    {
        _files = (files ?? [file]).DistinctBy(item => item.FullPath, StringComparer.OrdinalIgnoreCase).ToArray();
        _index = Array.FindIndex(_files, item => string.Equals(item.FullPath, file.FullPath, StringComparison.OrdinalIgnoreCase));
        if (_index < 0) { _files = [file]; _index = 0; }
        ResetView(); UpdateLabels();
        if (IsLoaded) _ = LoadAsync();
    }
    public void Navigate(int direction)
    {
        if (_files.Length < 2 || _closed) return;
        _index = (_index + Math.Sign(direction) + _files.Length) % _files.Length;
        ResetView(); UpdateLabels(); _ = LoadAsync();
    }
    private void UpdateLabels()
    {
        FileTitle.Text = CurrentFile.Name;
        PositionText.Text = $"{_index + 1} / {_files.Length}";
        PreviousButton.IsEnabled = NextButton.IsEnabled = _files.Length > 1;
        ZoomText.Text = Zoom == 1 ? "Fit" : $"{Zoom * 100:0}% Fit";
    }
    private async Task LoadAsync()
    {
        if (_closed) return;
        _cancellation?.Cancel(); _cancellation?.Dispose(); _cancellation = new();
        var token = _cancellation.Token;
        var file = CurrentFile;
        var edge = Zoom > 1 ? 0 : 2400;
        PreviewImage.Source = null;
        PreviewDescription.Text = LanguageService.Text("Loading preview…");
        try
        {
            var result = await Task.Run(async () =>
            {
                await _decodeGate.WaitAsync(token);
                try { return _preview.Load(file.FullPath, token, edge); }
                finally { _decodeGate.Release(); }
            }, token);
            if (token.IsCancellationRequested || _closed || CurrentFile != file) return;
            PreviewImage.Source = result.Image; PreviewDescription.Text = result.Description;
        }
        catch (OperationCanceledException) { }
        catch (Exception error) { if (!token.IsCancellationRequested && !_closed) PreviewDescription.Text = error.Message; }
    }
    public void ZoomBy(int direction, double anchorX = 0, double anchorY = 0)
    {
        var previous = Zoom;
        var next = Math.Clamp(direction > 0 ? previous * 1.25 : previous / 1.25, 1, 8);
        ImageScale.ScaleX = ImageScale.ScaleY = next;
        ImagePan.X = next == 1 ? 0 : anchorX - (anchorX - ImagePan.X) * next / previous;
        ImagePan.Y = next == 1 ? 0 : anchorY - (anchorY - ImagePan.Y) * next / previous;
        UpdateLabels();
        if ((previous == 1) != (next == 1)) _ = LoadAsync();
    }
    public void RotatePreview() { ImageRotation.Angle = (ImageRotation.Angle + 90) % 360; ImagePan.X = ImagePan.Y = 0; }
    private void ResetView() { ImageScale.ScaleX = ImageScale.ScaleY = 1; ImagePan.X = ImagePan.Y = ImageRotation.Angle = 0; }
    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape: Close(); break;
            case Key.Left: Navigate(-1); break;
            case Key.Right: Navigate(1); break;
            case Key.R: RotatePreview(); break;
            case Key.Add: case Key.OemPlus: ZoomBy(1); break;
            case Key.Subtract: case Key.OemMinus: ZoomBy(-1); break;
            case Key.D0 when Keyboard.Modifiers.HasFlag(ModifierKeys.Control): OnFit(sender, e); break;
            default: return;
        }
        e.Handled = true;
    }
    private void OnWheel(object sender, MouseWheelEventArgs e)
    {
        var point = e.GetPosition(ImageViewport);
        ZoomBy(e.Delta > 0 ? 1 : -1, point.X - ImageViewport.ActualWidth / 2, point.Y - ImageViewport.ActualHeight / 2); e.Handled = true;
    }
    private void OnPanDown(object sender, MouseButtonEventArgs e)
    {
        if (Zoom <= 1) return;
        _panStart = e.GetPosition(ImageViewport); _panOrigin = new(ImagePan.X, ImagePan.Y);
        ImageViewport.CaptureMouse(); ImageViewport.Cursor = Cursors.Hand; e.Handled = true;
    }
    private void OnPanMove(object sender, MouseEventArgs e)
    {
        if (_panStart is not { } start || e.LeftButton != MouseButtonState.Pressed) return;
        var point = e.GetPosition(ImageViewport);
        ImagePan.X = _panOrigin.X + point.X - start.X; ImagePan.Y = _panOrigin.Y + point.Y - start.Y;
    }
    private void OnPanUp(object sender, MouseButtonEventArgs e) => ImageViewport.ReleaseMouseCapture();
    private void OnLostCapture(object sender, MouseEventArgs e) { _panStart = null; ImageViewport.Cursor = Cursors.Arrow; }
    private void OnPrevious(object sender, RoutedEventArgs e) => Navigate(-1);
    private void OnNext(object sender, RoutedEventArgs e) => Navigate(1);
    private void OnZoomIn(object sender, RoutedEventArgs e) => ZoomBy(1);
    private void OnZoomOut(object sender, RoutedEventArgs e) => ZoomBy(-1);
    private void OnFit(object sender, RoutedEventArgs e) { ResetView(); UpdateLabels(); _ = LoadAsync(); }
    private void OnRotate(object sender, RoutedEventArgs e) => RotatePreview();
    private void OnReveal(object sender, RoutedEventArgs e)
    {
        try { new DialogService().RevealFile(CurrentFile.FullPath); }
        catch (Exception error) { PreviewDescription.Text = error.Message; }
    }
}
