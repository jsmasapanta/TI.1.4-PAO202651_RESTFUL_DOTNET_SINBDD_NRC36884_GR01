using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ec.edu.monster.servicio
{
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
