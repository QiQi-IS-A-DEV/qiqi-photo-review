using System.Text;

namespace PhotoFileFilter.Core;

public sealed class TxtParserService
{
    public async Task<ParseResult> ReadAsync(string path, ParseOptions options, CancellationToken token)
    {
        // BOM detection supports UTF-8 and UTF-16; invalid UTF-8 is reported instead of silently changing names.
        using var reader = new StreamReader(path, new UTF8Encoding(false, true), true);
        return Parse(await reader.ReadToEndAsync(token), options);
    }

    public ParseResult Parse(string text, ParseOptions options)
    {
        if (!options.Comma && !options.Space && !options.NewLine)
            throw new ArgumentException("Select at least one TXT separator.");
        var separators = new List<char>();
        if (options.Comma) separators.Add(',');
        if (options.Space) separators.AddRange([' ', '\t']);
        if (options.NewLine) separators.AddRange(['\r', '\n']);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var names = new List<string>();
        var duplicates = 0;
        foreach (var part in text.TrimStart('\uFEFF').Split(separators.ToArray(), StringSplitOptions.RemoveEmptyEntries))
        {
            var name = part.Trim().Trim('"', '\'').Trim();
            if (name.Length == 0) continue;
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name is "." or "..")
                throw new ArgumentException($"Invalid name: {name}. The TXT file must contain filenames, not paths. Check the selected separators.");
            if (options.IgnoreExtension) name = Path.GetFileNameWithoutExtension(name);
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("The TXT list contains an empty filename after removing its extension.");
            if (seen.Add(name)) names.Add(name); else duplicates++;
        }
        if (names.Count == 0) throw new ArgumentException("The TXT file does not contain any filenames.");
        return new(names, duplicates);
    }
}
