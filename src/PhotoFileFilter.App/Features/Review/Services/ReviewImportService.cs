using System.IO;
using PhotoFileFilter.Features.Review.Models;

namespace PhotoFileFilter.Features.Review.Services;

public sealed record ReviewImportResult(IReadOnlyList<ReviewPhoto> Photos, IReadOnlyList<string> Warnings, int Examined);

public sealed class ReviewImportService
{
    public static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    { ".jpg", ".jpeg", ".png", ".tif", ".tiff", ".heic", ".arw", ".cr2", ".cr3", ".nef", ".raf", ".orf", ".rw2", ".dng" };

    public ReviewImportResult Import(string root, bool recursive, IProgress<int>? progress, CancellationToken token)
    {
        if (!Directory.Exists(root)) throw new DirectoryNotFoundException("The import folder does not exist.");
        root = Path.GetFullPath(root);
        var photos = new List<ReviewPhoto>();
        var warnings = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var examined = ImportFolder(root, root, recursive, photos, warnings, seen, 0, progress, token);
        photos.Sort((a, b) => NaturalNameComparer.Instance.Compare(a.RelativePath, b.RelativePath));
        return new(photos, warnings, examined);
    }

    public ReviewImportResult ImportPaths(IEnumerable<string> paths, bool recursive, IProgress<int>? progress, CancellationToken token)
    {
        var inputs = paths.Where(path => !string.IsNullOrWhiteSpace(path)).Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (inputs.Length == 0) throw new ArgumentException("Drag photos or a photo folder into the window.");
        var photos = new List<ReviewPhoto>();
        var warnings = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var examined = 0;
        foreach (var input in inputs)
        {
            token.ThrowIfCancellationRequested();
            if (Directory.Exists(input))
            {
                examined = ImportFolder(input, input, recursive, photos, warnings, seen, examined, progress, token);
            }
            else if (IsSupportedFile(input))
            {
                examined++;
                AddPhoto(input, Path.GetDirectoryName(input)!, photos, warnings, seen);
                progress?.Report(examined);
            }
        }
        photos.Sort((a, b) => NaturalNameComparer.Instance.Compare(a.FullPath, b.FullPath));
        return new(photos, warnings, examined);
    }

    public static bool IsSupportedFile(string path) => File.Exists(path) && SupportedExtensions.Contains(Path.GetExtension(path));

    private static int ImportFolder(string root, string initialFolder, bool recursive, List<ReviewPhoto> photos, List<string> warnings, HashSet<string> seen, int examined, IProgress<int>? progress, CancellationToken token)
    {
        var folders = new Stack<string>();
        folders.Push(initialFolder);
        while (folders.TryPop(out var currentFolder))
        {
            token.ThrowIfCancellationRequested();
            try
            {
                foreach (var path in Directory.EnumerateFileSystemEntries(currentFolder))
                {
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        var attributes = File.GetAttributes(path);
                        if ((attributes & FileAttributes.ReparsePoint) != 0) continue;
                        if ((attributes & FileAttributes.Directory) != 0)
                        {
                            if (recursive) folders.Push(path);
                            continue;
                        }
                        examined++;
                        if (examined % 250 == 0) progress?.Report(examined);
                        if (!SupportedExtensions.Contains(Path.GetExtension(path))) continue;
                        AddPhoto(path, root, photos, warnings, seen);
                    }
                    catch (Exception e) when (e is IOException or UnauthorizedAccessException) { warnings.Add($"{path}: {e.Message}"); }
                }
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException) { warnings.Add($"{currentFolder}: {e.Message}"); }
        }
        return examined;
    }

    private static void AddPhoto(string path, string root, List<ReviewPhoto> photos, List<string> warnings, HashSet<string> seen)
    {
        try
        {
            path = Path.GetFullPath(path);
            if (!seen.Add(path)) return;
            var info = new FileInfo(path);
            photos.Add(new(path, root, info.Length, info.LastWriteTimeUtc));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException) { warnings.Add($"{path}: {e.Message}"); }
    }
}

internal sealed class NaturalNameComparer : IComparer<string>
{
    public static NaturalNameComparer Instance { get; } = new();
    public int Compare(string? x, string? y) => StrCmpLogicalW(x ?? "", y ?? "");
    [System.Runtime.InteropServices.DllImport("shlwapi.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern int StrCmpLogicalW(string x, string y);
}
