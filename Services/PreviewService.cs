using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoFileFilter.Services;

public record PreviewResult(BitmapSource? Image, string Description);

public sealed class PreviewService
{
    private static readonly HashSet<string> Raster = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".tif", ".tiff", ".heic" };
    public PreviewResult Load(string path, CancellationToken token, int maxEdge = 1400)
    {
        token.ThrowIfCancellationRequested();
        try
        {
            var direct = Decode(path, !Raster.Contains(Path.GetExtension(path)), token, maxEdge);
            if (direct != null) return new(direct, (LanguageService.IsVietnamese ? "Preview: " : "Preview: ") + Path.GetFileName(path));
        }
        catch (Exception e) when (IsImageError(e)) { }
        if (!Raster.Contains(Path.GetExtension(path)))
        {
            try
            {
                var stem = Path.GetFileNameWithoutExtension(path);
                foreach (var candidate in Directory.EnumerateFiles(Path.GetDirectoryName(path)!))
                {
                    token.ThrowIfCancellationRequested();
                    if (!string.Equals(Path.GetFileNameWithoutExtension(candidate), stem, StringComparison.OrdinalIgnoreCase) ||
                        !(Path.GetExtension(candidate).Equals(".jpg", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(candidate).Equals(".jpeg", StringComparison.OrdinalIgnoreCase))) continue;
                    try
                    {
                        var image = Decode(candidate, false, token, maxEdge);
                        if (image != null) return new(image, LanguageService.IsVietnamese ? $"Dùng preview JPG cùng tên: {Path.GetFileName(candidate)} — đây không phải RAW đã giải mã." : $"Using matching JPG preview: {Path.GetFileName(candidate)} — this is not a decoded RAW image.");
                    }
                    catch (Exception e) when (IsImageError(e)) { }
                }
            }
            catch (Exception e) when (IsImageError(e)) { }
        }
        return new(null, LanguageService.IsVietnamese ? "Không thể xem preview file này. Windows có thể chưa có codec phù hợp, hoặc file bị hỏng/không còn tồn tại. Bạn vẫn có thể mở vị trí file trong File Explorer." : "This file could not be previewed. Its format may not be supported by the installed Windows codec, or the file may be damaged or missing. You can still show it in File Explorer.");
    }

    private static BitmapSource? Decode(string path, bool thumbnailOnly, CancellationToken token, int maxEdge)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnDemand);
        var frame = decoder.Frames[0];
        var orientation = 1;
        if (!thumbnailOnly)
        {
            try
            {
                if (frame.Metadata is BitmapMetadata metadata)
                    orientation = Convert.ToInt32(metadata.GetQuery("/app1/ifd/{ushort=274}") ?? metadata.GetQuery("/ifd/{ushort=274}") ?? 1);
            }
            catch (Exception e) when (IsImageError(e)) { }
        }
        BitmapSource? image = null;
        if (!thumbnailOnly)
        {
            stream.Position = 0;
            var bitmap = new BitmapImage();
            bitmap.BeginInit(); bitmap.CacheOption = BitmapCacheOption.OnLoad;
            maxEdge = Math.Clamp(maxEdge, 96, 2400);
            if (frame.PixelWidth >= frame.PixelHeight) bitmap.DecodePixelWidth = Math.Min(maxEdge, frame.PixelWidth);
            else bitmap.DecodePixelHeight = Math.Min(maxEdge, frame.PixelHeight);
            bitmap.StreamSource = stream; bitmap.EndInit(); image = bitmap;
        }
        if (image == null)
        {
            try { image = frame.Thumbnail; } catch (Exception e) when (IsImageError(e)) { }
        }
        if (image == null) return null;
        token.ThrowIfCancellationRequested();
        // Materialize pixels before closing the decoder's stream. No source is held open by the UI.
        var copy = new WriteableBitmap(image);
        copy.Freeze();
        var matrix = orientation switch
        {
            2 => new Matrix(-1, 0, 0, 1, 0, 0), 3 => new Matrix(-1, 0, 0, -1, 0, 0),
            4 => new Matrix(1, 0, 0, -1, 0, 0), 5 => new Matrix(0, 1, 1, 0, 0, 0),
            6 => new Matrix(0, 1, -1, 0, 0, 0), 7 => new Matrix(0, -1, -1, 0, 0, 0),
            8 => new Matrix(0, -1, 1, 0, 0, 0), _ => Matrix.Identity
        };
        if (matrix.IsIdentity) return copy;
        var rotated = new TransformedBitmap(copy, new MatrixTransform(matrix)); rotated.Freeze(); return rotated;
    }
    private static bool IsImageError(Exception e) => e is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException or InvalidOperationException or System.Runtime.InteropServices.COMException;
}
