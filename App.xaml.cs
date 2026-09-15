using System.Windows;
using PhotoFileFilter.Services;

namespace PhotoFileFilter;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        LanguageService.Initialize();
        if (!LanguageService.HasSavedLanguage && new LanguageWindow().ShowDialog() != true) { Shutdown(); return; }
        var window = new ReviewWindow();
        MainWindow = window;
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        window.Show();
    }
}
