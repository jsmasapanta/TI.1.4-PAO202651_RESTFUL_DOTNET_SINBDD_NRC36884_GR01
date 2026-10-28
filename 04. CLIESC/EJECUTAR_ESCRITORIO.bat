@echo off
rem ==================================================================
rem  Abre el cliente de escritorio (Monsters Inc. Converter) con doble clic.
rem  Si todavia no esta compilado, lo compila con el MSBuild de Visual Studio.
rem  En la ventana de ingreso se escribe el servidor REST, el usuario y la clave.
rem ==================================================================
title Cliente de escritorio - Conversor REST
set "PROYECTO=%~dp0CONUNI_RESTFUL_DOTNET_CLIESC\CONUNI_RESTFUL_DOTNET_CLIESC"
set "EXE=%PROYECTO%\bin\Debug\CONUNI_RESTFUL_DOTNET_CLIESC.exe"

if exist "%EXE%" goto abrir

echo Compilando el cliente de escritorio por primera vez...
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" (
    echo No se encontro Visual Studio. Abra la solucion y compilela una vez ^(Compilar ^> Compilar solucion^).
    pause
    exit /b 1
)
for /f "usebackq delims=" %%i in (`"%VSWHERE%" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do set "MSBUILD=%%i"
"%MSBUILD%" "%PROYECTO%\CONUNI_RESTFUL_DOTNET_CLIESC.csproj" /p:Configuration=Debug /v:minimal /nologo
if not exist "%EXE%" (
    echo No se pudo compilar. Revise los errores de arriba.
    pause
    exit /b 1
)

:abrir
start "" "%EXE%"
