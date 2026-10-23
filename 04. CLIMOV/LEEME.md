# CLIMOV — Cliente móvil (.NET MAUI, sucesor de Xamarin.Forms)

Aplicación Android en C# que consume el API REST del servidor. Se abre y ejecuta en **Visual Studio 2022**.

## Estructura (MVC + Singleton)

```
CONUNI_RESTFUL_DOTNET_CLIMOV.sln
Directory.Build.props        (obj en ruta corta: Android falla con rutas > 260 caracteres)
CONUNI_RESTFUL_DOTNET_CLIMOV/
├── Modelo/Catalogo.cs                    Categoria, Unidad, Conversion (con su ruta REST) y las 16 conversiones
├── Servicio/ClienteRest.cs               HttpClient + JSON · SINGLETON (ClienteRest.Instancia)
├── Controlador/ControladorConversion.cs  login, conversión, formato, historial · SINGLETON
├── Vista/LoginPage.xaml                  CU01 login (3 intentos) + CU08 URL del servidor
├── Vista/PrincipalPage.xaml              CU02–CU07: categorías, De/A, intercambiar, historial, salir
└── Platforms/Android/AndroidManifest.xml permiso INTERNET + usesCleartextTraffic (HTTP sin TLS)
```

El identificador de la app es `ec.edu.espe.conuni.rest` y su nombre en el celular **Conversor REST**, así que se puede tener instalada junto a la app SOAP del TI.1.2.

## Cómo funciona la llamada REST

1. Login: `POST http://SERVIDOR:8086/api/autenticacion/login` con el JSON `{ "usuario": "monster", "clave": "monster" }`. Si responde **401**, la clave es incorrecta.
2. Conversión: `GET http://SERVIDOR:8086/api/temperatura/celsiusAFahrenheit?valor=100` → `{ ..., "resultado": 212 }`.
3. Si responde **400**, la app muestra el `mensaje` del JSON de error (por ejemplo, el del cero absoluto).

## Ejecutar en un celular real

1. Celular: Ajustes → Acerca del teléfono → tocar 7 veces **Número de compilación** → Opciones de desarrollador → activar **Depuración USB**.
2. Conectar por USB y aceptar "¿Permitir depuración USB?".
3. Visual Studio: abrir `CONUNI_RESTFUL_DOTNET_CLIMOV.sln`, elegir el celular en la lista de dispositivos y **Depurar → Iniciar sin depurar**.
4. En la app, campo **Servidor REST** (solo la dirección base, sin `/api`):
   - Por USB (prueba rápida, después de `adb reverse tcp:8086 tcp:8086`): `http://localhost:8086`
   - Por WiFi (servidor con el puerto abierto, ver `04. SERVIDOR\LEEME.md`): `http://IP-DE-LA-PC:8086`
   - Emulador: `http://10.0.2.2:8086`
5. Usuario `monster`, clave `monster`.

Comando `adb reverse` completo:

```powershell
& "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" reverse tcp:8086 tcp:8086
```
