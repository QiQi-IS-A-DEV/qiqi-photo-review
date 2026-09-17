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
            var raw = !Raster.Contains(Path.GetExtension(path));
            // Grid thumbnails can use the embedded image; Loupe must try the full decoder.
            var embeddedOnly = raw && maxEdge is > 0 and <= 320;
            var direct = Decode(path, embeddedOnly, token, maxEdge);
            if (direct == null && embeddedOnly)
            {
                direct = Decode(path, false, token, maxEdge);
                embeddedOnly = false;
            }
            if (direct != null) return new(direct, Describe(direct, path, embeddedOnly
                ? LanguageService.Text("Embedded RAW preview") : raw ? LanguageService.Text("RAW via Windows codec") : "Preview"));
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
                        if (image != null) return new(image, Describe(image, candidate, LanguageService.IsVietnamese ? "Dùng preview JPG cùng tên" : "Using matching JPG preview") +
                            (LanguageService.IsVietnamese ? " · Không phải RAW đã giải mã." : " · Not a decoded RAW image."));
                    }
                    catch (Exception e) when (IsImageError(e)) { }
                }
            }
            catch (Exception e) when (IsImageError(e)) { }
            try
            {
                var embedded = Decode(path, true, token, maxEdge);
                if (embedded != null) return new(embedded, Describe(embedded, path, LanguageService.Text("Embedded RAW preview")) +
                    (LanguageService.IsVietnamese ? " · Chi tiết phụ thuộc preview nhúng; hãy dùng JPG cùng tên hoặc codec RAW phù hợp để soi nét." : " · Detail is limited by the embedded preview; use a matching JPG or a compatible RAW codec for close inspection."));
            }
            catch (Exception e) when (IsImageError(e)) { }
        }
        return new(null, LanguageService.IsVietnamese ? "Không thể xem preview file này. Windows có thể chưa có codec phù hợp, hoặc file bị hỏng/không còn tồn tại. Bạn vẫn có thể mở vị trí file trong File Explorer." : "This file could not be previewed. Its format may not be supported by the installed Windows codec, or the file may be damaged or missing. You can still show it in File Explorer.");
    }

    private static string Describe(BitmapSource image, string path, string source)
        => $"{source} · {image.PixelWidth:N0} × {image.PixelHeight:N0} px · {Path.GetFileName(path)}";

    // Return an independent, small bitmap so a catalog thumbnail cannot retain a full-resolution image.
    public static BitmapSource CreateThumbnail(BitmapSource source, int maxEdge = 240)
    {
        var scale = Math.Min(1d, Math.Max(1, maxEdge) / (double)Math.Max(source.PixelWidth, source.PixelHeight));
        BitmapSource resized = scale < 1 ? new TransformedBitmap(source, new ScaleTransform(scale, scale)) : source;
        var thumbnail = new WriteableBitmap(resized); thumbnail.Freeze(); return thumbnail;
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
            // Zero means original pixel dimensions. Never enlarge a source during decoding.
            if (maxEdge != 0)
            {
                maxEdge = Math.Clamp(maxEdge, 96, 8192);
                if (frame.PixelWidth >= frame.PixelHeight) bitmap.DecodePixelWidth = Math.Min(maxEdge, frame.PixelWidth);
                else bitmap.DecodePixelHeight = Math.Min(maxEdge, frame.PixelHeight);
            }
            bitmap.StreamSource = stream; bitmap.EndInit(); image = bitmap;
        }
        if (image == null)
        {
            try { image = decoder.Preview; } catch (Exception e) when (IsImageError(e)) { }
            try
            {
                var thumbnail = frame.Thumbnail;
                if (thumbnail != null && (image == null || (long)thumbnail.PixelWidth * thumbnail.PixelHeight > (long)image.PixelWidth * image.PixelHeight)) image = thumbnail;
            }
            catch (Exception e) when (IsImageError(e)) { }
        }
        if (image == null) return null;
        token.ThrowIfCancellationRequested();
        // Embedded previews depend on the open decoder stream and must be detached.
        // BitmapImage with OnLoad already owns its pixels; avoid a second full-size allocation.
        var copy = thumbnailOnly ? CreateThumbnail(image, maxEdge == 0 ? Math.Max(image.PixelWidth, image.PixelHeight) : maxEdge) : image;
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
