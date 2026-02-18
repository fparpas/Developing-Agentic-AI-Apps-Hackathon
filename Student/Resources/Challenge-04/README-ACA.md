# Deploy MCP Server to Azure Container Apps

This guide shows you how to deploy the WeatherMCPServer to Azure Container Apps using the provided deployment script.

## 🚀 Quick Start

**TL;DR:** Just run this command and you're done!
```powershell
.\deploy-aca-script.ps1
```

## 📋 Prerequisites

Before deploying, ensure you have:

### 1. Azure CLI
Install and configure Azure CLI:
```powershell
# Install Azure CLI (Windows)
winget install Microsoft.AzureCLI

# Or download from: https://aka.ms/installazurecliwindows
```

### 2. Azure Account & Login
```powershell
# Login to Azure
az login

# Verify you're logged in
az account show

# Set subscription (if you have multiple)
az account set --subscription "your-subscription-name-or-id"
```

### 3. Required Permissions
Your Azure account needs:
- **Contributor** role on the subscription or resource group
- Ability to create:
  - Resource Groups
  - Container Apps
  - Container App Environments

## 🎯 Deployment Options

### Option 1: Default Deployment (Recommended)
Deploy with all default settings:
```powershell
.\deploy-aca-script.ps1
```

**Default Configuration:**
- **Resource Group:** `rg-weathermcp-demo`
- **Location:** `East US`
- **Container App Name:** `weathermcp-server`
- **Environment Name:** `weathermcp-env`

### Option 2: Custom Configuration
Customize your deployment:
```powershell
.\deploy-aca-script.ps1 `
  -ResourceGroupName "my-weathermcp-rg" `
  -Location "West US 2" `
  -ContainerAppName "my-weather-server" `
   -ContainerAppEnvironment "my-weather-env" `
   -ProjectPath ".\csharp\MCP.Server.Remote.Weather"
```

### Project Source Path
The script now accepts a `-ProjectPath` parameter that points to the root of the source project to build and deploy. By default it uses:

```powershell
ProjectPath = .\csharp\MCP.Server.Remote.Weather
```

Override it if your project lives elsewhere (for example a fork or renamed directory):

```powershell
./deploy-aca-script.ps1 -ProjectPath ".\src\WeatherRemoteMCPServer"
```

**Important:** For custom container builds, ensure the `ProjectPath` directory includes a `Dockerfile` at its root. When present:
- `az containerapp up` uses the Dockerfile to build an image
- The image is built remotely and pushed to an Azure-managed registry (or an existing one if already configured) without requiring local Docker.
- You can define additional OS packages, multi-stage builds, or custom startup commands in the Dockerfile.

If both a `.csproj` and a `Dockerfile` exist in the same directory, the Dockerfile takes precedence. Remove or rename the Dockerfile if you prefer the automatic source build.

Example custom path with Dockerfile:
```powershell
./deploy-aca-script.ps1 -ProjectPath .\containers\WeatherRemoteMCPServer
```
Where `containers/WeatherRemoteMCPServer/Dockerfile` contains your custom build instructions.

Minimum requirements for `ProjectPath`:
1. Either a `.csproj` (for source build) OR a `Dockerfile` (for container build)
2. Any project dependencies referenced by the Dockerfile must reside under that path
3. If using Dockerfile, expose the correct port (`8080`) in the final stage

## 📦 What the Script Does

The deployment script automatically handles **everything**:

1. ✅ **Installs Azure Container Apps extension** (if needed)
2. ✅ **Verifies Azure CLI authentication**
3. ✅ **Creates Resource Group** (if it doesn't exist)
4. ✅ **Creates Container App Environment** (if it doesn't exist)
5. ✅ **Detects .NET project** from source code
6. ✅ **Builds application in Azure cloud** (no Docker required!)
7. ✅ **Deploys to Container Apps** with optimal settings
8. ✅ **Configures networking and scaling**

## 🔧 Configuration Details

The deployed container app includes:

| Setting | Value | Description |
|---------|-------|-------------|
| **CPU** | 0.25 cores | Sufficient for MCP server workload |
| **Memory** | 0.5 GB | Optimized memory allocation |
| **Port** | 8080 | Application listening port |
| **Ingress** | External | Accessible from internet |
| **Min Replicas** | 1 | Always running instance |
| **Max Replicas** | 3 | Auto-scales under load |
| **Environment** | Production | .NET environment setting |

## 📊 Deployment Process

### Step-by-Step What Happens:

1. **🔍 Verification Phase**
   ```
   ✅ Azure CLI authenticated
   ✅ Container Apps extension ready
   ```

2. **🏗️ Infrastructure Setup**
   ```
   📦 Creating resource group (if needed)
   🌐 Creating container app environment (if needed)
   ```

3. **🚀 Application Deployment**
   ```
   🔨 Building .NET application in Azure cloud
   📱 Deploying to Container Apps
   🌍 Configuring external access
   ```

4. **✅ Completion**
   ```
   🌐 Container App URL: https://your-app.azurecontainerapps.io
   📋 Resource details displayed
   💡 Management commands provided
   ```

## 🎉 After Deployment

Once deployment completes, you'll see:

```
✅ Deployment completed successfully!
🌐 Container App URL: https://weathermcp-server--abc123.azurecontainerapps.io
📋 Resource Group: rg-weathermcp-demo
🏃 Container App: weathermcp-server
```

## 🔍 Testing Your Deployment

### Basic Health Check
```bash
# Test if the app is running (replace with your URL)
curl https://your-app-url.azurecontainerapps.io
```

## 🔄 Updating Your Application

To update the deployed application with new code:

```powershell
# Navigate to the project directory (default shown; adjust if you overrode -ProjectPath)
cd ".\csharp\MCP.Server.Remote.Weather"

# Deploy updates
az containerapp up --name weathermcp-server --resource-group rg-weathermcp-demo --source .
```

## 🧹 Cleanup

To remove all deployed resources:

```powershell
# Delete the entire resource group (removes everything)
az group delete --name rg-weathermcp-demo --yes --no-wait
```

Or remove just the container app:
```powershell
# Delete only the container app
az containerapp delete --name weathermcp-server --resource-group rg-weathermcp-demo --yes
```

## 🔐 Security Best Practices

The deployment includes several security features:
- ✅ **Non-root container user**
- ✅ **Production environment settings**
- ✅ **Managed identity support**
- ✅ **Private networking options available**

For production deployments, consider:
- Using Azure Key Vault for secrets
- Implementing authentication
- Configuring custom domains with SSL
- Setting up VNet integration

## 📚 Additional Resources

- [Azure Container Apps Documentation](https://docs.microsoft.com/azure/container-apps/)
- [Azure CLI Reference](https://docs.microsoft.com/cli/azure/)
- [Container Apps Pricing](https://azure.microsoft.com/pricing/details/container-apps/)
- [Model Context Protocol Documentation](https://modelcontextprotocol.io/)

