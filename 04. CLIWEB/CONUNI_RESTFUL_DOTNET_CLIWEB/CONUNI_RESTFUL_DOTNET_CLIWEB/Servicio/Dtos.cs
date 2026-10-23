namespace ConversorUnidades.Web.Servicio;

// Objetos JSON que intercambia el API REST.
// System.Text.Json (web) usa camelCase, igual que el servidor: { "usuario": ..., "clave": ... }.

public record SolicitudLogin(string Usuario, string Clave);

public record RespuestaLogin(bool Autenticado, string? Usuario, string? Mensaje);

public record ResultadoConversion(
    string? Categoria,
    string? Operacion,
    double Valor,
    string? UnidadOrigen,
    string? UnidadDestino,
    double Resultado);

/// <summary>Cuerpo de las respuestas de error (400, 401, 500).</summary>
public record ErrorServicio(string? Codigo, string? Mensaje);
