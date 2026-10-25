using ec.edu.monster.modelo;
using ec.edu.monster.servicio;
using Microsoft.AspNetCore.Mvc;

namespace ec.edu.monster.controlador;

/// <summary>
/// Login y logout. El servidor decide si las credenciales son correctas, cuenta los 3 intentos y bloquea
/// al usuario: aquí solo se pasa la llamada y se muestra su mensaje.
/// </summary>
public class CuentaController : Controller
{
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
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel entrada)
    {
        try
        {
            await _cliente.LoginAsync(entrada.Usuario ?? "", entrada.Clave ?? "");
            HttpContext.Session.SetString(RequiereSesionAttribute.ClaveUsuario, (entrada.Usuario ?? "").Trim());
            return RedirectToAction("Index", "Conversion");
        }
        catch (ErrorServicioException ex)
        {
            // Mensaje del servidor: clave incorrecta con intentos restantes, usuario bloqueado o falla de conexión.
            return View(new LoginViewModel { Usuario = entrada.Usuario, Error = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}
