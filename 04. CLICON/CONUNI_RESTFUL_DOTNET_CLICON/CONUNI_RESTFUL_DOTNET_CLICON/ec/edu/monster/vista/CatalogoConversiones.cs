using System.Collections.Generic;
using System.Linq;
using ec.edu.monster.modelo;

namespace ec.edu.monster.vista
{
    /// <summary>
    /// Opciones que muestra el menú: los 16 recursos del API REST, agrupados por categoría,
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

        /// <summary>Texto de la opción: "Celsius (°C) → Fahrenheit (°F)".</summary>
        public static string Descripcion(Conversion c) =>
            $"{c.UnidadOrigen} ({c.SimboloOrigen}) → {c.UnidadDestino} ({c.SimboloDestino})";

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
