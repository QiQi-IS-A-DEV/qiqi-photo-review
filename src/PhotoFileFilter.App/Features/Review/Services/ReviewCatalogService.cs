using System.Text.Json;
using System.IO;
using PhotoFileFilter.Features.Review.Models;

namespace PhotoFileFilter.Features.Review.Services;

public sealed record ReviewMark(int Rating, ReviewFlag Flag, long Size, DateTime LastWriteUtc, ReviewColor ColorLabel = ReviewColor.None, int Rotation = 0);

public sealed class ReviewCatalogService(string? filePath = null)
{
    private readonly object _gate = new();
    private Dictionary<string, ReviewMark>? _marks;
    public string FilePath { get; } = filePath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QiQiStudio", "PhotoFileFilter", "review-ratings.json");

    public ReviewMark? Get(string path)
    {
        lock (_gate)
        {
            EnsureLoaded();
            return _marks!.TryGetValue(Path.GetFullPath(path), out var mark) ? mark : null;
        }
    }

    public void Set(ReviewPhoto photo)
        => SetMany([photo]);

    public void SetMany(IEnumerable<ReviewPhoto> photos)
    {
        lock (_gate)
        {
            EnsureLoaded();
            foreach (var photo in photos)
            {
                var key = Path.GetFullPath(photo.FullPath);
                if (photo.Rating == 0 && photo.Flag == ReviewFlag.None && photo.ColorLabel == ReviewColor.None && photo.Rotation == 0) _marks!.Remove(key);
                else _marks![key] = new(photo.Rating, photo.Flag, photo.Size, photo.LastWriteUtc, photo.ColorLabel, photo.Rotation);
            }
            Save();
        }
    }

    private void EnsureLoaded()
    {
        if (_marks != null) return;
        try
        {
            _marks = File.Exists(FilePath)
                ? JsonSerializer.Deserialize<Dictionary<string, ReviewMark>>(File.ReadAllText(FilePath)) ?? new(StringComparer.OrdinalIgnoreCase)
                : new(StringComparer.OrdinalIgnoreCase);
            _marks = new(_marks, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException) { _marks = new(StringComparer.OrdinalIgnoreCase); }
    }

    private void Save()
    {
        var temporary = FilePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(temporary, JsonSerializer.Serialize(_marks, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temporary, FilePath, true);
        }
        finally
        {
            try { if (File.Exists(temporary)) File.Delete(temporary); }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException) { }
        }
    }
}
