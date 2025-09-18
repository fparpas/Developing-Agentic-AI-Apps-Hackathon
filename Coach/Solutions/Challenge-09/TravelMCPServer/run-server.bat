@echo off
setlocal enabledelayedexpansion

echo 🚀 Travel MCP Server Quick Launcher
echo.

if "%1"=="" goto stdio_mode
if "%1"=="http" goto http_mode
if "%1"=="stdio" goto stdio_mode
if "%1"=="--help" goto help
if "%1"=="-h" goto help

goto help

:stdio_mode
echo 📡 Starting in stdio mode...
set Transport__Mode=stdio
goto run_server

:http_mode
echo 🌐 Starting in HTTP mode on localhost:8080...
set Transport__Mode=http
set Transport__HttpHost=localhost
set Transport__HttpPort=8080
echo 🔗 Server will be available at: http://localhost:8080
echo 💡 Test endpoint: http://localhost:8080/mcp
goto run_server

:run_server
echo 🔨 Building project...
dotnet build --nologo --verbosity quiet
if errorlevel 1 (
    echo ❌ Build failed!
    pause
    exit /b 1
)
echo ✅ Build successful
echo.
echo ▶️ Starting server...
dotnet run --no-build --verbosity quiet
goto end

:help
echo Usage:
echo   run-server.bat [mode]
echo.
echo Modes:
echo   stdio    - Run in stdio mode (default, for pipe communication)
echo   http     - Run in HTTP mode on localhost:8080
echo   --help   - Show this help
echo.
echo Examples:
echo   run-server.bat           # Run in stdio mode
echo   run-server.bat stdio     # Run in stdio mode  
echo   run-server.bat http      # Run in HTTP mode
echo.
echo For advanced options, use run-server.ps1
pause

:end