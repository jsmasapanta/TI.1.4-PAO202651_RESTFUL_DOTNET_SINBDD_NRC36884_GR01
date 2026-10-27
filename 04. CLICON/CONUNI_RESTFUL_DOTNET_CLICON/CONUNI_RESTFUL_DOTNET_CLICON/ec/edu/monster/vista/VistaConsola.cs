using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ec.edu.monster.controlador;
using ec.edu.monster.modelo;
using ec.edu.monster.servicio;

namespace ec.edu.monster.vista
{
    /// <summary>
    /// Pantallas de la consola: pide los datos, se los pasa al controlador y muestra lo que devuelve
    /// el servidor (el resultado o su mensaje de error). No valida rangos ni cuenta intentos de login.
    /// </summary>
    public class VistaConsola
    {
        private const int Ancho = 64;

        // Estilo terminal retro: verde sobre negro, marcos dobles y título MONSTER en grande.
        private const ConsoleColor Principal = ConsoleColor.Green;
        private const ConsoleColor Marco = ConsoleColor.DarkGreen;

        private static readonly string[] Titulo =
        {
            "███╗   ███╗ ██████╗ ███╗   ██╗███████╗████████╗███████╗██████╗ ",
            "████╗ ████║██╔═══██╗████╗  ██║██╔════╝╚══██╔══╝██╔════╝██╔══██╗",
            "██╔████╔██║██║   ██║██╔██╗ ██║███████╗   ██║   █████╗  ██████╔╝",
            "██║╚██╔╝██║██║   ██║██║╚██╗██║╚════██║   ██║   ██╔══╝  ██╔══██╗",
            "██║ ╚═╝ ██║╚██████╔╝██║ ╚████║███████║   ██║   ███████╗██║  ██║",
            "╚═╝     ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝   ╚═╝   ╚══════╝╚═╝  ╚═╝",
        };

        private readonly ControladorConsola _controlador = new ControladorConsola();

        public VistaConsola()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            Console.Title = "MONSTERS INC · Conversor de Unidades - RESTful .NET";
            if (!Console.IsOutputRedirected)
            {
                Console.BackgroundColor = ConsoleColor.Black;
            }
        }

        // ================= Flujo de pantallas =================

        public void Iniciar()
        {
            if (!PantallaLogin())
            {
                return;
            }
            PantallaMenu();
            MostrarEncabezado("Sesión finalizada. ¡Hasta pronto!");
        }

        /// <summary>El servidor decide si el login es correcto, cuenta los intentos y bloquea al usuario.</summary>
        private bool PantallaLogin()
        {
            while (true)
            {
                MostrarEncabezado("INICIO DE SESIÓN");
                MostrarServidor(_controlador.UrlServidor);
                string usuario = PedirTexto("Usuario");
                string clave = PedirClave("Clave");
                try
                {
                    _controlador.IniciarSesion(usuario, clave);
                    MostrarExito($"Bienvenido, {_controlador.Usuario}.");
                    Pausa();
                    return true;
                }
                catch (ErrorServicioException ex)
                {
                    MostrarError(ex.Message);
                }
                if (!PreguntarSiNo("¿Intentar de nuevo?"))
                {
                    return false;
                }
            }
        }

        private void PantallaMenu()
        {
            var categorias = (Categoria[])Enum.GetValues(typeof(Categoria));
            while (true)
            {
                MostrarEncabezado($"MENÚ PRINCIPAL · {_controlador.Usuario} · Elija una categoría");
                int opcion = PedirOpcion(categorias.Select(c => c.ToString()).ToList(), "Cerrar sesión");
                if (opcion == 0)
                {
                    _controlador.CerrarSesion();
                    return;
                }
                PantallaCategoria(categorias[opcion - 1]);
            }
        }

        private void PantallaCategoria(Categoria categoria)
        {
            IList<Conversion> conversiones = CatalogoConversiones.DeCategoria(categoria);
            while (true)
            {
                MostrarEncabezado($"{categoria.ToString().ToUpperInvariant()} · Elija la conversión");
                int opcion = PedirOpcion(conversiones.Select(CatalogoConversiones.Descripcion).ToList(), "Volver");
                if (opcion == 0)
                {
                    return;
                }
                PantallaConversion(conversiones[opcion - 1]);
            }
        }

        private void PantallaConversion(Conversion conversion)
        {
            do
            {
                MostrarEncabezado(CatalogoConversiones.Descripcion(conversion));
                double valor = PedirNumero($"Valor en {conversion.UnidadOrigen} ({conversion.SimboloOrigen})");
                try
                {
                    double resultado = _controlador.Convertir(conversion, valor);
                    MostrarResultado(conversion, valor, resultado);
                }
                catch (ErrorServicioException ex)
                {
                    // Por ejemplo: «La temperatura no puede ser menor al cero absoluto…» (lo valida el servidor).
                    Console.WriteLine();
                    MostrarError(ex.Message);
                }
            }
            while (PreguntarSiNo("¿Convertir otro valor?"));
        }

        // ================= Entrada y salida =================

        public void MostrarEncabezado(string subtitulo)
        {
            if (!Console.IsOutputRedirected)
            {
                Console.Clear();
            }
            Console.WriteLine();
            Console.ForegroundColor = Principal;
            foreach (string linea in Titulo)
            {
                Console.WriteLine(" " + linea);
            }

            string usuario = string.IsNullOrEmpty(_controlador.Usuario) ? "—" : _controlador.Usuario;
            AbrirCaja();
            LineaCaja(Centrar("MONSTERS INC · CONVERSOR DE UNIDADES · RESTful .NET", Ancho - 4), Principal);
            LineaCaja($"usuario: {usuario}   srv: {Servidor(_controlador.UrlServidor)}", ConsoleColor.Gray);
            CerrarCaja();
            if (!string.IsNullOrEmpty(subtitulo))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(" > " + subtitulo);
                Console.ResetColor();
            }
            Console.WriteLine();
        }

        public void MostrarServidor(string url)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("   Servidor: " + url);
            Console.ResetColor();
            Console.WriteLine();
        }

        // ================= Marco retro =================

        private static void AbrirCaja()
        {
            Console.ForegroundColor = Marco;
            Console.WriteLine(" ╔" + new string('═', Ancho - 2) + "╗");
        }

        private static void SeparadorCaja()
        {
            Console.ForegroundColor = Marco;
            Console.WriteLine(" ╠" + new string('═', Ancho - 2) + "╣");
        }

        private static void CerrarCaja()
        {
            Console.ForegroundColor = Marco;
            Console.WriteLine(" ╚" + new string('═', Ancho - 2) + "╝");
            Console.ResetColor();
        }

        /// <summary>Escribe una línea dentro del marco ║ … ║, rellenando hasta el ancho.</summary>
        private static void LineaCaja(string texto, ConsoleColor color)
        {
            int espacio = Ancho - 4;
            if (texto.Length > espacio) texto = texto.Substring(0, espacio);
            Console.ForegroundColor = Marco;
            Console.Write(" ║ ");
            Console.ForegroundColor = color;
            Console.Write(texto.PadRight(espacio));
            Console.ForegroundColor = Marco;
            Console.WriteLine(" ║");
            Console.ResetColor();
        }

        /// <summary>Solo host:puerto de la URL, para que quepa en el marco.</summary>
        private static string Servidor(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uri) ? $"{uri.Host}:{uri.Port}" : url;
        }

        public string PedirTexto(string etiqueta)
        {
            Console.Write("  " + etiqueta + ": ");
            return Console.ReadLine() ?? string.Empty;
        }

        /// <summary>Lee la clave mostrando asteriscos.</summary>
        public string PedirClave(string etiqueta)
        {
            Console.Write("  " + etiqueta + ": ");
            if (Console.IsInputRedirected)
            {
                // Entrada desde archivo o pruebas automáticas: no hay teclado para leer tecla por tecla.
                string linea = Console.ReadLine() ?? string.Empty;
                Console.WriteLine();
                return linea;
            }
            var clave = new StringBuilder();
            while (true)
            {
                ConsoleKeyInfo tecla = Console.ReadKey(true);
                if (tecla.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return clave.ToString();
                }
                if (tecla.Key == ConsoleKey.Backspace)
                {
                    if (clave.Length > 0)
                    {
                        clave.Length--;
                        Console.Write("\b \b");
                    }
                }
                else if (!char.IsControl(tecla.KeyChar))
                {
                    clave.Append(tecla.KeyChar);
                    Console.Write('*');
                }
            }
        }

        /// <summary>Muestra opciones numeradas y devuelve la elegida (0 = volver/salir).</summary>
        public int PedirOpcion(IList<string> opciones, string textoCero)
        {
            AbrirCaja();
            for (int i = 0; i < opciones.Count; i++)
            {
                LineaCaja($"[{i + 1}] {opciones[i]}", Principal);
            }
            SeparadorCaja();
            LineaCaja($"[0] {textoCero}", ConsoleColor.DarkGray);
            CerrarCaja();
            Console.WriteLine();

            while (true)
            {
                string entrada = PedirTexto("Opción");
                if (int.TryParse(entrada, out int opcion) && opcion >= 0 && opcion <= opciones.Count)
                {
                    return opcion;
                }
                MostrarError($"Opción inválida. Escriba un número entre 0 y {opciones.Count}.");
            }
        }

        /// <summary>
        /// Pide un número aceptando punto o coma decimal (12.5 o 12,5). Solo comprueba que sea un número
        /// para poder enviarlo; si está fuera de rango lo rechaza el servidor.
        /// </summary>
        public double PedirNumero(string etiqueta)
        {
            while (true)
            {
                string entrada = PedirTexto(etiqueta).Trim().Replace(',', '.');
                if (double.TryParse(entrada, NumberStyles.Float, CultureInfo.InvariantCulture, out double valor)
                    && !double.IsNaN(valor) && !double.IsInfinity(valor))
                {
                    return valor;
                }
                MostrarError("Ingrese un número válido (ejemplo: 25 o 12.5).");
            }
        }

        public void MostrarResultado(Conversion conversion, double valor, double resultado)
        {
            Console.WriteLine();
            AbrirCaja();
            LineaCaja($"> {Formatear(valor)} {conversion.SimboloOrigen}  ({conversion.UnidadOrigen})", ConsoleColor.Gray);
            LineaCaja($"= {Formatear(resultado)} {conversion.SimboloDestino}  ({conversion.UnidadDestino})  ✔", ConsoleColor.White);
            CerrarCaja();
        }

        public void MostrarExito(string mensaje)
        {
            Console.ForegroundColor = Principal;
            Console.WriteLine("   ✔ " + mensaje);
            Console.ResetColor();
        }

        public void MostrarError(string mensaje)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("   ✖ " + mensaje);
            Console.ResetColor();
        }

        public bool PreguntarSiNo(string pregunta)
        {
            Console.WriteLine();
            string r = PedirTexto(pregunta + " (s/n)").Trim().ToLowerInvariant();
            return r == "s" || r == "si" || r == "sí";
        }

        public void Pausa()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  Presione una tecla para continuar...");
            Console.ResetColor();
            if (Console.IsInputRedirected)
            {
                Console.ReadLine();
                Console.WriteLine();
                return;
            }
            Console.ReadKey(true);
        }

        /// <summary>Hasta 6 decimales, sin ceros sobrantes, con punto decimal.</summary>
        public static string Formatear(double numero)
        {
            return Math.Round(numero, 6).ToString("0.######", CultureInfo.InvariantCulture);
        }

        private static string Centrar(string texto, int ancho = Ancho)
        {
            if (texto.Length >= ancho) return texto;
            int izquierda = (ancho - texto.Length) / 2;
            return new string(' ', izquierda) + texto + new string(' ', ancho - texto.Length - izquierda);
        }
    }
}
