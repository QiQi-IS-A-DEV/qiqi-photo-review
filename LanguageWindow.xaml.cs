using System.Windows;
using System.Windows.Input;
using PhotoFileFilter.Services;

namespace PhotoFileFilter;

public partial class LanguageWindow : Window
{
    public LanguageWindow() => InitializeComponent();
    private void OnLanguageKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.None) return;
        if (e.Key is Key.D1 or Key.NumPad1 or Key.V) OnVietnamese(sender, e);
        else if (e.Key is Key.D2 or Key.NumPad2 or Key.E) OnEnglish(sender, e);
        else return;
        e.Handled = true;
    }
    private void OnVietnamese(object sender, RoutedEventArgs e) { LanguageService.Set(LanguageService.Vietnamese); DialogResult = true; }
    private void OnEnglish(object sender, RoutedEventArgs e) { LanguageService.Set(LanguageService.English); DialogResult = true; }
}
