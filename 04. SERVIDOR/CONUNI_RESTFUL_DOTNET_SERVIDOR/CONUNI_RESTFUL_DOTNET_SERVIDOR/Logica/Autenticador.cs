namespace ConversorUnidades.Servidor.Logica;

/// <summary>
/// Valida credenciales contra los valores de appsettings.json (sección "Credenciales") y controla los
/// intentos: al tercer intento fallido el usuario queda bloqueado 2 minutos. Los clientes no cuentan
/// intentos: solo muestran el mensaje que llega en el error (HTTP 401).
/// Patrón Singleton: una sola instancia lee la configuración una vez al arrancar y guarda los intentos de
/// todas las peticiones. Program.cs llama a Configurar(); después todos usan Autenticador.Instancia.
/// </summary>
public sealed class Autenticador
{
    private const int IntentosMaximos = 3;
    private static readonly TimeSpan TiempoBloqueo = TimeSpan.FromMinutes(2);

    private static readonly object _candado = new();
    private static Autenticador? _instancia;

    public static Autenticador Instancia =>
        _instancia ?? throw new InvalidOperationException("El Autenticador no fue configurado en Program.cs.");

    private readonly string? _usuarioValido;
    private readonly string? _claveValida;

    // Intentos fallidos por usuario (en minúsculas).
    private readonly Dictionary<string, ControlIntentos> _intentos = new();

    private sealed class ControlIntentos
    {
        public int Fallidos { get; set; }
        public DateTime? BloqueadoHasta { get; set; }
    }

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

    /// <summary>Devuelve true si las credenciales son correctas; si no, lanza el motivo.</summary>
    public bool Validar(string? usuario, string? clave)
    {
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrEmpty(clave))
        {
            throw new ArgumentException("Ingrese usuario y clave.");
        }

        if (string.IsNullOrEmpty(_usuarioValido) || string.IsNullOrEmpty(_claveValida))
        {
            throw new InvalidOperationException("El servidor no tiene credenciales configuradas en appsettings.json.");
        }

        string llave = usuario.Trim().ToLowerInvariant();
        lock (_intentos)
        {
            if (_intentos.TryGetValue(llave, out ControlIntentos? control)
                && control.BloqueadoHasta.HasValue && control.BloqueadoHasta.Value > DateTime.Now)
            {
                int segundos = (int)Math.Ceiling((control.BloqueadoHasta.Value - DateTime.Now).TotalSeconds);
                throw new ReglaNegocioException(
                    $"Usuario bloqueado por superar {IntentosMaximos} intentos. Intente de nuevo en {segundos} segundos.");
            }
        }

        // El usuario no distingue mayúsculas; la clave sí.
        bool correcto = string.Equals(usuario.Trim(), _usuarioValido, StringComparison.OrdinalIgnoreCase)
            && string.Equals(clave, _claveValida, StringComparison.Ordinal);

        lock (_intentos)
        {
            if (correcto)
            {
                _intentos.Remove(llave);
                return true;
            }

            if (!_intentos.TryGetValue(llave, out ControlIntentos? control) || control.BloqueadoHasta.HasValue)
            {
                // Primer fallo, o el bloqueo anterior ya terminó: se empieza a contar de nuevo.
                control = new ControlIntentos();
                _intentos[llave] = control;
            }
            control.Fallidos++;

            if (control.Fallidos >= IntentosMaximos)
            {
                control.BloqueadoHasta = DateTime.Now.Add(TiempoBloqueo);
                throw new ReglaNegocioException(
                    $"Se superaron los {IntentosMaximos} intentos. El usuario queda bloqueado {TiempoBloqueo.TotalMinutes:0} minutos.");
            }
            throw new ReglaNegocioException(
                $"Usuario o clave incorrectos. Intentos restantes: {IntentosMaximos - control.Fallidos}.");
        }
    }
}
