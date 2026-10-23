using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ConversorUnidades.Consola.Modelo;

namespace ConversorUnidades.Consola.Vista
{
    /// <summary>
    /// Todo lo que se lee o escribe en la consola. No conoce al servidor.
    /// </summary>
    public class VistaConsola
    {
        private const int Ancho = 56;

        public VistaConsola()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            Console.Title = "Conversor de Unidades - RESTful .NET";
        }

        public void MostrarEncabezado(string subtitulo)
        {
            if (!Console.IsOutputRedirected)
            {
                Console.Clear();
            }
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(new string('═', Ancho));
            Console.WriteLine(Centrar("CONVERSOR DE UNIDADES · RESTful .NET"));
            Console.WriteLine(Centrar("ESPE · Arquitectura de Software"));
            Console.WriteLine(new string('═', Ancho));
            Console.ResetColor();
            if (!string.IsNullOrEmpty(subtitulo))
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  " + subtitulo);
                Console.ResetColor();
            }
            Console.WriteLine();
        }

        public void MostrarServidor(string url)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Servidor: " + url);
            Console.ResetColor();
            Console.WriteLine();
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
            for (int i = 0; i < opciones.Count; i++)
            {
                Console.WriteLine($"   [{i + 1}] {opciones[i]}");
            }
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"   [0] {textoCero}");
            Console.ResetColor();
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

        /// <summary>Pide un número aceptando punto o coma decimal (12.5 o 12,5).</summary>
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
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ┌" + new string('─', Ancho - 4) + "┐");
            Console.WriteLine("  │" + Centrar($"{Formatear(valor)} {conversion.SimboloOrigen}  =  {Formatear(resultado)} {conversion.SimboloDestino}", Ancho - 4) + "│");
            Console.WriteLine("  └" + new string('─', Ancho - 4) + "┘");
            Console.ResetColor();
        }

        public void MostrarExito(string mensaje)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✔ " + mensaje);
            Console.ResetColor();
        }

        public void MostrarError(string mensaje)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ✖ " + mensaje);
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
