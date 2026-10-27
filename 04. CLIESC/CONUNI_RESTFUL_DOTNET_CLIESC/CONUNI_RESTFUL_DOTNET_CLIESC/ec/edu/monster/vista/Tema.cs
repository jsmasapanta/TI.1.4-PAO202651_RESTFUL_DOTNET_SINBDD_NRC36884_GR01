using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ec.edu.monster.modelo;

namespace ec.edu.monster.vista
{
    /// <summary>
    /// Colores, fuentes y controles del estilo Bento (los mismos que la web y el móvil):
    /// fondo cálido, tarjetas redondeadas y un color por categoría.
    /// </summary>
    internal static class Tema
    {
        public static readonly Color Fondo = Color.FromArgb(241, 239, 232);
        public static readonly Color Tarjeta = Color.White;
        public static readonly Color MoradoOscuro = Color.FromArgb(38, 33, 92);
        public static readonly Color Morado = Color.FromArgb(60, 52, 137);
        public static readonly Color MoradoClaro = Color.FromArgb(206, 203, 246);
        public static readonly Color CategoriaInactiva = Color.FromArgb(55, 51, 106);
        public static readonly Color Verde = Color.FromArgb(15, 110, 86);
        public static readonly Color VerdeClaro = Color.FromArgb(225, 245, 238);
        public static readonly Color Menta = Color.FromArgb(159, 225, 203);
        public static readonly Color Texto = Color.FromArgb(44, 44, 42);
        public static readonly Color TextoSuave = Color.FromArgb(111, 110, 104);
        public static readonly Color Borde = Color.FromArgb(227, 224, 214);
        public static readonly Color Error = Color.FromArgb(163, 45, 45);
        public static readonly Color ErrorClaro = Color.FromArgb(252, 235, 235);

        public static Font Fuente(float tamano, FontStyle estilo = FontStyle.Regular)
        {
            return new Font("Segoe UI", tamano, estilo);
        }

        public static Color ColorDe(Categoria categoria)
        {
            switch (categoria)
            {
                case Categoria.Temperatura: return Color.FromArgb(216, 90, 48);
                case Categoria.Longitud: return Color.FromArgb(24, 95, 165);
                default: return Color.FromArgb(83, 74, 183);
            }
        }

        public static Color ColorClaroDe(Categoria categoria)
        {
            switch (categoria)
            {
                case Categoria.Temperatura: return Color.FromArgb(250, 236, 231);
                case Categoria.Longitud: return Color.FromArgb(230, 241, 251);
                default: return Color.FromArgb(238, 237, 254);
            }
        }

        public static string IconoDe(Categoria categoria)
        {
            switch (categoria)
            {
                case Categoria.Temperatura: return "°C";
                case Categoria.Longitud: return "m";
                default: return "kg";
            }
        }

        public static Button Boton(string texto, Color fondo, Color? colorTexto = null)
        {
            var boton = new Button
            {
                Text = texto,
                BackColor = fondo,
                ForeColor = colorTexto ?? Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = Fuente(10f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Height = 38
            };
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(fondo, 0.05f);
            return boton;
        }

        public static Label Etiqueta(string texto, float tamano = 9.5f, FontStyle estilo = FontStyle.Regular, Color? color = null)
        {
            return new Label
            {
                Text = texto,
                AutoSize = true,
                Font = Fuente(tamano, estilo),
                ForeColor = color ?? Texto,
                BackColor = Color.Transparent
            };
        }
    }

    /// <summary>Tarjeta del diseño Bento: un panel con esquinas redondeadas y color de relleno.</summary>
    internal class TarjetaBento : Panel
    {
        private Color _relleno = Color.White;

        public int Radio { get; set; } = 18;

        public Color Relleno
        {
            get => _relleno;
            set
            {
                _relleno = value;
                Invalidate(true);
            }
        }

        public TarjetaBento()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            int d = Radio * 2;
            using (var camino = new GraphicsPath())
            using (var pincel = new SolidBrush(_relleno))
            {
                camino.AddArc(r.X, r.Y, d, d, 180, 90);
                camino.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                camino.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                camino.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                camino.CloseFigure();
                e.Graphics.FillPath(pincel, camino);
            }
            base.OnPaint(e);
        }
    }
}
