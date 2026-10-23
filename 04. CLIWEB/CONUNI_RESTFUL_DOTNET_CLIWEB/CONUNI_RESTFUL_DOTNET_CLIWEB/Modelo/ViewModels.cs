namespace ConversorUnidades.Web.Modelo;

public class LoginViewModel
{
    public string? Usuario { get; set; }
    public string? Clave { get; set; }
    public string? Error { get; set; }
    public int IntentosRestantes { get; set; }
    public bool Bloqueado { get; set; }
}

public class ConversionViewModel
{
    public Categoria Categoria { get; set; }
    public IList<Unidad> UnidadesOrigen { get; set; } = new List<Unidad>();
    public IList<Unidad> UnidadesDestino { get; set; } = new List<Unidad>();
    public string? Origen { get; set; }
    public string? Destino { get; set; }
    public string? Valor { get; set; }

    public string? Resultado { get; set; }
    public string? Detalle { get; set; }
    public string? Error { get; set; }

    public IList<string> Historial { get; set; } = new List<string>();

    /// <summary>Mapa origen → destinos posibles, para que el JavaScript actualice la lista "A".</summary>
    public IDictionary<string, IList<Unidad>> MapaDestinos { get; set; } = new Dictionary<string, IList<Unidad>>();
}
