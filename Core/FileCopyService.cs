namespace PhotoFileFilter.Core;

public sealed class FileCopyService
{
    public async Task<CopyResult> CopyAsync(ScanResult scan, CopyOptions options, IProgress<OperationProgress>? progress, CancellationToken token)
    {
        var destination = PathSafety.Normalize(options.Destination);
        if (PathSafety.Equal(destination, scan.SourceFolder)) throw new IOException("Files cannot be copied directly into the source folder.");
        PathSafety.RejectLinkedAncestors(destination);
        Directory.CreateDirectory(destination);
        var sources = scan.Files.Select(f => PathSafety.Normalize(f.FullPath)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var written = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var errors = new List<CopyIssue>();
        var copied = 0;
        var skipped = 0;
        var completed = 0;
        // Keep the full scan as the source protection set even when only a subset is copied.
        var batch = scan.Files.Where(f => options.IncludedPaths == null || options.IncludedPaths.Contains(f.FullPath)).ToArray();
        foreach (var photo in batch)
        {
            if (token.IsCancellationRequested) return new(copied, skipped, errors, true);
            string? temporary = null;
            try
            {
                progress?.Report(new(completed, batch.Length, photo.RelativePath));
                PathSafety.RejectLinkedAncestors(destination);
                PathSafety.RejectLinkedAncestors(Path.GetDirectoryName(photo.FullPath)!);
                var info = new FileInfo(photo.FullPath);
                if ((info.Attributes & FileAttributes.ReparsePoint) != 0) throw new IOException("The source file is a link.");
                if (info.Length != photo.Size || info.LastWriteTimeUtc != photo.LastWriteUtc) throw new IOException("A source file changed after the scan. Please scan again.");
                var target = Path.Combine(destination, photo.Name);
                if (sources.Contains(PathSafety.Normalize(target))) throw new IOException("The destination matches an original photo; it was skipped to protect the source.");
                if (options.Policy == CollisionPolicy.Skip && File.Exists(target)) { skipped++; continue; }
                // Two source files with the same name must never overwrite one another in a batch.
                if (options.Policy == CollisionPolicy.Replace && written.Contains(target)) throw new IOException("Multiple source photos have the same filename. Choose Rename to keep every file.");
                if (File.Exists(target) && (File.GetAttributes(target) & FileAttributes.ReparsePoint) != 0) throw new IOException("The destination file is a link.");
                temporary = Path.Combine(destination, $".photofilter-{Guid.NewGuid():N}.tmp");
                await using (var input = new FileStream(photo.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, true))
                await using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1024 * 1024, true))
                {
                    await input.CopyToAsync(output, 1024 * 1024, token);
                    await output.FlushAsync(token);
                }
                token.ThrowIfCancellationRequested();
                File.SetLastWriteTimeUtc(temporary, photo.LastWriteUtc);
                if (options.Policy == CollisionPolicy.Rename)
                {
                    var suffix = 0;
                    while (true)
                    {
                        var candidate = suffix == 0 ? target : Path.Combine(destination, $"{Path.GetFileNameWithoutExtension(photo.Name)} ({suffix}){Path.GetExtension(photo.Name)}");
                        if (File.Exists(candidate) || Directory.Exists(candidate)) { suffix++; continue; }
                        try { File.Move(temporary, candidate, false); target = candidate; break; }
                        catch (IOException) when (File.Exists(candidate)) { suffix++; }
                    }
                }
                else
                {
                    try { File.Move(temporary, target, options.Policy == CollisionPolicy.Replace); }
                    catch (IOException) when (options.Policy == CollisionPolicy.Skip && File.Exists(target)) { skipped++; continue; }
                }
                temporary = null;
                written.Add(target);
                copied++;
            }
            catch (OperationCanceledException) { return new(copied, skipped, errors, true); }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException) { errors.Add(new(photo.RelativePath, e.Message)); }
            finally
            {
                if (temporary != null)
                {
                    try { File.Delete(temporary); }
                    catch (Exception e) when (e is IOException or UnauthorizedAccessException) { errors.Add(new(temporary, $"Could not remove the temporary file: {e.Message}")); }
                }
                completed++;
                progress?.Report(new(completed, batch.Length, photo.RelativePath));
            }
        }
        return new(copied, skipped, errors, false);
    }
}
