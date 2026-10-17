using System.Globalization;

namespace AgapeApp.Mobile.Helpers;

/// <summary>Formata um decimal como moeda brasileira nas telas (XAML).</summary>
public class MoedaConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is decimal valor ? Formatos.Moeda(valor) : "—";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
