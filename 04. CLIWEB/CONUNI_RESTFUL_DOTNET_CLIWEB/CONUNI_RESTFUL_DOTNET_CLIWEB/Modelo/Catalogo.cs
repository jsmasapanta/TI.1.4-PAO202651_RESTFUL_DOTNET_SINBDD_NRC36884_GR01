using System.Globalization;

namespace ConversorUnidades.Web.Modelo;

public enum Categoria
{
    Temperatura,
    Longitud,
    Masa
}

/// <summary>Una conversión disponible y la ruta del recurso REST que la resuelve (sin "api/").</summary>
public record Conversion(
    Categoria Categoria,
    string UnidadOrigen, string SimboloOrigen,
    string UnidadDestino, string SimboloDestino,
    string Ruta);

public record Unidad(string Nombre, string Simbolo)
{
    public string Texto => $"{Nombre} ({Simbolo})";
}

/// <summary>Las 16 conversiones del servidor, agrupadas por categoría.</summary>
public static class Catalogo
{
    public static readonly IReadOnlyList<Conversion> Todas = new List<Conversion>
    {
        new(Categoria.Temperatura, "Celsius", "°C", "Fahrenheit", "°F", "temperatura/celsiusAFahrenheit"),
        new(Categoria.Temperatura, "Fahrenheit", "°F", "Celsius", "°C", "temperatura/fahrenheitACelsius"),
        new(Categoria.Temperatura, "Celsius", "°C", "Kelvin", "K", "temperatura/celsiusAKelvin"),
        new(Categoria.Temperatura, "Kelvin", "K", "Celsius", "°C", "temperatura/kelvinACelsius"),
        new(Categoria.Temperatura, "Fahrenheit", "°F", "Kelvin", "K", "temperatura/fahrenheitAKelvin"),
        new(Categoria.Temperatura, "Kelvin", "K", "Fahrenheit", "°F", "temperatura/kelvinAFahrenheit"),

        new(Categoria.Longitud, "Metros", "m", "Pies", "ft", "longitud/metrosAPies"),
        new(Categoria.Longitud, "Pies", "ft", "Metros", "m", "longitud/piesAMetros"),
        new(Categoria.Longitud, "Metros", "m", "Pulgadas", "in", "longitud/metrosAPulgadas"),
        new(Categoria.Longitud, "Pulgadas", "in", "Metros", "m", "longitud/pulgadasAMetros"),
        new(Categoria.Longitud, "Kilómetros", "km", "Millas", "mi", "longitud/kilometrosAMillas"),
        new(Categoria.Longitud, "Millas", "mi", "Kilómetros", "km", "longitud/millasAKilometros"),

        new(Categoria.Masa, "Kilogramos", "kg", "Libras", "lb", "masa/kilogramosALibras"),
        new(Categoria.Masa, "Libras", "lb", "Kilogramos", "kg", "masa/librasAKilogramos"),
        new(Categoria.Masa, "Gramos", "g", "Onzas", "oz", "masa/gramosAOnzas"),
        new(Categoria.Masa, "Onzas", "oz", "Gramos", "g", "masa/onzasAGramos"),
    };

    public static IList<Conversion> DeCategoria(Categoria categoria) =>
        Todas.Where(c => c.Categoria == categoria).ToList();

    public static IList<Unidad> UnidadesOrigen(Categoria categoria) =>
        DeCategoria(categoria)
            .GroupBy(c => c.SimboloOrigen)
            .Select(g => new Unidad(g.First().UnidadOrigen, g.Key))
            .ToList();

    public static IList<Unidad> UnidadesDestino(Categoria categoria, string simboloOrigen) =>
        DeCategoria(categoria)
            .Where(c => c.SimboloOrigen == simboloOrigen)
            .Select(c => new Unidad(c.UnidadDestino, c.SimboloDestino))
            .ToList();

    public static Conversion? Buscar(Categoria categoria, string? origen, string? destino) =>
        Todas.FirstOrDefault(c => c.Categoria == categoria && c.SimboloOrigen == origen && c.SimboloDestino == destino);

    /// <summary>Acepta punto o coma decimal.</summary>
    public static bool IntentarLeerNumero(string? texto, out double valor)
    {
        string normalizado = (texto ?? string.Empty).Trim().Replace(',', '.');
        return double.TryParse(normalizado, NumberStyles.Float, CultureInfo.InvariantCulture, out valor)
            && double.IsFinite(valor);
    }

    /// <summary>Hasta 6 decimales, sin ceros sobrantes.</summary>
    public static string Formatear(double numero) =>
        Math.Round(numero, 6).ToString("0.######", CultureInfo.InvariantCulture);
}
