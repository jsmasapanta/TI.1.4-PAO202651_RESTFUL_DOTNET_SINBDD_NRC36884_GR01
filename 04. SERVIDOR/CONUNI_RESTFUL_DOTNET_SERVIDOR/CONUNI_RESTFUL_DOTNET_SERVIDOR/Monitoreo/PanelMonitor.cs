using System.Globalization;
using System.Net;
using System.Text;
using System.Xml.Linq;
using ConversorUnidades.Servidor.Logica;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace ConversorUnidades.Servidor.Monitoreo;

/// <summary>
/// Panel del servidor (/monitor). Pide usuario y clave (los mismos del API, validados por el Autenticador
/// Singleton) y luego muestra dos pestañas:
///   Servicio   -> los endpoints publicados y los enlaces a Swagger y al documento OpenAPI.
///   Monitoreo  -> gráficos en vivo de lo que consume cada cliente (Consola, Escritorio, Web, Móvil).
/// </summary>
public static class PanelMonitor
{
    private const string LlaveSesion = "PanelUsuario";
    private static readonly string[] TiposCliente = { "Consola", "Escritorio", "Web", "Movil" };
    private static readonly string[] ColoresCliente = { "#2e9e6b", "#3d6fd9", "#d9582b", "#8e44ad", "#7f8c8d", "#b8860b" };

    public static void MapPanelMonitor(this WebApplication app)
    {
        app.MapGet("/monitor", (HttpContext contexto, IApiDescriptionGroupCollectionProvider api) =>
        {
            string? usuario = contexto.Session.GetString(LlaveSesion);
            if (usuario == null) return Html(PaginaLogin(null, null));
            bool monitoreo = contexto.Request.Query["vista"] != "servicio";
            return Html(PaginaPanel(contexto, api, usuario, monitoreo));
        }).ExcludeFromDescription();

        app.MapPost("/monitor", async (HttpContext contexto) =>
        {
            IFormCollection formulario = await contexto.Request.ReadFormAsync();
            switch (formulario["accion"].ToString())
            {
                case "ingresar":
                    string? error = Ingresar(contexto, formulario["usuario"], formulario["clave"]);
                    return error == null ? Results.Redirect("/monitor?vista=monitoreo") : Html(PaginaLogin(error, formulario["usuario"]));
                case "salir":
                    contexto.Session.Remove(LlaveSesion);
                    return Results.Redirect("/monitor");
                case "limpiar" when contexto.Session.GetString(LlaveSesion) != null:
                    MonitorServicio.Instancia.Limpiar();
                    return Results.Redirect("/monitor?vista=monitoreo");
                default:
                    return Results.Redirect("/monitor");
            }
        }).DisableAntiforgery().ExcludeFromDescription();
    }

    private static string? Ingresar(HttpContext contexto, string? usuario, string? clave)
    {
        try
        {
            Autenticador.Instancia.Validar(usuario, clave);
            contexto.Session.SetString(LlaveSesion, usuario!.Trim());
            return null;
        }
        catch (Exception ex) when (ex is ArgumentException or ReglaNegocioException or InvalidOperationException)
        {
            return ex.Message;
        }
    }

    // ---------------- Páginas ----------------

    private static string PaginaLogin(string? error, string? usuario)
    {
        var html = new StringBuilder(Cabecera("Panel CONUNI REST · Ingreso", false));
        html.Append("<div class='login'><div class='tarjeta caja'>")
            .Append("<div class='logo'>⇄</div><h1>Panel del servidor</h1><p class='sub'>CONUNI · RESTful .NET</p>")
            .Append("<form method='post'><input type='hidden' name='accion' value='ingresar'>")
            .Append($"<label>Usuario</label><input name='usuario' autocomplete='username' value='{E(usuario)}' autofocus>")
            .Append("<label>Clave</label><input name='clave' type='password' autocomplete='current-password'>");
        if (error != null) html.Append($"<div class='alerta'>{E(error)}</div>");
        html.Append("<button>Ingresar</button></form>")
            .Append("<p class='nota'>Mismas credenciales del API. Al tercer intento fallido el usuario queda bloqueado 2 minutos.</p>")
            .Append("</div></div></body></html>");
        return html.ToString();
    }

    private static string PaginaPanel(HttpContext contexto, IApiDescriptionGroupCollectionProvider api, string usuario, bool monitoreo)
    {
        string baseUrl = $"{contexto.Request.Scheme}://{contexto.Request.Host}";
        var html = new StringBuilder(Cabecera("Panel CONUNI REST", monitoreo));
        html.Append("<header><div class='marca'><div class='logo chico'>⇄</div><div><h1>CONUNI · API REST</h1>")
            .Append("<span>Conversor de unidades · RESTful .NET sin base de datos</span></div></div>")
            .Append($"<div class='usuario'>{E(usuario)}<form method='post'><input type='hidden' name='accion' value='salir'><button class='claro'>Cerrar sesión</button></form></div></header>")
            .Append("<nav>")
            .Append($"<a class='{(monitoreo ? "" : "activa")}' href='/monitor?vista=servicio'>Servicio</a>")
            .Append($"<a class='{(monitoreo ? "activa" : "")}' href='/monitor?vista=monitoreo'>Monitoreo de clientes</a>")
            .Append("</nav><main>");
        html.Append(monitoreo ? VistaMonitoreo() : VistaServicio(baseUrl, api));
        html.Append("</main></body></html>");
        return html.ToString();
    }

    private static string VistaServicio(string baseUrl, IApiDescriptionGroupCollectionProvider api)
    {
        var html = new StringBuilder();
        html.Append("<div class='fila'>")
            .Append(Enlace("Swagger UI", baseUrl + "/swagger", "Página para probar cada endpoint del API"))
            .Append(Enlace("Documento OpenAPI", baseUrl + "/swagger/v1/swagger.json", "Contrato del API en JSON"))
            .Append(Enlace("Ejemplo GET", baseUrl + "/api/temperatura/celsiusAFahrenheit?valor=100", "100 °C → °F en JSON"))
            .Append("</div>");

        var resumenes = Resumenes();
        var endpoints = api.ApiDescriptionGroups.Items.SelectMany(g => g.Items)
            .OrderBy(d => d.RelativePath).ToList();

        html.Append($"<div class='tarjeta'><h2>Endpoints publicados ({endpoints.Count})</h2><table><tr><th>Método</th><th>Ruta</th><th>Parámetros</th><th>Descripción</th></tr>");
        foreach (ApiDescription d in endpoints)
        {
            string parametros = string.Join(", ", d.ParameterDescriptions.Select(p => $"{p.Name} ({p.Source.DisplayName})"));
            string descripcion = d.ActionDescriptor is ControllerActionDescriptor accion
                && resumenes.TryGetValue($"M:{accion.MethodInfo.DeclaringType!.FullName}.{accion.MethodInfo.Name}", out string? texto) ? texto : "";
            html.Append($"<tr><td><span class='chip {(d.HttpMethod == "POST" ? "post" : "get")}'>{d.HttpMethod}</span></td>")
                .Append($"<td><code>/{E(d.RelativePath)}</code></td><td><code>{E(parametros)}</code></td><td>{E(descripcion)}</td></tr>");
        }
        html.Append("</table></div>");
        return html.ToString();
    }

    private static string VistaMonitoreo()
    {
        ResumenMonitor r = MonitorServicio.Instancia.Resumen();
        TimeSpan activo = DateTime.Now - r.Inicio;
        var html = new StringBuilder();

        html.Append("<div class='fila'>")
            .Append(Tarjeta("Peticiones", r.Total.ToString(), ""))
            .Append(Tarjeta("Correctas (2xx)", (r.Total - r.Errores).ToString(), "ok"))
            .Append(Tarjeta("Con error (4xx / 5xx)", r.Errores.ToString(), "mal"))
            .Append(Tarjeta("Clientes activos", r.PorTipo.Count.ToString(), ""))
            .Append(Tarjeta("Servidor activo", $"{(int)activo.TotalHours:00}:{activo.Minutes:00}:{activo.Seconds:00}", ""))
            .Append("</div>");

        // Tarjeta por cliente: los 4 de siempre y cualquier otro que haya llamado (por ejemplo, Swagger).
        var tipos = TiposCliente.Concat(r.PorTipo.Keys.Where(k => !TiposCliente.Contains(k))).ToList();
        html.Append("<div class='clientes'>");
        for (int i = 0; i < tipos.Count; i++)
        {
            r.PorTipo.TryGetValue(tipos[i], out ConsumoCliente? c);
            bool reciente = c != null && (DateTime.Now - c.Ultima).TotalSeconds < 60;
            html.Append($"<div class='tarjeta cliente' style='border-top:5px solid {Color(i)}'>")
                .Append($"<div class='etq'>{Icono(tipos[i])} Cliente {E(Nombre(tipos[i]))}</div>")
                .Append($"<div class='numero'>{c?.Llamadas ?? 0}</div><div class='sub'>peticiones · {c?.Errores ?? 0} con error</div>")
                .Append($"<div class='estado {(reciente ? "on" : "")}'>{(c == null ? "Sin conexión todavía" : (reciente ? "● Activo" : "○ Inactivo") + " · última " + c.Ultima.ToString("HH:mm:ss"))}</div>")
                .Append("</div>");
        }
        html.Append("</div>");

        html.Append("<div class='dos'>")
            .Append("<div class='tarjeta'><h2>Consumo por cliente</h2>").Append(GraficoDona(r, tipos)).Append("</div>")
            .Append("<div class='tarjeta'><h2>Endpoints más consumidos</h2>").Append(Barras(r)).Append("</div>")
            .Append("</div>");

        html.Append("<div class='tarjeta'><div class='titulo'><h2>Últimas peticiones</h2>")
            .Append("<form method='post'><input type='hidden' name='accion' value='limpiar'><button>Limpiar registro</button></form></div>")
            .Append("<table><tr><th>Hora</th><th>Cliente</th><th>IP</th><th>Método</th><th>Endpoint</th><th>Entrada</th><th>HTTP</th><th>Resultado</th><th>ms</th></tr>");
        foreach (RegistroLlamada l in r.Registros.Take(40))
        {
            html.Append($"<tr class='{(l.Exito ? "" : "error")}'><td>{l.Hora:HH:mm:ss}</td>")
                .Append($"<td><span class='chip' style='background:{Color(tipos.IndexOf(l.TipoCliente))}'>{E(Nombre(l.TipoCliente))}</span></td>")
                .Append($"<td>{E(l.Cliente)}</td><td>{l.Metodo}</td><td>{E(l.Operacion)}</td><td>{E(l.Entrada)}</td>")
                .Append($"<td class='{(l.Exito ? "ok" : "mal")}'><b>{l.CodigoHttp}</b></td>")
                .Append($"<td class='{(l.Exito ? "ok" : "mal")}'>{(l.Exito ? "✔ " : "✖ ")}{E(l.Resultado)}</td>")
                .Append($"<td>{l.Milisegundos.ToString("0.0", CultureInfo.InvariantCulture)}</td></tr>");
        }
        if (r.Registros.Count == 0) html.Append("<tr><td colspan='9'>Inicie sesión o convierta desde cualquier cliente para ver aquí la petición.</td></tr>");
        html.Append("</table></div>");
        return html.ToString();
    }

    // ---------------- Gráficos (SVG y CSS, sin librerías externas) ----------------

    private static string GraficoDona(ResumenMonitor r, List<string> tipos)
    {
        int total = r.PorTipo.Values.Sum(c => c.Llamadas);
        const double radio = 70, perimetro = 2 * Math.PI * radio;
        var svg = new StringBuilder("<div class='dona'><svg viewBox='0 0 200 200' width='200' height='200'>");
        svg.Append($"<circle cx='100' cy='100' r='{radio}' fill='none' stroke='#ece9f5' stroke-width='28'/>");
        double desplazamiento = 0;
        var leyenda = new StringBuilder("<ul class='leyenda'>");
        for (int i = 0; i < tipos.Count; i++)
        {
            r.PorTipo.TryGetValue(tipos[i], out ConsumoCliente? c);
            int llamadas = c?.Llamadas ?? 0;
            double porcentaje = total == 0 ? 0 : (double)llamadas / total;
            if (llamadas > 0)
            {
                double largo = porcentaje * perimetro;
                svg.Append($"<circle cx='100' cy='100' r='{radio}' fill='none' stroke='{Color(i)}' stroke-width='28' ")
                   .Append($"stroke-dasharray='{N(largo)} {N(perimetro - largo)}' stroke-dashoffset='{N(-desplazamiento)}' transform='rotate(-90 100 100)'/>");
                desplazamiento += largo;
            }
            leyenda.Append($"<li><i style='background:{Color(i)}'></i>{E(Nombre(tipos[i]))}<b>{llamadas} ({porcentaje:P0})</b></li>");
        }
        svg.Append($"<text x='100' y='96' text-anchor='middle' class='dona-num'>{total}</text><text x='100' y='118' text-anchor='middle' class='dona-txt'>peticiones</text></svg>");
        return svg + leyenda.ToString() + "</ul></div>";
    }

    private static string Barras(ResumenMonitor r)
    {
        if (r.PorOperacion.Count == 0) return "<p class='sub'>Aún no hay peticiones.</p>";
        int maximo = r.PorOperacion.Max(p => p.Value);
        var html = new StringBuilder("<div class='barras'>");
        foreach (var op in r.PorOperacion.Take(10))
        {
            html.Append($"<div class='barra'><span>{E(op.Key)}</span><div class='riel'><div style='width:{op.Value * 100 / maximo}%'></div></div><b>{op.Value}</b></div>");
        }
        return html.Append("</div>").ToString();
    }

    // ---------------- Utilidades ----------------

    // Resúmenes /// de los controladores (archivo XML que también usa Swagger).
    private static Dictionary<string, string> Resumenes()
    {
        string archivo = Path.Combine(AppContext.BaseDirectory, "CONUNI_RESTFUL_DOTNET_SERVIDOR.xml");
        if (!File.Exists(archivo)) return new();
        return XDocument.Load(archivo).Descendants("member")
            .Where(m => ((string?)m.Attribute("name"))?.StartsWith("M:") == true)
            .GroupBy(m => ((string)m.Attribute("name")!).Split('(')[0])
            .ToDictionary(g => g.Key, g => ((string?)g.First().Element("summary") ?? "").Trim());
    }

    private static IResult Html(string contenido) => Results.Content(contenido, "text/html; charset=utf-8");

    private static string Cabecera(string titulo, bool refrescar) =>
        "<!DOCTYPE html><html lang='es'><head><meta charset='utf-8'><meta name='viewport' content='width=device-width, initial-scale=1'>"
        + (refrescar ? "<meta http-equiv='refresh' content='3'>" : "")
        + $"<title>{titulo}</title><style>{Estilos}</style></head><body>";

    private const string Estilos = @"
body{margin:0;font-family:Segoe UI,Arial,sans-serif;background:#f3f1ec;color:#1f1b3a}
header{background:#29215c;color:#fff;padding:16px 28px;display:flex;justify-content:space-between;align-items:center;flex-wrap:wrap;gap:10px}
header h1{margin:0;font-size:22px}header span{color:#b9b2e8;font-size:13px}.marca{display:flex;gap:12px;align-items:center}
.usuario{display:flex;gap:12px;align-items:center;color:#d8d3f5}
nav{background:#fff;padding:0 28px;display:flex;gap:4px;border-bottom:1px solid #e6e3f2}
nav a{padding:14px 18px;color:#6b6790;text-decoration:none;font-weight:600;border-bottom:3px solid transparent}
nav a.activa{color:#29215c;border-color:#d9582b}
main{padding:20px 28px;display:grid;gap:16px}
.fila,.clientes{display:grid;grid-template-columns:repeat(auto-fit,minmax(180px,1fr));gap:16px}
.tarjeta{background:#fff;border-radius:16px;padding:16px 20px;box-shadow:0 1px 3px rgba(0,0,0,.08)}
.numero{font-size:34px;font-weight:700;margin-top:4px}.etq{color:#6b6790;font-size:13px;text-transform:uppercase;letter-spacing:.04em}
.sub{color:#6b6790;font-size:13px;margin:2px 0 0}.ok{color:#0f7b5f}.mal{color:#c0392b}
.estado{margin-top:10px;font-size:13px;color:#9a96b5}.estado.on{color:#0f7b5f;font-weight:600}
.dos{display:grid;grid-template-columns:1fr 1fr;gap:16px}@media(max-width:900px){.dos{grid-template-columns:1fr}}
table{width:100%;border-collapse:collapse;font-size:14px}th{text-align:left;color:#6b6790;font-weight:600;border-bottom:2px solid #e6e3f2;padding:6px}
td{padding:6px;border-bottom:1px solid #efedf6;vertical-align:top}tr.error td{background:#fdecea}
.chip{display:inline-block;padding:2px 9px;border-radius:10px;font-size:12px;color:#fff;background:#7f8c8d}.chip.get{background:#2e9e6b}.chip.post{background:#3d6fd9}
h2{margin:0 0 12px;font-size:17px}.titulo{display:flex;justify-content:space-between;align-items:center}
button{background:#d9582b;color:#fff;border:0;border-radius:8px;padding:8px 14px;cursor:pointer;font-weight:600}
button.claro{background:#fff;color:#29215c}a{color:#3d6fd9}code{font-size:12px;color:#29215c}
.enlace{display:block;text-decoration:none;color:inherit}.enlace b{color:#29215c;font-size:16px}.enlace code{display:block;margin:6px 0;word-break:break-all;color:#3d6fd9}
.dona{display:flex;gap:24px;align-items:center;flex-wrap:wrap}.dona-num{font-size:30px;font-weight:700;fill:#1f1b3a}.dona-txt{font-size:13px;fill:#6b6790}
.leyenda{list-style:none;padding:0;margin:0;flex:1;min-width:180px}.leyenda li{display:flex;align-items:center;gap:8px;padding:6px 0;border-bottom:1px solid #efedf6}
.leyenda i{width:14px;height:14px;border-radius:4px}.leyenda b{margin-left:auto}
.barras{display:grid;gap:8px}.barra{display:grid;grid-template-columns:200px 1fr 34px;gap:8px;align-items:center;font-size:13px}
.riel{background:#ece9f5;border-radius:6px;height:16px;overflow:hidden}.riel div{background:#29215c;height:100%;border-radius:6px}
.login{min-height:100vh;display:flex;align-items:center;justify-content:center;background:#29215c}
.caja{width:320px;padding:28px}.caja h1{margin:10px 0 0;font-size:22px}.caja label{display:block;margin:14px 0 4px;font-weight:600;font-size:14px}
.caja input{width:100%;box-sizing:border-box;padding:10px;border:1px solid #cfcbe3;border-radius:8px;font-size:15px}
.caja button{width:100%;margin-top:18px;padding:12px;font-size:15px}.nota{color:#9a96b5;font-size:12px;margin-top:14px}
.alerta{margin-top:12px;background:#fdecea;color:#c0392b;padding:10px;border-radius:8px;font-size:14px}
.logo{width:48px;height:48px;border-radius:12px;background:#29215c;color:#7fe0c0;display:flex;align-items:center;justify-content:center;font-size:24px}
.logo.chico{width:40px;height:40px;background:#3b3175}";

    private static string Enlace(string titulo, string url, string texto) =>
        $"<a class='tarjeta enlace' href='{url}' target='_blank'><b>{titulo}</b><code>{E(url)}</code><span class='sub'>{texto}</span></a>";

    private static string Tarjeta(string etiqueta, string valor, string clase) =>
        $"<div class='tarjeta'><div class='etq'>{etiqueta}</div><div class='numero {clase}'>{valor}</div></div>";

    private static string Nombre(string tipo) => tipo == "Movil" ? "Móvil" : tipo;

    private static string Icono(string tipo) => tipo switch
    {
        "Consola" => "⌨",
        "Escritorio" => "🖥",
        "Web" => "🌐",
        "Movil" => "📱",
        _ => "•",
    };

    private static string Color(int indice) => indice < 0 ? "#7f8c8d" : ColoresCliente[indice % ColoresCliente.Length];

    private static string N(double valor) => valor.ToString("0.##", CultureInfo.InvariantCulture);

    private static string E(string? texto) => WebUtility.HtmlEncode(texto ?? "");
}
