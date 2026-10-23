namespace ConversorUnidades.Escritorio.Modelo
{
    public enum Categoria
    {
        Temperatura,
        Longitud,
        Masa
    }

    /// <summary>
    /// Una conversión disponible: de qué unidad a cuál, y qué recurso del API REST la resuelve.
    /// </summary>
    public class Conversion
    {
        public Categoria Categoria { get; }
        public string UnidadOrigen { get; }
        public string SimboloOrigen { get; }
        public string UnidadDestino { get; }
        public string SimboloDestino { get; }
        /// <summary>Ruta del recurso REST sin "api/", por ejemplo "temperatura/celsiusAFahrenheit".</summary>
        public string Ruta { get; }

        public Conversion(Categoria categoria,
                          string unidadOrigen, string simboloOrigen,
                          string unidadDestino, string simboloDestino,
                          string ruta)
        {
            Categoria = categoria;
            UnidadOrigen = unidadOrigen;
            SimboloOrigen = simboloOrigen;
            UnidadDestino = unidadDestino;
            SimboloDestino = simboloDestino;
            Ruta = ruta;
        }

        public string Descripcion => $"{UnidadOrigen} ({SimboloOrigen}) → {UnidadDestino} ({SimboloDestino})";
    }
}
