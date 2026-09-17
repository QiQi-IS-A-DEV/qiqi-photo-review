using System.IO;
using System.Text.Json;

namespace PhotoFileFilter.Review;

public sealed record ReviewPreferences(
    int PreviewMaxEdge = 2400,
    int ZoomStepPercent = 25,
    int OverlayDurationMs = 950,
    int OverlayPosition = 0,
    int DefaultView = 0,
    bool StartWithPanelsHidden = false,
    bool HistogramEnabled = true,
    string HelpShortcut = "F1",
    string ZenShortcut = "Tab",
    string UndoShortcut = "Ctrl+Z",
    string ResetZoomShortcut = "` + Ctrl+0",
    string GridShortcut = "G",
    string LoupeShortcut = "E",
    int ClickZoomPercent = 200,
    bool FullResolutionOnZoom = true);

public sealed class ReviewPreferencesService(string? filePath = null)
{
    public string FilePath { get; } = filePath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QiQiStudio", "PhotoFileFilter", "review-preferences.json");

    public ReviewPreferences Load()
    {
        try { return File.Exists(FilePath) ? JsonSerializer.Deserialize<ReviewPreferences>(File.ReadAllText(FilePath)) ?? new() : new(); }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException) { return new(); }
    }

    public bool Save(ReviewPreferences preferences)
    {
        var temporary = FilePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(FilePath))!);
            File.WriteAllText(temporary, JsonSerializer.Serialize(preferences, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temporary, FilePath, true); return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException) { return false; }
        finally { try { if (File.Exists(temporary)) File.Delete(temporary); } catch (Exception e) when (e is IOException or UnauthorizedAccessException) { } }
    }
}
