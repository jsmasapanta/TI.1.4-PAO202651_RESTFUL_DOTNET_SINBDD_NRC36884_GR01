namespace ConversorUnidades.Servidor.Logica;

/// <summary>
/// Registra en memoria cada petición que hacen los clientes al API (api/...):
/// hora, IP, cliente (cabecera X-Cliente), método, operación, entrada, código HTTP, resultado o error y duración.
/// Patrón Singleton: una sola instancia junta las llamadas de todas las peticiones; el panel /monitor la lee
/// para mostrar el consumo en vivo. Sin base de datos: al reiniciar el servidor el registro empieza de cero.
/// </summary>
public sealed class MonitorServicio
{
    private const int MaximoRegistros = 300;

    private static readonly Lazy<MonitorServicio> _instancia = new(() => new MonitorServicio());

    public static MonitorServicio Instancia => _instancia.Value;

    private readonly LinkedList<RegistroLlamada> _registros = new();
    private readonly Dictionary<string, int> _porOperacion = new();
    private readonly Dictionary<string, ConsumoCliente> _porTipo = new();
    private int _total;
    private int _errores;

    public DateTime Inicio { get; } = DateTime.Now;

    // Constructor privado: nadie fuera de la clase puede hacer "new MonitorServicio()".
    private MonitorServicio() { }

    public void Registrar(RegistroLlamada registro)
    {
        lock (_registros)
        {
            _registros.AddFirst(registro);
            if (_registros.Count > MaximoRegistros) _registros.RemoveLast();

            _total++;
            if (!registro.Exito) _errores++;
            _porOperacion.TryGetValue(registro.Operacion, out int veces);
            _porOperacion[registro.Operacion] = veces + 1;

            if (!_porTipo.TryGetValue(registro.TipoCliente, out ConsumoCliente? consumo))
            {
                consumo = new ConsumoCliente();
                _porTipo[registro.TipoCliente] = consumo;
            }
            consumo.Llamadas++;
            if (!registro.Exito) consumo.Errores++;
            consumo.Ultima = registro.Hora;
        }
    }

    /// <summary>Copia de los datos para mostrarlos sin bloquear a los que siguen llamando.</summary>
    public ResumenMonitor Resumen()
    {
        lock (_registros)
        {
            return new ResumenMonitor
            {
                Inicio = Inicio,
                Total = _total,
                Errores = _errores,
                Registros = _registros.ToList(),
                PorOperacion = _porOperacion.OrderByDescending(p => p.Value).ToList(),
                PorTipo = _porTipo.ToDictionary(p => p.Key, p => new ConsumoCliente
                {
                    Llamadas = p.Value.Llamadas, Errores = p.Value.Errores, Ultima = p.Value.Ultima,
                }),
            };
        }
    }

    public void Limpiar()
    {
        lock (_registros)
        {
            _registros.Clear();
            _porOperacion.Clear();
            _porTipo.Clear();
            _total = 0;
            _errores = 0;
        }
    }
}

public sealed class RegistroLlamada
{
    public DateTime Hora { get; set; }
    public string Cliente { get; set; } = "";
    public string TipoCliente { get; set; } = "";
    public string Metodo { get; set; } = "";
    public string Operacion { get; set; } = "";
    public string Entrada { get; set; } = "";
    public int CodigoHttp { get; set; }
    public string Resultado { get; set; } = "";
    public bool Exito { get; set; }
    public double Milisegundos { get; set; }
}

public sealed class ResumenMonitor
{
    public DateTime Inicio { get; set; }
    public int Total { get; set; }
    public int Errores { get; set; }
    public List<RegistroLlamada> Registros { get; set; } = new();
    public List<KeyValuePair<string, int>> PorOperacion { get; set; } = new();
    public Dictionary<string, ConsumoCliente> PorTipo { get; set; } = new();
}

public sealed class ConsumoCliente
{
    public int Llamadas { get; set; }
    public int Errores { get; set; }
    public DateTime Ultima { get; set; }
}
