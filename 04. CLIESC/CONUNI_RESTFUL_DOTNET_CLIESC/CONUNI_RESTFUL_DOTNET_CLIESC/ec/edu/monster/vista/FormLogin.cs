using System;
using System.Drawing;
using System.Windows.Forms;
using ec.edu.monster.controlador;
using ec.edu.monster.servicio;

namespace ec.edu.monster.vista
{
    /// <summary>
    /// Ventana de inicio de sesión con estilo Bento. Devuelve DialogResult.OK si el servidor acepta
    /// las credenciales. Los 3 intentos y el bloqueo los controla el servidor: aquí solo se muestra su mensaje.
    /// </summary>
    public class FormLogin : Form
    {
        private readonly ControladorConversion _controlador;
        private readonly TextBox _txtUsuario;
        private readonly TextBox _txtClave;
        private readonly TextBox _txtServidor;
        private readonly Button _btnIngresar;
        private readonly Label _lblMensaje;

        public FormLogin(ControladorConversion controlador)
        {
            _controlador = controlador;

            Text = "Monsters Inc. Converter · Iniciar sesión";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(860, 500);
            BackColor = Tema.Fondo;
            Font = Tema.Fuente(9.5f);
            AutoScaleMode = AutoScaleMode.Dpi;

            // ---------- Imagen (completa, sin recortar) ----------
            var celdaImagen = new TarjetaBento { Location = new Point(16, 16), Size = new Size(400, 468), Relleno = Tema.MoradoOscuro };
            celdaImagen.Controls.Add(new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(380, 448),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Tema.MoradoOscuro,
                Image = Formato.Monster()
            });

            // ---------- Marca ----------
            var celdaMarca = new TarjetaBento { Location = new Point(428, 16), Size = new Size(416, 100), Relleno = Tema.MoradoOscuro };
            var icono = Tema.Etiqueta("⇄", 26f, FontStyle.Bold, Tema.Menta);
            icono.Location = new Point(20, 22);
            var titulo = Tema.Etiqueta("Monsters Inc. Converter", 15f, FontStyle.Bold, Color.White);
            titulo.Location = new Point(80, 26);
            var subtitulo = Tema.Etiqueta("Cliente de escritorio · RESTful .NET", 9.5f, FontStyle.Regular, Tema.MoradoClaro);
            subtitulo.Location = new Point(82, 58);
            celdaMarca.Controls.AddRange(new Control[] { icono, titulo, subtitulo });

            // ---------- Formulario ----------
            var celdaFormulario = new TarjetaBento { Location = new Point(428, 128), Size = new Size(416, 290), Relleno = Tema.Tarjeta };
            var lblIniciar = Tema.Etiqueta("Iniciar sesión", 14f, FontStyle.Bold, Tema.Texto);
            lblIniciar.Location = new Point(24, 16);

            var lblUsuario = Tema.Etiqueta("Usuario", 9f, FontStyle.Bold, Tema.TextoSuave);
            lblUsuario.Location = new Point(26, 58);
            _txtUsuario = new TextBox { Location = new Point(26, 80), Width = 364, Font = Tema.Fuente(11f), BorderStyle = BorderStyle.FixedSingle };

            var lblClave = Tema.Etiqueta("Clave", 9f, FontStyle.Bold, Tema.TextoSuave);
            lblClave.Location = new Point(26, 118);
            _txtClave = new TextBox { Location = new Point(26, 140), Width = 364, Font = Tema.Fuente(11f), BorderStyle = BorderStyle.FixedSingle, UseSystemPasswordChar = true };

            var chkMostrar = new CheckBox { Text = "Mostrar clave", Location = new Point(26, 174), AutoSize = true, ForeColor = Tema.TextoSuave, BackColor = Color.Transparent };
            chkMostrar.CheckedChanged += (s, e) => _txtClave.UseSystemPasswordChar = !chkMostrar.Checked;

            _btnIngresar = Tema.Boton("Ingresar", Tema.Morado);
            _btnIngresar.Font = Tema.Fuente(11f, FontStyle.Bold);
            _btnIngresar.Location = new Point(26, 206);
            _btnIngresar.Size = new Size(364, 44);
            _btnIngresar.Click += BtnIngresar_Click;

            _lblMensaje = new Label
            {
                Location = new Point(26, 254),
                Size = new Size(364, 32),
                ForeColor = Tema.Error,
                BackColor = Color.Transparent,
                Font = Tema.Fuente(9f),
                TextAlign = ContentAlignment.TopCenter
            };
            celdaFormulario.Controls.AddRange(new Control[] { lblIniciar, lblUsuario, _txtUsuario, lblClave, _txtClave, chkMostrar, _btnIngresar, _lblMensaje });

            // ---------- Servidor (editable: IP de la PC donde corre el API) ----------
            var celdaServidor = new TarjetaBento { Location = new Point(428, 430), Size = new Size(416, 54), Relleno = Tema.VerdeClaro };
            var lblServidor = Tema.Etiqueta("Servidor REST", 8.5f, FontStyle.Bold, Tema.Verde);
            lblServidor.Location = new Point(16, 5);
            _txtServidor = new TextBox
            {
                Text = _controlador.UrlServidor,
                Location = new Point(18, 24),
                Width = 380,
                Font = Tema.Fuente(9.5f),
                BorderStyle = BorderStyle.FixedSingle
            };
            celdaServidor.Controls.AddRange(new Control[] { lblServidor, _txtServidor });

            Controls.AddRange(new Control[] { celdaImagen, celdaMarca, celdaFormulario, celdaServidor });
            AcceptButton = _btnIngresar;
            Shown += (s, e) => _txtUsuario.Focus();
        }

        private async void BtnIngresar_Click(object sender, EventArgs e)
        {
            CambiarEstadoEspera(true);
            try
            {
                _controlador.CambiarServidor(_txtServidor.Text);
                _txtServidor.Text = _controlador.UrlServidor;
                await _controlador.IniciarSesionAsync(_txtUsuario.Text, _txtClave.Text);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ErrorServicioException ex)
            {
                // Mensaje del servidor: clave incorrecta con intentos restantes, usuario bloqueado o falla de conexión.
                MostrarMensaje(ex.Message);
                _txtClave.Clear();
                _txtClave.Focus();
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
            _btnIngresar.Text = esperando ? "Conectando..." : "Ingresar";
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
