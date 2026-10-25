using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ec.edu.monster.controlador;
using ec.edu.monster.modelo;
using ec.edu.monster.servicio;

namespace ec.edu.monster.vista
{
    /// <summary>
    /// Pantalla de conversión de una categoría. Se reutiliza para Temperatura, Longitud y Masa.
    /// </summary>
    public class PanelConversion : UserControl
    {
        private readonly Categoria _categoria;
        private readonly ControladorConversion _controlador;
        private readonly Color _color;

        private readonly ComboBox _cboOrigen;
        private readonly ComboBox _cboDestino;
        private readonly TextBox _txtValor;
        private readonly Button _btnConvertir;
        private readonly Label _lblResultado;
        private readonly Label _lblDetalle;
        private readonly ListBox _lstHistorial;

        public PanelConversion(Categoria categoria, ControladorConversion controlador)
        {
            _categoria = categoria;
            _controlador = controlador;
            _color = Tema.ColorDe(categoria);

            Dock = DockStyle.Fill;
            BackColor = Tema.Fondo;
            Font = Tema.Fuente(9.5f);

            var titulo = Tema.Etiqueta($"{categoria}  ·  {Tema.IconoDe(categoria)}", 18f, FontStyle.Bold, _color);
            titulo.Location = new Point(30, 22);
            var ayuda = Tema.Etiqueta("Elija las unidades, escriba el valor y presione Convertir (o Enter).", 9.5f, FontStyle.Regular, Tema.TextoSuave);
            ayuda.Location = new Point(33, 62);

            // ---------- Tarjeta de conversión ----------
            var tarjeta = new Panel { Location = new Point(30, 95), Size = new Size(560, 300), BackColor = Tema.Tarjeta };
            tarjeta.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, tarjeta.ClientRectangle, Tema.Borde, ButtonBorderStyle.Solid);

            var lblDe = Tema.Etiqueta("De", 9.5f, FontStyle.Bold);
            lblDe.Location = new Point(24, 20);
            _cboOrigen = CrearCombo(new Point(24, 44));

            var btnInvertir = Tema.Boton("⇄", _color);
            btnInvertir.Font = Tema.Fuente(14f, FontStyle.Bold);
            btnInvertir.Location = new Point(258, 40);
            btnInvertir.Size = new Size(44, 36);
            new ToolTip().SetToolTip(btnInvertir, "Intercambiar unidades");
            btnInvertir.Click += (s, e) => InvertirUnidades();

            var lblA = Tema.Etiqueta("A", 9.5f, FontStyle.Bold);
            lblA.Location = new Point(316, 20);
            _cboDestino = CrearCombo(new Point(316, 44));

            var lblValor = Tema.Etiqueta("Valor", 9.5f, FontStyle.Bold);
            lblValor.Location = new Point(24, 96);
            _txtValor = new TextBox { Location = new Point(24, 120), Width = 330, Font = Tema.Fuente(14f) };
            _txtValor.KeyDown += TxtValor_KeyDown;

            _btnConvertir = Tema.Boton("CONVERTIR", _color);
            _btnConvertir.Location = new Point(370, 118);
            _btnConvertir.Size = new Size(166, 36);
            _btnConvertir.Click += async (s, e) => await ConvertirAsync();

            var separador = new Label { Location = new Point(24, 176), Size = new Size(512, 1), BackColor = Tema.Borde };

            _lblResultado = new Label
            {
                Location = new Point(24, 192),
                Size = new Size(512, 52),
                Font = Tema.Fuente(22f, FontStyle.Bold),
                ForeColor = _color,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "—"
            };
            _lblDetalle = new Label
            {
                Location = new Point(24, 248),
                Size = new Size(512, 36),
                Font = Tema.Fuente(9.5f),
                ForeColor = Tema.TextoSuave,
                TextAlign = ContentAlignment.MiddleCenter
            };

            tarjeta.Controls.AddRange(new Control[] { lblDe, _cboOrigen, btnInvertir, lblA, _cboDestino, lblValor, _txtValor, _btnConvertir, separador, _lblResultado, _lblDetalle });

            // ---------- Historial ----------
            var lblHistorial = Tema.Etiqueta("Historial de la sesión", 10f, FontStyle.Bold);
            lblHistorial.Location = new Point(610, 95);
            _lstHistorial = new ListBox
            {
                Location = new Point(610, 122),
                Size = new Size(250, 232),
                BorderStyle = BorderStyle.FixedSingle,
                Font = Tema.Fuente(9f),
                IntegralHeight = false
            };
            var btnLimpiar = Tema.Boton("Limpiar historial", Tema.TextoSuave);
            btnLimpiar.Font = Tema.Fuente(9f, FontStyle.Bold);
            btnLimpiar.Location = new Point(610, 362);
            btnLimpiar.Size = new Size(250, 32);
            btnLimpiar.Click += (s, e) => _lstHistorial.Items.Clear();

            Controls.AddRange(new Control[] { titulo, ayuda, tarjeta, lblHistorial, _lstHistorial, btnLimpiar });

            _cboOrigen.SelectedIndexChanged += (s, e) => CargarDestinos(null);
            _cboOrigen.Items.AddRange(CatalogoConversiones.UnidadesOrigen(categoria).Cast<object>().ToArray());
            _cboOrigen.SelectedIndex = 0;
        }

        private static ComboBox CrearCombo(Point ubicacion)
        {
            return new ComboBox
            {
                Location = ubicacion,
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Tema.Fuente(10.5f)
            };
        }

        private Unidad Origen => _cboOrigen.SelectedItem as Unidad;
        private Unidad Destino => _cboDestino.SelectedItem as Unidad;

        /// <summary>Llena "A" con las unidades a las que se puede convertir desde "De".</summary>
        private void CargarDestinos(string simboloPreferido)
        {
            if (Origen == null)
            {
                return;
            }
            var destinos = CatalogoConversiones.UnidadesDestino(_categoria, Origen.Simbolo);
            _cboDestino.Items.Clear();
            _cboDestino.Items.AddRange(destinos.Cast<object>().ToArray());
            int indicePreferido = destinos.ToList().FindIndex(u => u.Simbolo == simboloPreferido);
            _cboDestino.SelectedIndex = indicePreferido >= 0 ? indicePreferido : 0;
            LimpiarResultado();
        }

        private void InvertirUnidades()
        {
            if (Origen == null || Destino == null)
            {
                return;
            }
            string nuevoOrigen = Destino.Simbolo;
            string nuevoDestino = Origen.Simbolo;
            Unidad origen = _cboOrigen.Items.Cast<Unidad>().FirstOrDefault(u => u.Simbolo == nuevoOrigen);
            if (origen == null)
            {
                return;
            }
            _cboOrigen.SelectedItem = origen;   // dispara CargarDestinos(null)
            CargarDestinos(nuevoDestino);
        }

        private async void TxtValor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await ConvertirAsync();
            }
        }

        private async System.Threading.Tasks.Task ConvertirAsync()
        {
            if (!_btnConvertir.Enabled || Origen == null || Destino == null)
            {
                return;
            }
            if (!Formato.IntentarLeerNumero(_txtValor.Text, out double valor))
            {
                MostrarError("Ingrese un número válido (ejemplo: 25 o 12.5).");
                _txtValor.Focus();
                return;
            }

            Conversion conversion = CatalogoConversiones.Buscar(Origen.Simbolo, Destino.Simbolo);
            _btnConvertir.Enabled = false;
            _btnConvertir.Text = "...";
            try
            {
                double resultado = await _controlador.ConvertirAsync(conversion, valor);
                string entrada = $"{Formato.Numero(valor)} {conversion.SimboloOrigen}";
                string salida = $"{Formato.Numero(resultado)} {conversion.SimboloDestino}";

                _lblResultado.ForeColor = _color;
                _lblResultado.Font = Tema.Fuente(22f, FontStyle.Bold);
                _lblResultado.Text = salida;
                _lblDetalle.ForeColor = Tema.TextoSuave;
                _lblDetalle.Text = $"{entrada} ({conversion.UnidadOrigen})  =  {salida} ({conversion.UnidadDestino})";
                _lstHistorial.Items.Insert(0, $"{DateTime.Now:HH:mm:ss}   {entrada} → {salida}");
            }
            catch (ErrorServicioException ex)
            {
                MostrarError(ex.Message);
            }
            finally
            {
                _btnConvertir.Enabled = true;
                _btnConvertir.Text = "CONVERTIR";
                _txtValor.Focus();
                _txtValor.SelectAll();
            }
        }

        private void MostrarError(string mensaje)
        {
            _lblResultado.ForeColor = Tema.Error;
            _lblResultado.Font = Tema.Fuente(11f, FontStyle.Bold);
            _lblResultado.Text = "No se pudo convertir";
            _lblDetalle.ForeColor = Tema.Error;
            _lblDetalle.Text = mensaje;
        }

        private void LimpiarResultado()
        {
            _lblResultado.ForeColor = _color;
            _lblResultado.Font = Tema.Fuente(22f, FontStyle.Bold);
            _lblResultado.Text = "—";
            _lblDetalle.Text = string.Empty;
        }

        public void EnfocarValor()
        {
            _txtValor.Focus();
        }
    }
}
