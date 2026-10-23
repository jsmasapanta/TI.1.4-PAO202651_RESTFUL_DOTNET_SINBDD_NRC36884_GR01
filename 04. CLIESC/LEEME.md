# 04. CLIESC — Cliente de escritorio

Aplicación Windows Forms en .NET Framework 4.7.2 que consume el API REST del servidor.

## Estructura (MVC)

```
CONUNI_RESTFUL_DOTNET_CLIESC/
├── Modelo/
│   ├── Conversion.cs              igual que en CLICON (ruta REST de cada conversión)
│   └── CatalogoConversiones.cs    + Unidad, UnidadesOrigen(), UnidadesDestino(), Buscar()
├── Servicio/
│   ├── Dtos.cs                    objetos JSON del API
│   └── ClienteConversion.cs       SINGLETON con HttpClient + mensajes de error claros (mismo código que CLICON)
├── Controlador/
│   └── ControladorConversion.cs   llamadas asíncronas (la ventana no se congela), lectura y formato de números
├── Vista/
│   ├── Tema.cs                    colores y estilos compartidos
│   ├── FormLogin.cs               inicio de sesión (3 intentos, mostrar clave)
│   ├── FormPrincipal.cs           menú lateral + barra de estado con la URL del servidor
│   └── PanelConversion.cs         pantalla reutilizable para cada categoría
├── Program.cs                     ciclo login → principal → cerrar sesión → login
└── App.config                     URL del servidor
```

Las ventanas están hechas en código, sin archivos `.Designer.cs`, para que se pueda leer cada control y su posición.

## Funcionalidad

- **Login** contra el servidor: botón "Mostrar clave", aviso de intentos restantes y bloqueo al tercer intento fallido. Mientras espera al servidor, el botón muestra "CONECTANDO...".
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
3. Usuario `monster`, clave `monster`.

Para cambiar de servidor, editar `UrlServidor` en `App.config` (`http://IP-DEL-SERVIDOR:8086`) (o en `bin\Debug\CONUNI_RESTFUL_DOTNET_CLIESC.exe.config` ya compilado).

Las capturas de pantalla están en `05. DOCUMENTACIÓN\capturas\CLIESC`.
