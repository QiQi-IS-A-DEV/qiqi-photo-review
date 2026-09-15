using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PhotoFileFilter.Services;

namespace PhotoFileFilter.ViewModels;

public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void Notify([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
    protected bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (value is string text) value = (T)(object)LanguageService.Text(text);
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value; Notify(name); return true;
    }
}
public sealed class RelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{
    public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => execute();
    public event EventHandler? CanExecuteChanged;
    public void Refresh() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
public sealed class ExtensionOption(string name, bool selected) : ObservableObject
{
    public string Name { get; } = name;
    private bool _selected = selected;
    public bool Selected { get => _selected; set => Set(ref _selected, value); }
}
