using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using ConversorUnidades.Servidor.Logica;

namespace ConversorUnidades.Servidor.Monitoreo;

/// <summary>
/// Middleware que mide cada petición a api/... y la anota en MonitorServicio. No toca los controladores:
/// lee la entrada (el "valor" del query string o el usuario del login, nunca la clave), deja pasar la petición,
/// y de la respuesta toma el código HTTP y el resultado o el mensaje de error.
/// </summary>
public static class RegistroConsumo
{
    public static IApplicationBuilder UseRegistroConsumo(this IApplicationBuilder app) => app.Use(async (contexto, siguiente) =>
    {
        if (!contexto.Request.Path.StartsWithSegments("/api"))
        {
            await siguiente();
            return;
        }

        var reloj = Stopwatch.StartNew();
        var registro = new RegistroLlamada
        {
            Hora = DateTime.Now,
            Cliente = IpCliente(contexto),
            TipoCliente = TipoCliente(contexto.Request),
            Metodo = contexto.Request.Method,
            Operacion = contexto.Request.Path.Value!.Replace("/api/", "", StringComparison.OrdinalIgnoreCase),
            Entrada = await LeerEntrada(contexto.Request),
        };

        // La respuesta se escribe en memoria para poder leer el resultado y luego se copia al cliente.
        Stream original = contexto.Response.Body;
        using var memoria = new MemoryStream();
        contexto.Response.Body = memoria;
        try
        {
            await siguiente();
        }
        finally
        {
            memoria.Position = 0;
            string cuerpo = await new StreamReader(memoria).ReadToEndAsync();
            memoria.Position = 0;
            await memoria.CopyToAsync(original);
            contexto.Response.Body = original;

            registro.CodigoHttp = contexto.Response.StatusCode;
            registro.Exito = registro.CodigoHttp < 400;
            registro.Resultado = Resultado(cuerpo);
            registro.Milisegundos = reloj.Elapsed.TotalMilliseconds;
            MonitorServicio.Instancia.Registrar(registro);
        }
    });

    // Cada cliente manda la cabecera HTTP "X-Cliente" (Consola, Escritorio, Web o Movil).
    // Sin cabecera: si viene de un navegador es Swagger.
    private static string TipoCliente(HttpRequest peticion)
    {
        string? tipo = peticion.Headers["X-Cliente"];
        if (!string.IsNullOrWhiteSpace(tipo)) return tipo.Trim();
        return peticion.Headers.UserAgent.ToString().Contains("Mozilla") ? "Navegador" : "Otro";
    }

    private static string IpCliente(HttpContext contexto)
    {
        var ip = contexto.Connection.RemoteIpAddress;
        if (ip == null) return "desconocido";
        if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();
        string texto = ip.ToString();
        return texto == "::1" ? "127.0.0.1" : texto;
    }

    private static async Task<string> LeerEntrada(HttpRequest peticion)
    {
        string? valor = peticion.Query["valor"];
        if (!string.IsNullOrEmpty(valor)) return valor;
        if (!HttpMethods.IsPost(peticion.Method)) return "";

        // Login: solo se anota el usuario.
        peticion.EnableBuffering();
        string cuerpo = await new StreamReader(peticion.Body, leaveOpen: true).ReadToEndAsync();
        peticion.Body.Position = 0;
        return Propiedad(cuerpo, "usuario") ?? "";
    }

    private static string Resultado(string cuerpo)
    {
        string? resultado = Propiedad(cuerpo, "resultado");
        if (resultado != null) return $"{resultado} {Propiedad(cuerpo, "unidadDestino")}".Trim();
        return Propiedad(cuerpo, "mensaje") ?? "";
    }

    private static string? Propiedad(string json, string nombre)
    {
        try
        {
            using var documento = JsonDocument.Parse(json);
            if (documento.RootElement.ValueKind != JsonValueKind.Object) return null;
            foreach (var propiedad in documento.RootElement.EnumerateObject())
            {
                if (!propiedad.Name.Equals(nombre, StringComparison.OrdinalIgnoreCase)) continue;
                return propiedad.Value.ValueKind == JsonValueKind.Number
                    ? propiedad.Value.GetDouble().ToString("0.######", CultureInfo.InvariantCulture)
                    : propiedad.Value.ToString();
            }
        }
        catch (JsonException) { }
        return null;
    }
}
