using System;

namespace ConversorUnidades.Servidor.Logica
{
    internal static class Validacion
    {
        public static void ValidarNumero(double valor)
        {
            if (double.IsNaN(valor) || double.IsInfinity(valor))
            {
                throw new ArgumentException("El valor ingresado no es un número válido.", nameof(valor));
            }
        }

        public static void ValidarNoNegativo(double valor, string magnitud)
        {
            ValidarNumero(valor);
            if (valor < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(valor),
                    $"La {magnitud} no puede ser negativa.");
            }
        }
    }
}
