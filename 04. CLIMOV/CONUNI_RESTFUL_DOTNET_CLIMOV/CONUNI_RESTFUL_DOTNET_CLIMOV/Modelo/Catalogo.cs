namespace ConversorUnidades.Movil.Modelo;

public enum Categoria
{
    Temperatura,
    Longitud,
    Masa
}

public record Unidad(string Nombre, string Simbolo)
{
    public override string ToString() => $"{Nombre} ({Simbolo})";
}

/// <summary>
/// Una conversión disponible. <see cref="Ruta"/> es el recurso REST del servidor sin "api/",
/// por ejemplo "temperatura/celsiusAFahrenheit".
/// </summary>
public record Conversion(Categoria Categoria, Unidad Origen, Unidad Destino, string Ruta);

/// <summary>Las 16 conversiones del servidor, agrupadas por categoría.</summary>
public static class Catalogo
{
    private static readonly Unidad Celsius = new("Celsius", "°C");
    private static readonly Unidad Fahrenheit = new("Fahrenheit", "°F");
    private static readonly Unidad Kelvin = new("Kelvin", "K");
    private static readonly Unidad Metros = new("Metros", "m");
    private static readonly Unidad Pies = new("Pies", "ft");
    private static readonly Unidad Pulgadas = new("Pulgadas", "in");
    private static readonly Unidad Kilometros = new("Kilómetros", "km");
    private static readonly Unidad Millas = new("Millas", "mi");
    private static readonly Unidad Kilogramos = new("Kilogramos", "kg");
    private static readonly Unidad Libras = new("Libras", "lb");
    private static readonly Unidad Gramos = new("Gramos", "g");
    private static readonly Unidad Onzas = new("Onzas", "oz");

    public static readonly IReadOnlyList<Conversion> Todas = new List<Conversion>
    {
        new(Categoria.Temperatura, Celsius, Fahrenheit, "temperatura/celsiusAFahrenheit"),
        new(Categoria.Temperatura, Fahrenheit, Celsius, "temperatura/fahrenheitACelsius"),
        new(Categoria.Temperatura, Celsius, Kelvin, "temperatura/celsiusAKelvin"),
        new(Categoria.Temperatura, Kelvin, Celsius, "temperatura/kelvinACelsius"),
        new(Categoria.Temperatura, Fahrenheit, Kelvin, "temperatura/fahrenheitAKelvin"),
        new(Categoria.Temperatura, Kelvin, Fahrenheit, "temperatura/kelvinAFahrenheit"),

        new(Categoria.Longitud, Metros, Pies, "longitud/metrosAPies"),
        new(Categoria.Longitud, Pies, Metros, "longitud/piesAMetros"),
        new(Categoria.Longitud, Metros, Pulgadas, "longitud/metrosAPulgadas"),
        new(Categoria.Longitud, Pulgadas, Metros, "longitud/pulgadasAMetros"),
        new(Categoria.Longitud, Kilometros, Millas, "longitud/kilometrosAMillas"),
        new(Categoria.Longitud, Millas, Kilometros, "longitud/millasAKilometros"),

        new(Categoria.Masa, Kilogramos, Libras, "masa/kilogramosALibras"),
        new(Categoria.Masa, Libras, Kilogramos, "masa/librasAKilogramos"),
        new(Categoria.Masa, Gramos, Onzas, "masa/gramosAOnzas"),
        new(Categoria.Masa, Onzas, Gramos, "masa/onzasAGramos")
    };

    public static List<Unidad> UnidadesOrigen(Categoria categoria) =>
        Todas.Where(c => c.Categoria == categoria).Select(c => c.Origen).Distinct().ToList();

    public static List<Unidad> UnidadesDestino(Categoria categoria, Unidad origen) =>
        Todas.Where(c => c.Categoria == categoria && c.Origen == origen).Select(c => c.Destino).ToList();

    public static Conversion? Buscar(Unidad origen, Unidad destino) =>
        Todas.FirstOrDefault(c => c.Origen == origen && c.Destino == destino);

    public static string Simbolo(Categoria categoria) => categoria switch
    {
        Categoria.Temperatura => "°C",
        Categoria.Longitud => "m",
        _ => "kg"
    };
}
