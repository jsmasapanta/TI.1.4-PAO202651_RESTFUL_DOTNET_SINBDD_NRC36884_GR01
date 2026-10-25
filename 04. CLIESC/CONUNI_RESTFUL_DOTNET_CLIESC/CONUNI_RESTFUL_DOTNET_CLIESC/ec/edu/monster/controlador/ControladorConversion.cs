using System.Threading.Tasks;
using ec.edu.monster.modelo;
using ec.edu.monster.servicio;

namespace ec.edu.monster.controlador
{
    /// <summary>
    /// Intermediario entre las ventanas y el servicio REST: solo pasa las llamadas y recuerda quién inició sesión.
    /// El login, los 3 intentos y las validaciones de los valores los hace el servidor.
    /// Las llamadas son asíncronas para que la ventana no se congele mientras responde el servidor.
    /// </summary>
    public class ControladorConversion
    {
        private readonly ClienteConversion _servicio = ClienteConversion.Instancia;

        public string UrlServidor => _servicio.UrlServidor;

        public string UsuarioActual { get; private set; }

        public async Task IniciarSesionAsync(string usuario, string clave)
        {
            await Task.Run(() => _servicio.Login(usuario, clave));   // si no es correcto, llega el mensaje del servidor
            UsuarioActual = usuario.Trim();
        }

        public void CerrarSesion() => UsuarioActual = null;

        public Task<double> ConvertirAsync(Conversion conversion, double valor) =>
            Task.Run(() => _servicio.Convertir(conversion.Ruta, valor));
    }
}
