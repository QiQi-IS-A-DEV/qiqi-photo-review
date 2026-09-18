namespace PhotoFileFilter.Core;

public sealed class PhotoScannerService
{
    public ScanResult Scan(ScanOptions options, IProgress<OperationProgress>? progress, CancellationToken token)
    {
        if (!Directory.Exists(options.SourceFolder)) throw new DirectoryNotFoundException("The source folder does not exist.");
        if (options.Names.Count == 0 || options.Extensions.Count == 0) throw new ArgumentException("A filename list and at least one file type are required.");
        var source = PathSafety.Normalize(options.SourceFolder);
        PathSafety.RejectLinkedAncestors(source);
        var extensions = options.Extensions.Select(e => "." + e.TrimStart('.')).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var requested = options.Names.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var files = new List<PhotoFile>();
        var warnings = new List<string>();
        var pending = new Stack<string>();
        pending.Push(source);
        var examined = 0;
        while (pending.TryPop(out var folder))
        {
            token.ThrowIfCancellationRequested();
            try
            {
                foreach (var path in Directory.EnumerateFileSystemEntries(folder))
                {
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        var attributes = File.GetAttributes(path);
                        if ((attributes & FileAttributes.ReparsePoint) != 0) { warnings.Add($"Skipped link: {path}"); continue; }
                        if ((attributes & FileAttributes.Directory) != 0)
                        {
                            if (options.Recursive && (options.ExcludedFolder == null || !PathSafety.IsWithin(path, options.ExcludedFolder))) pending.Push(path);
                            continue;
                        }
                        examined++;
                        if (examined % 200 == 0) progress?.Report(new(examined, 0, path));
                        if (!extensions.Contains(Path.GetExtension(path))) continue;
                        var key = options.IgnoreExtension ? Path.GetFileNameWithoutExtension(path) : Path.GetFileName(path);
                        // Exact mode still allows bare stems, but explicit extensions must match exactly.
                        var bare = Path.GetFileNameWithoutExtension(path);
                        var matchesBare = !options.IgnoreExtension && requested.Contains(bare);
                        if (!requested.Contains(key) && !matchesBare) continue;
                        var info = new FileInfo(path);
                        files.Add(new(path, Path.GetRelativePath(source, path), info.Name, info.Length, info.LastWriteTimeUtc));
                        if (requested.Contains(key)) found.Add(key);
                        if (matchesBare) found.Add(bare);
                    }
                    catch (Exception e) when (e is IOException or UnauthorizedAccessException) { warnings.Add($"{path}: {e.Message}"); }
                }
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException) { warnings.Add($"{folder}: {e.Message}"); }
        }
        files.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.RelativePath, b.RelativePath));
        progress?.Report(new(examined, examined, "Scan complete"));
        return new(source, options.Names, files, options.Names.Where(n => !found.Contains(n)).ToArray(), warnings, examined);
    }
}
