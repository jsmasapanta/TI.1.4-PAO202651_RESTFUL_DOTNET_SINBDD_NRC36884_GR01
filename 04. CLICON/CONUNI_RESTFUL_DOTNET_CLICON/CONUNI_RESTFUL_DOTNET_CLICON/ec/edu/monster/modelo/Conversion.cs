namespace ec.edu.monster.modelo
{
    public enum Categoria
    {
        Temperatura,
        Longitud,
        Masa
    }

    /// <summary>
    /// Una conversión que ofrece el servicio. Solo datos (get/set): las fórmulas y las validaciones
    /// están en el servidor.
    /// </summary>
    public class Conversion
    {
        public Categoria Categoria { get; set; }
        /// <summary>Ruta del recurso REST sin "api/", por ejemplo "temperatura/celsiusAFahrenheit".</summary>
        public string Ruta { get; set; }
        public string UnidadOrigen { get; set; }
        public string SimboloOrigen { get; set; }
        public string UnidadDestino { get; set; }
        public string SimboloDestino { get; set; }
    }
}
