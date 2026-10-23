using ConversorUnidades.Servidor.Logica;
using ConversorUnidades.Servidor.Modelo;
using Microsoft.AspNetCore.Mvc;

namespace ConversorUnidades.Servidor.Controllers;

/// <summary>Inicio de sesión.</summary>
[ApiController]
[Route("api/autenticacion")]
[Produces("application/json")]
public class AutenticacionController : ControllerBase
{
    /// <summary>Valida usuario y clave (monster / monster).</summary>
    /// <remarks>
    /// La clave viaja en el cuerpo (POST), nunca en la URL.
    /// 200 = credenciales correctas; 401 = usuario o clave incorrectos.
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(RespuestaLogin), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorServicio), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorServicio), StatusCodes.Status500InternalServerError)]
    public ActionResult<RespuestaLogin> Login([FromBody] SolicitudLogin solicitud)
    {
        try
        {
            if (Autenticador.Instancia.Validar(solicitud.Usuario, solicitud.Clave))
            {
                string usuario = solicitud.Usuario!.Trim();
                return Ok(new RespuestaLogin(true, usuario, $"Bienvenido, {usuario}."));
            }
            return Unauthorized(new ErrorServicio("CREDENCIALES_INVALIDAS", "Usuario o clave incorrectos."));
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorServicio("ERROR_SERVIDOR", ex.Message));
        }
    }
}
