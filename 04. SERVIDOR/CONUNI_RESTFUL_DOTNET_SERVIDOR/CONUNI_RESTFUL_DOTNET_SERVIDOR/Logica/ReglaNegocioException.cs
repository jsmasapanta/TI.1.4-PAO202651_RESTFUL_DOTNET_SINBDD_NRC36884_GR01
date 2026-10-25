namespace ConversorUnidades.Servidor.Logica;

/// <summary>
/// Regla del servicio que no se cumple (usuario o clave incorrectos, usuario bloqueado).
/// El API la responde con HTTP 401 y código REGLA_NEGOCIO; el cliente muestra el mensaje.
/// </summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje) { }
}
