using ec.edu.monster.modelo;

namespace ec.edu.monster.vista;

/// <summary>
/// Opciones que muestran las listas: los 16 recursos del API REST, agrupadas por categoría.
/// No calcula nada: cada opción solo dice qué recurso llamar.
/// </summary>
public static class CatalogoConversiones
{
    private static readonly Unidad Celsius = U("Celsius", "°C");
    private static readonly Unidad Fahrenheit = U("Fahrenheit", "°F");
    private static readonly Unidad Kelvin = U("Kelvin", "K");
    private static readonly Unidad Metros = U("Metros", "m");
    private static readonly Unidad Pies = U("Pies", "ft");
    private static readonly Unidad Pulgadas = U("Pulgadas", "in");
    private static readonly Unidad Kilometros = U("Kilómetros", "km");
    private static readonly Unidad Millas = U("Millas", "mi");
    private static readonly Unidad Kilogramos = U("Kilogramos", "kg");
    private static readonly Unidad Libras = U("Libras", "lb");
    private static readonly Unidad Gramos = U("Gramos", "g");
    private static readonly Unidad Onzas = U("Onzas", "oz");

    public static readonly IReadOnlyList<Conversion> Todas = new List<Conversion>
    {
        C(Categoria.Temperatura, Celsius, Fahrenheit, "temperatura/celsiusAFahrenheit"),
        C(Categoria.Temperatura, Fahrenheit, Celsius, "temperatura/fahrenheitACelsius"),
        C(Categoria.Temperatura, Celsius, Kelvin, "temperatura/celsiusAKelvin"),
        C(Categoria.Temperatura, Kelvin, Celsius, "temperatura/kelvinACelsius"),
        C(Categoria.Temperatura, Fahrenheit, Kelvin, "temperatura/fahrenheitAKelvin"),
        C(Categoria.Temperatura, Kelvin, Fahrenheit, "temperatura/kelvinAFahrenheit"),

        C(Categoria.Longitud, Metros, Pies, "longitud/metrosAPies"),
        C(Categoria.Longitud, Pies, Metros, "longitud/piesAMetros"),
        C(Categoria.Longitud, Metros, Pulgadas, "longitud/metrosAPulgadas"),
        C(Categoria.Longitud, Pulgadas, Metros, "longitud/pulgadasAMetros"),
        C(Categoria.Longitud, Kilometros, Millas, "longitud/kilometrosAMillas"),
        C(Categoria.Longitud, Millas, Kilometros, "longitud/millasAKilometros"),

        C(Categoria.Masa, Kilogramos, Libras, "masa/kilogramosALibras"),
        C(Categoria.Masa, Libras, Kilogramos, "masa/librasAKilogramos"),
        C(Categoria.Masa, Gramos, Onzas, "masa/gramosAOnzas"),
        C(Categoria.Masa, Onzas, Gramos, "masa/onzasAGramos")
    };

    public static List<Unidad> UnidadesOrigen(Categoria categoria) =>
        Todas.Where(c => c.Categoria == categoria).Select(c => c.Origen).Distinct().ToList();

    public static List<Unidad> UnidadesDestino(Categoria categoria, Unidad origen) =>
        Todas.Where(c => c.Categoria == categoria && c.Origen == origen).Select(c => c.Destino).ToList();

    public static Conversion? Buscar(Unidad origen, Unidad destino) =>
        Todas.FirstOrDefault(c => c.Origen == origen && c.Destino == destino);

    private static Unidad U(string nombre, string simbolo) =>
        new() { Nombre = nombre, Simbolo = simbolo, Texto = $"{nombre} ({simbolo})" };

    private static Conversion C(Categoria categoria, Unidad origen, Unidad destino, string ruta) =>
        new() { Categoria = categoria, Origen = origen, Destino = destino, Ruta = ruta };
}
