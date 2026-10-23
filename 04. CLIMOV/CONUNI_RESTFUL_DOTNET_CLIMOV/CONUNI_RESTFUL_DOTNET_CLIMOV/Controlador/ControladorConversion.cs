using System.Globalization;
using ConversorUnidades.Movil.Modelo;
using ConversorUnidades.Movil.Servicio;

namespace ConversorUnidades.Movil.Controlador;

/// <summary>Resultado listo para mostrar en pantalla.</summary>
public record ResultadoConversion(string Resultado, string Detalle);

/// <summary>
/// Intermediario entre las pantallas (Vista) y el API REST.
/// Valida la entrada, llama al servidor, da formato y guarda el historial de la sesión.
/// Patrón Singleton: las dos pantallas comparten ControladorConversion.Instancia.
/// </summary>
public sealed class ControladorConversion
{
    public const int MaximoIntentos = 3;
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

    public async Task<bool> IniciarSesionAsync(string usuario, string clave)
    {
        bool correcto = await _cliente.LoginAsync(usuario.Trim(), clave);
        if (correcto)
        {
            Usuario = usuario.Trim();
            _historial.Clear();
        }
        return correcto;
    }

    public void CerrarSesion()
    {
        Usuario = "";
        _historial.Clear();
    }

    public async Task<ResultadoConversion> ConvertirAsync(Conversion conversion, double valor)
    {
        double resultado = await _cliente.ConvertirAsync(conversion.Ruta, valor);
        string entrada = $"{Formatear(valor)} {conversion.Origen.Simbolo}";
        string salida = $"{Formatear(resultado)} {conversion.Destino.Simbolo}";

        if (!_historial.TryGetValue(conversion.Categoria, out var lista))
        {
            lista = new List<string>();
            _historial[conversion.Categoria] = lista;
        }
        lista.Insert(0, $"{DateTime.Now:HH:mm:ss}  ·  {entrada} → {salida}");
        if (lista.Count > MaximoHistorial)
        {
            lista.RemoveAt(lista.Count - 1);
        }

        return new ResultadoConversion(salida, $"{entrada} ({conversion.Origen.Nombre})  =  {salida} ({conversion.Destino.Nombre})");
    }

    public IReadOnlyList<string> HistorialDe(Categoria categoria) =>
        _historial.TryGetValue(categoria, out var lista) ? lista : Array.Empty<string>();

    public void LimpiarHistorial(Categoria categoria) => _historial.Remove(categoria);

    /// <summary>Acepta punto o coma decimal ("12.5" o "12,5"). Devuelve null si no es un número.</summary>
    public static double? LeerNumero(string? texto)
    {
        string limpio = (texto ?? "").Trim().Replace(',', '.');
        return double.TryParse(limpio, NumberStyles.Float, CultureInfo.InvariantCulture, out double numero)
               && double.IsFinite(numero)
            ? numero
            : null;
    }

    /// <summary>Hasta 6 decimales, sin ceros sobrantes.</summary>
    public static string Formatear(double numero)
    {
        string texto = Math.Round(numero, 6).ToString("0.######", CultureInfo.CurrentCulture);
        return texto == "-0" ? "0" : texto;
    }
}
