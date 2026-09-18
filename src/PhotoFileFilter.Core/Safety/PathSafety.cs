namespace PhotoFileFilter.Core;

public static class PathSafety
{
    public static string Normalize(string path) => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
    public static bool Equal(string a, string b) => string.Equals(Normalize(a), Normalize(b), StringComparison.OrdinalIgnoreCase);
    public static bool IsWithin(string path, string folder) => Equal(path, folder) ||
        Normalize(path).StartsWith(Path.EndsInDirectorySeparator(Normalize(folder)) ? Normalize(folder) : Normalize(folder) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

    public static void RejectLinkedAncestors(string path)
    {
        for (var current = new DirectoryInfo(Path.GetFullPath(path)); current != null; current = current.Parent)
            if (current.Exists && (current.Attributes & FileAttributes.ReparsePoint) != 0)
                throw new IOException($"Linked folders (junctions or symlinks) are not supported: {current.FullName}");
    }

    public static string Destination(string source, bool alongside, string output, bool subfolder, string name)
    {
        var root = alongside ? source : output;
        if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("Choose a destination folder.");
        if (!Path.IsPathFullyQualified(root)) throw new ArgumentException("The destination must be a full folder path.");
        if (alongside || subfolder)
        {
            name = name.Trim();
            var stem = name.Split('.')[0];
            var reserved = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
            if (string.IsNullOrWhiteSpace(name) || name is "." or ".." || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name.EndsWith('.') || reserved.Contains(stem, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Enter a valid subfolder name, such as Selected. Do not enter a path.");
            root = Path.Combine(root, name);
        }
        root = Normalize(root);
        if (Equal(root, source)) throw new ArgumentException("The destination must differ from the source folder. Enable Create subfolder.");
        RejectLinkedAncestors(root);
        return root;
    }
}
