#!/usr/bin/env pwsh

param(
    [Parameter()]
    [string]$ServerUrl = "http://localhost:8080",
    
    [Parameter()]
    [switch]$Help
)

if ($Help) {
    Write-Host "MCP Server HTTP Test Client" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage:"
    Write-Host "  ./test-client.ps1 [-ServerUrl <url>]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Parameters:"
    Write-Host "  -ServerUrl   MCP server URL (default: http://localhost:8080)" -ForegroundColor Cyan
    Write-Host "  -Help        Show this help message" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  ./test-client.ps1                                    # Test localhost:8080"
    Write-Host "  ./test-client.ps1 -ServerUrl http://server:9000      # Test remote server"
    Write-Host ""
    exit 0
}

Write-Host "🧪 MCP Server HTTP Test Client" -ForegroundColor Green
Write-Host "🎯 Target: $ServerUrl" -ForegroundColor Cyan
Write-Host ""

function Test-McpEndpoint {
    param([string]$Url, [string]$Method = "GET", [object]$Body = $null)
    
    try {
        $headers = @{
            "Content-Type" = "application/json"
            "Accept" = "application/json"
        }
        
        $params = @{
            Uri = $Url
            Method = $Method
            Headers = $headers
            TimeoutSec = 10
        }
        
        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
        }
        
        $response = Invoke-WebRequest @params
        return @{
            Success = $true
            StatusCode = $response.StatusCode
            Content = $response.Content
        }
    } catch {
        return @{
            Success = $false
            Error = $_.Exception.Message
            StatusCode = if ($_.Exception.Response) { $_.Exception.Response.StatusCode } else { "N/A" }
        }
    }
}

# Test 1: Basic connectivity
Write-Host "1️⃣ Testing basic connectivity..." -ForegroundColor Yellow
$result = Test-McpEndpoint -Url "$ServerUrl/"
if ($result.Success) {
    Write-Host "   ✅ Server is reachable (Status: $($result.StatusCode))" -ForegroundColor Green
} else {
    Write-Host "   ❌ Server unreachable: $($result.Error)" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Make sure the server is running in HTTP mode:" -ForegroundColor Magenta
    Write-Host "   ./run-server.ps1 -Transport http" -ForegroundColor Yellow
    exit 1
}

# Test 2: MCP endpoint
Write-Host "2️⃣ Testing MCP endpoint..." -ForegroundColor Yellow
$result = Test-McpEndpoint -Url "$ServerUrl/mcp"
if ($result.Success) {
    Write-Host "   ✅ MCP endpoint available (Status: $($result.StatusCode))" -ForegroundColor Green
} else {
    Write-Host "   ⚠️ MCP endpoint test: $($result.Error)" -ForegroundColor Orange
}

# Test 3: List tools (if supported)
Write-Host "3️⃣ Testing tools discovery..." -ForegroundColor Yellow
$toolsRequest = @{
    jsonrpc = "2.0"
    id = 1
    method = "tools/list"
    params = @{}
}
$result = Test-McpEndpoint -Url "$ServerUrl/mcp" -Method "POST" -Body $toolsRequest
if ($result.Success) {
    Write-Host "   ✅ Tools discovery successful" -ForegroundColor Green
    try {
        $response = $result.Content | ConvertFrom-Json
        if ($response.result -and $response.result.tools) {
            $toolCount = $response.result.tools.Count
            Write-Host "   📋 Found $toolCount tools available" -ForegroundColor Cyan
            
            if ($toolCount -gt 0) {
                Write-Host "   🔧 Sample tools:" -ForegroundColor Cyan
                $response.result.tools | Select-Object -First 5 | ForEach-Object {
                    Write-Host "      • $($_.name)" -ForegroundColor Gray
                }
                if ($toolCount -gt 5) {
                    Write-Host "      ... and $($toolCount - 5) more" -ForegroundColor Gray
                }
            }
        }
    } catch {
        Write-Host "   ⚠️ Could not parse tools response" -ForegroundColor Orange
    }
} else {
    Write-Host "   ❌ Tools discovery failed: $($result.Error)" -ForegroundColor Red
}

# Test 4: Server info
Write-Host "4️⃣ Testing server info..." -ForegroundColor Yellow
$infoRequest = @{
    jsonrpc = "2.0"
    id = 2
    method = "initialize"
    params = @{
        protocolVersion = "2024-11-05"
        capabilities = @{}
        clientInfo = @{
            name = "test-client"
            version = "1.0.0"
        }
    }
}
$result = Test-McpEndpoint -Url "$ServerUrl/mcp" -Method "POST" -Body $infoRequest
if ($result.Success) {
    Write-Host "   ✅ Server initialization successful" -ForegroundColor Green
    try {
        $response = $result.Content | ConvertFrom-Json
        if ($response.result -and $response.result.serverInfo) {
            $serverInfo = $response.result.serverInfo
            Write-Host "   📋 Server: $($serverInfo.name) v$($serverInfo.version)" -ForegroundColor Cyan
        }
    } catch {
        Write-Host "   ⚠️ Could not parse server info" -ForegroundColor Orange
    }
} else {
    Write-Host "   ❌ Server initialization failed: $($result.Error)" -ForegroundColor Red
}

Write-Host ""
Write-Host "🎉 Remote MCP Server testing complete!" -ForegroundColor Green
Write-Host "🔗 Server URL: $ServerUrl" -ForegroundColor Cyan
Write-Host ""
Write-Host "💡 To connect an MCP client:" -ForegroundColor Magenta
Write-Host "   Server URL: $ServerUrl" -ForegroundColor Yellow
Write-Host "   Protocol: HTTP/HTTPS" -ForegroundColor Yellow
Write-Host "   Endpoint: /mcp" -ForegroundColor Yellow