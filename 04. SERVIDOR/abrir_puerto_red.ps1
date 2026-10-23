# ===================================================================
#  Prepara la PC del servidor REST para que los demás integrantes
#  (web, escritorio, móvil) se conecten por WiFi al puerto 8086.
#
#  EJECUTAR COMO ADMINISTRADOR (una sola vez por PC):
#    clic derecho en el menú Inicio > "Terminal (Administrador)" y luego:
#    powershell -ExecutionPolicy Bypass -File ".\abrir_puerto_red.ps1"
#
#  Después basta con ejecutar el servidor en Visual Studio (Ctrl + F5):
#  el perfil escucha en http://*:8086, es decir, en todas las tarjetas de red.
# ===================================================================

$ErrorActionPreference = 'Stop'
$Puerto = 8086
$Regla  = "CONUNI REST $Puerto"

function Paso($texto) { Write-Host "`n>> $texto" -ForegroundColor Cyan }

$admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $admin) { throw 'Abra la terminal COMO ADMINISTRADOR y vuelva a ejecutar el script.' }

# 1. Firewall: permitir conexiones entrantes al puerto 8086 (redes públicas y privadas)
Paso "1/2  Abriendo el puerto $Puerto en el firewall..."
if (-not (Get-NetFirewallRule -DisplayName $Regla -ErrorAction SilentlyContinue)) {
    New-NetFirewallRule -DisplayName $Regla -Direction Inbound -Protocol TCP `
        -LocalPort $Puerto -Action Allow -Profile Any | Out-Null
    Write-Host "   Regla '$Regla' creada." -ForegroundColor Green
} else {
    Write-Host "   La regla '$Regla' ya existía." -ForegroundColor Green
}

# 2. Comprobación y URL que deben usar los demás
Paso '2/2  Comprobando...'
try {
    $r = Invoke-WebRequest "http://localhost:$Puerto/swagger/v1/swagger.json" -UseBasicParsing -TimeoutSec 5
    Write-Host "   Servidor encendido (HTTP $($r.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "   El servidor aún no está encendido: ejecútelo en Visual Studio (Ctrl + F5) y pruebe de nuevo." -ForegroundColor Yellow
}

$ips = Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.IPAddress -notmatch '^(127|169)\.' -and $_.InterfaceAlias -match 'Wi-?Fi|Ethernet' }
Write-Host "`nListo. Los demás integrantes usan una de estas URL (la del adaptador Wi-Fi):" -ForegroundColor Green
foreach ($ip in $ips) { Write-Host ("   {0,-12} http://{1}:{2}" -f $ip.InterfaceAlias, $ip.IPAddress, $Puerto) }
Write-Host "`nPrueba desde el navegador de otro equipo: http://IP:$Puerto/swagger"
