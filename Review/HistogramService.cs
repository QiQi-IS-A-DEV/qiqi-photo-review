using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoFileFilter.Review;

public sealed record HistogramResult(BitmapSource Chart, double HighlightPercent, double ShadowPercent, double AverageLuminance)
{
    public string Summary => $"Highlights {HighlightPercent:0.0}%  ·  Shadows {ShadowPercent:0.0}%  ·  Average {AverageLuminance * 100:0}%";
    public string Assessment => HighlightPercent >= 2 ? "Check highlights for possible clipping" : ShadowPercent >= 8 ? "Check shadows for possible clipping" : AverageLuminance < 0.22 ? "The photo is relatively dark" : AverageLuminance > 0.78 ? "The photo is relatively bright" : "Tonal distribution looks balanced";
}

public sealed class HistogramService
{
    public HistogramResult Calculate(BitmapSource source, CancellationToken token)
    {
        BitmapSource bitmap = source.Format == PixelFormats.Bgra32 ? source : new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        var width = bitmap.PixelWidth; var height = bitmap.PixelHeight; var stride = width * 4;
        var pixels = new byte[stride * height]; bitmap.CopyPixels(pixels, stride, 0);
        var red = new long[256]; var green = new long[256]; var blue = new long[256];
        var step = Math.Max(1, (int)Math.Sqrt((width * (double)height) / 250000d));
        long count = 0, highlights = 0, shadows = 0; double luminanceTotal = 0;
        for (var y = 0; y < height; y += step)
        {
            token.ThrowIfCancellationRequested();
            for (var x = 0; x < width; x += step)
            {
                var index = y * stride + x * 4; var b = pixels[index]; var g = pixels[index + 1]; var r = pixels[index + 2];
                blue[b]++; green[g]++; red[r]++; count++;
                var luminance = 0.2126 * r + 0.7152 * g + 0.0722 * b;
                luminanceTotal += luminance;
                if (r >= 250 || g >= 250 || b >= 250) highlights++;
                if (luminance <= 5) shadows++;
            }
        }
        var chart = Draw(red, green, blue, token);
        return new(chart, count == 0 ? 0 : highlights * 100d / count, count == 0 ? 0 : shadows * 100d / count, count == 0 ? 0 : luminanceTotal / count / 255d);
    }

    private static BitmapSource Draw(long[] red, long[] green, long[] blue, CancellationToken token)
    {
        const int width = 232, height = 92, stride = width * 4;
        var output = new byte[stride * height];
        for (var i = 0; i < output.Length; i += 4) { output[i] = 27; output[i + 1] = 27; output[i + 2] = 27; output[i + 3] = 255; }
        var maxLog = Math.Max(red.Max(value => Math.Log(1 + value)), Math.Max(green.Max(value => Math.Log(1 + value)), blue.Max(value => Math.Log(1 + value))));
        for (var x = 0; x < width; x++)
        {
            token.ThrowIfCancellationRequested();
            var bin = Math.Clamp((int)Math.Round(x * 255d / (width - 1)), 0, 255);
            var rh = maxLog == 0 ? 0 : (int)Math.Round(Math.Log(1 + red[bin]) / maxLog * (height - 3));
            var gh = maxLog == 0 ? 0 : (int)Math.Round(Math.Log(1 + green[bin]) / maxLog * (height - 3));
            var bh = maxLog == 0 ? 0 : (int)Math.Round(Math.Log(1 + blue[bin]) / maxLog * (height - 3));
            for (var y = 0; y < height; y++)
            {
                var fromBottom = height - 1 - y; var index = y * stride + x * 4;
                if (fromBottom < bh) output[index] = 220;
                if (fromBottom < gh) output[index + 1] = 205;
                if (fromBottom < rh) output[index + 2] = 225;
            }
        }
        var chart = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, output, stride); chart.Freeze(); return chart;
    }
}
