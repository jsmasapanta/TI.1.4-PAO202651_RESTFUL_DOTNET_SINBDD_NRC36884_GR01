using System;
using System.Windows.Forms;
using ec.edu.monster.controlador;
using ec.edu.monster.vista;

namespace ec.edu.monster
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ControladorConversion controlador;
            try
            {
                controlador = new ControladorConversion();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo leer la configuración del servidor en App.config:\n" + ex.Message,
                    "Conversor de Unidades", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Login → Principal. "Cerrar sesión" vuelve al login; cerrar la ventana termina la aplicación.
            while (true)
            {
                using (var login = new FormLogin(controlador))
                {
                    if (login.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }
                }
                using (var principal = new FormPrincipal(controlador))
                {
                    if (principal.ShowDialog() != DialogResult.Retry)
                    {
                        return;
                    }
                }
            }
        }
    }
}
