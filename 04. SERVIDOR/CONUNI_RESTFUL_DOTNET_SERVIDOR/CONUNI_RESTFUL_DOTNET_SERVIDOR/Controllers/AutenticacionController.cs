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
    /// <summary>Valida usuario y clave (monster / monster69).</summary>
    /// <remarks>
    /// La clave viaja en el cuerpo (POST), nunca en la URL.
    /// 200 = credenciales correctas; 401 = usuario o clave incorrectos (con los intentos restantes)
    /// o usuario bloqueado 2 minutos por superar 3 intentos; 400 = usuario o clave vacíos.
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(RespuestaLogin), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorServicio), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorServicio), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorServicio), StatusCodes.Status500InternalServerError)]
    public ActionResult<RespuestaLogin> Login([FromBody] SolicitudLogin solicitud)
    {
        try
        {
            Autenticador.Instancia.Validar(solicitud.Usuario, solicitud.Clave);
            string usuario = solicitud.Usuario!.Trim();
            return Ok(new RespuestaLogin(true, usuario, $"Bienvenido, {usuario}."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorServicio("VALOR_INVALIDO", ex.Message));
        }
        catch (ReglaNegocioException ex)
        {
            // Usuario o clave incorrectos (con los intentos restantes) o usuario bloqueado.
            return Unauthorized(new ErrorServicio("REGLA_NEGOCIO", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorServicio("ERROR_SERVIDOR", ex.Message));
        }
    }
}
