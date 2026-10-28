# ===================================================================
#  Publica el API REST CONUNI en IIS (puerto 8086) para que quede
#  encendido siempre y los demás integrantes (web, escritorio, móvil)
#  se conecten por WiFi aunque Visual Studio esté cerrado.
#
#  ANTES (una sola vez): instalar el "ASP.NET Core Hosting Bundle" de .NET 9
#  https://dotnet.microsoft.com/download/dotnet/9.0  → columna "ASP.NET Core Runtime" → "Hosting Bundle"
#
#  EJECUTAR COMO ADMINISTRADOR:
#    clic derecho en el menú Inicio > "Terminal (Administrador)" y luego:
#    powershell -ExecutionPolicy Bypass -File ".\publicar_iis.ps1"
#
#  Antes de ejecutarlo, CIERRE la ventana negra del servidor en Visual Studio
#  (Kestrel también usa el puerto 8086).
# ===================================================================

$ErrorActionPreference = 'Stop'
$Puerto   = 8086
$Sitio    = 'CONUNI_REST'
$Destino  = 'C:\inetpub\conuni_rest'
$Proyecto = Join-Path $PSScriptRoot 'CONUNI_RESTFUL_DOTNET_SERVIDOR\CONUNI_RESTFUL_DOTNET_SERVIDOR\CONUNI_RESTFUL_DOTNET_SERVIDOR.csproj'
$Dotnet   = Join-Path $env:ProgramFiles 'dotnet\dotnet.exe'

function Paso($texto) { Write-Host "`n>> $texto" -ForegroundColor Cyan }

$admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $admin) { throw 'Abra la terminal COMO ADMINISTRADOR y vuelva a ejecutar el script.' }
if (-not (Test-Path $Dotnet)) { throw "No se encontró $Dotnet. Instale el SDK de .NET 9 (viene con Visual Studio 2022)." }

# 1. Características de Windows: IIS y su consola de administración
Paso '1/6  Activando IIS...'
foreach ($c in 'IIS-WebServerRole', 'IIS-WebServer', 'IIS-ManagementConsole', 'IIS-ManagementScriptingTools') {
    if ((Get-WindowsOptionalFeature -Online -FeatureName $c).State -ne 'Enabled') {
        Write-Host "   activando $c"
        Enable-WindowsOptionalFeature -Online -FeatureName $c -All -NoRestart | Out-Null
    }
}

# 2. El módulo ASP.NET Core (AspNetCoreModuleV2) lo instala el Hosting Bundle
Paso '2/6  Comprobando el ASP.NET Core Hosting Bundle...'
if (-not (Test-Path "$env:ProgramFiles\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll")) {
    throw "Falta el ASP.NET Core Hosting Bundle de .NET 9. Descárguelo de https://dotnet.microsoft.com/download/dotnet/9.0 (Hosting Bundle), instálelo y vuelva a ejecutar este script."
}
Write-Host '   Hosting Bundle instalado.' -ForegroundColor Green

# 3. Si el sitio ya existe, detenerlo para poder reemplazar los archivos
Import-Module WebAdministration
if (Test-Path "IIS:\Sites\$Sitio") { Stop-Website -Name $Sitio -ErrorAction SilentlyContinue }
if ((Test-Path "IIS:\AppPools\$Sitio") -and (Get-WebAppPoolState -Name $Sitio).Value -ne 'Stopped') {
    Stop-WebAppPool -Name $Sitio; Start-Sleep -Seconds 3
}
$enUso = Get-NetTCPConnection -LocalPort $Puerto -State Listen -ErrorAction SilentlyContinue
if ($enUso) {
    $proceso = (Get-Process -Id $enUso[0].OwningProcess -ErrorAction SilentlyContinue).ProcessName
    throw "El puerto $Puerto está ocupado por '$proceso'. Cierre la ventana negra del servidor en Visual Studio y vuelva a ejecutar."
}

# 4. Publicar (compila en Release y copia solo lo necesario, con el web.config para IIS)
Paso "3/6  Publicando el servidor en $Destino..."
& $Dotnet publish $Proyecto -c Release -o $Destino --nologo
if ($LASTEXITCODE -ne 0) { throw 'La publicación falló (revise los errores de compilación de arriba).' }

# 5. Grupo de aplicaciones "Sin código administrado" (ASP.NET Core no usa el CLR de IIS) y sitio en el puerto 8086
Paso "4/6  Creando el sitio '$Sitio' en el puerto $Puerto..."
if (-not (Test-Path "IIS:\AppPools\$Sitio")) { New-WebAppPool -Name $Sitio | Out-Null }
Set-ItemProperty "IIS:\AppPools\$Sitio" -Name managedRuntimeVersion -Value ''
if (Test-Path "IIS:\Sites\$Sitio") { Remove-Website -Name $Sitio }
New-Website -Name $Sitio -Port $Puerto -PhysicalPath $Destino -ApplicationPool $Sitio | Out-Null
icacls $Destino /grant "IIS AppPool\$($Sitio):(OI)(CI)RX" /T /Q | Out-Null
Start-WebAppPool -Name $Sitio
Start-Website -Name $Sitio

# 6. Firewall: permitir conexiones entrantes al puerto 8086 (la misma regla de abrir_puerto_red.ps1)
Paso "5/6  Abriendo el puerto $Puerto en el firewall..."
if (-not (Get-NetFirewallRule -DisplayName "CONUNI REST $Puerto" -ErrorAction SilentlyContinue)) {
    New-NetFirewallRule -DisplayName "CONUNI REST $Puerto" -Direction Inbound -Protocol TCP `
        -LocalPort $Puerto -Action Allow -Profile Any | Out-Null
}

# 7. Comprobación
Paso '6/6  Comprobando...'
try {
    $r = Invoke-WebRequest "http://localhost:$Puerto/api/temperatura/celsiusAFahrenheit?valor=100" -UseBasicParsing -TimeoutSec 30
    Write-Host "   API OK (HTTP $($r.StatusCode)): $($r.Content)" -ForegroundColor Green
} catch {
    Write-Host "   No respondió: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Revise el Visor de eventos > Registros de Windows > Aplicación (origen IIS AspNetCore Module V2)." -ForegroundColor Yellow
}

$ips = Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.IPAddress -notmatch '^(127|169)\.' -and $_.InterfaceAlias -match 'Wi-?Fi|Ethernet' }
Write-Host "`nListo. El servidor queda encendido en IIS. Los demás integrantes usan la URL del adaptador Wi-Fi:" -ForegroundColor Green
foreach ($ip in $ips) { Write-Host ("   {0,-12} http://{1}:{2}" -f $ip.InterfaceAlias, $ip.IPAddress, $Puerto) }
Write-Host "`nSwagger: http://localhost:$Puerto/swagger"
Write-Host "Panel de monitoreo (usuario y clave del API): http://localhost:$Puerto/monitor" -ForegroundColor Green
Write-Host "Si cambia el código del servidor, vuelva a ejecutar este script."
