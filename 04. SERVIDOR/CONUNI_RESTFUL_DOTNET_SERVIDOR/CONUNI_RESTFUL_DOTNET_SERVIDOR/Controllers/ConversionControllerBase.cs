using ConversorUnidades.Servidor.Modelo;
using Microsoft.AspNetCore.Mvc;

namespace ConversorUnidades.Servidor.Controllers;

/// <summary>
/// Base de los controladores de conversión. Solo llama a la capa Logica y
/// traduce los errores de validación a HTTP 400 con un ErrorServicio en JSON.
/// </summary>
[ApiController]
[Produces("application/json")]
[ProducesResponseType(typeof(ResultadoConversion), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorServicio), StatusCodes.Status400BadRequest)]
public abstract class ConversionControllerBase : ControllerBase
{
    /// <summary>Nombre de la categoría que aparece en la respuesta (Temperatura, Longitud o Masa).</summary>
    protected abstract string Categoria { get; }

    protected ActionResult<ResultadoConversion> Convertir(
        string operacion, double valor, Func<double, double> formula, string unidadOrigen, string unidadDestino)
    {
        try
        {
            double resultado = formula(valor);
            return Ok(new ResultadoConversion(Categoria, operacion, valor, unidadOrigen, unidadDestino, resultado));
        }
        catch (ArgumentException ex)
        {
            // ArgumentOutOfRangeException agrega "(Parameter 'valor')" al mensaje; lo quitamos.
            string mensaje = ex.Message.Split(" (Parameter", StringSplitOptions.None)[0];
            return BadRequest(new ErrorServicio("VALOR_INVALIDO", mensaje));
        }
    }
}
