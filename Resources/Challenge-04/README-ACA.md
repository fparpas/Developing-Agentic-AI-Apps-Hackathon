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
  -ContainerAppEnvironment "my-weather-env"
```

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

### View Application Logs
```bash
az containerapp logs show --name weathermcp-server --resource-group rg-weathermcp-demo --follow
```

### Check App Status
```bash
az containerapp show --name weathermcp-server --resource-group rg-weathermcp-demo
```

## 🔄 Updating Your Application

To update the deployed application with new code:

```powershell
# Navigate to the project directory
cd "..\..\Coach\Solutions\Challenge-02\WeatherMcpServer"

# Deploy updates
az containerapp up --name weathermcp-server --resource-group rg-weathermcp-demo --source .
```

## 📊 Monitoring & Management

### View Real-time Logs
```bash
az containerapp logs show --name weathermcp-server --resource-group rg-weathermcp-demo --follow
```

### Scale the Application
```bash
# Manual scaling
az containerapp update --name weathermcp-server --resource-group rg-weathermcp-demo --min-replicas 2 --max-replicas 5

# Update resources
az containerapp update --name weathermcp-server --resource-group rg-weathermcp-demo --cpu 0.5 --memory 1.0Gi
```

### Check Application Metrics
```bash
# View app details
az containerapp show --name weathermcp-server --resource-group rg-weathermcp-demo

# List all revisions
az containerapp revision list --name weathermcp-server --resource-group rg-weathermcp-demo
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

## ❗ Troubleshooting

### Common Issues

#### 1. "Please run 'az login' to setup account"
**Solution:**
```powershell
az login
az account set --subscription "your-subscription-id"
```

#### 2. "Insufficient permissions"
**Solution:** Ensure your account has Contributor role:
```powershell
# Check your permissions
az role assignment list --assignee your-email@domain.com

# Your admin needs to grant Contributor role
az role assignment create --assignee your-email@domain.com --role Contributor
```

#### 3. "Container Apps extension not found"
**Solution:** The script handles this automatically, but you can manually install:
```powershell
az extension add --name containerapp --upgrade
```

#### 4. "Region not supported"
**Solution:** Use a different Azure region:
```powershell
.\deploy-aca-script.ps1 -Location "West US 2"
```

#### 5. "Resource name already exists"
**Solution:** Use different names:
```powershell
.\deploy-aca-script.ps1 -ResourceGroupName "my-unique-rg" -ContainerAppName "my-unique-app"
```

### Getting Help

#### View Detailed Logs
```bash
az containerapp logs show --name weathermcp-server --resource-group rg-weathermcp-demo --follow
```

#### Check Resource Status
```bash
# Container App status
az containerapp show --name weathermcp-server --resource-group rg-weathermcp-demo --query "properties.provisioningState"

# Environment status  
az containerapp env show --name weathermcp-env --resource-group rg-weathermcp-demo --query "properties.provisioningState"
```

#### Debugging Failed Deployments
```bash
# Check recent operations
az group deployment operation list --resource-group rg-weathermcp-demo

# View activity log
az monitor activity-log list --resource-group rg-weathermcp-demo --max-events 10
```

## 💰 Cost Considerations

Azure Container Apps pricing is based on:
- **vCPU-seconds** and **GiB-seconds** consumed
- **HTTP requests** processed
- **Storage** used

**Estimated monthly cost** for default configuration:
- **Small usage:** $5-15/month
- **Medium usage:** $15-50/month
- **Production usage:** $50-200/month

💡 **Cost Optimization Tips:**
- Use `--min-replicas 0` for development (app sleeps when not used)
- Monitor usage in Azure portal
- Set up cost alerts

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

## 🆘 Support

If you encounter issues:
1. Check the **Troubleshooting** section above
2. Review Azure Container Apps documentation
3. Check Azure status page for service issues
4. Contact Azure support if needed

---

## 🎯 Summary

This deployment script provides the **easiest way** to get your MCP server running in Azure:

✅ **No Docker required** - builds in Azure cloud  
✅ **No manual setup** - handles all Azure resources  
✅ **Production ready** - includes scaling and monitoring  
✅ **Cost effective** - optimized resource allocation  
✅ **Secure** - follows Azure best practices  

**Just run `.\deploy-aca-script.ps1` and you're live in Azure! 🚀**
