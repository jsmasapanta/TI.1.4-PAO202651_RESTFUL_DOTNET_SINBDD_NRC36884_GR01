using ec.edu.monster.modelo;
using ec.edu.monster.servicio;

namespace ec.edu.monster.controlador
{
    /// <summary>
    /// Intermediario entre la vista y el servicio: solo pasa las llamadas y recuerda quién inició sesión.
    /// El login, los 3 intentos y las validaciones de los valores los hace el servidor.
    /// </summary>
    public class ControladorConsola
    {
        private readonly ClienteConversion _servicio = ClienteConversion.Instancia;

        public string UrlServidor => _servicio.UrlServidor;

        public string Usuario { get; private set; }

        public void IniciarSesion(string usuario, string clave)
        {
            _servicio.Login(usuario, clave);   // si no es correcto, llega el mensaje del servidor como excepción
            Usuario = usuario.Trim();
        }

        public void CerrarSesion() => Usuario = null;

        public double Convertir(Conversion conversion, double valor) => _servicio.Convertir(conversion.Ruta, valor);
    }
}
