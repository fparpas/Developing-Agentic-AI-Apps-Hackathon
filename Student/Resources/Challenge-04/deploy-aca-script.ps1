#!/usr/bin/env pwsh

# Azure CLI script to deploy WeatherRemoteMCPServer to Azure Container Apps
# This version uses 'az containerapp up' which automatically handles EVERYTHING:
# - Creates resource group (if needed)
# - Creates container app environment (if needed) 
# - Detects .NET project and builds it in the cloud
# - Deploys to Container Apps
# NO Docker, NO Dockerfile, NO manual resource creation required!

param(
    [Parameter(Mandatory = $false)]
    [string]$ResourceGroupName = "AgenticAI",
    
    [Parameter(Mandatory = $false)]
    [string]$Location = "swedencentral",
    
    [Parameter(Mandatory = $false)]
    [string]$ContainerAppName = "weathermcp-server",
    
    [Parameter(Mandatory = $false)]
    [string]$ContainerAppEnvironment = "weathermcp-env"
)

# Set error action preference
$ErrorActionPreference = "Stop"

Write-Host "🚀 Starting ultra-simplified deployment of WeatherRemoteMCPServer to Azure Container Apps..." -ForegroundColor Green
Write-Host "✨ Using 'az containerapp up' - handles everything automatically!" -ForegroundColor Cyan

# Ensure Azure Container Apps extension is installed
Write-Host "Ensuring Azure Container Apps extension is installed..." -ForegroundColor Yellow
az extension add --name containerapp --upgrade --only-show-errors

# Verify Azure CLI is logged in
Write-Host "Verifying Azure CLI authentication..." -ForegroundColor Yellow
try {
    $account = az account show --query "name" --output tsv 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Logged in to Azure account: $account" -ForegroundColor Green
    } else {
        throw "Not logged in"
    }
}
catch {
    Write-Error "❌ Not logged in to Azure. Please run 'az login' first."
    exit 1
}

# Variables
$ProjectPath = ".\csharp\MCP.Server.Remote.Weather"

try {
    # Deploy using source-to-cloud (creates everything automatically)
    Write-Host "Deploying Container App from source code..." -ForegroundColor Yellow
    Write-Host "📦 Azure will automatically:" -ForegroundColor Cyan
    Write-Host "   • Create resource group (if it doesn't exist)" -ForegroundColor Cyan
    Write-Host "   • Create container app environment (if it doesn't exist)" -ForegroundColor Cyan
    Write-Host "   • Detect this is a .NET project and build it" -ForegroundColor Cyan
    Write-Host "   • Deploy to Container Apps" -ForegroundColor Cyan
    
    Push-Location $ProjectPath
    
    try {
        # First, use az containerapp up with basic parameters
        az containerapp up `
            --name $ContainerAppName `
            --resource-group $ResourceGroupName `
            --location $Location `
            --environment $ContainerAppEnvironment `
            --source . `
            --target-port 8080 `
            --ingress external `
            --env-vars "ASPNETCORE_ENVIRONMENT=Production"
        
        # Then update with additional configuration
        Write-Host "Configuring scaling and resource settings..." -ForegroundColor Yellow
        az containerapp update `
            --name $ContainerAppName `
            --resource-group $ResourceGroupName `
            --cpu 0.25 `
            --memory 0.5Gi `
            --min-replicas 1 `
            --max-replicas 1
    }
    finally {
        Pop-Location
    }
    
    # Get the Container App URL
    Write-Host "Retrieving Container App URL..." -ForegroundColor Yellow
    $AppUrl = az containerapp show --name $ContainerAppName --resource-group $ResourceGroupName --query "properties.configuration.ingress.fqdn" --output tsv
    
    Write-Host "`n✅ Deployment completed successfully!" -ForegroundColor Green
    Write-Host "🌐 Container App URL: https://$AppUrl" -ForegroundColor Cyan
    Write-Host "📋 Resource Group: $ResourceGroupName" -ForegroundColor Cyan
    Write-Host "🏃 Container App: $ContainerAppName" -ForegroundColor Cyan
    
    Write-Host "`nTo view logs:" -ForegroundColor Yellow
    Write-Host "az containerapp logs show --name $ContainerAppName --resource-group $ResourceGroupName --follow" -ForegroundColor White
    
    Write-Host "`nTo update the app:" -ForegroundColor Yellow
    Write-Host "cd '$ProjectPath' && az containerapp up --name $ContainerAppName --resource-group $ResourceGroupName --source ." -ForegroundColor White
    
    Write-Host "`nTo clean up resources:" -ForegroundColor Yellow
    Write-Host "az group delete --name $ResourceGroupName --yes --no-wait" -ForegroundColor White
}
catch {
    Write-Error "❌ Deployment failed: $($_.Exception.Message)"
    Write-Host "`nTo clean up any partially created resources:" -ForegroundColor Yellow
    Write-Host "az group delete --name $ResourceGroupName --yes --no-wait" -ForegroundColor White
    exit 1
}

Write-Host "`n🎉 WeatherRemoteMCPServer has been successfully deployed to Azure Container Apps!" -ForegroundColor Green
Write-Host "💡 This deployment used source-to-cloud build - no Docker required!" -ForegroundColor Cyan
