using System.Collections.Generic;
using System.Linq;

namespace ConversorUnidades.Consola.Modelo
{
    /// <summary>
    /// Las 16 conversiones que ofrece el servidor, agrupadas por categoría.
    /// </summary>
    public static class CatalogoConversiones
    {
        public static readonly IReadOnlyList<Conversion> Todas = new List<Conversion>
        {
            new Conversion(Categoria.Temperatura, "Celsius", "°C", "Fahrenheit", "°F", "temperatura/celsiusAFahrenheit"),
            new Conversion(Categoria.Temperatura, "Fahrenheit", "°F", "Celsius", "°C", "temperatura/fahrenheitACelsius"),
            new Conversion(Categoria.Temperatura, "Celsius", "°C", "Kelvin", "K", "temperatura/celsiusAKelvin"),
            new Conversion(Categoria.Temperatura, "Kelvin", "K", "Celsius", "°C", "temperatura/kelvinACelsius"),
            new Conversion(Categoria.Temperatura, "Fahrenheit", "°F", "Kelvin", "K", "temperatura/fahrenheitAKelvin"),
            new Conversion(Categoria.Temperatura, "Kelvin", "K", "Fahrenheit", "°F", "temperatura/kelvinAFahrenheit"),

            new Conversion(Categoria.Longitud, "Metros", "m", "Pies", "ft", "longitud/metrosAPies"),
            new Conversion(Categoria.Longitud, "Pies", "ft", "Metros", "m", "longitud/piesAMetros"),
            new Conversion(Categoria.Longitud, "Metros", "m", "Pulgadas", "in", "longitud/metrosAPulgadas"),
            new Conversion(Categoria.Longitud, "Pulgadas", "in", "Metros", "m", "longitud/pulgadasAMetros"),
            new Conversion(Categoria.Longitud, "Kilómetros", "km", "Millas", "mi", "longitud/kilometrosAMillas"),
            new Conversion(Categoria.Longitud, "Millas", "mi", "Kilómetros", "km", "longitud/millasAKilometros"),

            new Conversion(Categoria.Masa, "Kilogramos", "kg", "Libras", "lb", "masa/kilogramosALibras"),
            new Conversion(Categoria.Masa, "Libras", "lb", "Kilogramos", "kg", "masa/librasAKilogramos"),
            new Conversion(Categoria.Masa, "Gramos", "g", "Onzas", "oz", "masa/gramosAOnzas"),
            new Conversion(Categoria.Masa, "Onzas", "oz", "Gramos", "g", "masa/onzasAGramos"),
        };

        public static IList<Conversion> DeCategoria(Categoria categoria)
        {
            return Todas.Where(c => c.Categoria == categoria).ToList();
        }
    }
}
