#!/usr/bin/env pwsh

# Azure CLI script to deploy the Travel MCP Server to Azure Container Apps.
# Modeled on the Challenge-04 ACA deployment, but uses an explicit, robust flow:
#   1. Create the resource group
#   2. Create an Azure Container Registry (ACR)
#   3. Build the image in the cloud with 'az acr build' (uses the Dockerfile; no local Docker)
#   4. Create the Container Apps environment
#   5. Create/update the Container App from the prebuilt image
#
# This avoids the 'az containerapp up --source' auto-ACR path, which currently
# fails with: AttributeError: 'NoneType' object has no attribute 'linux'
# (a known Azure CLI containerapp extension bug when it auto-creates the registry).

param(
    [Parameter(Mandatory = $false)]
    [string]$ResourceGroupName = "rg-travelmcp-demo",

    [Parameter(Mandatory = $false)]
    [string]$Location = "eastus",

    [Parameter(Mandatory = $false)]
    [string]$ContainerAppName = "travelmcp-server",

    [Parameter(Mandatory = $false)]
    [string]$ContainerAppEnvironment = "travelmcp-env",

    # ACR name (5-50 lowercase alphanumeric, globally unique). Auto-generated if empty.
    [Parameter(Mandatory = $false)]
    [string]$AcrName = "",

    [Parameter(Mandatory = $false)]
    [string]$ImageName = "travelmcp-server",

    [Parameter(Mandatory = $false)]
    [string]$ImageTag = "latest",

    # Path to the Travel MCP server project folder (defaults to this script's folder).
    [Parameter(Mandatory = $false)]
    [string]$ProjectPath = $PSScriptRoot,

    # Amadeus API credentials used by the Travel MCP tools.
    # Provide your own; defaults read from the AMADEUS_CLIENT_ID / AMADEUS_CLIENT_SECRET
    # environment variables if set.
    [Parameter(Mandatory = $false)]
    [string]$AmadeusClientId = $env:AMADEUS_CLIENT_ID,

    [Parameter(Mandatory = $false)]
    [string]$AmadeusClientSecret = $env:AMADEUS_CLIENT_SECRET,

    [Parameter(Mandatory = $false)]
    [string]$AmadeusBaseUrl = "https://test.api.amadeus.com"
)

$ErrorActionPreference = "Stop"

# Helper: run an az command and fail fast if it returns a non-zero exit code.
function Invoke-Az {
    param([Parameter(Mandatory = $true)][scriptblock]$Command)
    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw "Azure CLI command failed (exit code $LASTEXITCODE): $($Command.ToString().Trim())"
    }
}

Write-Host "🚀 Starting deployment of Travel MCP Server to Azure Container Apps..." -ForegroundColor Green

# Ensure Azure Container Apps extension is installed
Write-Host "Ensuring Azure Container Apps extension is installed..." -ForegroundColor Yellow
az extension add --name containerapp --upgrade --only-show-errors

# Verify Azure CLI is logged in
Write-Host "Verifying Azure CLI authentication..." -ForegroundColor Yellow
$account = az account show --query "name" --output tsv 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Not logged in to Azure. Please run 'az login' first."
    exit 1
}
Write-Host "✅ Logged in to Azure account: $account" -ForegroundColor Green

# Validate Amadeus credentials are provided (the Travel MCP tools require them)
if ([string]::IsNullOrWhiteSpace($AmadeusClientId) -or [string]::IsNullOrWhiteSpace($AmadeusClientSecret)) {
    Write-Error "❌ Amadeus credentials are required. Pass -AmadeusClientId and -AmadeusClientSecret, or set the AMADEUS_CLIENT_ID / AMADEUS_CLIENT_SECRET environment variables."
    exit 1
}

# Generate a unique ACR name if one was not provided
if ([string]::IsNullOrWhiteSpace($AcrName)) {
    $suffix = -join ((1..8) | ForEach-Object { '{0:x}' -f (Get-Random -Maximum 16) })
    $AcrName = "travelmcpacr$suffix"
}

$imageRef = "$ImageName`:$ImageTag"

try {
    # 1. Resource group
    Write-Host "Creating resource group '$ResourceGroupName'..." -ForegroundColor Yellow
    Invoke-Az { az group create --name $ResourceGroupName --location $Location --output none }

    # 2. Azure Container Registry
    Write-Host "Creating Azure Container Registry '$AcrName'..." -ForegroundColor Yellow
    Invoke-Az {
        az acr create `
            --resource-group $ResourceGroupName `
            --name $AcrName `
            --sku Basic `
            --admin-enabled true `
            --location $Location `
            --output none
    }

    # 3. Build the image in the cloud from the Dockerfile (no local Docker required)
    Write-Host "Building image '$imageRef' in ACR from the Dockerfile..." -ForegroundColor Yellow
    Push-Location $ProjectPath
    try {
        Invoke-Az { az acr build --registry $AcrName --image $imageRef --file Dockerfile . }
    }
    finally {
        Pop-Location
    }

    # 4. Container Apps environment (create only if it doesn't already exist)
    $envExists = az containerapp env show --name $ContainerAppEnvironment --resource-group $ResourceGroupName --query "name" --output tsv 2>$null
    if ([string]::IsNullOrWhiteSpace($envExists)) {
        Write-Host "Creating Container Apps environment '$ContainerAppEnvironment'..." -ForegroundColor Yellow
        Invoke-Az {
            az containerapp env create `
                --name $ContainerAppEnvironment `
                --resource-group $ResourceGroupName `
                --location $Location `
                --output none
        }
    }
    else {
        Write-Host "Container Apps environment '$ContainerAppEnvironment' already exists. Reusing." -ForegroundColor Yellow
    }

    # Registry login server + admin credentials (used by the Container App to pull the image)
    $acrLoginServer = az acr show --name $AcrName --resource-group $ResourceGroupName --query "loginServer" --output tsv
    $acrUsername = az acr credential show --name $AcrName --resource-group $ResourceGroupName --query "username" --output tsv
    $acrPassword = az acr credential show --name $AcrName --resource-group $ResourceGroupName --query "passwords[0].value" --output tsv
    $fullImage = "$acrLoginServer/$imageRef"

    # Environment variables for the container.
    # HttpHost=0.0.0.0 is required so Container Apps ingress can reach the server.
    # Amadeus__* maps to the "Amadeus:*" configuration section.
    $envVars = @(
        "ASPNETCORE_ENVIRONMENT=Production",
        "HttpHost=0.0.0.0",
        "HttpPort=8080",
        "Amadeus__ClientId=$AmadeusClientId",
        "Amadeus__ClientSecret=$AmadeusClientSecret",
        "Amadeus__BaseUrl=$AmadeusBaseUrl"
    )

    # 5. Create or update the Container App
    $appExists = az containerapp show --name $ContainerAppName --resource-group $ResourceGroupName --query "name" --output tsv 2>$null
    if ([string]::IsNullOrWhiteSpace($appExists)) {
        Write-Host "Creating Container App '$ContainerAppName'..." -ForegroundColor Yellow
        Invoke-Az {
            az containerapp create `
                --name $ContainerAppName `
                --resource-group $ResourceGroupName `
                --environment $ContainerAppEnvironment `
                --image $fullImage `
                --registry-server $acrLoginServer `
                --registry-username $acrUsername `
                --registry-password $acrPassword `
                --target-port 8080 `
                --ingress external `
                --cpu 0.25 `
                --memory 0.5Gi `
                --min-replicas 1 `
                --max-replicas 1 `
                --env-vars @envVars `
                --output none
        }
    }
    else {
        Write-Host "Container App '$ContainerAppName' already exists. Updating image and configuration..." -ForegroundColor Yellow
        Invoke-Az {
            az containerapp registry set `
                --name $ContainerAppName `
                --resource-group $ResourceGroupName `
                --server $acrLoginServer `
                --username $acrUsername `
                --password $acrPassword `
                --output none
        }
        Invoke-Az {
            az containerapp update `
                --name $ContainerAppName `
                --resource-group $ResourceGroupName `
                --image $fullImage `
                --set-env-vars @envVars `
                --output none
        }
    }

    # Retrieve the Container App URL
    Write-Host "Retrieving Container App URL..." -ForegroundColor Yellow
    $AppUrl = az containerapp show --name $ContainerAppName --resource-group $ResourceGroupName --query "properties.configuration.ingress.fqdn" --output tsv

    Write-Host "`n✅ Deployment completed successfully!" -ForegroundColor Green
    Write-Host "🌐 Container App URL: https://$AppUrl" -ForegroundColor Cyan
    Write-Host "🔌 MCP endpoint:      https://$AppUrl (point your MCP client's BaseUrl here)" -ForegroundColor Cyan
    Write-Host "📦 Image:             $fullImage" -ForegroundColor Cyan
    Write-Host "🗄️  Container Registry: $acrLoginServer" -ForegroundColor Cyan
    Write-Host "📋 Resource Group:    $ResourceGroupName" -ForegroundColor Cyan
    Write-Host "🏃 Container App:     $ContainerAppName" -ForegroundColor Cyan

    Write-Host "`nTo view logs:" -ForegroundColor Yellow
    Write-Host "az containerapp logs show --name $ContainerAppName --resource-group $ResourceGroupName --follow" -ForegroundColor White

    Write-Host "`nTo redeploy after code changes (rebuild + update):" -ForegroundColor Yellow
    Write-Host "./deploy-aca-script.ps1 -AcrName $AcrName" -ForegroundColor White

    Write-Host "`nTo clean up resources:" -ForegroundColor Yellow
    Write-Host "az group delete --name $ResourceGroupName --yes --no-wait" -ForegroundColor White
}
catch {
    Write-Error "❌ Deployment failed: $($_.Exception.Message)"
    Write-Host "`nTo clean up any partially created resources:" -ForegroundColor Yellow
    Write-Host "az group delete --name $ResourceGroupName --yes --no-wait" -ForegroundColor White
    exit 1
}

Write-Host "`n🎉 Travel MCP Server has been successfully deployed to Azure Container Apps!" -ForegroundColor Green
Write-Host "💡 Deployment used 'az acr build' for the cloud image build - no local Docker required!" -ForegroundColor Cyan
