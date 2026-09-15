using System.Windows;
using PhotoFileFilter.Services;

namespace PhotoFileFilter;

public partial class LanguageWindow : Window
{
    public LanguageWindow() => InitializeComponent();
    private void OnVietnamese(object sender, RoutedEventArgs e) { LanguageService.Set(LanguageService.Vietnamese); DialogResult = true; }
    private void OnEnglish(object sender, RoutedEventArgs e) { LanguageService.Set(LanguageService.English); DialogResult = true; }
}
