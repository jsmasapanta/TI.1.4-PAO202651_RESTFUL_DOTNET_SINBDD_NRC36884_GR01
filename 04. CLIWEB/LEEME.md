# 04. CLIWEB — Cliente web

Aplicación ASP.NET Core MVC (.NET 8) que consume el API REST del servidor.

## Estructura (MVC)

```
CONUNI_RESTFUL_DOTNET_CLIWEB/
├── Controllers/
│   ├── CuentaController.cs       login (3 intentos), logout
│   └── ConversionController.cs   pantalla por categoría, conversión, historial
├── Modelo/
│   ├── Catalogo.cs               categorías, unidades y las 16 conversiones con su ruta REST
│   └── ViewModels.cs             datos que reciben las vistas
├── Servicio/
│   ├── Dtos.cs                   objetos JSON del API (records)
│   └── ClienteConversion.cs      HttpClient (singleton) + mensajes de error claros
├── Filtros/
│   └── RequiereSesionAttribute.cs  sin sesión → redirige al login
├── Views/
│   ├── Cuenta/Login.cshtml
│   ├── Conversion/Index.cshtml   una sola vista para las 3 categorías
│   └── Shared/_Layout.cshtml
├── wwwroot/
│   ├── css/site.css              diseño propio (sin Bootstrap), adaptable a celular
│   └── js/conversion.js          lista "A" dinámica y botón ⇄
├── Program.cs                    sesión + inyección del ClienteConversion
└── appsettings.json              URL del servidor REST
```

## Cómo se conecta

No usa paquetes NuGet: `HttpClient` + `System.Net.Http.Json`. Login con `PostAsJsonAsync("api/autenticacion/login", ...)` (401 = credenciales incorrectas) y conversiones con `GetAsync("api/temperatura/celsiusAFahrenheit?valor=100")`. Los errores 400 traen `{ codigo, mensaje }` y se muestra el `mensaje`.

La URL está en `appsettings.json`:

```json
"ServicioRest": { "Url": "http://localhost:8086" }
```

## Funcionalidad

- **Login** contra el servidor: botón "Ver" para mostrar la clave, intentos restantes y bloqueo después de 3 intentos.
- **Protección de páginas:** si alguien escribe `/Conversion` sin haber iniciado sesión, el sitio lo manda al login.
- **Menú lateral** con Temperatura, Longitud y Masa, cada una con su color (los mismos del cliente de escritorio).
- **Pantalla de conversión:**
  - Listas "De" y "A"; "A" se actualiza sola según "De".
  - Botón **⇄** para intercambiar unidades.
  - Acepta coma o punto decimal.
  - Resultado grande con detalle.
- **Historial** de las últimas 10 conversiones por categoría, guardado en la sesión, con botón para limpiarlo.
- **Mensajes de error claros:** número inválido, valor imposible (lo rechaza el servidor), servidor apagado.
- **Diseño adaptable:** en celular el menú pasa arriba y los campos se apilan.
- **Seguridad:** todos los formularios llevan token antifalsificación (`ValidateAntiForgeryToken`).

## Ejecutar

1. Levantar el SERVIDOR.
2. Abrir `CONUNI_RESTFUL_DOTNET_CLIWEB.sln` y presionar F5. Se abre `http://localhost:5086`.
3. Usuario `monster`, clave `monster`.

Las capturas están en `05. DOCUMENTACIÓN\capturas\CLIWEB`.
