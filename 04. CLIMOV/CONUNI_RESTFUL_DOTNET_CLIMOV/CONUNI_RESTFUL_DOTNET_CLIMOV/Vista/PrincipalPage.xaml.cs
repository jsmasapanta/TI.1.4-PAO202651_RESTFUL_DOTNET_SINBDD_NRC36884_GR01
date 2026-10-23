using ConversorUnidades.Movil.Controlador;
using ConversorUnidades.Movil.Modelo;
using ConversorUnidades.Movil.Servicio;

namespace ConversorUnidades.Movil.Vista;

/// <summary>Pantalla principal: conversiones por categoría (CU02–CU05), historial (CU06) y cierre de sesión (CU07).</summary>
public partial class PrincipalPage : ContentPage
{
    private static readonly Dictionary<Categoria, Color> ColoresCategoria = new()
    {
        [Categoria.Temperatura] = Color.FromArgb("#D65D0E"),
        [Categoria.Longitud] = Color.FromArgb("#1971C2"),
        [Categoria.Masa] = Color.FromArgb("#7C3AAD")
    };

    private readonly ControladorConversion _controlador = ControladorConversion.Instancia;
    private Categoria _categoria = Categoria.Temperatura;

    public PrincipalPage()
    {
        InitializeComponent();
        LblUsuario.Text = "Usuario: " + _controlador.Usuario;
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
            boton.BackgroundColor = activo ? ColoresCategoria[cat] : Colors.White;
            boton.TextColor = activo ? Colors.White : ColoresCategoria[cat];
        }
        LblTitulo.Text = $"{categoria}  ·  {Catalogo.Simbolo(categoria)}";
        LblTitulo.TextColor = color;
        BtnConvertir.BackgroundColor = color;
        BtnIntercambiar.BackgroundColor = color;
        BtnIntercambiar.TextColor = Colors.White;
        Cargando.Color = color;
        LblResultado.TextColor = color;

        PkrOrigen.ItemsSource = Catalogo.UnidadesOrigen(categoria);
        PkrOrigen.SelectedIndex = 0;
        LimpiarResultado();
        MostrarHistorial();
    }

    private void OnOrigenCambiado(object? sender, EventArgs e)
    {
        if (PkrOrigen.SelectedItem is not Unidad origen) return;
        PkrDestino.ItemsSource = Catalogo.UnidadesDestino(_categoria, origen);
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
        Conversion? conversion = Catalogo.Buscar(origen, destino);
        if (conversion == null) return;

        double? valor = ControladorConversion.LeerNumero(TxtValor.Text);
        if (valor == null)
        {
            MostrarError("Ingrese un número válido (puede usar coma o punto).");
            return;
        }

        Ocupado(true);
        try
        {
            ResultadoConversion r = await _controlador.ConvertirAsync(conversion, valor.Value);
            LblResultado.TextColor = ColoresCategoria[_categoria];
            LblResultado.Text = r.Resultado;
            LblDetalle.TextColor = Color.FromArgb("#6C757D");
            LblDetalle.Text = r.Detalle;
            MostrarHistorial();
        }
        catch (ErrorConversionException ex)
        {
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
    }

    private void LimpiarResultado()
    {
        TxtValor.Text = "";
        LblResultado.Text = "—";
        LblDetalle.Text = "";
    }

    private void MostrarError(string mensaje)
    {
        LblResultado.Text = "Error";
        LblResultado.TextColor = Color.FromArgb("#C0392B");
        LblDetalle.TextColor = Color.FromArgb("#C0392B");
        LblDetalle.Text = mensaje;
    }

    private void Ocupado(bool ocupado)
    {
        BtnConvertir.IsEnabled = !ocupado;
        Cargando.IsRunning = Cargando.IsVisible = ocupado;
    }
}
