using System;
using System.Globalization;
using System.Threading.Tasks;
using ConversorUnidades.Escritorio.Modelo;
using ConversorUnidades.Escritorio.Servicio;

namespace ConversorUnidades.Escritorio.Controlador
{
    /// <summary>
    /// Intermediario entre las ventanas y el servicio REST.
    /// Las llamadas son asíncronas para que la ventana no se congele mientras responde el servidor.
    /// </summary>
    public class ControladorConversion
    {
        private readonly ClienteConversion _cliente = ClienteConversion.Instancia;

        public string UrlServidor => _cliente.UrlServidor;

        public string UsuarioActual { get; private set; }

        public async Task<bool> IniciarSesionAsync(string usuario, string clave)
        {
            bool valido = await Task.Run(() => _cliente.Login(usuario, clave));
            if (valido)
            {
                UsuarioActual = usuario.Trim();
            }
            return valido;
        }

        public void CerrarSesion()
        {
            UsuarioActual = null;
        }

        public Task<double> ConvertirAsync(Conversion conversion, double valor)
        {
            return Task.Run(() => _cliente.Convertir(conversion.Ruta, valor));
        }

        /// <summary>Acepta punto o coma decimal.</summary>
        public static bool IntentarLeerNumero(string texto, out double valor)
        {
            string normalizado = (texto ?? string.Empty).Trim().Replace(',', '.');
            return double.TryParse(normalizado, NumberStyles.Float, CultureInfo.InvariantCulture, out valor)
                && !double.IsNaN(valor) && !double.IsInfinity(valor);
        }

        /// <summary>Hasta 6 decimales, sin ceros sobrantes.</summary>
        public static string Formatear(double numero)
        {
            return Math.Round(numero, 6).ToString("0.######", CultureInfo.InvariantCulture);
        }
    }
}
