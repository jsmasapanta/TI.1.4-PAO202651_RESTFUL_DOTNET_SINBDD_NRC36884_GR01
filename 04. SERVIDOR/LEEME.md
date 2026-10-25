# 04. SERVIDOR — API RESTful de conversión de unidades

API REST en **ASP.NET Core Web API (.NET 9)**, sin base de datos. Responde en **JSON** y se prueba desde **Swagger**.

## Estructura

```
CONUNI_RESTFUL_DOTNET_SERVIDOR/
├── Controllers/                    recursos REST (lo que ven los clientes)
│   ├── AutenticacionController.cs  POST api/autenticacion/login
│   ├── ConversionControllerBase.cs base común: llama a Logica y convierte errores en HTTP 400
│   ├── TemperaturaController.cs    GET api/temperatura/{operacion}?valor=
│   ├── LongitudController.cs       GET api/longitud/{operacion}?valor=
│   └── MasaController.cs           GET api/masa/{operacion}?valor=
├── Modelo/
│   └── Dtos.cs                     SolicitudLogin, RespuestaLogin, ResultadoConversion, ErrorServicio
├── Logica/                         reglas de negocio (no saben nada de HTTP); las mismas del TI.1.2
│   ├── ConvertidorTemperatura.cs
│   ├── ConvertidorLongitud.cs
│   ├── ConvertidorMasa.cs
│   ├── Validacion.cs
│   └── Autenticador.cs             SINGLETON: lee usuario/clave de appsettings.json una sola vez
├── Program.cs                      controladores, Swagger, CORS
├── appsettings.json                credenciales monster / monster69
├── Properties/launchSettings.json  escucha en http://*:8086 (todas las tarjetas de red)
└── CONUNI_RESTFUL_DOTNET_SERVIDOR.http   pruebas rápidas desde Visual Studio
```

## Recursos del API

| Método | Ruta | Cuerpo / parámetros | Respuesta correcta |
|---|---|---|---|
| POST | `/api/autenticacion/login` | JSON `{ "usuario": "monster", "clave": "monster69" }` | `200` `{ "autenticado": true, "usuario": "monster", "mensaje": "Bienvenido, monster." }` |
| GET | `/api/temperatura/celsiusAFahrenheit?valor=100` | `valor` (número) | `200` `{ "categoria": "Temperatura", "operacion": "celsiusAFahrenheit", "valor": 100, "unidadOrigen": "°C", "unidadDestino": "°F", "resultado": 212 }` |

Operaciones disponibles (todas `GET ...?valor=`):

| Recurso | Operaciones |
|---|---|
| `api/temperatura/` | `celsiusAFahrenheit`, `fahrenheitACelsius`, `celsiusAKelvin`, `kelvinACelsius`, `fahrenheitAKelvin`, `kelvinAFahrenheit` |
| `api/longitud/` | `metrosAPies`, `piesAMetros`, `metrosAPulgadas`, `pulgadasAMetros`, `kilometrosAMillas`, `millasAKilometros` |
| `api/masa/` | `kilogramosALibras`, `librasAKilogramos`, `gramosAOnzas`, `onzasAGramos` |

### Códigos de estado y errores

Todo error trae el mismo JSON: `{ "codigo": "...", "mensaje": "..." }`.

| HTTP | `codigo` | Cuándo |
|---|---|---|
| 200 | — | Login correcto o conversión correcta |
| 400 | `VALOR_INVALIDO` | Temperatura bajo el cero absoluto, longitud o masa negativa, `valor` vacío o que no es número |
| 400 | `VALOR_INVALIDO` | Usuario o clave vacíos |
| 401 | `REGLA_NEGOCIO` | "Usuario o clave incorrectos. Intentos restantes: N." o, al tercer intento, "Se superaron los 3 intentos. El usuario queda bloqueado 2 minutos." |
| 500 | `ERROR_SERVIDOR` | Falta configurar las credenciales en `appsettings.json` |

**Credenciales** (`appsettings.json`, sección `Credenciales`): usuario `monster`, clave `monster69`. El usuario no distingue mayúsculas.

**Intentos:** `Logica/Autenticador.cs` (Singleton) cuenta los intentos fallidos de cada usuario y, al tercero, lo bloquea 2 minutos. Los clientes no cuentan intentos: muestran el `mensaje` del 401.

## Ejecutar en Visual Studio

1. Abrir `CONUNI_RESTFUL_DOTNET_SERVIDOR.sln`.
2. En el botón verde debe decir **CONUNI_RESTFUL_DOTNET_SERVIDOR**.
3. **Depurar → Iniciar sin depurar** (`Ctrl + F5`). Se abre una ventana negra (Kestrel) que **no se debe cerrar**, y el navegador en `http://localhost:8086/swagger`.
4. En Swagger: abrir un recurso → **Try it out** → escribir el valor → **Execute**.
5. También se puede usar `CONUNI_RESTFUL_DOTNET_SERVIDOR.http`: clic en **Enviar solicitud** encima de cada bloque.

## Que los demás se conecten por WiFi

El perfil de ejecución escucha en `http://*:8086`, así que el servidor ya acepta conexiones de otras máquinas; solo hay que abrir el puerto en el firewall **una vez**:

1. Clic derecho en **Inicio** → **Terminal (Administrador)**.
2. Ejecutar:
   ```powershell
   powershell -ExecutionPolicy Bypass -File "RUTA\04. SERVIDOR\abrir_puerto_red.ps1"
   ```
3. El script muestra la URL del adaptador Wi-Fi, por ejemplo `http://192.168.0.106:8086`. Esa es la URL que ponen los clientes.
4. Prueba desde otro equipo: `http://IP-DEL-SERVIDOR:8086/swagger`.

> No hace falta IIS: el servidor corre en Kestrel mientras la ventana negra esté abierta. Si Windows pregunta la primera vez si permite el acceso a la red, responder **Permitir**.
