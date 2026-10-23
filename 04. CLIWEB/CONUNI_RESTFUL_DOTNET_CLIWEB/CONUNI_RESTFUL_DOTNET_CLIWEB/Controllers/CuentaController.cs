using ConversorUnidades.Web.Filtros;
using ConversorUnidades.Web.Modelo;
using ConversorUnidades.Web.Servicio;
using Microsoft.AspNetCore.Mvc;

namespace ConversorUnidades.Web.Controllers;

public class CuentaController : Controller
{
    private const int IntentosMaximos = 3;
    private const string ClaveIntentos = "IntentosFallidos";

    private readonly ClienteConversion _cliente;

    public CuentaController(ClienteConversion cliente)
    {
        _cliente = cliente;
    }

    /// <summary>La raíz del sitio (/) lleva al login.</summary>
    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(Login));

    [HttpGet]
    public IActionResult Login()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString(RequiereSesionAttribute.ClaveUsuario)))
        {
            return RedirectToAction("Index", "Conversion");
        }
        return View(CrearModelo());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel entrada)
    {
        LoginViewModel modelo = CrearModelo();
        modelo.Usuario = entrada.Usuario;

        if (modelo.Bloqueado)
        {
            return View(modelo);
        }
        if (string.IsNullOrWhiteSpace(entrada.Usuario) || string.IsNullOrEmpty(entrada.Clave))
        {
            modelo.Error = "Ingrese usuario y clave.";
            return View(modelo);
        }

        try
        {
            if (await _cliente.LoginAsync(entrada.Usuario, entrada.Clave))
            {
                HttpContext.Session.Remove(ClaveIntentos);
                HttpContext.Session.SetString(RequiereSesionAttribute.ClaveUsuario, entrada.Usuario.Trim());
                return RedirectToAction("Index", "Conversion");
            }

            int fallidos = (HttpContext.Session.GetInt32(ClaveIntentos) ?? 0) + 1;
            HttpContext.Session.SetInt32(ClaveIntentos, fallidos);
            modelo = CrearModelo();
            modelo.Usuario = entrada.Usuario;
            modelo.Error = modelo.Bloqueado
                ? $"Se superaron los {IntentosMaximos} intentos. Cierre el navegador para volver a intentarlo."
                : $"Usuario o clave incorrectos. Intentos restantes: {modelo.IntentosRestantes}.";
        }
        catch (ErrorConversionException ex)
        {
            modelo.Error = ex.Message;
        }
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }

    private LoginViewModel CrearModelo()
    {
        int fallidos = HttpContext.Session.GetInt32(ClaveIntentos) ?? 0;
        return new LoginViewModel
        {
            IntentosRestantes = Math.Max(0, IntentosMaximos - fallidos),
            Bloqueado = fallidos >= IntentosMaximos
        };
    }
}
