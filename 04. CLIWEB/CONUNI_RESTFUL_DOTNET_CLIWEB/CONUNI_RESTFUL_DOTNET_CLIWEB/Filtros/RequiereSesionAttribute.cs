using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ConversorUnidades.Web.Filtros;

/// <summary>
/// Si no hay usuario en la sesión, redirige al login.
/// Así nadie entra a /Conversion escribiendo la URL directamente.
/// </summary>
public class RequiereSesionAttribute : ActionFilterAttribute
{
    public const string ClaveUsuario = "Usuario";

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (string.IsNullOrEmpty(context.HttpContext.Session.GetString(ClaveUsuario)))
        {
            context.Result = new RedirectToActionResult("Login", "Cuenta", null);
        }
    }
}
