using System;
using ConversorUnidades.Consola.Controlador;
using ConversorUnidades.Consola.Servicio;
using ConversorUnidades.Consola.Vista;

namespace ConversorUnidades.Consola
{
    internal static class Program
    {
        private static void Main()
        {
            var vista = new VistaConsola();
            try
            {
                var controlador = new ControladorConsola(vista, ClienteConversion.Instancia);
                controlador.Iniciar();
            }
            catch (Exception ex)
            {
                // Por ejemplo, App.config sin el endpoint "WSConversion".
                vista.MostrarError("Error inesperado: " + ex.Message);
                vista.Pausa();
            }
        }
    }
}
