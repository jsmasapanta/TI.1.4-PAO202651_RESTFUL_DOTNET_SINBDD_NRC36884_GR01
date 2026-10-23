using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ConversorUnidades.Escritorio.Servicio
{
    // Objetos JSON que intercambia el API REST. Los nombres en camelCase coinciden con los del servidor.

    [DataContract]
    public class SolicitudLogin
    {
        [DataMember(Name = "usuario")] public string Usuario { get; set; }
        [DataMember(Name = "clave")] public string Clave { get; set; }
    }

    [DataContract]
    public class RespuestaLogin
    {
        [DataMember(Name = "autenticado")] public bool Autenticado { get; set; }
        [DataMember(Name = "usuario")] public string Usuario { get; set; }
        [DataMember(Name = "mensaje")] public string Mensaje { get; set; }
    }

    [DataContract]
    public class ResultadoConversion
    {
        [DataMember(Name = "categoria")] public string Categoria { get; set; }
        [DataMember(Name = "operacion")] public string Operacion { get; set; }
        [DataMember(Name = "valor")] public double Valor { get; set; }
        [DataMember(Name = "unidadOrigen")] public string UnidadOrigen { get; set; }
        [DataMember(Name = "unidadDestino")] public string UnidadDestino { get; set; }
        [DataMember(Name = "resultado")] public double Resultado { get; set; }
    }

    /// <summary>Cuerpo de las respuestas de error (400, 401, 500).</summary>
    [DataContract]
    public class ErrorServicio
    {
        [DataMember(Name = "codigo")] public string Codigo { get; set; }
        [DataMember(Name = "mensaje")] public string Mensaje { get; set; }
    }

    /// <summary>Convierte objetos a JSON y viceversa con el serializador que trae .NET Framework.</summary>
    internal static class Json
    {
        public static string Serializar<T>(T objeto)
        {
            using (var memoria = new MemoryStream())
            {
                new DataContractJsonSerializer(typeof(T)).WriteObject(memoria, objeto);
                return Encoding.UTF8.GetString(memoria.ToArray());
            }
        }

        public static T Deserializar<T>(string json)
        {
            using (var memoria = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(memoria);
            }
        }
    }
}
