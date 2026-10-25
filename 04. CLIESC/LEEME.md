# 04. CLIESC — Cliente de escritorio

Aplicación Windows Forms en .NET Framework 4.7.2 que consume el API REST del servidor.

## Estructura (MVC)

```
CONUNI_RESTFUL_DOTNET_CLIESC/
├── ec/edu/monster/
│   ├── modelo/
│   │   ├── Conversion.cs              Categoria + Conversion con su ruta REST (solo get/set)
│   │   └── Dtos.cs                    objetos JSON (solo get/set)
│   ├── servicio/
│   │   ├── ClienteConversion.cs       SINGLETON: HttpClient + mensaje del error
│   │   └── Json.cs
│   ├── controlador/
│   │   └── ControladorConversion.cs   solo pasa las llamadas (asíncronas) y recuerda el usuario
│   └── vista/
│       ├── FormLogin.cs               inicio de sesión con la imagen (mensaje del servidor)
│       ├── FormPrincipal.cs           menú lateral con la imagen + panel
│       ├── PanelConversion.cs         De / A / valor / historial
│       ├── CatalogoConversiones.cs    opciones de las listas (+ Unidad)
│       ├── Formato.cs                 número, imagen
│       └── Tema.cs
├── Recursos/monster.jpg               imagen incrustada en el .exe
└── App.config                         URL del servidor
```

Las ventanas están hechas en código, sin archivos `.Designer.cs`, para que se pueda leer cada control y su posición.

## Funcionalidad

- **Login** contra el servidor: imagen de Monsters Inc. a la izquierda, botón "Mostrar clave" y el mensaje del servidor (intentos restantes o bloqueo de 2 minutos). Mientras espera al servidor, el botón muestra "CONECTANDO...".
- **Menú lateral** con Temperatura, Longitud y Masa; cada categoría tiene su color.
- **Pantalla de conversión:**
  - Listas "De" y "A". "A" solo muestra las unidades a las que el servidor sabe convertir desde "De".
  - Botón **⇄** para intercambiar unidades.
  - Convierte con el botón o con **Enter**; acepta punto o coma decimal.
  - Resultado grande, con el detalle `150 kg (Kilogramos) = 330.693393 lb (Libras)`.
  - **Historial de la sesión** por categoría, con botón para limpiarlo.
- **Errores en rojo** dentro de la pantalla, sin ventanas emergentes molestas: valor inválido, servidor apagado o tiempo de espera agotado.
- **Cerrar sesión** vuelve al login sin cerrar el programa.

## Ejecutar

1. Levantar el SERVIDOR.
2. Abrir `CONUNI_RESTFUL_DOTNET_CLIESC.sln` y presionar F5.
3. Usuario `monster`, clave `monster69`.

Para cambiar de servidor, editar `UrlServidor` en `App.config` (`http://IP-DEL-SERVIDOR:8086`) (o en `bin\Debug\CONUNI_RESTFUL_DOTNET_CLIESC.exe.config` ya compilado).

Las capturas de pantalla están en `05. DOCUMENTACIÓN\capturas\CLIESC`.
