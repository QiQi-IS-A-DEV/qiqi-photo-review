namespace PhotoFileFilter.Core;

public record ParseOptions(bool Comma = true, bool Space = true, bool NewLine = true, bool IgnoreExtension = true);
public record ParseResult(IReadOnlyList<string> Names, int DuplicateCount);
public record PhotoFile(string FullPath, string RelativePath, string Name, long Size, DateTime LastWriteUtc) : System.ComponentModel.INotifyPropertyChanged
{
    private bool _includeInCopy = true;
    public bool IncludeInCopy { get => _includeInCopy; set { if (_includeInCopy == value) return; _includeInCopy = value; PropertyChanged?.Invoke(this, new(nameof(IncludeInCopy))); } }
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    public string FileType => Path.GetExtension(Name).TrimStart('.').ToUpperInvariant();
    public string SizeLabel => Size >= 1048576 ? $"{Size / 1048576d:N1} MB" : $"{Size / 1024d:N1} KB";
}
public record ScanOptions(string SourceFolder, IReadOnlyList<string> Names, IReadOnlyList<string> Extensions,
    bool IgnoreExtension, bool Recursive, string? ExcludedFolder = null);
public record ScanResult(string SourceFolder, IReadOnlyList<string> RequestedNames, IReadOnlyList<PhotoFile> Files,
    IReadOnlyList<string> Missing, IReadOnlyList<string> Warnings, int ExaminedCount);
public enum CollisionPolicy { Rename, Skip, Replace }
public record CopyOptions(string Destination, CollisionPolicy Policy, IReadOnlySet<string>? IncludedPaths = null);
public record OperationProgress(int Completed, int Total, string CurrentFile,
    long ProcessedBytes = 0, long TotalBytes = 0, long TransferredBytes = 0, double ElapsedSeconds = 0)
{
    public double BytesPerSecond => ElapsedSeconds > 0 ? TransferredBytes / ElapsedSeconds : 0;
    public double? RemainingSeconds => BytesPerSecond > 0 ? Math.Max(0, TotalBytes - ProcessedBytes) / BytesPerSecond : null;
}
public record CopyIssue(string File, string Reason);
public record CopyResult(int Copied, int Skipped, IReadOnlyList<CopyIssue> Errors, bool Cancelled);

public sealed class CopyPauseToken
{
    private readonly object _gate = new();
    private TaskCompletionSource _resume = CompletedSource();
    public bool IsPaused { get; private set; }

    public void Pause()
    {
        lock (_gate)
        {
            if (IsPaused) return;
            IsPaused = true;
            _resume = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

    public void Resume()
    {
        TaskCompletionSource resume;
        lock (_gate)
        {
            if (!IsPaused) return;
            IsPaused = false;
            resume = _resume;
        }
        resume.TrySetResult();
    }

    public Task WaitIfPausedAsync(CancellationToken token)
    {
        Task wait;
        lock (_gate) wait = IsPaused ? _resume.Task : Task.CompletedTask;
        return wait.WaitAsync(token);
    }

    private static TaskCompletionSource CompletedSource()
    {
        var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        source.SetResult();
        return source;
    }
}
