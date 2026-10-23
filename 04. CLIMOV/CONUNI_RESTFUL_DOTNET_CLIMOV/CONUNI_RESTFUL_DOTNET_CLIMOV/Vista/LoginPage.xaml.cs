using ConversorUnidades.Movil.Controlador;
using ConversorUnidades.Movil.Servicio;

namespace ConversorUnidades.Movil.Vista;

/// <summary>Pantalla de inicio de sesión (CU01) y configuración del servidor (CU08).</summary>
public partial class LoginPage : ContentPage
{
    private readonly ControladorConversion _controlador = ControladorConversion.Instancia;
    private int _intentos;

    public LoginPage()
    {
        InitializeComponent();
        TxtUrl.Text = _controlador.UrlServidor;
        MostrarIntentos();
    }

    private async void OnIngresarClicked(object? sender, EventArgs e)
    {
        string usuario = TxtUsuario.Text ?? "";
        string clave = TxtClave.Text ?? "";
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrEmpty(clave))
        {
            MostrarError("Ingrese usuario y clave.");
            return;
        }
        if (string.IsNullOrWhiteSpace(TxtUrl.Text))
        {
            MostrarError("Ingrese la URL del servidor.");
            return;
        }

        _controlador.UrlServidor = TxtUrl.Text;
        Ocupado(true);
        try
        {
            if (await _controlador.IniciarSesionAsync(usuario, clave))
            {
                Window.Page = new PrincipalPage();
                return;
            }

            _intentos++;
            if (_intentos >= ControladorConversion.MaximoIntentos)
            {
                await DisplayAlert("Acceso bloqueado",
                    $"Superó los {ControladorConversion.MaximoIntentos} intentos permitidos. La aplicación se cerrará.", "Aceptar");
                Application.Current?.Quit();
                return;
            }
            TxtClave.Text = "";
            MostrarError("Usuario o clave incorrectos.");
            MostrarIntentos();
        }
        catch (ErrorConversionException ex)
        {
            // Un error de conexión no cuenta como intento fallido.
            MostrarError(ex.Message);
        }
        finally
        {
            Ocupado(false);
        }
    }

    private void MostrarIntentos() =>
        LblIntentos.Text = $"Intento {_intentos + 1} de {ControladorConversion.MaximoIntentos}";

    private void MostrarError(string mensaje)
    {
        LblError.Text = mensaje;
        LblError.IsVisible = true;
    }

    private void Ocupado(bool ocupado)
    {
        BtnIngresar.IsEnabled = !ocupado;
        Cargando.IsRunning = Cargando.IsVisible = ocupado;
        if (ocupado)
        {
            LblError.IsVisible = false;
        }
    }
}
