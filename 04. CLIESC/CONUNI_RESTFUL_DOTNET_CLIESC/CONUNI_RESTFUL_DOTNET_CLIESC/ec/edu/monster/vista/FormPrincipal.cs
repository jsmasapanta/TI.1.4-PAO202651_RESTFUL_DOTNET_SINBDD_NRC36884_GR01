using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ec.edu.monster.controlador;
using ec.edu.monster.modelo;

namespace ec.edu.monster.vista
{
    /// <summary>
    /// Ventana principal con estilo Bento: barra superior y, debajo, la cuadrícula de tarjetas
    /// de la categoría elegida (PanelConversion). Si el usuario cierra sesión, devuelve
    /// DialogResult.Retry para volver al login.
    /// </summary>
    public class FormPrincipal : Form
    {
        private readonly ControladorConversion _controlador;
        private readonly Panel _contenido;
        private readonly Dictionary<Categoria, PanelConversion> _paneles = new Dictionary<Categoria, PanelConversion>();

        public FormPrincipal(ControladorConversion controlador)
        {
            _controlador = controlador;

            Text = "Monsters Inc. Converter · RESTful .NET";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1120, 650);
            BackColor = Tema.Fondo;
            Font = Tema.Fuente(9.5f);
            AutoScaleMode = AutoScaleMode.Dpi;

            // ---------- Barra superior ----------
            var logo = new TarjetaBento { Location = new Point(16, 14), Size = new Size(40, 40), Relleno = Tema.MoradoOscuro, Radio = 12 };
            var flecha = Tema.Etiqueta("⇄", 15f, FontStyle.Bold, Tema.Menta);
            flecha.Location = new Point(7, 5);
            logo.Controls.Add(flecha);

            var marca = Tema.Etiqueta("Monsters Inc. Converter", 15f, FontStyle.Bold, Tema.MoradoOscuro);
            marca.Location = new Point(64, 18);

            var btnSalir = Tema.Boton("Salir", Color.White, Tema.MoradoOscuro);
            btnSalir.FlatAppearance.BorderSize = 1;
            btnSalir.FlatAppearance.BorderColor = Tema.Borde;
            btnSalir.Size = new Size(86, 34);
            btnSalir.Location = new Point(ClientSize.Width - 16 - btnSalir.Width, 17);
            btnSalir.Click += (s, e) =>
            {
                _controlador.CerrarSesion();
                DialogResult = DialogResult.Retry;
                Close();
            };

            var usuario = Tema.Etiqueta(_controlador.UsuarioActual, 10f, FontStyle.Regular, Tema.TextoSuave);
            usuario.AutoSize = false;
            usuario.TextAlign = ContentAlignment.MiddleRight;
            usuario.Size = new Size(200, 34);
            usuario.Location = new Point(btnSalir.Left - 210, 17);

            _contenido = new Panel { Location = new Point(0, 64), Size = new Size(ClientSize.Width, ClientSize.Height - 64), BackColor = Tema.Fondo };

            Controls.AddRange(new Control[] { logo, marca, usuario, btnSalir, _contenido });

            MostrarCategoria(Categoria.Temperatura);
        }

        private void MostrarCategoria(Categoria categoria)
        {
            if (!_paneles.TryGetValue(categoria, out PanelConversion panel))
            {
                panel = new PanelConversion(categoria, _controlador, MostrarCategoria);
                _paneles[categoria] = panel;   // se conserva el historial al cambiar de categoría
            }

            _contenido.Controls.Clear();
            _contenido.Controls.Add(panel);
            panel.EnfocarValor();
        }
    }
}
