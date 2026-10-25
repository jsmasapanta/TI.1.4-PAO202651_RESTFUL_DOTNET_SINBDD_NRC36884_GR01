using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ec.edu.monster.controlador;
using ec.edu.monster.modelo;

namespace ec.edu.monster.vista
{
    /// <summary>
    /// Ventana principal: menú lateral con las 3 categorías y el panel de conversión a la derecha.
    /// Si el usuario cierra sesión, devuelve DialogResult.Retry para volver al login.
    /// </summary>
    public class FormPrincipal : Form
    {
        private readonly ControladorConversion _controlador;
        private readonly Panel _contenido;
        private readonly Dictionary<Categoria, PanelConversion> _paneles = new Dictionary<Categoria, PanelConversion>();
        private readonly Dictionary<Categoria, Button> _botonesMenu = new Dictionary<Categoria, Button>();

        public FormPrincipal(ControladorConversion controlador)
        {
            _controlador = controlador;

            Text = "Conversor de Unidades · RESTful .NET";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1110, 580);
            BackColor = Tema.Fondo;
            Font = Tema.Fuente(9.5f);
            AutoScaleMode = AutoScaleMode.Dpi;

            // ---------- Menú lateral ----------
            var menu = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = Tema.PrimarioOscuro };
            var marca = Tema.Etiqueta("⇄ Conversor", 16f, FontStyle.Bold, Color.White);
            marca.Location = new Point(22, 24);
            var usuario = Tema.Etiqueta("Usuario: " + _controlador.UsuarioActual, 9.5f, FontStyle.Regular, Color.FromArgb(190, 225, 215));
            usuario.Location = new Point(25, 62);
            menu.Controls.Add(marca);
            menu.Controls.Add(usuario);

            int y = 110;
            foreach (Categoria categoria in Enum.GetValues(typeof(Categoria)))
            {
                Categoria actual = categoria;
                var boton = new Button
                {
                    Text = $"   {Tema.IconoDe(categoria),-4} {categoria}",
                    TextAlign = ContentAlignment.MiddleLeft,
                    Location = new Point(0, y),
                    Size = new Size(220, 48),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    BackColor = Tema.PrimarioOscuro,
                    Font = Tema.Fuente(11f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                boton.FlatAppearance.BorderSize = 0;
                boton.FlatAppearance.MouseOverBackColor = Tema.Primario;
                boton.Click += (s, e) => MostrarCategoria(actual);
                menu.Controls.Add(boton);
                _botonesMenu[categoria] = boton;
                y += 52;
            }

            var imagen = new PictureBox
            {
                Location = new Point(15, y + 14),
                Size = new Size(190, 160),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Formato.Monster()
            };
            menu.Controls.Add(imagen);

            var btnSalir = Tema.Boton("Cerrar sesión", Color.FromArgb(160, 50, 40));
            btnSalir.Location = new Point(20, 480);
            btnSalir.Width = 180;
            btnSalir.Click += (s, e) =>
            {
                _controlador.CerrarSesion();
                DialogResult = DialogResult.Retry;
                Close();
            };
            menu.Controls.Add(btnSalir);

            // ---------- Barra de estado ----------
            var estado = new StatusStrip { BackColor = Color.White, SizingGrip = false };
            estado.Items.Add(new ToolStripStatusLabel("Conectado a: " + _controlador.UrlServidor) { ForeColor = Tema.TextoSuave });

            _contenido = new Panel { Dock = DockStyle.Fill, BackColor = Tema.Fondo };

            Controls.Add(_contenido);
            Controls.Add(menu);
            Controls.Add(estado);

            MostrarCategoria(Categoria.Temperatura);
        }

        private void MostrarCategoria(Categoria categoria)
        {
            if (!_paneles.TryGetValue(categoria, out PanelConversion panel))
            {
                panel = new PanelConversion(categoria, _controlador);
                _paneles[categoria] = panel;   // se conserva el historial al cambiar de categoría
            }

            _contenido.Controls.Clear();
            _contenido.Controls.Add(panel);

            foreach (var par in _botonesMenu)
            {
                bool activo = par.Key == categoria;
                par.Value.BackColor = activo ? Tema.ColorDe(par.Key) : Tema.PrimarioOscuro;
                par.Value.FlatAppearance.MouseOverBackColor = activo ? Tema.ColorDe(par.Key) : Tema.Primario;
            }
            panel.EnfocarValor();
        }
    }
}
