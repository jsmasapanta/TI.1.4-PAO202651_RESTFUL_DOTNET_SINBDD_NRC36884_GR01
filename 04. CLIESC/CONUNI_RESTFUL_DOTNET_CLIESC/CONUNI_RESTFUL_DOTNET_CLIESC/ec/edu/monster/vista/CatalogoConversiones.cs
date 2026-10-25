using System.Collections.Generic;
using System.Linq;
using ec.edu.monster.modelo;

namespace ec.edu.monster.vista
{
    /// <summary>Unidad de medida que se muestra en las listas desplegables.</summary>
    public class Unidad
    {
        public string Nombre { get; set; }
        public string Simbolo { get; set; }

        public override string ToString() => $"{Nombre} ({Simbolo})";
    }

    /// <summary>
    /// Opciones que muestran las listas: los 16 recursos del API REST, agrupadas por categoría,
    /// con los nombres y símbolos de las unidades. No calcula nada: cada opción solo dice qué recurso llamar.
    /// </summary>
    public static class CatalogoConversiones
    {
        public static readonly IReadOnlyList<Conversion> Todas = new List<Conversion>
        {
            Nueva(Categoria.Temperatura, "temperatura/celsiusAFahrenheit", "Celsius", "°C", "Fahrenheit", "°F"),
            Nueva(Categoria.Temperatura, "temperatura/fahrenheitACelsius", "Fahrenheit", "°F", "Celsius", "°C"),
            Nueva(Categoria.Temperatura, "temperatura/celsiusAKelvin", "Celsius", "°C", "Kelvin", "K"),
            Nueva(Categoria.Temperatura, "temperatura/kelvinACelsius", "Kelvin", "K", "Celsius", "°C"),
            Nueva(Categoria.Temperatura, "temperatura/fahrenheitAKelvin", "Fahrenheit", "°F", "Kelvin", "K"),
            Nueva(Categoria.Temperatura, "temperatura/kelvinAFahrenheit", "Kelvin", "K", "Fahrenheit", "°F"),

            Nueva(Categoria.Longitud, "longitud/metrosAPies", "Metros", "m", "Pies", "ft"),
            Nueva(Categoria.Longitud, "longitud/piesAMetros", "Pies", "ft", "Metros", "m"),
            Nueva(Categoria.Longitud, "longitud/metrosAPulgadas", "Metros", "m", "Pulgadas", "in"),
            Nueva(Categoria.Longitud, "longitud/pulgadasAMetros", "Pulgadas", "in", "Metros", "m"),
            Nueva(Categoria.Longitud, "longitud/kilometrosAMillas", "Kilómetros", "km", "Millas", "mi"),
            Nueva(Categoria.Longitud, "longitud/millasAKilometros", "Millas", "mi", "Kilómetros", "km"),

            Nueva(Categoria.Masa, "masa/kilogramosALibras", "Kilogramos", "kg", "Libras", "lb"),
            Nueva(Categoria.Masa, "masa/librasAKilogramos", "Libras", "lb", "Kilogramos", "kg"),
            Nueva(Categoria.Masa, "masa/gramosAOnzas", "Gramos", "g", "Onzas", "oz"),
            Nueva(Categoria.Masa, "masa/onzasAGramos", "Onzas", "oz", "Gramos", "g"),
        };

        public static IList<Conversion> DeCategoria(Categoria categoria) =>
            Todas.Where(c => c.Categoria == categoria).ToList();

        /// <summary>Unidades que se pueden usar como origen en la categoría.</summary>
        public static IList<Unidad> UnidadesOrigen(Categoria categoria) =>
            DeCategoria(categoria)
                .GroupBy(c => c.SimboloOrigen)
                .Select(g => new Unidad { Nombre = g.First().UnidadOrigen, Simbolo = g.Key })
                .ToList();

        /// <summary>Unidades a las que el servicio convierte desde la unidad de origen.</summary>
        public static IList<Unidad> UnidadesDestino(Categoria categoria, string simboloOrigen) =>
            DeCategoria(categoria)
                .Where(c => c.SimboloOrigen == simboloOrigen)
                .Select(c => new Unidad { Nombre = c.UnidadDestino, Simbolo = c.SimboloDestino })
                .ToList();

        public static Conversion Buscar(string simboloOrigen, string simboloDestino) =>
            Todas.FirstOrDefault(c => c.SimboloOrigen == simboloOrigen && c.SimboloDestino == simboloDestino);

        private static Conversion Nueva(Categoria categoria, string ruta,
            string unidadOrigen, string simboloOrigen, string unidadDestino, string simboloDestino) =>
            new Conversion
            {
                Categoria = categoria,
                Ruta = ruta,
                UnidadOrigen = unidadOrigen,
                SimboloOrigen = simboloOrigen,
                UnidadDestino = unidadDestino,
                SimboloDestino = simboloDestino
            };
    }
}
