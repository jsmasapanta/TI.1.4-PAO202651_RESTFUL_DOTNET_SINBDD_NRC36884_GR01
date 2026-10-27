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
    /// Cuadrícula Bento de una categoría (se reutiliza para Temperatura, Longitud y Masa):
    ///   [ categorías ][ categorías ][ resultado ][ resultado ]
    ///   [ conversión ][ conversión ][ resultado ][ resultado ]
    ///   [ conversión ][ conversión ][ historial ][  imagen   ]
    ///   [ contador   ][ servidor   ][ historial ][  imagen   ]
    /// </summary>
    public class PanelConversion : UserControl
    {
        private const int Margen = 16;
        private const int Separacion = 12;
        private const int AnchoColumna = 263;
        private static readonly int[] AltoFila = { 112, 150, 140, 112 };

        private readonly Categoria _categoria;
        private readonly ControladorConversion _controlador;
        private readonly Color _color;

        private readonly ComboBox _cboOrigen;
        private readonly ComboBox _cboDestino;
        private readonly TextBox _txtValor;
        private readonly Button _btnConvertir;
        private readonly TarjetaBento _celdaResultado;
        private readonly Label _lblTituloResultado;
        private readonly Label _lblResultado;
        private readonly Label _lblDetalle;
        private readonly ListBox _lstHistorial;
        private readonly Label _lblContador;

        public PanelConversion(Categoria categoria, ControladorConversion controlador, Action<Categoria> alElegirCategoria)
        {
            _categoria = categoria;
            _controlador = controlador;
            _color = Tema.ColorDe(categoria);

            Dock = DockStyle.Fill;
            BackColor = Tema.Fondo;
            Font = Tema.Fuente(9.5f);

            // ---------- Categorías ----------
            TarjetaBento celdaCategorias = Celda(0, 0, 2, 1, Tema.MoradoOscuro);
            var lblCategoria = Tema.Etiqueta("Categoría", 9f, FontStyle.Regular, Tema.MoradoClaro);
            lblCategoria.Location = new Point(18, 12);
            celdaCategorias.Controls.Add(lblCategoria);
            int x = 18;
            foreach (Categoria c in Enum.GetValues(typeof(Categoria)))
            {
                Categoria actual = c;
                bool activa = c == categoria;
                var boton = Tema.Boton($"{Tema.IconoDe(c)}\n{c}", activa ? Tema.ColorDe(c) : Tema.CategoriaInactiva);
                boton.TextAlign = ContentAlignment.MiddleLeft;
                boton.Padding = new Padding(8, 0, 0, 0);
                boton.Location = new Point(x, 38);
                boton.Size = new Size(156, 58);
                boton.Click += (s, e) => alElegirCategoria(actual);
                celdaCategorias.Controls.Add(boton);
                x += 164;
            }

            // ---------- Resultado (la tarjeta protagonista) ----------
            _celdaResultado = Celda(2, 0, 2, 2, Tema.ColorClaroDe(categoria));
            _lblTituloResultado = Tema.Etiqueta($"Resultado · {categoria}", 10f, FontStyle.Regular, _color);
            _lblTituloResultado.Location = new Point(22, 20);
            _lblResultado = new Label
            {
                Location = new Point(18, 62),
                Size = new Size(_celdaResultado.Width - 36, 100),
                Font = Tema.Fuente(40f, FontStyle.Bold),
                ForeColor = _color,
                BackColor = Color.Transparent,
                Text = "—"
            };
            _lblDetalle = new Label
            {
                Location = new Point(22, 170),
                Size = new Size(_celdaResultado.Width - 44, 70),
                Font = Tema.Fuente(10f),
                ForeColor = Tema.Texto,
                BackColor = Color.Transparent,
                Text = "Elija las unidades, escriba el valor y presione Convertir (o Enter)."
            };
            _celdaResultado.Controls.AddRange(new Control[] { _lblTituloResultado, _lblResultado, _lblDetalle });

            // ---------- Formulario de conversión ----------
            TarjetaBento celdaFormulario = Celda(0, 1, 2, 2, Tema.Tarjeta);
            var lblConversion = Tema.Etiqueta("Conversión", 9f, FontStyle.Regular, Tema.TextoSuave);
            lblConversion.Location = new Point(20, 14);

            var lblDe = Tema.Etiqueta("De", 9f, FontStyle.Bold, Tema.TextoSuave);
            lblDe.Location = new Point(20, 44);
            _cboOrigen = CrearCombo(new Point(20, 66));

            var btnInvertir = Tema.Boton("⇄", Tema.ColorClaroDe(categoria), _color);
            btnInvertir.Font = Tema.Fuente(14f, FontStyle.Bold);
            btnInvertir.Location = new Point(250, 61);
            btnInvertir.Size = new Size(36, 36);
            new ToolTip().SetToolTip(btnInvertir, "Intercambiar unidades");
            btnInvertir.Click += (s, e) => InvertirUnidades();

            var lblA = Tema.Etiqueta("A", 9f, FontStyle.Bold, Tema.TextoSuave);
            lblA.Location = new Point(298, 44);
            _cboDestino = CrearCombo(new Point(298, 66));

            var lblValor = Tema.Etiqueta("Valor", 9f, FontStyle.Bold, Tema.TextoSuave);
            lblValor.Location = new Point(20, 120);
            _txtValor = new TextBox { Location = new Point(20, 142), Width = 498, Font = Tema.Fuente(16f), BorderStyle = BorderStyle.FixedSingle };
            _txtValor.KeyDown += TxtValor_KeyDown;

            _btnConvertir = Tema.Boton("Convertir", _color);
            _btnConvertir.Font = Tema.Fuente(11f, FontStyle.Bold);
            _btnConvertir.Location = new Point(20, 200);
            _btnConvertir.Size = new Size(498, 44);
            _btnConvertir.Click += async (s, e) => await ConvertirAsync();

            celdaFormulario.Controls.AddRange(new Control[] { lblConversion, lblDe, _cboOrigen, btnInvertir, lblA, _cboDestino, lblValor, _txtValor, _btnConvertir });

            // ---------- Historial ----------
            TarjetaBento celdaHistorial = Celda(2, 2, 1, 2, Tema.Tarjeta);
            var lblHistorial = Tema.Etiqueta("Historial de la sesión", 9f, FontStyle.Regular, Tema.TextoSuave);
            lblHistorial.Location = new Point(16, 14);
            var btnLimpiar = new LinkLabel
            {
                Text = "Limpiar",
                AutoSize = true,
                Location = new Point(celdaHistorial.Width - 62, 14),
                LinkColor = Tema.TextoSuave,
                BackColor = Color.Transparent
            };
            btnLimpiar.LinkClicked += (s, e) =>
            {
                _lstHistorial.Items.Clear();
                ActualizarContador();
            };
            _lstHistorial = new ListBox
            {
                Location = new Point(16, 40),
                Size = new Size(celdaHistorial.Width - 32, celdaHistorial.Height - 56),
                BorderStyle = BorderStyle.None,
                Font = Tema.Fuente(9f),
                IntegralHeight = false
            };
            celdaHistorial.Controls.AddRange(new Control[] { lblHistorial, btnLimpiar, _lstHistorial });

            // ---------- Imagen de Monsters Inc. (completa, sin recortar) ----------
            TarjetaBento celdaImagen = Celda(3, 2, 1, 2, Tema.MoradoOscuro);
            var imagen = new PictureBox
            {
                Location = new Point(8, 8),
                Size = new Size(celdaImagen.Width - 16, celdaImagen.Height - 16),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Tema.MoradoOscuro,
                Image = Formato.Monster()
            };
            celdaImagen.Controls.Add(imagen);

            // ---------- Contador y servidor ----------
            TarjetaBento celdaContador = Celda(0, 3, 1, 1, Tema.VerdeClaro);
            var lblConversiones = Tema.Etiqueta("Conversiones", 9f, FontStyle.Regular, Tema.Verde);
            lblConversiones.Location = new Point(18, 14);
            _lblContador = Tema.Etiqueta("0", 26f, FontStyle.Bold, Tema.Verde);
            _lblContador.Location = new Point(14, 34);
            var lblEn = Tema.Etiqueta("en " + categoria.ToString().ToLowerInvariant(), 9f, FontStyle.Regular, Tema.Verde);
            lblEn.Location = new Point(18, 82);
            celdaContador.Controls.AddRange(new Control[] { lblConversiones, _lblContador, lblEn });

            TarjetaBento celdaServidor = Celda(1, 3, 1, 1, Tema.Tarjeta);
            var lblServidor = Tema.Etiqueta("Servidor REST", 9f, FontStyle.Regular, Tema.TextoSuave);
            lblServidor.Location = new Point(18, 14);
            var lblConectado = Tema.Etiqueta("●  Conectado", 10.5f, FontStyle.Bold, Tema.Texto);
            lblConectado.Location = new Point(16, 38);
            var lblUrl = new Label
            {
                Text = _controlador.UrlServidor,
                Location = new Point(18, 66),
                Size = new Size(celdaServidor.Width - 32, 40),
                Font = Tema.Fuente(8f),
                ForeColor = Tema.TextoSuave,
                BackColor = Color.Transparent
            };
            celdaServidor.Controls.AddRange(new Control[] { lblServidor, lblConectado, lblUrl });

            Controls.AddRange(new Control[] { celdaCategorias, _celdaResultado, celdaFormulario, celdaHistorial, celdaImagen, celdaContador, celdaServidor });

            _cboOrigen.SelectedIndexChanged += (s, e) => CargarDestinos(null);
            _cboOrigen.Items.AddRange(CatalogoConversiones.UnidadesOrigen(categoria).Cast<object>().ToArray());
            _cboOrigen.SelectedIndex = 0;
        }

        /// <summary>Crea una tarjeta que ocupa columnas y filas de la cuadrícula Bento.</summary>
        private static TarjetaBento Celda(int columna, int fila, int columnas, int filas, Color relleno)
        {
            int y = 0;
            for (int i = 0; i < fila; i++) y += AltoFila[i] + Separacion;
            int alto = (filas - 1) * Separacion;
            for (int i = fila; i < fila + filas; i++) alto += AltoFila[i];
            return new TarjetaBento
            {
                Location = new Point(Margen + columna * (AnchoColumna + Separacion), y),
                Size = new Size(columnas * AnchoColumna + (columnas - 1) * Separacion, alto),
                Relleno = relleno
            };
        }

        private static ComboBox CrearCombo(Point ubicacion)
        {
            return new ComboBox
            {
                Location = ubicacion,
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
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

                PintarResultado(error: false);
                _lblResultado.Text = salida;
                _lblDetalle.Text = $"{entrada} ({conversion.UnidadOrigen})  =  {salida} ({conversion.UnidadDestino})";
                _lstHistorial.Items.Insert(0, $"{DateTime.Now:HH:mm:ss}   {entrada} → {salida}");
                ActualizarContador();
            }
            catch (ErrorServicioException ex)
            {
                MostrarError(ex.Message);
            }
            finally
            {
                _btnConvertir.Enabled = true;
                _btnConvertir.Text = "Convertir";
                _txtValor.Focus();
                _txtValor.SelectAll();
            }
        }

        private void MostrarError(string mensaje)
        {
            PintarResultado(error: true);
            _lblResultado.Text = "No se pudo convertir";
            _lblDetalle.Text = mensaje;
        }

        private void LimpiarResultado()
        {
            PintarResultado(error: false);
            _lblResultado.Text = "—";
            _lblDetalle.Text = "Elija las unidades, escriba el valor y presione Convertir (o Enter).";
        }

        /// <summary>Tarjeta del resultado: color claro de la categoría, o rojo claro si el servidor rechazó el valor.</summary>
        private void PintarResultado(bool error)
        {
            Color fuerte = error ? Tema.Error : _color;
            _celdaResultado.Relleno = error ? Tema.ErrorClaro : Tema.ColorClaroDe(_categoria);
            _lblTituloResultado.ForeColor = fuerte;
            _lblResultado.ForeColor = fuerte;
            _lblResultado.Font = Tema.Fuente(error ? 18f : 40f, FontStyle.Bold);
            _lblResultado.Height = error ? 44 : 100;
            _lblDetalle.Top = error ? 110 : 170;
            _lblDetalle.ForeColor = error ? Tema.Error : Tema.Texto;
        }

        private void ActualizarContador()
        {
            _lblContador.Text = _lstHistorial.Items.Count.ToString();
        }

        public void EnfocarValor()
        {
            _txtValor.Focus();
        }
    }
}
