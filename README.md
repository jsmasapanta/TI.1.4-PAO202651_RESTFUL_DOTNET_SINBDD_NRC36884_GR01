# TI.1.4 · Conversor de Unidades — RESTful .NET sin BDD

Proyecto de **Arquitectura de Software (ESPE)**: un **servicio web RESTful** hecho en .NET (ASP.NET Core Web API) que convierte unidades de **temperatura, longitud y masa**, y **cuatro clientes** que lo consumen: **consola, escritorio, web y móvil**.

Es la versión REST del TI.1.2 (SOAP): las mismas 16 conversiones, las mismas validaciones y los mismos clientes, pero ahora la comunicación es **HTTP + JSON** en lugar de SOAP + XML.

No usa base de datos: las conversiones se calculan con fórmulas y factores exactos, y el usuario y la clave están en el `appsettings.json` del servidor.

| | |
|---|---|
| **Usuario** | `monster` |
| **Clave** | `monster` |
| **Servidor REST** | `http://localhost:8086` (Swagger: `/swagger`) |
| **Cliente web** | `http://localhost:5086` |
| **IDE** | Visual Studio 2022 Community |
| **Patrones** | MVC en los clientes · Singleton en servidor y clientes |

---

## Índice

1. [Arquitectura](#1-arquitectura)
2. [Estructura del repositorio](#2-estructura-del-repositorio)
3. [Recursos del API](#3-recursos-del-api)
4. [Patrones de diseño: MVC y Singleton](#4-patrones-de-diseño-mvc-y-singleton)
5. [SOAP (TI.1.2) vs REST (TI.1.4)](#5-soap-ti12-vs-rest-ti14)
6. [Requisitos previos](#6-requisitos-previos)
7. [Paso 1 — Servidor REST](#paso-1--servidor-rest-04-servidor)
8. [Paso 2 — Cliente consola](#paso-2--cliente-consola-04-clicon)
9. [Paso 3 — Cliente escritorio](#paso-3--cliente-escritorio-04-cliesc)
10. [Paso 4 — Cliente web](#paso-4--cliente-web-04-cliweb)
11. [Paso 5 — Cliente móvil](#paso-5--cliente-móvil-04-climov)
12. [Paso 6 — Trabajo en red](#paso-6--trabajo-en-red-cada-integrante-en-su-máquina)
13. [Casos de prueba](#casos-de-prueba)
14. [Solución de problemas](#solución-de-problemas)

---

## 1. Arquitectura

```
                         ┌──────────────────────────────────────────────┐
  CLICON (consola)  ───► │                                              │
  CLIESC (escritorio) ─► │   Servidor Kestrel :8086                     │
  CLIWEB (navegador) ──► │   ASP.NET Core Web API (.NET 9) · JSON       │
     └ ASP.NET MVC :5086 │     ├── Controllers  Autenticacion,          │
  CLIMOV (Android) ────► │     │                Temperatura, Longitud,  │
                         │     │                Masa                    │
       REST / HTTP+JSON  │     ├── Modelo       DTOs JSON, ErrorServicio│
                         │     └── Lógica       Convertidores,          │
                         │                      Validación, Autenticador│
                         └──────────────────────────────────────────────┘
```

- **Estilo:** REST sobre HTTP. Cada conversión es un recurso `GET /api/{categoría}/{operación}?valor=`; el login es `POST` con JSON.
- **Formato:** JSON en camelCase.
- **Documentación viva:** Swagger UI en `http://servidor:8086/swagger`.
- **Errores:** código HTTP + cuerpo `{ "codigo": "...", "mensaje": "..." }`. Cada cliente muestra el `mensaje` en rojo.

## 2. Estructura del repositorio

```
01. UML/
    01. ERS/        Especificación de requisitos
    02. ECUD/       Especificación de casos de uso CU01–CU08
    03. DIAGRAMAS/  Casos de uso, actividad, secuencia, clases y arquitectura (.puml + .png)
02. MER/            No aplica (sin base de datos)
03. BDD/            No aplica (sin base de datos)
04. SERVIDOR/       API REST ASP.NET Core Web API (.NET 9) + script para abrir el puerto en red
04. CLICON/         Cliente consola (.NET Framework 4.7.2)
04. CLIESC/         Cliente escritorio Windows Forms (.NET Framework 4.7.2)
04. CLIWEB/         Cliente web ASP.NET Core MVC (.NET 8)
04. CLIMOV/         Cliente móvil Android .NET MAUI (sucesor de Xamarin.Forms, .NET 9)
05. DOCUMENTACIÓN/  Manual técnico, presentación y capturas
```

Cada carpeta `04. …` tiene su propio `LEEME.md` con más detalle.

## 3. Recursos del API

| Método | Ruta | Respuesta |
|---|---|---|
| POST | `/api/autenticacion/login` · cuerpo `{ "usuario", "clave" }` | `200 { autenticado, usuario, mensaje }` · `401 CREDENCIALES_INVALIDAS` |
| GET | `/api/temperatura/{operación}?valor=` | `200 { categoria, operacion, valor, unidadOrigen, unidadDestino, resultado }` · `400 VALOR_INVALIDO` |
| GET | `/api/longitud/{operación}?valor=` | igual |
| GET | `/api/masa/{operación}?valor=` | igual |

| Categoría | Operaciones | Fórmula / factor |
|---|---|---|
| Temperatura | `celsiusAFahrenheit`, `fahrenheitACelsius` | °F = °C × 9/5 + 32 |
| | `celsiusAKelvin`, `kelvinACelsius` | K = °C + 273,15 |
| | `fahrenheitAKelvin`, `kelvinAFahrenheit` | combinación de las anteriores |
| Longitud | `metrosAPies`, `piesAMetros` | 1 ft = 0,3048 m |
| | `metrosAPulgadas`, `pulgadasAMetros` | 1 in = 0,0254 m |
| | `kilometrosAMillas`, `millasAKilometros` | 1 mi = 1,609344 km |
| Masa | `kilogramosALibras`, `librasAKilogramos` | 1 lb = 0,45359237 kg |
| | `gramosAOnzas`, `onzasAGramos` | 1 oz = 28,349523125 g |

Ejemplo:

```
GET http://localhost:8086/api/temperatura/celsiusAFahrenheit?valor=100

200 OK
{ "categoria": "Temperatura", "operacion": "celsiusAFahrenheit", "valor": 100,
  "unidadOrigen": "°C", "unidadDestino": "°F", "resultado": 212 }
```

**Validaciones del servidor (HTTP 400):**
- La temperatura no puede ser menor al cero absoluto: −273,15 °C, −459,67 °F o 0 K.
- La longitud y la masa no pueden ser negativas.
- `valor` es obligatorio y debe ser un número.

## 4. Patrones de diseño: MVC y Singleton

### MVC en los clientes

| Cliente | Modelo | Vista | Controlador | Servicio (acceso REST) |
|---|---|---|---|---|
| CLICON | `Modelo/Conversion.cs`, `CatalogoConversiones.cs` | `Vista/VistaConsola.cs` | `Controlador/ControladorConsola.cs` | `Servicio/ClienteConversion.cs` |
| CLIESC | `Modelo/CatalogoConversiones.cs` | `Vista/FormLogin`, `FormPrincipal`, `PanelConversion` | `Controlador/ControladorConversion.cs` | `Servicio/ClienteConversion.cs` |
| CLIWEB | `Modelo/Catalogo.cs`, `ViewModels.cs` | `Views/Cuenta`, `Views/Conversion` (.cshtml) | `Controllers/CuentaController`, `ConversionController` | `Servicio/ClienteConversion.cs` |
| CLIMOV | `Modelo/Catalogo.cs` | `Vista/LoginPage.xaml`, `PrincipalPage.xaml` | `Controlador/ControladorConversion.cs` | `Servicio/ClienteRest.cs` |

El **servidor** está organizado en capas: `Controllers` (recursos REST), `Modelo` (objetos JSON) y `Logica` (cálculos, validación y autenticación). La capa `Logica` es la misma del TI.1.2: solo cambió la forma de exponerla.

### Singleton

Una sola instancia compartida por toda la aplicación, con constructor privado y acceso por `Instancia`.

| Dónde | Clase | Cómo se usa |
|---|---|---|
| Servidor | `Logica/Autenticador.cs` | `Program.cs` llama una vez a `Autenticador.Configurar(...)`; los controladores usan `Autenticador.Instancia.Validar(usuario, clave)` |
| CLICON | `Servicio/ClienteConversion.cs` | `ClienteConversion.Instancia` (un solo `HttpClient`, con `Lazy<T>`) |
| CLIESC | `Servicio/ClienteConversion.cs` | `ClienteConversion.Instancia` |
| CLIWEB | `Servicio/ClienteConversion.cs` | `builder.Services.AddSingleton<ClienteConversion>()` en `Program.cs` |
| CLIMOV | `Servicio/ClienteRest.cs` y `Controlador/ControladorConversion.cs` | `ClienteRest.Instancia`, `ControladorConversion.Instancia` |

```csharp
public sealed class ClienteConversion
{
    private static readonly Lazy<ClienteConversion> _instancia =
        new Lazy<ClienteConversion>(() => new ClienteConversion());

    public static ClienteConversion Instancia => _instancia.Value;

    private ClienteConversion() { /* crea el HttpClient una sola vez */ }
}
```

## 5. SOAP (TI.1.2) vs REST (TI.1.4)

| | TI.1.2 SOAP | TI.1.4 REST |
|---|---|---|
| Tecnología del servidor | WCF (.NET Framework 4.7.2) en IIS :8085 | ASP.NET Core Web API (.NET 9) en Kestrel :8086 |
| Mensajes | Sobre SOAP en XML | JSON |
| Contrato | WSDL (`?wsdl`) | OpenAPI / Swagger (`/swagger`) |
| Operación | `POST` + cabecera `SOAPAction` | Verbo HTTP + URL (`GET /api/masa/kilogramosALibras?valor=1`) |
| Errores | SOAP Fault con `ErrorServicio` | Código HTTP (400/401/500) + `{ codigo, mensaje }` |
| Clientes | `ChannelFactory` (WCF) o sobre XML a mano | `HttpClient` + JSON, sin paquetes extra |
| Credenciales | `Web.config` | `appsettings.json` |

## 6. Requisitos previos

| Herramienta | Para qué | Cómo instalar |
|---|---|---|
| **Visual Studio 2022 Community** | Abrir y ejecutar todo | Visual Studio Installer |
| ↳ carga *Desarrollo de ASP.NET y web* | Servidor y web | incluye el SDK de .NET |
| ↳ carga *Desarrollo de escritorio de .NET* | Consola y escritorio | incluye .NET Framework 4.7.2 |
| ↳ carga *Desarrollo de interfaz de usuario de aplicaciones multiplataforma de .NET (.NET MAUI)* | Móvil | incluye el SDK de Android y el JDK |
| **SDK de .NET 9** | Servidor, web y móvil | viene con Visual Studio 17.12 o superior |
| Celular o tablet Android 7.0+ | Móvil | con *Depuración USB* activada |

> **Importante:** `C:\Program Files\dotnet\` debe estar en la variable **Path**; si no, el servidor, la web y el móvil aparecen como **"(Descargado)"** en Visual Studio.

> **Regla de oro:** abre **cada solución en su propia ventana de Visual Studio** y no cierres la ventana negra del servidor.

---

## Paso 1 — Servidor REST (`04. SERVIDOR`)

1. Abre `04. SERVIDOR\CONUNI_RESTFUL_DOTNET_SERVIDOR\CONUNI_RESTFUL_DOTNET_SERVIDOR.sln`.
2. En el botón verde debe aparecer **CONUNI_RESTFUL_DOTNET_SERVIDOR**.
3. **Depurar → Iniciar sin depurar** (`Ctrl + F5`). La primera vez descarga el paquete NuGet de Swagger.
4. Se abre una **ventana negra** (Kestrel) que **no debes cerrar**, y el navegador en `http://localhost:8086/swagger`.
5. En Swagger prueba `POST /api/autenticacion/login` y cualquier conversión con **Try it out → Execute**.

**Configuración** (`appsettings.json`):
```json
"Credenciales": { "Usuario": "monster", "Clave": "monster" }
```

## Paso 2 — Cliente consola (`04. CLICON`)

1. Con el servidor encendido, abre **otra ventana** de Visual Studio con `04. CLICON\CONUNI_RESTFUL_DOTNET_CLICON\CONUNI_RESTFUL_DOTNET_CLICON.sln`.
2. **Depurar → Iniciar sin depurar**.
3. Usuario `monster`, clave `monster` (se muestra con `*`).
4. Navega con los números: **[1] Temperatura, [2] Longitud, [3] Masa → conversión → valor**. Con **[0]** vuelves o cierras sesión.

La URL del servidor está en `App.config`:
```xml
<add key="UrlServidor" value="http://localhost:8086" />
```

## Paso 3 — Cliente escritorio (`04. CLIESC`)

1. En otra ventana de Visual Studio, abre `04. CLIESC\CONUNI_RESTFUL_DOTNET_CLIESC\CONUNI_RESTFUL_DOTNET_CLIESC.sln`.
2. **Depurar → Iniciar sin depurar** e ingresa con `monster` / `monster`.
3. En el menú de la izquierda elige **Temperatura**, **Longitud** o **Masa**; elige **De** y **A**, escribe el valor y presiona **CONVERTIR** (o Enter).
4. **⇄** intercambia las unidades; a la derecha está el **Historial de la sesión**; abajo se ve a qué servidor está conectado.
5. La URL está en `App.config` (`UrlServidor`), igual que en la consola.

## Paso 4 — Cliente web (`04. CLIWEB`)

1. En otra ventana de Visual Studio, abre `04. CLIWEB\CONUNI_RESTFUL_DOTNET_CLIWEB\CONUNI_RESTFUL_DOTNET_CLIWEB.sln`.
2. En el botón verde debe aparecer **CONUNI_RESTFUL_DOTNET_CLIWEB**.
3. **Depurar → Iniciar sin depurar**. Si pregunta por un certificado SSL, responde **No**.
4. Se abre una ventana negra (no la cierres) y el navegador en `http://localhost:5086`.
5. Ingresa con `monster` / `monster` y convierte. La página es *responsive*.

La URL del servidor está en `appsettings.json`:
```json
"ServicioRest": { "Url": "http://localhost:8086" }
```

## Paso 5 — Cliente móvil (`04. CLIMOV`)

Aplicación **Android en C# con .NET MAUI** (sucesor oficial de Xamarin.Forms). Se ejecuta en un **celular real**. Se instala como **Conversor REST** (`ec.edu.espe.conuni.rest`), así que convive con la app SOAP del TI.1.2.

1. Celular: *Ajustes → Acerca del teléfono → Información de software* → tocar **7 veces "Número de compilación"** → *Opciones de desarrollador* → **Depuración USB**.
2. Conecta el celular por USB y acepta **"¿Permitir depuración USB?"**.
3. Abre `04. CLIMOV\CONUNI_RESTFUL_DOTNET_CLIMOV\CONUNI_RESTFUL_DOTNET_CLIMOV.sln`, elige tu celular en la flecha ▾ del botón verde y **Depurar → Iniciar sin depurar**.
4. En la app, cuadro **Servidor REST** (solo la dirección base):
   - **Por cable** (prueba rápida): ejecuta `adb reverse tcp:8086 tcp:8086` y escribe `http://localhost:8086`.
     ```powershell
     & "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" reverse tcp:8086 tcp:8086
     ```
   - **Por WiFi** (presentación): `http://IP-DE-LA-PC-SERVIDOR:8086` (ver Paso 6).
   - **Emulador:** `http://10.0.2.2:8086` (valor por defecto; no funciona en un celular real).

> **Ruta larga:** `Directory.Build.props` envía la carpeta `obj` a `%LOCALAPPDATA%\CONUNI_REST_CLIMOV\obj` porque Android falla con rutas de más de 260 caracteres.

---

## Paso 6 — Trabajo en red (cada integrante en su máquina)

| Integrante | Rol | Qué ejecuta |
|---|---|---|
| 1 | Servidor + consola | Pasos 1, 2 y 6.1 en su PC |
| 2 | Web + escritorio | Pasos 3 y 4 en su PC, apuntando a la IP del integrante 1 |
| 3 | Móvil | Paso 5 en su celular real, apuntando a la IP del integrante 1 |

A diferencia del TI.1.2, **no hace falta IIS**: el servidor escucha en `http://*:8086` (todas las tarjetas de red) mientras su ventana negra esté abierta. Todos deben estar en **la misma red WiFi**.

### 6.1 Abrir el puerto (integrante 1, una sola vez)

1. Clic derecho en **Inicio** → **Terminal (Administrador)**.
2. Ejecuta:
   ```powershell
   powershell -ExecutionPolicy Bypass -File "RUTA\DEL\PROYECTO\04. SERVIDOR\abrir_puerto_red.ps1"
   ```
3. El script crea la regla del firewall y muestra la URL del adaptador **Wi-Fi**, por ejemplo `http://192.168.0.106:8086`.
4. Enciende el servidor (Paso 1). Si Windows pregunta si permite el acceso a la red, responde **Permitir**.

### 6.2 Comprobar la red

En el navegador de cada equipo (otra PC o el celular) abre `http://IP-DEL-SERVIDOR:8086/swagger`. Si no aparece Swagger, ningún cliente va a conectar: revisa la WiFi y el firewall.

### 6.3 Apuntar cada cliente a la IP del servidor

| Cliente | Dónde se cambia | Valor |
|---|---|---|
| Consola | `App.config` → `UrlServidor` | `http://IP-DEL-SERVIDOR:8086` |
| Escritorio | `App.config` → `UrlServidor` | igual |
| Web | `appsettings.json` → `"ServicioRest": { "Url": … }` | igual |
| Móvil | Cuadro **Servidor REST** del login de la app | igual |

### 6.4 Prueba final con los 3 integrantes a la vez

| Equipo | Cliente | Conversión | Resultado |
|---|---|---|---|
| PC 1 | Consola | 100 °C → °F | 212 °F |
| PC 2 | Escritorio | 1 m → ft | 3.28084 ft |
| PC 2 | Web | 10 km → mi | 6.213712 mi |
| Celular | Móvil | 1 kg → lb | 2.204623 lb |

---

## Casos de prueba

| # | Caso | Entrada | Resultado esperado |
|---|---|---|---|
| 1 | Login correcto | monster / monster | HTTP 200 · acceso al sistema |
| 2 | Login incorrecto | monster / otra | HTTP 401 · "Usuario o clave incorrectos" |
| 3 | 3 intentos fallidos | clave mala ×3 | Se bloquea / cierra |
| 4 | Celsius → Fahrenheit | 100 | 212 °F |
| 5 | Celsius → Kelvin | 25 | 298.15 K |
| 6 | Bajo el cero absoluto | −300 °C | HTTP 400 · *no puede ser menor al cero absoluto* |
| 7 | Kilómetros → Millas | 10 | 6.213712 mi |
| 8 | Millas → Kilómetros | 1 | 1.609344 km |
| 9 | Metros → Pies | 1 | 3.28084 ft |
| 10 | Longitud negativa | −5 m | HTTP 400 · *La longitud no puede ser negativa* |
| 11 | Kilogramos → Libras | 1 | 2.204623 lb |
| 12 | Gramos → Onzas | 100 | 3.527396 oz |
| 13 | Valor no numérico en el cliente | abc | "Ingrese un número válido" (no llama al servidor) |
| 14 | Valor no numérico directo al API | `?valor=abc` | HTTP 400 · VALOR_INVALIDO |
| 15 | Coma decimal | 36,5 °C | 97.7 °F |
| 16 | Servidor apagado | — | "No se pudo conectar con el servidor…" |

## Solución de problemas

| Síntoma | Causa | Solución |
|---|---|---|
| *"No se pudo conectar con el servidor"* | El servidor está apagado o se cerró su ventana negra | Vuelve a ejecutar el servidor (Paso 1) |
| Funciona en la PC del servidor pero no desde otra | Firewall | Ejecuta `abrir_puerto_red.ps1` como administrador (Paso 6.1) |
| Error al arrancar el servidor: *address already in use* | Otro programa usa el puerto 8086 (o el servidor ya está abierto) | Cierra la otra ventana del servidor |
| Servidor, web o móvil **"(Descargado)"** | `C:\Program Files\dotnet\` no está en el **Path** | Agrégalo al Path y reinicia Visual Studio |
| Móvil: *"El servidor tardó demasiado en responder"* | La app tiene la URL del emulador (`10.0.2.2`) | Cambia la URL en la app (Paso 5) |
| Visual Studio pide instalar un certificado SSL | Certificado de desarrollo de ASP.NET Core | Responde **No**: todo usa HTTP |
| `Ctrl + F5` no hace nada | Teclado de laptop | Usa **Depurar → Iniciar sin depurar** o `Ctrl + Fn + F5` |
