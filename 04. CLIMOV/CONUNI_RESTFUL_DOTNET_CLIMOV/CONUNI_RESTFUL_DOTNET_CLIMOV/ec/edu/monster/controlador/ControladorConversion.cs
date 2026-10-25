using ec.edu.monster.modelo;
using ec.edu.monster.servicio;

namespace ec.edu.monster.controlador;

/// <summary>
/// Intermediario entre las pantallas (vista) y el API REST: solo pasa las llamadas y guarda la sesión
/// (usuario e historial). El login, los 3 intentos y las validaciones de los valores los hace el servidor.
/// Patrón Singleton: las dos pantallas comparten ControladorConversion.Instancia.
/// </summary>
public sealed class ControladorConversion
{
    private const int MaximoHistorial = 10;

    private static readonly Lazy<ControladorConversion> _instancia =
        new(() => new ControladorConversion(ClienteRest.Instancia));

    public static ControladorConversion Instancia => _instancia.Value;

    private readonly ClienteRest _cliente;
    private readonly Dictionary<Categoria, List<string>> _historial = new();

    private ControladorConversion(ClienteRest cliente)
    {
        _cliente = cliente;
    }

    public string Usuario { get; private set; } = "";

    public string UrlServidor
    {
        get => _cliente.Url;
        set => _cliente.Url = value;
    }

    public async Task IniciarSesionAsync(string usuario, string clave)
    {
        await _cliente.LoginAsync(usuario, clave);   // si no es correcto, llega el mensaje del servidor
        Usuario = usuario.Trim();
        _historial.Clear();
    }

    public void CerrarSesion()
    {
        Usuario = "";
        _historial.Clear();
    }

    public Task<double> ConvertirAsync(Conversion conversion, double valor) =>
        _cliente.ConvertirAsync(conversion.Ruta, valor);

    /// <summary>Anota una línea (ya formateada por la vista) en el historial de la categoría.</summary>
    public void AnotarHistorial(Categoria categoria, string linea)
    {
        if (!_historial.TryGetValue(categoria, out var lista))
        {
            lista = new List<string>();
            _historial[categoria] = lista;
        }
        lista.Insert(0, linea);
        if (lista.Count > MaximoHistorial)
        {
            lista.RemoveAt(lista.Count - 1);
        }
    }

    public IReadOnlyList<string> HistorialDe(Categoria categoria) =>
        _historial.TryGetValue(categoria, out var lista) ? lista : Array.Empty<string>();

    public void LimpiarHistorial(Categoria categoria) => _historial.Remove(categoria);
}
