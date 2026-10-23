namespace ConversorUnidades.Servidor.Logica
{
    /// <summary>
    /// Fórmulas de longitud con los factores exactos del acuerdo internacional de 1959.
    /// </summary>
    public static class ConvertidorLongitud
    {
        public const double MetrosPorPie = 0.3048;
        public const double MetrosPorPulgada = 0.0254;
        public const double KilometrosPorMilla = 1.609344;

        public static double MetrosAPies(double metros)
        {
            Validacion.ValidarNoNegativo(metros, "longitud");
            return metros / MetrosPorPie;
        }

        public static double PiesAMetros(double pies)
        {
            Validacion.ValidarNoNegativo(pies, "longitud");
            return pies * MetrosPorPie;
        }

        public static double MetrosAPulgadas(double metros)
        {
            Validacion.ValidarNoNegativo(metros, "longitud");
            return metros / MetrosPorPulgada;
        }

        public static double PulgadasAMetros(double pulgadas)
        {
            Validacion.ValidarNoNegativo(pulgadas, "longitud");
            return pulgadas * MetrosPorPulgada;
        }

        public static double KilometrosAMillas(double kilometros)
        {
            Validacion.ValidarNoNegativo(kilometros, "longitud");
            return kilometros / KilometrosPorMilla;
        }

        public static double MillasAKilometros(double millas)
        {
            Validacion.ValidarNoNegativo(millas, "longitud");
            return millas * KilometrosPorMilla;
        }
    }
}
