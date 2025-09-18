#!/usr/bin/env pwsh

param(
    [Parameter()]
    [ValidateSet("stdio", "http")]
    [string]$Transport = "stdio",
    
    [Parameter()]
    [string]$Host = "localhost",
    
    [Parameter()]
    [int]$Port = 8080,
    
    [Parameter()]
    [switch]$Help
)

if ($Help) {
    Write-Host "Travel MCP Server Launcher" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage:"
    Write-Host "  ./run-server.ps1 [-Transport <stdio|http>] [-Host <hostname>] [-Port <port>]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Parameters:"
    Write-Host "  -Transport   Transport mode: 'stdio' or 'http' (default: stdio)" -ForegroundColor Cyan
    Write-Host "  -Host        HTTP host binding (default: localhost)" -ForegroundColor Cyan  
    Write-Host "  -Port        HTTP port number (default: 8080)" -ForegroundColor Cyan
    Write-Host "  -Help        Show this help message" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  ./run-server.ps1                                    # Run in stdio mode"
    Write-Host "  ./run-server.ps1 -Transport http                    # Run in HTTP mode on localhost:8080"
    Write-Host "  ./run-server.ps1 -Transport http -Port 9000         # Run in HTTP mode on port 9000"
    Write-Host "  ./run-server.ps1 -Transport http -Host 0.0.0.0      # Run in HTTP mode, accept external connections"
    Write-Host ""
    exit 0
}

Write-Host "🚀 Starting Travel MCP Server..." -ForegroundColor Green
Write-Host "📋 Transport: $Transport" -ForegroundColor Cyan

if ($Transport -eq "http") {
    Write-Host "🌐 HTTP Server: http://$Host`:$Port" -ForegroundColor Cyan
    Write-Host "🔗 Test endpoint: http://$Host`:$Port/mcp" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "💡 To connect from an MCP client, use: http://$Host`:$Port" -ForegroundColor Magenta
} else {
    Write-Host "📡 Using stdio transport (pipe-based communication)" -ForegroundColor Cyan
}

Write-Host ""

# Build the project first
Write-Host "🔨 Building project..." -ForegroundColor Yellow
dotnet build --nologo --verbosity quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build successful" -ForegroundColor Green
Write-Host ""

# Run the server with appropriate configuration
$env:Transport__Mode = $Transport
$env:Transport__HttpHost = $Host
$env:Transport__HttpPort = $Port

try {
    dotnet run --no-build --verbosity quiet
} catch {
    Write-Host "❌ Server failed to start: $_" -ForegroundColor Red
    exit 1
}