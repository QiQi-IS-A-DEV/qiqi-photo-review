using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace PhotoFileFilter.Features.Review.Models;

public enum ReviewFlag { None, Pick, Reject }
public enum ReviewColor { None, Red, Yellow, Green, Blue, Purple }

public sealed class ReviewPhoto : INotifyPropertyChanged
{
    private int _rating;
    private ReviewFlag _flag;
    private ReviewColor _color;
    private BitmapSource? _thumbnail;
    private int _rotation;

    public ReviewPhoto(string fullPath, string root, long size, DateTime lastWriteUtc)
    {
        FullPath = fullPath;
        RelativePath = Path.GetRelativePath(root, fullPath);
        Name = Path.GetFileName(fullPath);
        Extension = Path.GetExtension(fullPath).TrimStart('.').ToUpperInvariant();
        Size = size;
        LastWriteUtc = lastWriteUtc;
    }

    public string FullPath { get; }
    public string RelativePath { get; }
    public string Name { get; }
    public string Extension { get; }
    public long Size { get; }
    public DateTime LastWriteUtc { get; }
    public int Rating { get => _rating; set { value = Math.Clamp(value, 0, 5); if (Set(ref _rating, value)) { Notify(nameof(Stars)); Notify(nameof(RatingLabel)); } } }
    public ReviewFlag Flag { get => _flag; set { if (Set(ref _flag, value)) { Notify(nameof(IsPicked)); Notify(nameof(IsRejected)); Notify(nameof(FlagLabel)); } } }
    public ReviewColor ColorLabel { get => _color; set { if (Set(ref _color, value)) { Notify(nameof(ColorLabelName)); Notify(nameof(ColorHex)); Notify(nameof(ColorBackgroundHex)); Notify(nameof(TileTextHex)); } } }
    public bool IsPicked => Flag == ReviewFlag.Pick;
    public bool IsRejected => Flag == ReviewFlag.Reject;
    public string Stars => Rating == 0 ? "—" : new string('★', Rating) + new string('☆', 5 - Rating);
    public string RatingLabel => Rating == 0 ? "Unrated" : $"{Rating} stars";
    public string FlagLabel => Flag switch { ReviewFlag.Pick => "PICK", ReviewFlag.Reject => "REJECT", _ => "" };
    public string ColorLabelName => ColorLabel switch { ReviewColor.Red => "Red", ReviewColor.Yellow => "Yellow", ReviewColor.Green => "Green", ReviewColor.Blue => "Blue", ReviewColor.Purple => "Purple", _ => "No color" };
    public string ColorHex => ColorLabel switch { ReviewColor.Red => "#D95454", ReviewColor.Yellow => "#E1B83D", ReviewColor.Green => "#4BAE71", ReviewColor.Blue => "#438BC5", ReviewColor.Purple => "#AC78D1", _ => "Transparent" };
    public string ColorBackgroundHex => ColorLabel switch { ReviewColor.Red => "#8A706E", ReviewColor.Yellow => "#8B8662", ReviewColor.Green => "#66806D", ReviewColor.Blue => "#66788B", ReviewColor.Purple => "#867091", _ => "#1C1C1C" };
    public string TileTextHex => ColorLabel == ReviewColor.None ? "#E4E4E4" : "#111111";
    public BitmapSource? Thumbnail { get => _thumbnail; set => Set(ref _thumbnail, value); }
    public int Rotation { get => _rotation; set => Set(ref _rotation, ((value % 360) + 360) % 360); }

    public event PropertyChangedEventHandler? PropertyChanged;
    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value; Notify(name); return true;
    }
    private void Notify([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}
