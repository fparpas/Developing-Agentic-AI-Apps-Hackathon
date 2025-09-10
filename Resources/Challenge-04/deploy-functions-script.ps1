#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Ultra-simplified deployment of WeatherMCPServer to Azure Functions using containers
.DESCRIPTION
    This script deploys the WeatherMCPServer as a containerized Azure Function App.
    It uses 'az functionapp create' with container deployment for maximum simplicity.
    
    The script will:
    - Verify Azure CLI authentication
    - Create resource group (if it doesn't exist)
    - Create storage account for Azure Functions
    - Create Azure Container Registry (if needed)
    - Build and push container to ACR
    - Create Function App with container
    - Configure scaling and settings

.PARAMETER ProjectPath
    Path to the WeatherRemoteMcpServer project directory
.PARAMETER ResourceGroup
    Name of the Azure resource group to create/use
.PARAMETER Location
    Azure region for deployment
.PARAMETER FunctionAppName
    Name of the Azure Function App (will append random suffix if not provided)
.PARAMETER StorageAccountName
    Name of the storage account (will append random suffix if not provided)
.PARAMETER ContainerRegistryName
    Name of the container registry (will append random suffix if not provided)
.PARAMETER ImageName
    Name of the container image
.PARAMETER PlanName
    Name of the App Service Plan

.EXAMPLE
    .\deploy-functions-script.ps1
    .\deploy-functions-script.ps1 -ResourceGroup "my-rg" -Location "westus2"
    .\deploy-functions-script.ps1 -FunctionAppName "my-weather-app" -Location "eastus"
    
.NOTES
    Requirements:
    - Azure CLI installed and authenticated
    - Docker (for container build)
    - Source code in WeatherRemoteMcpServer directory
#>

param(
    [Parameter(HelpMessage = "Path to the WeatherRemoteMcpServer project directory")]
    [string]$ProjectPath = ".\src\WeatherRemoteMcpServer",
    
    [Parameter(HelpMessage = "Name of the Azure resource group")]
    [string]$ResourceGroup = "rg-weathermcp-functions",
    
    [Parameter(HelpMessage = "Azure region for deployment")]
    [string]$Location = "swedencentral",
    
    [Parameter(HelpMessage = "Name of the Azure Function App")]
    [string]$FunctionAppName = "weathermcp-functions-$(Get-Random -Minimum 1000 -Maximum 9999)",
    
    [Parameter(HelpMessage = "Name of the storage account")]
    [string]$StorageAccountName = "stweathermcp$(Get-Random -Minimum 100000 -Maximum 999999)",
    
    [Parameter(HelpMessage = "Name of the container registry")]
    [string]$ContainerRegistryName = "crweathermcp$(Get-Random -Minimum 100000 -Maximum 999999)",
    
    [Parameter(HelpMessage = "Name of the container image")]
    [string]$ImageName = "weathermcp-server",
    
    [Parameter(HelpMessage = "Name of the App Service Plan")]
    [string]$PlanName = "plan-weathermcp-functions"
)

Write-Host "🚀 Starting ultra-simplified deployment of WeatherMCPServer to Azure Functions (Container)..." -ForegroundColor Green
Write-Host "✨ Using containerized deployment for Azure Functions!" -ForegroundColor Cyan

# Ensure Azure Functions Core Tools extension is available
Write-Host "Ensuring Azure Functions extension is installed..." -ForegroundColor Yellow
try {
    az extension add --name azure-functions-core-tools --yes --only-show-errors 2>$null
    az extension update --name azure-functions-core-tools --only-show-errors 2>$null
} catch {
    Write-Host "⚠️  Azure Functions extension installation skipped (may already exist)" -ForegroundColor Yellow
}

# Verify Azure CLI authentication
Write-Host "Verifying Azure CLI authentication..." -ForegroundColor Yellow
try {
    $account = az account show --query "user.name" -o tsv 2>$null
    if ($LASTEXITCODE -ne 0) { throw "Not authenticated" }
    Write-Host "✅ Logged in to Azure account: $account" -ForegroundColor Green
}
catch {
    Write-Error "❌ Not logged in to Azure. Please run 'az login' first."
    exit 1
}

try {
    Write-Host "📝 Deployment Configuration:" -ForegroundColor Cyan
    Write-Host "   📂 Source Path: $ProjectPath" -ForegroundColor White
    Write-Host "   🏗️  Resource Group: $ResourceGroup" -ForegroundColor White
    Write-Host "   📍 Location: $Location" -ForegroundColor White
    Write-Host "   ⚡ Function App: $FunctionAppName" -ForegroundColor White
    Write-Host "   💾 Storage Account: $StorageAccountName" -ForegroundColor White
    Write-Host "   📦 Container Registry: $ContainerRegistryName" -ForegroundColor White
    Write-Host ""

    # Check if project directory exists
    if (!(Test-Path $ProjectPath)) {
        Write-Error "❌ Project directory not found: $ProjectPath"
        exit 1
    }

    # Create resource group
    Write-Host "🏗️  Creating resource group..." -ForegroundColor Yellow
    az group create --name $ResourceGroup --location $Location --only-show-errors
    if ($LASTEXITCODE -ne 0) { throw "Failed to create resource group" }
    Write-Host "✅ Resource group created: $ResourceGroup" -ForegroundColor Green

    # Create storage account for Azure Functions
    Write-Host "💾 Creating storage account..." -ForegroundColor Yellow
    az storage account create `
        --name $StorageAccountName `
        --location $Location `
        --resource-group $ResourceGroup `
        --sku Standard_LRS `
        --only-show-errors
    if ($LASTEXITCODE -ne 0) { throw "Failed to create storage account" }
    Write-Host "✅ Storage account created: $StorageAccountName" -ForegroundColor Green

    # Create Azure Container Registry
    Write-Host "📦 Creating Azure Container Registry..." -ForegroundColor Yellow
    az acr create `
        --resource-group $ResourceGroup `
        --name $ContainerRegistryName `
        --sku Basic `
        --admin-enabled true `
        --only-show-errors
    if ($LASTEXITCODE -ne 0) { throw "Failed to create container registry" }
    Write-Host "✅ Container Registry created: $ContainerRegistryName" -ForegroundColor Green

    # Get ACR login server
    $AcrLoginServer = az acr show --name $ContainerRegistryName --resource-group $ResourceGroup --query "loginServer" -o tsv
    $FullImageName = "$AcrLoginServer/${ImageName}:latest"
    Write-Host "🔗 Container image will be: $FullImageName" -ForegroundColor Cyan

    # Build and push container to ACR
    Write-Host "🐳 Building and pushing container..." -ForegroundColor Yellow
    Push-Location $ProjectPath
    try {
        # Use Functions-specific Dockerfile if it exists, otherwise use default
        $DockerfilePath = if (Test-Path "Dockerfile.functions") { "Dockerfile.functions" } else { "Dockerfile" }
        Write-Host "📄 Using Dockerfile: $DockerfilePath" -ForegroundColor Cyan
        
        az acr build --registry $ContainerRegistryName --resource-group $ResourceGroup --image "${ImageName}:latest" --file $DockerfilePath . --only-show-errors
        if ($LASTEXITCODE -ne 0) { throw "Failed to build and push container" }
        Write-Host "✅ Container built and pushed successfully" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }

    # Get ACR credentials
    Write-Host "🔐 Getting ACR credentials..." -ForegroundColor Yellow
    $AcrUsername = az acr credential show --name $ContainerRegistryName --resource-group $ResourceGroup --query "username" -o tsv
    $AcrPassword = az acr credential show --name $ContainerRegistryName --resource-group $ResourceGroup --query "passwords[0].value" -o tsv

    # Create App Service Plan for Functions (Linux, Basic)
    Write-Host "📋 Creating App Service Plan..." -ForegroundColor Yellow
    az appservice plan create `
        --resource-group $ResourceGroup `
        --name $PlanName `
        --location $Location `
        --is-linux `
        --sku B1 `
        --only-show-errors
    if ($LASTEXITCODE -ne 0) { throw "Failed to create app service plan" }
    Write-Host "✅ App Service Plan created: $PlanName" -ForegroundColor Green

    # Create Function App with container
    Write-Host "⚡ Creating Function App with container..." -ForegroundColor Yellow
    az functionapp create `
        --resource-group $ResourceGroup `
        --plan $PlanName `
        --name $FunctionAppName `
        --storage-account $StorageAccountName `
        --functions-version 4 `
        --runtime custom `
        --only-show-errors
    if ($LASTEXITCODE -ne 0) { throw "Failed to create function app" }
    Write-Host "✅ Function App created: $FunctionAppName" -ForegroundColor Green

    # Configure container settings
    Write-Host "🐳 Configuring container settings..." -ForegroundColor Yellow
    az functionapp config container set `
        --name $FunctionAppName `
        --resource-group $ResourceGroup `
        --docker-custom-image-name $FullImageName `
        --docker-registry-server-url "https://$AcrLoginServer" `
        --docker-registry-server-user $AcrUsername `
        --docker-registry-server-password $AcrPassword `
        --only-show-errors
    if ($LASTEXITCODE -ne 0) { throw "Failed to configure container settings" }
    Write-Host "✅ Function App created: $FunctionAppName" -ForegroundColor Green

    # Configure Function App settings
    Write-Host "⚙️  Configuring Function App settings..." -ForegroundColor Yellow
    az functionapp config appsettings set `
        --name $FunctionAppName `
        --resource-group $ResourceGroup `
        --settings `
        WEBSITES_ENABLE_APP_SERVICE_STORAGE=false `
        WEBSITES_PORT=8080 `
        SCM_DO_BUILD_DURING_DEPLOYMENT=false `
        ENABLE_ORYX_BUILD=false `
        --only-show-errors
    if ($LASTEXITCODE -ne 0) { throw "Failed to configure function app settings" }

    # Enable system-assigned managed identity
    Write-Host "🔐 Enabling managed identity..." -ForegroundColor Yellow
    az functionapp identity assign `
        --name $FunctionAppName `
        --resource-group $ResourceGroup `
        --only-show-errors
    if ($LASTEXITCODE -ne 0) { throw "Failed to enable managed identity" }

    # Get Function App URL
    Write-Host "Retrieving Function App URL..." -ForegroundColor Yellow
    $FunctionAppUrl = az functionapp show --name $FunctionAppName --resource-group $ResourceGroup --query "defaultHostName" -o tsv
    $FullUrl = "https://$FunctionAppUrl"

    Write-Host ""
    Write-Host "✅ Deployment completed successfully!" -ForegroundColor Green
    Write-Host "🌐 Function App URL: $FullUrl" -ForegroundColor Cyan
    Write-Host "📋 Resource Group: $ResourceGroup" -ForegroundColor White
    Write-Host "⚡ Function App: $FunctionAppName" -ForegroundColor White
    Write-Host "📦 Container Registry: $ContainerRegistryName" -ForegroundColor White
    Write-Host "💾 Storage Account: $StorageAccountName" -ForegroundColor White
    Write-Host ""
    Write-Host "To view logs:" -ForegroundColor Yellow
    Write-Host "az functionapp logs tail --name $FunctionAppName --resource-group $ResourceGroup" -ForegroundColor Gray
    Write-Host ""
    Write-Host "To update the container:" -ForegroundColor Yellow
    Write-Host "cd '$ProjectPath' && az acr build --registry $ContainerRegistryName --resource-group $ResourceGroup --image ${ImageName}:latest ." -ForegroundColor Gray
    Write-Host "az functionapp config container set --name $FunctionAppName --resource-group $ResourceGroup --docker-custom-image-name $FullImageName" -ForegroundColor Gray
    Write-Host ""
    Write-Host "To clean up resources:" -ForegroundColor Yellow
    Write-Host "az group delete --name $ResourceGroup --yes --no-wait" -ForegroundColor Gray
    Write-Host ""
    Write-Host "🎉 WeatherMCPServer has been successfully deployed to Azure Functions!" -ForegroundColor Green
    Write-Host "💡 This deployment uses containerized Azure Functions for full control!" -ForegroundColor Cyan

} catch {
    Write-Error "❌ Deployment failed: $_"
    Write-Host ""
    Write-Host "🧹 Cleaning up partial deployment..." -ForegroundColor Yellow
    Write-Host "To manually clean up, run:" -ForegroundColor Yellow
    Write-Host "az group delete --name $ResourceGroup --yes --no-wait" -ForegroundColor Gray
    exit 1
}
