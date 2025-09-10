# WeatherMCP Azure Functions Deployment

This repository contains a PowerShell script to deploy the WeatherMCP Server as a containerized Azure Function App.

## 📋 Overview

The `deploy-functions-script.ps1` script automates the complete deployment of a WeatherMCP server to Azure Functions using containers. It creates all necessary Azure resources and deploys a containerized .NET 9.0 application that implements the Model Context Protocol (MCP) for weather data.

## 🎯 What Gets Deployed

- **Azure Function App** - Containerized hosting environment
- **App Service Plan** - Basic (B1) tier for consistent performance
- **Azure Container Registry** - Private container storage
- **Storage Account** - Required for Azure Functions runtime
- **Resource Group** - Logical container for all resources
- **Managed Identity** - For secure Azure service authentication

## 🔧 Prerequisites

### Required Tools
- **Azure CLI** (version 2.0 or later)
  ```bash
  # Install Azure CLI
  winget install Microsoft.AzureCLI
  ```
- **PowerShell** (version 5.1 or later)
- **Docker** (for container builds - handled automatically by ACR)

### Required Permissions
- **Contributor** access to Azure subscription
- Ability to create resource groups and resources
- Access to create Azure Container Registry

### Project Structure
The script expects the following project structure:
```
Developing-Agentic-AI-Apps-Hackathon/
├── Resources/
│   └── Challenge-04/
│       ├── deploy-functions-script.ps1     # This script
│       └── README-Functions.md             # This file
└── Coach/
    └── Solutions/
        └── Challenge-05/
            └── WeatherRemoteMcpServer/     # Source code
                ├── Program.cs
                ├── WeatherRemoteMcpServer.csproj
                ├── Dockerfile.functions
                └── Tools/
                    └── WeatherTools.cs
```

## 🚀 Quick Start

### 1. Authentication
```powershell
# Login to Azure
az login

# Verify your subscription
az account show
```

### 2. Basic Deployment
```powershell
# Navigate to script directory
cd "C:\path\to\Developing-Agentic-AI-Apps-Hackathon\Resources\Challenge-04"

# Run with defaults (Sweden Central region)
.\deploy-functions-script.ps1
```

### 3. Custom Deployment
```powershell
# Deploy to specific region with custom names
.\deploy-functions-script.ps1 `
    -ResourceGroup "my-weather-rg" `
    -Location "westeurope" `
    -FunctionAppName "my-weather-functions"
```

## ⚙️ Script Parameters

| Parameter | Description | Default | Required |
|-----------|-------------|---------|----------|
| `ProjectPath` | Path to WeatherRemoteMcpServer directory | `..\..\Coach\Solutions\Challenge-05\WeatherRemoteMcpServer` | No |
| `ResourceGroup` | Azure resource group name | `rg-weathermcp-functions` | No |
| `Location` | Azure region | `swedencentral` | No |
| `FunctionAppName` | Function App name (+ random suffix) | `weathermcp-functions-{random}` | No |
| `StorageAccountName` | Storage account name (+ random suffix) | `stweathermcp{random}` | No |
| `ContainerRegistryName` | Container registry name (+ random suffix) | `crweathermcp{random}` | No |
| `ImageName` | Container image name | `weathermcp-server` | No |
| `PlanName` | App Service Plan name | `plan-weathermcp-functions` | No |

## 📍 Supported Azure Regions

Recommended regions with good quota availability:
- `swedencentral` (default) - EU, green energy
- `westeurope` - EU, high availability
- `eastus` - US East, cost-effective
- `westus2` - US West, latest features

## 🏗️ Deployment Process

The script performs these steps automatically:

1. **🔐 Authentication Check** - Verifies Azure CLI login
2. **🏗️ Resource Group** - Creates or uses existing resource group
3. **💾 Storage Account** - Creates storage for Azure Functions runtime
4. **📦 Container Registry** - Creates private container registry
5. **🐳 Container Build** - Builds and pushes Docker image to ACR
6. **📋 App Service Plan** - Creates Basic (B1) Linux plan
7. **⚡ Function App** - Creates containerized Function App
8. **🐳 Container Config** - Links Function App to container image
9. **⚙️ App Settings** - Configures port and runtime settings
10. **🔐 Managed Identity** - Enables system-assigned identity

## 📊 Expected Output

### Successful Deployment
```
🎉 WeatherMCPServer has been successfully deployed to Azure Functions!
🌐 Function App URL: https://weathermcp-functions-XXXX.azurewebsites.net
📋 Resource Group: rg-weathermcp-functions-se
⚡ Function App: weathermcp-functions-XXXX
```

### Testing the Deployment
```bash
# Test endpoint (expects MCP protocol headers)
curl -v https://weathermcp-functions-XXXX.azurewebsites.net

# Expected response (this means it's working!):
# {"error":{"code":-32000,"message":"Bad Request: Mcp-Session-Id header is required"}}
```

## 🔍 Troubleshooting

### Common Issues

#### 1. Quota Limitations
**Error:** `Operation cannot be completed without additional quota`
**Solution:** Try different regions or request quota increase
```powershell
# Try different region
.\deploy-functions-script.ps1 -Location "westeurope"
```

#### 2. Resource Name Conflicts
**Error:** `The storage account name is already taken`
**Solution:** Script auto-generates random suffixes, but you can specify custom names
```powershell
.\deploy-functions-script.ps1 -StorageAccountName "mystorageaccount$(Get-Random)"
```

#### 3. Authentication Issues
**Error:** `Not logged in to Azure`
**Solution:** 
```bash
az login
az account set --subscription "your-subscription-id"
```

#### 4. Container Build Failures
**Error:** `Failed to build and push container`
**Solution:** Check project path and ensure all files exist
```powershell
# Verify project structure
Get-ChildItem "..\..\Coach\Solutions\Challenge-05\WeatherRemoteMcpServer"
```

### Azure Portal Issues

**"We were not able to load some functions"** - This is expected! The portal can't enumerate custom containerized apps as traditional functions. Your MCP server is still working correctly.

## 🧪 Testing the MCP Server

### 1. Basic Connectivity Test
```bash
curl -v https://your-function-app.azurewebsites.net
```

### 2. MCP Protocol Test
The server expects proper MCP headers. Use an MCP client or tool like the MCP Inspector:
```bash
npx @modelcontextprotocol/inspector
```

### 3. Weather Tools Test
Once connected via MCP protocol, you can test:
- `get_forecast` - Get weather forecast for coordinates
- `get_alerts` - Get weather alerts for US states

## 📝 Management Commands

### View Logs
```bash
az functionapp log tail --name weathermcp-functions-XXXX --resource-group rg-weathermcp-functions-se
```

### Update Container
```bash
# Rebuild and update container
az acr build --registry crweathermcpXXXXXX --resource-group rg-weathermcp-functions-se --image weathermcp-server:latest .
az functionapp config container set --name weathermcp-functions-XXXX --resource-group rg-weathermcp-functions-se --docker-custom-image-name crweathermcpXXXXXX.azurecr.io/weathermcp-server:latest
```

### Scale Function App
```bash
# Scale to 2 instances
az functionapp plan update --name plan-weathermcp-functions --resource-group rg-weathermcp-functions-se --number-of-workers 2
```

### Stop/Start Function App
```bash
# Stop
az functionapp stop --name weathermcp-functions-XXXX --resource-group rg-weathermcp-functions-se

# Start  
az functionapp start --name weathermcp-functions-XXXX --resource-group rg-weathermcp-functions-se
```

## 🧹 Cleanup

### Delete All Resources
```bash
# Delete entire resource group (removes all resources)
az group delete --name rg-weathermcp-functions-se --yes --no-wait
```

### Delete Specific Resources
```bash
# Delete just the Function App
az functionapp delete --name weathermcp-functions-XXXX --resource-group rg-weathermcp-functions-se

# Delete App Service Plan
az appservice plan delete --name plan-weathermcp-functions --resource-group rg-weathermcp-functions-se
```

## 💰 Cost Estimation

### Basic (B1) Plan Costs (approximate)
- **App Service Plan B1:** ~$13.14/month
- **Storage Account:** ~$1-5/month  
- **Container Registry Basic:** ~$5/month
- **Data Transfer:** Variable based on usage

**Total Estimated:** ~$20-25/month for development/testing

### Cost Optimization Tips
- Use **Free tier** App Service Plan for development (limitations apply)
- Delete resources when not in use
- Monitor usage in Azure Cost Management

## 🔒 Security Considerations

### Managed Identity
The deployment enables system-assigned managed identity for:
- Secure access to Azure services
- Elimination of stored credentials
- Future integration capabilities

### Network Security
- HTTPS-only communication
- Azure-managed SSL certificates
- Private container registry access

### Best Practices
- Regularly update container images
- Monitor access logs
- Use Azure Key Vault for sensitive configuration
- Enable Application Insights for monitoring

## 🤝 Support

### Getting Help
1. **Script Issues:** Check this README and troubleshooting section
2. **Azure Issues:** Use `az --help` or Azure documentation
3. **MCP Protocol:** Refer to Model Context Protocol documentation

### Useful Resources
- [Azure Functions Documentation](https://docs.microsoft.com/en-us/azure/azure-functions/)
- [Azure Container Registry](https://docs.microsoft.com/en-us/azure/container-registry/)
- [Model Context Protocol](https://modelcontextprotocol.io/)
- [Azure CLI Reference](https://docs.microsoft.com/en-us/cli/azure/)

---

## 📄 License

This project is part of the Developing Agentic AI Apps Hackathon materials.

## 🏷️ Version

Script Version: 1.0
Last Updated: September 9, 2025
Compatible with: Azure CLI 2.0+, PowerShell 5.1+, .NET 9.0
