using System;
using System.Drawing;
using System.Globalization;
using System.Reflection;

namespace ec.edu.monster.vista
{
    /// <summary>Cómo se muestran los datos en pantalla. No tiene reglas: los rangos los valida el servidor.</summary>
    public static class Formato
    {
        /// <summary>
        /// Convierte lo escrito (punto o coma decimal) al número que recibe el servicio. Solo comprueba que
        /// sea un número para poder enviarlo; si está fuera de rango lo rechaza el servidor.
        /// </summary>
        public static bool IntentarLeerNumero(string texto, out double valor)
        {
            string normalizado = (texto ?? string.Empty).Trim().Replace(',', '.');
            return double.TryParse(normalizado, NumberStyles.Float, CultureInfo.InvariantCulture, out valor)
                && !double.IsNaN(valor) && !double.IsInfinity(valor);
        }

        /// <summary>Hasta 6 decimales, sin ceros sobrantes.</summary>
        public static string Numero(double numero) =>
            Math.Round(numero, 6).ToString("0.######", CultureInfo.InvariantCulture);

        /// <summary>Imagen de Monsters Inc. incrustada en el ejecutable (Recursos/monster.jpg).</summary>
        public static Image Monster()
        {
            using (var flujo = Assembly.GetExecutingAssembly().GetManifestResourceStream("monster.jpg"))
            {
                return flujo == null ? null : new Bitmap(Image.FromStream(flujo));
            }
        }
    }
}
