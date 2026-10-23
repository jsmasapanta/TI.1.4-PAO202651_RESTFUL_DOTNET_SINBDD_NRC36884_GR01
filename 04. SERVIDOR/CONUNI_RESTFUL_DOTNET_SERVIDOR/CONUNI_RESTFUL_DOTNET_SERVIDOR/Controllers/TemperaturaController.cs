using ConversorUnidades.Servidor.Logica;
using ConversorUnidades.Servidor.Modelo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ConversorUnidades.Servidor.Controllers;

/// <summary>Conversiones de temperatura. No acepta valores bajo el cero absoluto.</summary>
[Route("api/temperatura")]
public class TemperaturaController : ConversionControllerBase
{
    protected override string Categoria => "Temperatura";

    /// <summary>Celsius (°C) → Fahrenheit (°F)</summary>
    /// <param name="valor">Temperatura en °C (mínimo -273.15)</param>
    [HttpGet("celsiusAFahrenheit")]
    public ActionResult<ResultadoConversion> CelsiusAFahrenheit([FromQuery, BindRequired] double valor) =>
        Convertir("celsiusAFahrenheit", valor, ConvertidorTemperatura.CelsiusAFahrenheit, "°C", "°F");

    /// <summary>Fahrenheit (°F) → Celsius (°C)</summary>
    /// <param name="valor">Temperatura en °F (mínimo -459.67)</param>
    [HttpGet("fahrenheitACelsius")]
    public ActionResult<ResultadoConversion> FahrenheitACelsius([FromQuery, BindRequired] double valor) =>
        Convertir("fahrenheitACelsius", valor, ConvertidorTemperatura.FahrenheitACelsius, "°F", "°C");

    /// <summary>Celsius (°C) → Kelvin (K)</summary>
    /// <param name="valor">Temperatura en °C (mínimo -273.15)</param>
    [HttpGet("celsiusAKelvin")]
    public ActionResult<ResultadoConversion> CelsiusAKelvin([FromQuery, BindRequired] double valor) =>
        Convertir("celsiusAKelvin", valor, ConvertidorTemperatura.CelsiusAKelvin, "°C", "K");

    /// <summary>Kelvin (K) → Celsius (°C)</summary>
    /// <param name="valor">Temperatura en K (mínimo 0)</param>
    [HttpGet("kelvinACelsius")]
    public ActionResult<ResultadoConversion> KelvinACelsius([FromQuery, BindRequired] double valor) =>
        Convertir("kelvinACelsius", valor, ConvertidorTemperatura.KelvinACelsius, "K", "°C");

    /// <summary>Fahrenheit (°F) → Kelvin (K)</summary>
    /// <param name="valor">Temperatura en °F (mínimo -459.67)</param>
    [HttpGet("fahrenheitAKelvin")]
    public ActionResult<ResultadoConversion> FahrenheitAKelvin([FromQuery, BindRequired] double valor) =>
        Convertir("fahrenheitAKelvin", valor, ConvertidorTemperatura.FahrenheitAKelvin, "°F", "K");

    /// <summary>Kelvin (K) → Fahrenheit (°F)</summary>
    /// <param name="valor">Temperatura en K (mínimo 0)</param>
    [HttpGet("kelvinAFahrenheit")]
    public ActionResult<ResultadoConversion> KelvinAFahrenheit([FromQuery, BindRequired] double valor) =>
        Convertir("kelvinAFahrenheit", valor, ConvertidorTemperatura.KelvinAFahrenheit, "K", "°F");
}
