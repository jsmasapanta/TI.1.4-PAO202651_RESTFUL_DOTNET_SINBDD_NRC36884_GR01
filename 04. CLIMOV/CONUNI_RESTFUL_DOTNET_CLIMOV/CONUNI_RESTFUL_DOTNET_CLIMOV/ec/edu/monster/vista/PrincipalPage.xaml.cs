using ec.edu.monster.controlador;
using ec.edu.monster.modelo;
using ec.edu.monster.servicio;

namespace ec.edu.monster.vista;

/// <summary>Pantalla principal: conversiones por categoría (CU02–CU05), historial (CU06) y cierre de sesión (CU07).</summary>
public partial class PrincipalPage : ContentPage
{
    // Mismos colores que la web (estilo Bento): fuerte para textos y botones, claro para el fondo del resultado.
    private static readonly Dictionary<Categoria, Color> ColoresCategoria = new()
    {
        [Categoria.Temperatura] = Color.FromArgb("#D85A30"),
        [Categoria.Longitud] = Color.FromArgb("#185FA5"),
        [Categoria.Masa] = Color.FromArgb("#534AB7")
    };

    private static readonly Dictionary<Categoria, Color> ColoresClaros = new()
    {
        [Categoria.Temperatura] = Color.FromArgb("#FAECE7"),
        [Categoria.Longitud] = Color.FromArgb("#E6F1FB"),
        [Categoria.Masa] = Color.FromArgb("#EEEDFE")
    };

    private static readonly Color CategoriaInactiva = Color.FromArgb("#37336A");
    private static readonly Color ErrorFuerte = Color.FromArgb("#A32D2D");
    private static readonly Color ErrorClaro = Color.FromArgb("#FCEBEB");

    private readonly ControladorConversion _controlador = ControladorConversion.Instancia;
    private Categoria _categoria = Categoria.Temperatura;

    public PrincipalPage()
    {
        InitializeComponent();
        LblUsuario.Text = "Usuario: " + _controlador.Usuario;
        LblServidor.Text = _controlador.UrlServidor;
        SeleccionarCategoria(Categoria.Temperatura);
    }

    private void OnCategoriaClicked(object? sender, EventArgs e)
    {
        if (sender == BtnTemperatura) SeleccionarCategoria(Categoria.Temperatura);
        else if (sender == BtnLongitud) SeleccionarCategoria(Categoria.Longitud);
        else SeleccionarCategoria(Categoria.Masa);
    }

    private void SeleccionarCategoria(Categoria categoria)
    {
        _categoria = categoria;
        Color color = ColoresCategoria[categoria];

        foreach (var (boton, cat) in new[] { (BtnTemperatura, Categoria.Temperatura), (BtnLongitud, Categoria.Longitud), (BtnMasa, Categoria.Masa) })
        {
            bool activo = cat == categoria;
            boton.BackgroundColor = activo ? ColoresCategoria[cat] : CategoriaInactiva;
            boton.TextColor = Colors.White;
        }
        LblTitulo.Text = $"Resultado · {categoria} ({Formato.Simbolo(categoria)})";
        BtnConvertir.BackgroundColor = color;
        BtnIntercambiar.BackgroundColor = ColoresClaros[categoria];
        BtnIntercambiar.TextColor = color;
        Cargando.Color = color;
        PintarResultado(error: false);

        PkrOrigen.ItemsSource = CatalogoConversiones.UnidadesOrigen(categoria);
        PkrOrigen.SelectedIndex = 0;
        LimpiarResultado();
        MostrarHistorial();
    }

    private void OnOrigenCambiado(object? sender, EventArgs e)
    {
        if (PkrOrigen.SelectedItem is not Unidad origen) return;
        PkrDestino.ItemsSource = CatalogoConversiones.UnidadesDestino(_categoria, origen);
        PkrDestino.SelectedIndex = 0;
    }

    private void OnIntercambiarClicked(object? sender, EventArgs e)
    {
        if (PkrOrigen.SelectedItem is not Unidad origen || PkrDestino.SelectedItem is not Unidad destino) return;

        var origenes = (List<Unidad>)PkrOrigen.ItemsSource;
        PkrOrigen.SelectedIndex = origenes.IndexOf(destino);   // recarga los destinos
        var destinos = (List<Unidad>)PkrDestino.ItemsSource;
        PkrDestino.SelectedIndex = destinos.IndexOf(origen);
    }

    private async void OnConvertirClicked(object? sender, EventArgs e)
    {
        if (PkrOrigen.SelectedItem is not Unidad origen || PkrDestino.SelectedItem is not Unidad destino) return;
        Conversion? conversion = CatalogoConversiones.Buscar(origen, destino);
        if (conversion == null) return;

        double? valor = Formato.LeerNumero(TxtValor.Text);
        if (valor == null)
        {
            MostrarError("Ingrese un número válido (puede usar coma o punto).");
            return;
        }

        Ocupado(true);
        try
        {
            double resultado = await _controlador.ConvertirAsync(conversion, valor.Value);
            string entrada = $"{Formato.Numero(valor.Value)} {conversion.Origen.Simbolo}";
            string salida = $"{Formato.Numero(resultado)} {conversion.Destino.Simbolo}";
            PintarResultado(error: false);
            LblResultado.Text = salida;
            LblDetalle.Text = $"{entrada} ({conversion.Origen.Nombre})  =  {salida} ({conversion.Destino.Nombre})";
            _controlador.AnotarHistorial(_categoria, $"{DateTime.Now:HH:mm:ss}  ·  {entrada} → {salida}");
            MostrarHistorial();
        }
        catch (ErrorServicioException ex)
        {
            // Por ejemplo: «La temperatura no puede ser menor al cero absoluto…» (lo valida el servidor).
            MostrarError(ex.Message);
        }
        finally
        {
            Ocupado(false);
        }
    }

    private void OnLimpiarClicked(object? sender, EventArgs e)
    {
        _controlador.LimpiarHistorial(_categoria);
        MostrarHistorial();
    }

    private async void OnCerrarSesionClicked(object? sender, EventArgs e)
    {
        if (!await DisplayAlert("Cerrar sesión", "¿Desea salir de su cuenta?", "Sí", "No")) return;
        _controlador.CerrarSesion();
        Window.Page = new LoginPage();
    }

    private void MostrarHistorial()
    {
        ListaHistorial.Children.Clear();
        IReadOnlyList<string> historial = _controlador.HistorialDe(_categoria);
        foreach (string linea in historial)
        {
            ListaHistorial.Children.Add(new Label { Text = linea, FontSize = 13, TextColor = Color.FromArgb("#212529") });
        }
        LblHistorialVacio.IsVisible = historial.Count == 0;
        LblContador.Text = historial.Count.ToString();
        LblContadorPie.Text = "en " + _categoria.ToString().ToLowerInvariant();
    }

    private void LimpiarResultado()
    {
        TxtValor.Text = "";
        LblResultado.Text = "—";
        LblDetalle.Text = "Elija las unidades, escriba el valor y presione Convertir.";
    }

    private void MostrarError(string mensaje)
    {
        PintarResultado(error: true);
        LblResultado.Text = "No se pudo convertir";
        LblDetalle.Text = mensaje;
    }

    /// <summary>Tarjeta del resultado: color claro de la categoría, o rojo claro si el servidor rechazó el valor.</summary>
    private void PintarResultado(bool error)
    {
        Color fuerte = error ? ErrorFuerte : ColoresCategoria[_categoria];
        CeldaResultado.BackgroundColor = error ? ErrorClaro : ColoresClaros[_categoria];
        LblTitulo.TextColor = fuerte;
        LblResultado.TextColor = fuerte;
        LblResultado.FontSize = error ? 22 : 44;
        LblDetalle.TextColor = error ? ErrorFuerte : Color.FromArgb("#2C2C2A");
    }

    private void Ocupado(bool ocupado)
    {
        BtnConvertir.IsEnabled = !ocupado;
        Cargando.IsRunning = Cargando.IsVisible = ocupado;
    }
}
