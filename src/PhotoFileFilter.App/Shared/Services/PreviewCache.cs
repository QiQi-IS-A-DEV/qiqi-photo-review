using System.IO;

namespace PhotoFileFilter.Shared.Services;

// Per-workspace cache; frozen bitmaps can safely cross dispatcher/worker threads.
internal sealed class PreviewCache(int capacity, long byteLimit)
{
    private readonly object _gate = new();
    private readonly LinkedList<(string Key, PreviewResult Result, long Bytes)> _items = new();
    private long _bytes;

    public PreviewResult? Get(string key)
    {
        lock (_gate)
        {
            for (var node = _items.First; node != null; node = node.Next)
                if (node.Value.Key == key)
                {
                    _items.Remove(node); _items.AddFirst(node);
                    return node.Value.Result;
                }
            return null;
        }
    }

    public void Add(string key, PreviewResult result)
    {
        if (result.Image is not { IsFrozen: true } image) return;
        // Allow for transformed decoder backing storage as well as rendered pixels.
        var bytes = (long)image.PixelWidth * image.PixelHeight * Math.Max(8, (image.Format.BitsPerPixel + 7) / 8);
        if (bytes > byteLimit || capacity <= 0) return;
        lock (_gate)
        {
            if (Get(key) != null) return;
            while (_items.Count >= capacity || _bytes + bytes > byteLimit)
            {
                _bytes -= _items.Last!.Value.Bytes; _items.RemoveLast();
            }
            _items.AddFirst((key, result, bytes)); _bytes += bytes;
        }
    }
}

internal sealed class JpegCompanionIndex
{
    private readonly object _gate = new();
    private readonly Dictionary<string, Entry> _directories = new(StringComparer.OrdinalIgnoreCase);
    private sealed record Entry(DateTime Stamp, DateTime Built, Dictionary<string, string[]> Files);

    public string[] Find(string path, CancellationToken token)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path))!;
        lock (_gate)
        {
            token.ThrowIfCancellationRequested();
            var stamp = Directory.GetLastWriteTimeUtc(directory);
            // TTL also covers filesystems with coarse directory timestamps.
            if (!_directories.TryGetValue(directory, out var entry) || entry.Stamp != stamp || DateTime.UtcNow - entry.Built > TimeSpan.FromSeconds(2))
            {
                var files = new List<string>();
                foreach (var file in Directory.EnumerateFiles(directory))
                {
                    token.ThrowIfCancellationRequested();
                    if (Path.GetExtension(file).Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                        Path.GetExtension(file).Equals(".jpeg", StringComparison.OrdinalIgnoreCase)) files.Add(file);
                }
                entry = new(stamp, DateTime.UtcNow, files.Order(StringComparer.OrdinalIgnoreCase)
                    .GroupBy(file => Path.GetFileNameWithoutExtension(file), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.OrdinalIgnoreCase));
                if (_directories.Count >= 16) _directories.Remove(_directories.MinBy(pair => pair.Value.Built).Key);
                _directories[directory] = entry;
            }
            return entry.Files.GetValueOrDefault(Path.GetFileNameWithoutExtension(path)) ?? [];
        }
    }
}
