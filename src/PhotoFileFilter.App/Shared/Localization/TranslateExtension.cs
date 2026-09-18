using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using PhotoFileFilter.Shared.Services;

namespace PhotoFileFilter.Shared.Localization;

[MarkupExtensionReturnType(typeof(object))]
public sealed class TranslateExtension : MarkupExtension
{
    public TranslateExtension() { }
    public TranslateExtension(string key) => Key = key;

    [ConstructorArgument("key")]
    public string Key { get; set; } = "";

    public override object ProvideValue(IServiceProvider serviceProvider) => new Binding(nameof(LanguageService.LanguageState.Revision))
    {
        Source = LanguageService.State,
        Mode = BindingMode.OneWay,
        Converter = TranslationConverter.Instance,
        ConverterParameter = Key
    }.ProvideValue(serviceProvider);

    private sealed class TranslationConverter : IValueConverter
    {
        public static TranslationConverter Instance { get; } = new();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            LanguageService.Text(parameter as string ?? "");
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            Binding.DoNothing;
    }
}
