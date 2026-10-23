using ConversorUnidades.Servidor.Logica;
using ConversorUnidades.Servidor.Modelo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ConversorUnidades.Servidor.Controllers;

/// <summary>Conversiones de masa. No acepta valores negativos.</summary>
[Route("api/masa")]
public class MasaController : ConversionControllerBase
{
    protected override string Categoria => "Masa";

    /// <summary>Kilogramos (kg) → Libras (lb)</summary>
    [HttpGet("kilogramosALibras")]
    public ActionResult<ResultadoConversion> KilogramosALibras([FromQuery, BindRequired] double valor) =>
        Convertir("kilogramosALibras", valor, ConvertidorMasa.KilogramosALibras, "kg", "lb");

    /// <summary>Libras (lb) → Kilogramos (kg)</summary>
    [HttpGet("librasAKilogramos")]
    public ActionResult<ResultadoConversion> LibrasAKilogramos([FromQuery, BindRequired] double valor) =>
        Convertir("librasAKilogramos", valor, ConvertidorMasa.LibrasAKilogramos, "lb", "kg");

    /// <summary>Gramos (g) → Onzas (oz)</summary>
    [HttpGet("gramosAOnzas")]
    public ActionResult<ResultadoConversion> GramosAOnzas([FromQuery, BindRequired] double valor) =>
        Convertir("gramosAOnzas", valor, ConvertidorMasa.GramosAOnzas, "g", "oz");

    /// <summary>Onzas (oz) → Gramos (g)</summary>
    [HttpGet("onzasAGramos")]
    public ActionResult<ResultadoConversion> OnzasAGramos([FromQuery, BindRequired] double valor) =>
        Convertir("onzasAGramos", valor, ConvertidorMasa.OnzasAGramos, "oz", "g");
}
