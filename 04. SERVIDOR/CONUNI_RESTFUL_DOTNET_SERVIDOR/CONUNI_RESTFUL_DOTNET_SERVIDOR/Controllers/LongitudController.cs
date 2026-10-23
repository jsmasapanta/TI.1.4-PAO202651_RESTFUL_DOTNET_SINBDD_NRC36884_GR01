using ConversorUnidades.Servidor.Logica;
using ConversorUnidades.Servidor.Modelo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ConversorUnidades.Servidor.Controllers;

/// <summary>Conversiones de longitud. No acepta valores negativos.</summary>
[Route("api/longitud")]
public class LongitudController : ConversionControllerBase
{
    protected override string Categoria => "Longitud";

    /// <summary>Metros (m) → Pies (ft)</summary>
    [HttpGet("metrosAPies")]
    public ActionResult<ResultadoConversion> MetrosAPies([FromQuery, BindRequired] double valor) =>
        Convertir("metrosAPies", valor, ConvertidorLongitud.MetrosAPies, "m", "ft");

    /// <summary>Pies (ft) → Metros (m)</summary>
    [HttpGet("piesAMetros")]
    public ActionResult<ResultadoConversion> PiesAMetros([FromQuery, BindRequired] double valor) =>
        Convertir("piesAMetros", valor, ConvertidorLongitud.PiesAMetros, "ft", "m");

    /// <summary>Metros (m) → Pulgadas (in)</summary>
    [HttpGet("metrosAPulgadas")]
    public ActionResult<ResultadoConversion> MetrosAPulgadas([FromQuery, BindRequired] double valor) =>
        Convertir("metrosAPulgadas", valor, ConvertidorLongitud.MetrosAPulgadas, "m", "in");

    /// <summary>Pulgadas (in) → Metros (m)</summary>
    [HttpGet("pulgadasAMetros")]
    public ActionResult<ResultadoConversion> PulgadasAMetros([FromQuery, BindRequired] double valor) =>
        Convertir("pulgadasAMetros", valor, ConvertidorLongitud.PulgadasAMetros, "in", "m");

    /// <summary>Kilómetros (km) → Millas (mi)</summary>
    [HttpGet("kilometrosAMillas")]
    public ActionResult<ResultadoConversion> KilometrosAMillas([FromQuery, BindRequired] double valor) =>
        Convertir("kilometrosAMillas", valor, ConvertidorLongitud.KilometrosAMillas, "km", "mi");

    /// <summary>Millas (mi) → Kilómetros (km)</summary>
    [HttpGet("millasAKilometros")]
    public ActionResult<ResultadoConversion> MillasAKilometros([FromQuery, BindRequired] double valor) =>
        Convertir("millasAKilometros", valor, ConvertidorLongitud.MillasAKilometros, "mi", "km");
}
