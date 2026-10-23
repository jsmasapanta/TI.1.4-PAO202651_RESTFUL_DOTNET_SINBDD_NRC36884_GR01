namespace ConversorUnidades.Servidor.Logica
{
    /// <summary>
    /// Fórmulas de masa con factores exactos (1 lb = 0.45359237 kg, 1 oz = 28.349523125 g).
    /// </summary>
    public static class ConvertidorMasa
    {
        public const double KilogramosPorLibra = 0.45359237;
        public const double GramosPorOnza = 28.349523125;

        public static double KilogramosALibras(double kilogramos)
        {
            Validacion.ValidarNoNegativo(kilogramos, "masa");
            return kilogramos / KilogramosPorLibra;
        }

        public static double LibrasAKilogramos(double libras)
        {
            Validacion.ValidarNoNegativo(libras, "masa");
            return libras * KilogramosPorLibra;
        }

        public static double GramosAOnzas(double gramos)
        {
            Validacion.ValidarNoNegativo(gramos, "masa");
            return gramos / GramosPorOnza;
        }

        public static double OnzasAGramos(double onzas)
        {
            Validacion.ValidarNoNegativo(onzas, "masa");
            return onzas * GramosPorOnza;
        }
    }
}
