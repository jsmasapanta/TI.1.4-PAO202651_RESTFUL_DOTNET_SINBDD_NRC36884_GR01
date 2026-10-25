using System.Globalization;
using System.Text.Json;
using ec.edu.monster.modelo;
using ec.edu.monster.servicio;
using ec.edu.monster.vista;
using Microsoft.AspNetCore.Mvc;

namespace ec.edu.monster.controlador;

/// <summary>
/// Pantalla de conversión: pasa el valor al servicio y muestra el resultado o el mensaje del servidor.
/// Los rangos (cero absoluto, valores negativos) los valida el servidor.
/// </summary>
[RequiereSesion]
public class ConversionController : Controller
{
    private const int MaximoHistorial = 10;

    private readonly ClienteConversion _cliente;

    public ConversionController(ClienteConversion cliente)
    {
        _cliente = cliente;
    }

    [HttpGet]
    public IActionResult Index(Categoria categoria = Categoria.Temperatura)
    {
        return View(CrearModelo(categoria, null, null));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(Categoria categoria, string? origen, string? destino, string? valor)
    {
        ConversionViewModel modelo = CrearModelo(categoria, origen, destino);
        modelo.Valor = valor;

        Conversion? conversion = CatalogoConversiones.Buscar(categoria, modelo.Origen, modelo.Destino);
        if (conversion == null)
        {
            modelo.Error = "Seleccione una conversión válida.";
            return View(modelo);
        }
        if (!LeerNumero(valor, out double numero))
        {
            modelo.Error = "Ingrese un número válido (ejemplo: 25 o 12.5).";
            return View(modelo);
        }

        try
        {
            double resultado = await _cliente.ConvertirAsync(conversion.Ruta, numero);
            string entrada = $"{Formato.Numero(numero)} {conversion.SimboloOrigen}";
            string salida = $"{Formato.Numero(resultado)} {conversion.SimboloDestino}";
            modelo.Resultado = salida;
            modelo.Detalle = $"{entrada} ({conversion.UnidadOrigen})  =  {salida} ({conversion.UnidadDestino})";
            GuardarEnHistorial(categoria, $"{DateTime.Now:HH:mm:ss}  ·  {entrada} → {salida}");
            modelo.Historial = LeerHistorial(categoria);
        }
        catch (ErrorServicioException ex)
        {
            modelo.Error = ex.Message;
        }
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult LimpiarHistorial(Categoria categoria)
    {
        HttpContext.Session.Remove(ClaveHistorial(categoria));
        return RedirectToAction(nameof(Index), new { categoria });
    }

    private ConversionViewModel CrearModelo(Categoria categoria, string? origen, string? destino)
    {
        IList<Unidad> origenes = CatalogoConversiones.UnidadesOrigen(categoria);
        if (origenes.All(u => u.Simbolo != origen))
        {
            origen = origenes[0].Simbolo;
        }
        IList<Unidad> destinos = CatalogoConversiones.UnidadesDestino(categoria, origen!);
        if (destinos.All(u => u.Simbolo != destino))
        {
            destino = destinos[0].Simbolo;
        }

        ViewData["Usuario"] = HttpContext.Session.GetString(RequiereSesionAttribute.ClaveUsuario);
        ViewData["Servidor"] = _cliente.UrlServidor;

        return new ConversionViewModel
        {
            Categoria = categoria,
            UnidadesOrigen = origenes,
            UnidadesDestino = destinos,
            Origen = origen,
            Destino = destino,
            Historial = LeerHistorial(categoria),
            MapaDestinos = origenes.ToDictionary(u => u.Simbolo, u => CatalogoConversiones.UnidadesDestino(categoria, u.Simbolo))
        };
    }

    /// <summary>
    /// Convierte el texto (punto o coma decimal) al número que recibe el servicio. Solo comprueba que sea
    /// un número para poder enviarlo; si está fuera de rango lo rechaza el servidor.
    /// </summary>
    private static bool LeerNumero(string? texto, out double valor) =>
        double.TryParse((texto ?? "").Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out valor)
        && double.IsFinite(valor);

    // El historial vive en la sesión del navegador (no hay base de datos).
    private static string ClaveHistorial(Categoria categoria) => "Historial_" + categoria;

    private IList<string> LeerHistorial(Categoria categoria)
    {
        string? json = HttpContext.Session.GetString(ClaveHistorial(categoria));
        return json == null ? new List<string>() : JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
    }

    private void GuardarEnHistorial(Categoria categoria, string linea)
    {
        var historial = LeerHistorial(categoria).ToList();
        historial.Insert(0, linea);
        HttpContext.Session.SetString(ClaveHistorial(categoria), JsonSerializer.Serialize(historial.Take(MaximoHistorial)));
    }
}
