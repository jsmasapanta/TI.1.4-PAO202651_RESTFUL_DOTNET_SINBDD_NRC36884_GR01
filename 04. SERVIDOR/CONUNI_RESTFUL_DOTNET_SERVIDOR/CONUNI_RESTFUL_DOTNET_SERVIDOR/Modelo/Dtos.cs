namespace ConversorUnidades.Servidor.Modelo;

// Objetos que viajan en JSON entre el servidor y los clientes.
// ASP.NET Core los serializa en camelCase: { "usuario": ..., "clave": ... }.

/// <summary>Cuerpo de POST /api/autenticacion/login.</summary>
public class SolicitudLogin
{
    /// <example>monster</example>
    public string? Usuario { get; set; }

    /// <example>monster69</example>
    public string? Clave { get; set; }
}

/// <summary>Respuesta de un login correcto (HTTP 200).</summary>
public record RespuestaLogin(bool Autenticado, string Usuario, string Mensaje);

/// <summary>Respuesta de una conversión correcta (HTTP 200).</summary>
public record ResultadoConversion(
    string Categoria,
    string Operacion,
    double Valor,
    string UnidadOrigen,
    string UnidadDestino,
    double Resultado);

/// <summary>
/// Cuerpo de toda respuesta de error (400, 401, 500).
/// Codigo: VALOR_INVALIDO, REGLA_NEGOCIO (usuario o clave incorrectos, usuario bloqueado) o ERROR_SERVIDOR.
/// </summary>
public record ErrorServicio(string Codigo, string Mensaje);
