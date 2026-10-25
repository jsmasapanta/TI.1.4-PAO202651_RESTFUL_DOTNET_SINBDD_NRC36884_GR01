using System.Globalization;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using ec.edu.monster.modelo;

namespace ec.edu.monster.servicio;

/// <summary>Error que la vista puede mostrar tal cual al usuario (normalmente el mensaje del servidor).</summary>
public class ErrorServicioException : Exception
{
    public ErrorServicioException(string mensaje) : base(mensaje) { }
}

/// <summary>
/// Cliente del API REST con HttpClient.
/// Login: POST {url}/api/autenticacion/login con JSON { usuario, clave } (401 = clave incorrecta con los
/// intentos restantes, o usuario bloqueado: los cuenta el servidor).
/// Conversión: GET {url}/api/{categoria}/{operacion}?valor=... que devuelve { ..., resultado }.
/// Si el servidor responde un error, se muestra el "mensaje" del ErrorServicio. No tiene reglas propias.
/// Patrón Singleton: toda la app usa ClienteRest.Instancia (un solo HttpClient).
/// </summary>
public sealed class ClienteRest
{
    private const string ClavePreferencia = "url_servidor_rest";

    // IP de la PC con el servidor en la red WiFi; en el emulador sería http://10.0.2.2:8086.
    public const string UrlPorDefecto = "http://192.168.100.3:8086";

    private static readonly Lazy<ClienteRest> _instancia = new(() => new ClienteRest());

    public static ClienteRest Instancia => _instancia.Value;

    private readonly HttpClient _http;

    // Constructor privado: solo la propia clase crea la instancia.
    private ClienteRest()
    {
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
    }

    /// <summary>URL base del servidor; se guarda en el teléfono para no escribirla cada vez.</summary>
    public string Url
    {
        get => Preferences.Default.Get(ClavePreferencia, UrlPorDefecto);
        set => Preferences.Default.Set(ClavePreferencia, value.Trim().TrimEnd('/'));
    }

    public async Task<RespuestaLogin> LoginAsync(string usuario, string clave)
    {
        using HttpResponseMessage respuesta = await EnviarAsync(base_ =>
            _http.PostAsJsonAsync(new Uri(base_, "api/autenticacion/login"), new SolicitudLogin { Usuario = usuario, Clave = clave }));
        return await LeerAsync<RespuestaLogin>(respuesta);
    }

    /// <param name="ruta">Recurso sin "api/", por ejemplo "masa/kilogramosALibras".</param>
    public async Task<double> ConvertirAsync(string ruta, double valor)
    {
        string recurso = $"api/{ruta}?valor={Uri.EscapeDataString(valor.ToString("R", CultureInfo.InvariantCulture))}";
        using HttpResponseMessage respuesta = await EnviarAsync(base_ => _http.GetAsync(new Uri(base_, recurso)));
        return (await LeerAsync<RespuestaConversion>(respuesta)).Resultado;
    }

    private async Task<HttpResponseMessage> EnviarAsync(Func<Uri, Task<HttpResponseMessage>> peticion)
    {
        string url = Url;
        if (!Uri.TryCreate(url + "/", UriKind.Absolute, out Uri? base_)
            || (base_.Scheme != Uri.UriSchemeHttp && base_.Scheme != Uri.UriSchemeHttps))
        {
            throw new ErrorServicioException($"La URL del servidor no es válida: {url}");
        }

        try
        {
            return await peticion(base_);
        }
        catch (TaskCanceledException)
        {
            throw new ErrorServicioException("El servidor tardó demasiado en responder. Intente de nuevo.");
        }
        catch (Exception e) when (e is HttpRequestException || e is SocketException || e is IOException)
        {
            throw new ErrorServicioException(
                $"No se pudo conectar con el servidor en {url}. Verifique que esté encendido, la IP y que estén en la misma red WiFi.");
        }
    }

    /// <summary>Si la respuesta es 2xx devuelve el objeto; si no, lanza el mensaje de ErrorServicio.</summary>
    private static async Task<T> LeerAsync<T>(HttpResponseMessage respuesta)
    {
        try
        {
            if (respuesta.IsSuccessStatusCode)
            {
                T? datos = await respuesta.Content.ReadFromJsonAsync<T>();
                if (datos != null)
                {
                    return datos;
                }
            }
            else
            {
                ErrorServicio? error = await respuesta.Content.ReadFromJsonAsync<ErrorServicio>();
                if (!string.IsNullOrEmpty(error?.Mensaje))
                {
                    throw new ErrorServicioException(error.Mensaje);
                }
            }
        }
        catch (Exception e) when (e is JsonException || e is NotSupportedException)
        {
            // El cuerpo no era el JSON esperado (por ejemplo, una página de error HTML).
        }
        throw new ErrorServicioException($"Respuesta inesperada del servidor (HTTP {(int)respuesta.StatusCode}). Revise la URL.");
    }
}
