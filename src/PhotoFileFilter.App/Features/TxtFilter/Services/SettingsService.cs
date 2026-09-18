using System.IO;
using System.Text.Json;

namespace PhotoFileFilter.Features.TxtFilter.Services;

public sealed record UserSettings
{
    public string TxtPath { get; init; } = "";
    public string SourceFolder { get; init; } = "";
    public string OutputFolder { get; init; } = "";
    public string SubfolderName { get; init; } = "Selected";
    public bool Comma { get; init; } = true;
    public bool Space { get; init; } = true;
    public bool NewLine { get; init; } = true;
    public bool IgnoreExtension { get; init; } = true;
    public bool Recursive { get; init; } = true;
    public bool Alongside { get; init; } = true;
    public bool Subfolder { get; init; } = true;
    public int Policy { get; init; }
    public bool DarkMode { get; init; } = true;
    public bool CompletionSound { get; init; } = true;
    public string[] Extensions { get; init; } = ["JPG", "JPEG", "ARW", "CR2", "CR3"];
}

public sealed class SettingsService(string? filePath = null)
{
    public string FilePath { get; } = filePath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QiQiStudio", "PhotoFileFilter", "settings.json");

    public UserSettings? Load()
    {
        try { return File.Exists(FilePath) ? JsonSerializer.Deserialize<UserSettings>(File.ReadAllText(FilePath)) : null; }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException) { return null; }
    }

    public bool Save(UserSettings settings)
    {
        var temporary = FilePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(FilePath))!);
            File.WriteAllText(temporary, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temporary, FilePath, true);
            return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException) { return false; }
        finally
        {
            try { if (File.Exists(temporary)) File.Delete(temporary); }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException) { }
        }
    }
}
