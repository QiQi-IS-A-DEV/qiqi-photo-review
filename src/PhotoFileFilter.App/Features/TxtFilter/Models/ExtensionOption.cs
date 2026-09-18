using PhotoFileFilter.Shared.ViewModels;

namespace PhotoFileFilter.Features.TxtFilter.Models;

public sealed class ExtensionOption(string name, bool selected) : ObservableObject
{
    public string Name { get; } = name;
    private bool _selected = selected;
    public bool Selected { get => _selected; set => Set(ref _selected, value); }
}
