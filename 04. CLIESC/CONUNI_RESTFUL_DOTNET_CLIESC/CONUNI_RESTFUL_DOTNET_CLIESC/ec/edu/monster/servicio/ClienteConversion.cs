using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ec.edu.monster.modelo;

namespace ec.edu.monster.servicio
{
    /// <summary>Error que la vista puede mostrar tal cual al usuario (normalmente el mensaje del servidor).</summary>
    public class ErrorServicioException : Exception
    {
        public ErrorServicioException(string mensaje) : base(mensaje) { }
    }

    /// <summary>
    /// Cliente del API REST: arma las peticiones HTTP (POST para login, GET para conversiones),
    /// lee el JSON de respuesta y, si el servidor responde un error, entrega su mensaje. No tiene reglas:
    /// el login con sus 3 intentos y los rangos de los valores los valida el servidor.
    /// Patrón Singleton: toda la aplicación usa la misma instancia (ClienteConversion.Instancia),
    /// así el HttpClient se crea una sola vez.
    /// </summary>
    public sealed class ClienteConversion
    {
        // Clave de App.config (appSettings) con la URL base del servidor.
        private const string ClaveUrl = "UrlServidor";

        private static readonly Lazy<ClienteConversion> _instancia =
            new Lazy<ClienteConversion>(() => new ClienteConversion());

        public static ClienteConversion Instancia => _instancia.Value;

        // Archivo donde se recuerda el último servidor escrito en la ventana de ingreso.
        private static readonly string ArchivoServidor = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CONUNI_REST_CLIESC", "servidor.txt");

        private HttpClient _http;

        // Constructor privado: solo la propia clase crea la instancia.
        private ClienteConversion()
        {
            string url = ConfigurationManager.AppSettings[ClaveUrl];
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ConfigurationErrorsException("Falta la clave '" + ClaveUrl + "' en App.config.");
            }
            // Si ya se usó otro servidor, se arranca con ese; si no, con el de App.config.
            string guardado = File.Exists(ArchivoServidor) ? File.ReadAllText(ArchivoServidor).Trim() : null;
            UrlServidor = (string.IsNullOrEmpty(guardado) ? url : guardado).Trim().TrimEnd('/');
            _http = CrearHttp(UrlServidor);
        }

        public string UrlServidor { get; private set; }

        private static HttpClient CrearHttp(string url)
        {
            var http = new HttpClient
            {
                BaseAddress = new Uri(url + "/"),
                Timeout = TimeSpan.FromSeconds(15)
            };
            // Para que el panel de monitoreo del servidor sepa qué cliente consume el API.
            http.DefaultRequestHeaders.Add("X-Cliente", "Escritorio");
            return http;
        }

        /// <summary>
        /// Cambia el servidor al que se llama y lo recuerda para la próxima vez. Acepta la URL completa,
        /// "IP:puerto" o solo la IP (completa con el puerto 8086).
        /// </summary>
        public void CambiarServidor(string texto)
        {
            string url = NormalizarUrl(texto);
            if (url == UrlServidor) return;

            HttpClient anterior = _http;
            _http = CrearHttp(url);
            UrlServidor = url;
            anterior.Dispose();

            Directory.CreateDirectory(Path.GetDirectoryName(ArchivoServidor));
            File.WriteAllText(ArchivoServidor, url);
        }

        private static string NormalizarUrl(string texto)
        {
            string url = (texto ?? "").Trim();
            if (url.Length == 0) throw new ErrorServicioException("Ingrese la dirección del servidor REST.");
            if (!url.Contains("://")) url = "http://" + url;

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri) || uri.Scheme != Uri.UriSchemeHttp)
                throw new ErrorServicioException("Dirección de servidor no válida. Ejemplo: http://192.168.1.10:8086");

            var constructor = new UriBuilder(uri);
            if (uri.IsDefaultPort && !texto.Contains(":80")) constructor.Port = 8086;
            return constructor.Uri.ToString().TrimEnd('/');
        }

        /// <summary>
        /// POST api/autenticacion/login. Si el servidor responde 401 (clave incorrecta con los intentos restantes,
        /// o usuario bloqueado) o 400 (campos vacíos), lanza ErrorServicioException con su mensaje.
        /// </summary>
        public RespuestaLogin Login(string usuario, string clave)
        {
            string cuerpo = Json.Serializar(new SolicitudLogin { Usuario = usuario, Clave = clave });
            using (HttpResponseMessage respuesta = Enviar(() =>
                _http.PostAsync("api/autenticacion/login", new StringContent(cuerpo, Encoding.UTF8, "application/json"))))
            {
                return Leer<RespuestaLogin>(respuesta);
            }
        }

        /// <summary>GET api/{ruta}?valor=... (por ejemplo api/temperatura/celsiusAFahrenheit?valor=100).</summary>
        public double Convertir(string ruta, double valor)
        {
            string url = "api/" + ruta + "?valor=" + Uri.EscapeDataString(valor.ToString("R", CultureInfo.InvariantCulture));
            using (HttpResponseMessage respuesta = Enviar(() => _http.GetAsync(url)))
            {
                return Leer<ResultadoConversion>(respuesta).Resultado;
            }
        }

        private HttpResponseMessage Enviar(Func<Task<HttpResponseMessage>> peticion)
        {
            try
            {
                return peticion().GetAwaiter().GetResult();
            }
            catch (TaskCanceledException)
            {
                throw new ErrorServicioException("El servidor tardó demasiado en responder. Intente de nuevo.");
            }
            catch (HttpRequestException)
            {
                throw new ErrorServicioException(
                    "No se pudo conectar con el servidor en " + UrlServidor + ". Verifique que esté encendido y la dirección del servidor.");
            }
        }

        /// <summary>Si la respuesta es 2xx devuelve el objeto; si no, lanza el mensaje de ErrorServicio.</summary>
        private static T Leer<T>(HttpResponseMessage respuesta)
        {
            string json = respuesta.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            try
            {
                if (respuesta.IsSuccessStatusCode)
                {
                    return Json.Deserializar<T>(json);
                }
                ErrorServicio error = Json.Deserializar<ErrorServicio>(json);
                if (!string.IsNullOrEmpty(error?.Mensaje))
                {
                    throw new ErrorServicioException(error.Mensaje);
                }
            }
            catch (SerializationException)
            {
                // El cuerpo no era el JSON esperado (por ejemplo, una página de error HTML).
            }
            throw new ErrorServicioException(
                "Respuesta inesperada del servidor (HTTP " + (int)respuesta.StatusCode + "). Revise la dirección del servidor.");
        }
    }
}
