using System;
using System.Drawing;
using System.Windows.Forms;
using ConversorUnidades.Escritorio.Controlador;
using ConversorUnidades.Escritorio.Servicio;

namespace ConversorUnidades.Escritorio.Vista
{
    /// <summary>Ventana de inicio de sesión. Devuelve DialogResult.OK si el servidor acepta las credenciales.</summary>
    public class FormLogin : Form
    {
        private const int IntentosMaximos = 3;

        private readonly ControladorConversion _controlador;
        private readonly TextBox _txtUsuario;
        private readonly TextBox _txtClave;
        private readonly Button _btnIngresar;
        private readonly Label _lblMensaje;
        private int _intentosFallidos;

        public FormLogin(ControladorConversion controlador)
        {
            _controlador = controlador;

            Text = "Conversor de Unidades · Iniciar sesión";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(420, 470);
            BackColor = Tema.Fondo;
            Font = Tema.Fuente(9.5f);
            AutoScaleMode = AutoScaleMode.Dpi;

            var encabezado = new Panel { Dock = DockStyle.Top, Height = 130, BackColor = Tema.Primario };
            var icono = Tema.Etiqueta("⇄", 30f, FontStyle.Bold, Color.White);
            icono.Location = new Point(28, 30);
            var titulo = Tema.Etiqueta("Conversor de Unidades", 17f, FontStyle.Bold, Color.White);
            titulo.Location = new Point(92, 34);
            var subtitulo = Tema.Etiqueta("Cliente de escritorio · RESTful .NET", 9.5f, FontStyle.Regular, Color.FromArgb(200, 235, 225));
            subtitulo.Location = new Point(95, 72);
            encabezado.Controls.AddRange(new Control[] { icono, titulo, subtitulo });

            var lblUsuario = Tema.Etiqueta("Usuario", 9.5f, FontStyle.Bold);
            lblUsuario.Location = new Point(40, 160);
            _txtUsuario = new TextBox { Location = new Point(40, 184), Width = 340, Font = Tema.Fuente(11f) };

            var lblClave = Tema.Etiqueta("Clave", 9.5f, FontStyle.Bold);
            lblClave.Location = new Point(40, 226);
            _txtClave = new TextBox { Location = new Point(40, 250), Width = 340, Font = Tema.Fuente(11f), UseSystemPasswordChar = true };

            var chkMostrar = new CheckBox { Text = "Mostrar clave", Location = new Point(40, 284), AutoSize = true, ForeColor = Tema.TextoSuave };
            chkMostrar.CheckedChanged += (s, e) => _txtClave.UseSystemPasswordChar = !chkMostrar.Checked;

            _btnIngresar = Tema.Boton("INGRESAR", Tema.Primario);
            _btnIngresar.Location = new Point(40, 318);
            _btnIngresar.Width = 340;
            _btnIngresar.Click += BtnIngresar_Click;

            _lblMensaje = new Label
            {
                Location = new Point(40, 366),
                Size = new Size(340, 44),
                ForeColor = Tema.Error,
                Font = Tema.Fuente(9f),
                TextAlign = ContentAlignment.TopCenter
            };

            var lblServidor = new Label
            {
                Text = "Servidor: " + _controlador.UrlServidor,
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Tema.TextoSuave,
                Font = Tema.Fuente(8f)
            };

            Controls.AddRange(new Control[] { encabezado, lblUsuario, _txtUsuario, lblClave, _txtClave, chkMostrar, _btnIngresar, _lblMensaje, lblServidor });
            AcceptButton = _btnIngresar;
            Shown += (s, e) => _txtUsuario.Focus();
        }

        private async void BtnIngresar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtUsuario.Text) || string.IsNullOrEmpty(_txtClave.Text))
            {
                MostrarMensaje("Ingrese usuario y clave.");
                return;
            }

            CambiarEstadoEspera(true);
            try
            {
                bool valido = await _controlador.IniciarSesionAsync(_txtUsuario.Text, _txtClave.Text);
                if (valido)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }

                _intentosFallidos++;
                if (_intentosFallidos >= IntentosMaximos)
                {
                    MessageBox.Show(this, $"Se superaron los {IntentosMaximos} intentos. La aplicación se cerrará.",
                        "Acceso bloqueado", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    DialogResult = DialogResult.Abort;
                    Close();
                    return;
                }
                MostrarMensaje($"Usuario o clave incorrectos. Intentos restantes: {IntentosMaximos - _intentosFallidos}.");
                _txtClave.Clear();
                _txtClave.Focus();
            }
            catch (ErrorConversionException ex)
            {
                MostrarMensaje(ex.Message);
            }
            finally
            {
                if (!IsDisposed)
                {
                    CambiarEstadoEspera(false);
                }
            }
        }

        private void CambiarEstadoEspera(bool esperando)
        {
            _btnIngresar.Enabled = !esperando;
            _btnIngresar.Text = esperando ? "CONECTANDO..." : "INGRESAR";
            UseWaitCursor = esperando;
            if (esperando)
            {
                _lblMensaje.Text = string.Empty;
            }
        }

        private void MostrarMensaje(string mensaje)
        {
            _lblMensaje.Text = mensaje;
        }
    }
}
