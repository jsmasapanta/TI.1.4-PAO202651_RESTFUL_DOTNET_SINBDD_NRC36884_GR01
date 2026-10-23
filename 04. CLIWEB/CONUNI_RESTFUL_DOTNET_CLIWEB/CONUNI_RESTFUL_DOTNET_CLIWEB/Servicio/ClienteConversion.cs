using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ConversorUnidades.Web.Servicio;

/// <summary>Error que la vista puede mostrar tal cual al usuario.</summary>
public class ErrorConversionException : Exception
{
    public ErrorConversionException(string mensaje) : base(mensaje) { }
}

/// <summary>
/// Cliente del API REST. La URL base viene de appsettings.json ("ServicioRest:Url").
/// Se registra como singleton: un solo HttpClient para toda la aplicación (es seguro entre hilos).
/// </summary>
public class ClienteConversion : IDisposable
{
    private readonly HttpClient _http;

    public ClienteConversion(IConfiguration configuracion)
    {
        string url = configuracion["ServicioRest:Url"]
            ?? throw new InvalidOperationException("Falta 'ServicioRest:Url' en appsettings.json.");

        UrlServidor = url.Trim().TrimEnd('/');
        _http = new HttpClient
        {
            BaseAddress = new Uri(UrlServidor + "/"),
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    public string UrlServidor { get; }

    /// <summary>POST api/autenticacion/login. Devuelve false si el servidor responde 401.</summary>
    public async Task<bool> LoginAsync(string usuario, string clave)
    {
        using HttpResponseMessage respuesta = await EnviarAsync(() =>
            _http.PostAsJsonAsync("api/autenticacion/login", new SolicitudLogin(usuario, clave)));

        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
        {
            return false;
        }
        return (await LeerAsync<RespuestaLogin>(respuesta)).Autenticado;
    }

    /// <summary>GET api/{ruta}?valor=... (por ejemplo api/longitud/kilometrosAMillas?valor=10).</summary>
    public async Task<double> ConvertirAsync(string ruta, double valor)
    {
        string url = $"api/{ruta}?valor={Uri.EscapeDataString(valor.ToString("R", CultureInfo.InvariantCulture))}";
        using HttpResponseMessage respuesta = await EnviarAsync(() => _http.GetAsync(url));
        return (await LeerAsync<ResultadoConversion>(respuesta)).Resultado;
    }

    private async Task<HttpResponseMessage> EnviarAsync(Func<Task<HttpResponseMessage>> peticion)
    {
        try
        {
            return await peticion();
        }
        catch (TaskCanceledException)
        {
            throw new ErrorConversionException("El servidor tardó demasiado en responder. Intente de nuevo.");
        }
        catch (HttpRequestException)
        {
            throw new ErrorConversionException(
                $"No se pudo conectar con el servidor en {UrlServidor}. Verifique que esté encendido y la URL en appsettings.json.");
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
        throw new ErrorConversionException(
            $"Respuesta inesperada del servidor (HTTP {(int)respuesta.StatusCode}). Revise la URL en appsettings.json.");
    }

    public void Dispose() => _http.Dispose();
}
