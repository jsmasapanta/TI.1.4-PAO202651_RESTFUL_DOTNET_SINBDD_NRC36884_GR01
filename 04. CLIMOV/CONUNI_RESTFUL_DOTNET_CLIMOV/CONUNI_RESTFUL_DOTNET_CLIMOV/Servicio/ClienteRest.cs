using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;

namespace ConversorUnidades.Movil.Servicio;

/// <summary>Error que la vista puede mostrar tal cual al usuario.</summary>
public class ErrorConversionException : Exception
{
    public ErrorConversionException(string mensaje) : base(mensaje) { }
}

// Objetos JSON del API REST (System.Text.Json en modo web: camelCase, sin distinguir mayúsculas).
public record SolicitudLogin(string Usuario, string Clave);
public record RespuestaLogin(bool Autenticado, string? Usuario, string? Mensaje);
public record RespuestaConversion(string? Categoria, string? Operacion, double Valor, string? UnidadOrigen, string? UnidadDestino, double Resultado);
public record ErrorServicio(string? Codigo, string? Mensaje);

/// <summary>
/// Cliente del API REST con HttpClient.
/// Login: POST {url}/api/autenticacion/login con JSON { usuario, clave } (401 = credenciales incorrectas).
/// Conversión: GET {url}/api/{categoria}/{operacion}?valor=... que devuelve { ..., resultado }.
/// Si el servidor responde 400, se muestra el "mensaje" del ErrorServicio.
/// Patrón Singleton: toda la app usa ClienteRest.Instancia (un solo HttpClient).
/// </summary>
public sealed class ClienteRest
{
    private const string ClavePreferencia = "url_servidor_rest";

    // 10.0.2.2 es la PC vista desde el emulador; en un celular real se cambia por la IP de la PC.
    public const string UrlPorDefecto = "http://10.0.2.2:8086";

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

    public async Task<bool> LoginAsync(string usuario, string clave)
    {
        using HttpResponseMessage respuesta = await EnviarAsync(base_ =>
            _http.PostAsJsonAsync(new Uri(base_, "api/autenticacion/login"), new SolicitudLogin(usuario, clave)));

        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
        {
            return false;
        }
        return (await LeerAsync<RespuestaLogin>(respuesta)).Autenticado;
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
            throw new ErrorConversionException($"La URL del servidor no es válida: {url}");
        }

        try
        {
            return await peticion(base_);
        }
        catch (TaskCanceledException)
        {
            throw new ErrorConversionException("El servidor tardó demasiado en responder. Intente de nuevo.");
        }
        catch (Exception e) when (e is HttpRequestException || e is SocketException || e is IOException)
        {
            throw new ErrorConversionException(
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
                    throw new ErrorConversionException(error.Mensaje);
                }
            }
        }
        catch (Exception e) when (e is JsonException || e is NotSupportedException)
        {
            // El cuerpo no era el JSON esperado (por ejemplo, una página de error HTML).
        }
        throw new ErrorConversionException($"Respuesta inesperada del servidor (HTTP {(int)respuesta.StatusCode}). Revise la URL.");
    }
}
