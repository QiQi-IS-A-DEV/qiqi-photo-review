namespace PhotoFileFilter.Shared.Services;

public static class AppInfo
{
    public static string Version { get; } = typeof(AppInfo).Assembly.GetName().Version?.ToString(3) ?? "1.0.0";
    public static string BrandVersion => $"QIQI STUDIO  ·  v{Version}";
    public static string ReviewTitle => $"{LanguageService.Text("QiQi Studio · Import & Review")} · v{Version}";
    public static string FilterTitle => $"{LanguageService.Text("QiQi Studio · Filter Photos by TXT List")} · v{Version}";
}
