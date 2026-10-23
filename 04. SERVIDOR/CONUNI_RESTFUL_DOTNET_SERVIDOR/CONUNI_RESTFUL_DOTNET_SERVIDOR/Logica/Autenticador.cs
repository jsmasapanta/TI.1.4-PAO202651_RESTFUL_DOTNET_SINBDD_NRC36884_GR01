namespace ConversorUnidades.Servidor.Logica;

/// <summary>
/// Valida credenciales contra los valores de appsettings.json (sección "Credenciales").
/// Patrón Singleton: una sola instancia lee la configuración una vez al arrancar y la comparten
/// todas las peticiones. Program.cs llama a Configurar(); después todos usan Autenticador.Instancia.
/// </summary>
public sealed class Autenticador
{
    private static readonly object _candado = new();
    private static Autenticador? _instancia;

    public static Autenticador Instancia =>
        _instancia ?? throw new InvalidOperationException("El Autenticador no fue configurado en Program.cs.");

    private readonly string? _usuarioValido;
    private readonly string? _claveValida;

    // Constructor privado: nadie fuera de la clase puede hacer "new Autenticador()".
    private Autenticador(string? usuario, string? clave)
    {
        _usuarioValido = usuario;
        _claveValida = clave;
    }

    /// <summary>Crea la única instancia. Si ya existe, no hace nada.</summary>
    public static void Configurar(IConfiguration configuracion)
    {
        lock (_candado)
        {
            _instancia ??= new Autenticador(configuracion["Credenciales:Usuario"], configuracion["Credenciales:Clave"]);
        }
    }

    public bool Validar(string? usuario, string? clave)
    {
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrEmpty(clave))
        {
            return false;
        }

        if (string.IsNullOrEmpty(_usuarioValido) || string.IsNullOrEmpty(_claveValida))
        {
            throw new InvalidOperationException("El servidor no tiene credenciales configuradas en appsettings.json.");
        }

        // El usuario no distingue mayúsculas; la clave sí.
        return string.Equals(usuario.Trim(), _usuarioValido, StringComparison.OrdinalIgnoreCase)
            && string.Equals(clave, _claveValida, StringComparison.Ordinal);
    }
}
