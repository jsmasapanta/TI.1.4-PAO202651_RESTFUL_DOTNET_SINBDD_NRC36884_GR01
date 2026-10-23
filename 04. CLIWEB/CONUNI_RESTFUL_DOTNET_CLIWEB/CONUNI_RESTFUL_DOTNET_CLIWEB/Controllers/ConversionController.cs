using System.Text.Json;
using ConversorUnidades.Web.Filtros;
using ConversorUnidades.Web.Modelo;
using ConversorUnidades.Web.Servicio;
using Microsoft.AspNetCore.Mvc;

namespace ConversorUnidades.Web.Controllers;

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

        Conversion? conversion = Catalogo.Buscar(categoria, modelo.Origen, modelo.Destino);
        if (conversion == null)
        {
            modelo.Error = "Seleccione una conversión válida.";
            return View(modelo);
        }
        if (!Catalogo.IntentarLeerNumero(valor, out double numero))
        {
            modelo.Error = "Ingrese un número válido (ejemplo: 25 o 12.5).";
            return View(modelo);
        }

        try
        {
            double resultado = await _cliente.ConvertirAsync(conversion.Ruta, numero);
            string entrada = $"{Catalogo.Formatear(numero)} {conversion.SimboloOrigen}";
            string salida = $"{Catalogo.Formatear(resultado)} {conversion.SimboloDestino}";
            modelo.Resultado = salida;
            modelo.Detalle = $"{entrada} ({conversion.UnidadOrigen})  =  {salida} ({conversion.UnidadDestino})";
            GuardarEnHistorial(categoria, $"{DateTime.Now:HH:mm:ss}  ·  {entrada} → {salida}");
            modelo.Historial = LeerHistorial(categoria);
        }
        catch (ErrorConversionException ex)
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
        IList<Unidad> origenes = Catalogo.UnidadesOrigen(categoria);
        if (origenes.All(u => u.Simbolo != origen))
        {
            origen = origenes[0].Simbolo;
        }
        IList<Unidad> destinos = Catalogo.UnidadesDestino(categoria, origen!);
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
            MapaDestinos = origenes.ToDictionary(u => u.Simbolo, u => Catalogo.UnidadesDestino(categoria, u.Simbolo))
        };
    }

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
