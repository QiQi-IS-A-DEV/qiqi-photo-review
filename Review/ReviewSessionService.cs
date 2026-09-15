using System.IO;
using System.Text.Json;

namespace PhotoFileFilter.Review;

public sealed record ReviewSession(string[] Sources, bool Recursive, int FilterIndex, int ViewMode, int ExportPolicy, string? CurrentPath);

public sealed class ReviewSessionService(string? filePath = null)
{
    public string FilePath { get; } = filePath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QiQiStudio", "PhotoFileFilter", "review-session.json");

    public ReviewSession? Load()
    {
        try { return File.Exists(FilePath) ? JsonSerializer.Deserialize<ReviewSession>(File.ReadAllText(FilePath)) : null; }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException) { return null; }
    }

    public bool Save(ReviewSession session)
    {
        var temporary = FilePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(FilePath))!);
            File.WriteAllText(temporary, JsonSerializer.Serialize(session, new JsonSerializerOptions { WriteIndented = true }));
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

    public bool Delete()
    {
        try
        {
            if (File.Exists(FilePath)) File.Delete(FilePath);
            var transfer = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(FilePath))!, "review-selection.txt");
            if (File.Exists(transfer)) File.Delete(transfer);
            return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException) { return false; }
    }

    public string TransferFilePath => Path.Combine(Path.GetDirectoryName(Path.GetFullPath(FilePath))!, "review-selection.txt");
}
