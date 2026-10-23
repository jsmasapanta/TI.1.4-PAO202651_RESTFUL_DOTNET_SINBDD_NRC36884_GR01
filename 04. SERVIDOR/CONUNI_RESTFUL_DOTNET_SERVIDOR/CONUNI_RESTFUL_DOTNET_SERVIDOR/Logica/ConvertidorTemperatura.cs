using System;
using System.Globalization;

namespace ConversorUnidades.Servidor.Logica
{
    /// <summary>
    /// Fórmulas de temperatura. Rechaza valores por debajo del cero absoluto
    /// (0 K = -273.15 °C = -459.67 °F), que no existen físicamente.
    /// </summary>
    public static class ConvertidorTemperatura
    {
        public const double CeroAbsolutoCelsius = -273.15;
        public const double CeroAbsolutoFahrenheit = -459.67;
        public const double CeroAbsolutoKelvin = 0.0;

        public static double CelsiusAFahrenheit(double celsius)
        {
            Validar(celsius, CeroAbsolutoCelsius, "°C");
            return celsius * 9.0 / 5.0 + 32.0;
        }

        public static double FahrenheitACelsius(double fahrenheit)
        {
            Validar(fahrenheit, CeroAbsolutoFahrenheit, "°F");
            return (fahrenheit - 32.0) * 5.0 / 9.0;
        }

        public static double CelsiusAKelvin(double celsius)
        {
            Validar(celsius, CeroAbsolutoCelsius, "°C");
            return celsius + 273.15;
        }

        public static double KelvinACelsius(double kelvin)
        {
            Validar(kelvin, CeroAbsolutoKelvin, "K");
            return kelvin - 273.15;
        }

        public static double FahrenheitAKelvin(double fahrenheit)
        {
            return CelsiusAKelvin(FahrenheitACelsius(fahrenheit));
        }

        public static double KelvinAFahrenheit(double kelvin)
        {
            return CelsiusAFahrenheit(KelvinACelsius(kelvin));
        }

        private static void Validar(double valor, double minimo, string unidad)
        {
            Validacion.ValidarNumero(valor);
            // Pequeña tolerancia para no rechazar -273.15 por errores de redondeo.
            if (valor < minimo - 1e-9)
            {
                throw new ArgumentOutOfRangeException(nameof(valor),
                    $"La temperatura no puede ser menor al cero absoluto ({minimo.ToString(CultureInfo.InvariantCulture)} {unidad}).");
            }
        }
    }
}
