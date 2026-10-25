namespace ec.edu.monster.modelo;

// Objetos JSON del API REST (solo get/set). System.Text.Json en modo web: camelCase, sin distinguir mayúsculas.

public class SolicitudLogin
{
    public string Usuario { get; set; } = "";
    public string Clave { get; set; } = "";
}

public class RespuestaLogin
{
    public bool Autenticado { get; set; }
    public string? Usuario { get; set; }
    public string? Mensaje { get; set; }
}

public class RespuestaConversion
{
    public string? Categoria { get; set; }
    public string? Operacion { get; set; }
    public double Valor { get; set; }
    public string? UnidadOrigen { get; set; }
    public string? UnidadDestino { get; set; }
    public double Resultado { get; set; }
}

/// <summary>Cuerpo de las respuestas de error (400, 401, 500).</summary>
public class ErrorServicio
{
    public string? Codigo { get; set; }
    public string? Mensaje { get; set; }
}
