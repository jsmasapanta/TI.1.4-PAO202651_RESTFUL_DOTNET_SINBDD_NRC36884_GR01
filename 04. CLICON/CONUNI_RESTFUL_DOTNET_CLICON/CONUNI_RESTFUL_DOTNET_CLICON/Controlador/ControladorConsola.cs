using System;
using System.Collections.Generic;
using System.Linq;
using ConversorUnidades.Consola.Modelo;
using ConversorUnidades.Consola.Servicio;
using ConversorUnidades.Consola.Vista;

namespace ConversorUnidades.Consola.Controlador
{
    /// <summary>
    /// Flujo de la aplicación: login → menú de categorías → conversión.
    /// </summary>
    public class ControladorConsola
    {
        private const int IntentosLogin = 3;

        private readonly VistaConsola _vista;
        private readonly ClienteConversion _cliente;

        public ControladorConsola(VistaConsola vista, ClienteConversion cliente)
        {
            _vista = vista;
            _cliente = cliente;
        }

        public void Iniciar()
        {
            if (!IniciarSesion())
            {
                return;
            }
            MenuPrincipal();
            _vista.MostrarEncabezado("Sesión finalizada. ¡Hasta pronto!");
        }

        private bool IniciarSesion()
        {
            for (int intento = 1; intento <= IntentosLogin; intento++)
            {
                _vista.MostrarEncabezado($"INICIO DE SESIÓN  (intento {intento} de {IntentosLogin})");
                _vista.MostrarServidor(_cliente.UrlServidor);

                string usuario = _vista.PedirTexto("Usuario");
                string clave = _vista.PedirClave("Clave");

                try
                {
                    if (_cliente.Login(usuario, clave))
                    {
                        _vista.MostrarExito($"Bienvenido, {usuario.Trim()}.");
                        _vista.Pausa();
                        return true;
                    }
                    _vista.MostrarError("Usuario o clave incorrectos.");
                }
                catch (ErrorConversionException ex)
                {
                    _vista.MostrarError(ex.Message);
                }
                _vista.Pausa();
            }

            _vista.MostrarEncabezado("ACCESO BLOQUEADO");
            _vista.MostrarError($"Se superaron los {IntentosLogin} intentos. La aplicación se cerrará.");
            _vista.Pausa();
            return false;
        }

        private void MenuPrincipal()
        {
            var categorias = (Categoria[])Enum.GetValues(typeof(Categoria));
            while (true)
            {
                _vista.MostrarEncabezado("MENÚ PRINCIPAL · Elija una categoría");
                int opcion = _vista.PedirOpcion(categorias.Select(c => c.ToString()).ToList(), "Cerrar sesión");
                if (opcion == 0)
                {
                    return;
                }
                MenuCategoria(categorias[opcion - 1]);
            }
        }

        private void MenuCategoria(Categoria categoria)
        {
            IList<Conversion> conversiones = CatalogoConversiones.DeCategoria(categoria);
            while (true)
            {
                _vista.MostrarEncabezado($"{categoria.ToString().ToUpperInvariant()} · Elija la conversión");
                int opcion = _vista.PedirOpcion(conversiones.Select(c => c.Descripcion).ToList(), "Volver");
                if (opcion == 0)
                {
                    return;
                }
                RealizarConversiones(conversiones[opcion - 1]);
            }
        }

        private void RealizarConversiones(Conversion conversion)
        {
            do
            {
                _vista.MostrarEncabezado(conversion.Descripcion);
                double valor = _vista.PedirNumero($"Valor en {conversion.UnidadOrigen} ({conversion.SimboloOrigen})");
                try
                {
                    double resultado = _cliente.Convertir(conversion.Ruta, valor);
                    _vista.MostrarResultado(conversion, valor, resultado);
                }
                catch (ErrorConversionException ex)
                {
                    Console.WriteLine();
                    _vista.MostrarError(ex.Message);
                }
            }
            while (_vista.PreguntarSiNo("¿Convertir otro valor?"));
        }
    }
}
