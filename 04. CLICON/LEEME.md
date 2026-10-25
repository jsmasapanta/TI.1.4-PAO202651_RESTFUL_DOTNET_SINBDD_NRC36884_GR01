# 04. CLICON — Cliente de consola

Aplicación de consola en .NET Framework 4.7.2 que consume el API REST del servidor.

## Estructura (MVC)

```
CONUNI_RESTFUL_DOTNET_CLICON/
├── ec/edu/monster/
│   ├── modelo/
│   │   ├── Conversion.cs            Categoria + Conversion con su ruta REST (solo get/set)
│   │   └── Dtos.cs                  objetos JSON: SolicitudLogin, RespuestaLogin, ResultadoConversion, ErrorServicio
│   ├── servicio/
│   │   ├── ClienteConversion.cs     SINGLETON: HttpClient (POST login, GET conversiones) + mensaje del error
│   │   └── Json.cs                  DataContractJsonSerializer
│   ├── controlador/
│   │   └── ControladorConsola.cs    solo pasa las llamadas y recuerda el usuario
│   └── vista/
│       ├── VistaConsola.cs          pantallas: login → categoría → conversión
│       └── CatalogoConversiones.cs  opciones del menú (16 recursos REST)
├── Program.cs
└── App.config                       URL del servidor (appSettings "UrlServidor")
```

## Cómo se conecta

`ClienteConversion` usa `HttpClient` (incluido en .NET Framework) y el serializador JSON `DataContractJsonSerializer`, así que **no necesita paquetes NuGet**.

- Login: `POST {UrlServidor}/api/autenticacion/login` con `{ "usuario", "clave" }`. Si responde **401**, la consola muestra el `mensaje` del servidor (intentos restantes o usuario bloqueado 2 minutos).
- Conversión: `GET {UrlServidor}/api/{ruta}?valor=100` y lee `resultado` del JSON.
- Si responde **400**, muestra el `mensaje` del error (por ejemplo, *"La temperatura no puede ser menor al cero absoluto"*).

La URL está en `App.config`:

```xml
<add key="UrlServidor" value="http://192.168.100.3:8086" />
```

Para conectarse a otra máquina basta cambiar ese valor; no hay que recompilar. Después de compilar, el archivo editable es `bin\Debug\CONUNI_RESTFUL_DOTNET_CLICON.exe.config`.

## Funcionalidad

- Login contra el servidor, con la clave oculta con asteriscos. Los 3 intentos y el bloqueo los controla el servidor; la consola pregunta "¿Intentar de nuevo?".
- Menú de 3 categorías; cada una tiene sus conversiones (temperatura 6, longitud 6, masa 4).
- Acepta punto o coma decimal (`12.5` o `12,5`).
- Muestra el resultado con hasta 6 decimales.
- Mensajes claros cuando el valor es inválido (HTTP 400), el servidor está apagado o tarda demasiado.

## Ejecutar

1. Levantar el SERVIDOR (`Ctrl + F5` en su solución).
2. Abrir `CONUNI_RESTFUL_DOTNET_CLICON.sln` en **otra ventana** de Visual Studio y presionar `Ctrl + F5`.
3. Usuario `monster`, clave `monster69`.
