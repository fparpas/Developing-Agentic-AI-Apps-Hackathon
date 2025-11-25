# Remote MCP Weather Server - Challenge 04 (Python)

This is the Python starter for Challenge 04 of the Developing Agentic AI Apps Hackathon. Your goal is to convert this MCP server from stdio transport to HTTP transport, enabling remote deployment to Azure Container Apps or Azure Functions.

## Your Challenge

The server currently uses **stdio transport** (like Challenge 02). You need to:

1. **Add HTTP Transport**: Convert from stdio to HTTP using FastAPI
2. **Add Required Endpoints**: Health check and MCP protocol endpoints
3. **Containerize**: Ensure the server works in Docker
4. **Deploy to Azure**: Deploy to Azure Container Apps or Functions

## Current State

The server implements:
- ✅ Two weather tools: `get_forecast` and `get_alerts`
- ✅ Integration with National Weather Service API
- ✅ Basic MCP server with stdio transport
- ❌ HTTP transport (you need to add this)
- ❌ Azure deployment configuration (you need to add this)

## What You Need to Add

### 1. HTTP Transport with FastAPI

Convert the stdio-based server to use HTTP. Look for TODO comments in `weather_remote_server.py`:

```python
# TODO: Add FastAPI integration for HTTP transport
#       1. Import FastAPI and create an app instance
#       2. Add health check endpoint at /health for Azure monitoring
#       3. Add root endpoint at / with server information
#       4. Integrate MCP with FastAPI for HTTP communication
#       5. Configure uvicorn to run on 0.0.0.0:8000
```

### 2. Update Dependencies

Add to `requirements.txt` (see TODO comments in the file):
- `fastapi>=0.104.0`
- `uvicorn[standard]>=0.24.0`
- Azure dependencies as needed

### 3. Update Dockerfile

Ensure the Dockerfile:
- Exposes port 8000
- Includes health check pointing to `/health` endpoint
- Runs the HTTP server (not stdio)

## Architecture

```
Challenge 02 (Local - stdio):   Challenge 04 (Remote - HTTP):
┌─────────────────┐            ┌──────────────┐
│  Local Client   │            │  Remote      │
│  (Claude/VSCode)│            │  Client      │
└────────┬────────┘            └──────┬───────┘
         │                            │
         ▼ stdio                      ▼ HTTP
   ┌──────────┐               ┌──────────────┐
   │  Server  │               │  Azure       │
   │  Local   │               │  Container   │
   └──────────┘               │  App Server  │
                              └──────────────┘
```

## Setup and Development

### Local Testing (Current State)

**Using `uv` (recommended for performance):**
```bash
# Create virtual environment
uv venv .venv
source .venv/bin/activate  # Windows: .venv\Scripts\activate

# Install dependencies
uv pip install -r requirements.txt

# Test with stdio (current state)
python weather_remote_server.py
```

**Or using standard `pip`:**
```bash
# Create virtual environment
python -m venv .venv
source .venv/bin/activate  # Windows: .venv\Scripts\activate

# Install dependencies
pip install -r requirements.txt

# Test with stdio (current state)
python weather_remote_server.py
```

### After Adding HTTP Transport

Once you've implemented HTTP transport:

```bash
# Run locally
python weather_remote_server.py

# Server should be available at: http://localhost:8000
# Test health check
curl http://localhost:8000/health
```

## Docker Build (After Implementation)

```bash
# Build Docker image
docker build -t weather-mcp-server:latest .

# Run container locally
docker run -p 8000:8000 weather-mcp-server:latest

# Test
curl http://localhost:8000/health
```

## Azure Container Apps Deployment

```bash
# Login to Azure
az login

# Create resource group
az group create --name weather-rg --location eastus

# Create container registry
az acr create --resource-group weather-rg --name weatherregistry --sku Basic

# Build and push image
az acr build --registry weatherregistry --image weather-mcp-server:latest .

# Create container app
az containerapp create \
  --name weather-mcp-server \
  --resource-group weather-rg \
  --image weatherregistry.azurecr.io/weather-mcp-server:latest \
  --target-port 8000 \
  --ingress 'external' \
  --environment-variables "PORT=8000"
```

## Expected API Endpoints

After completing the challenge, your server should have:

### Health Check
```
GET /health
Returns: {"status": "healthy", "service": "weather-mcp-server"}
```

### Root Information
```
GET /
Returns: Server information, available tools, and documentation link
```

### MCP Protocol Endpoints
FastAPI integration with MCP will provide standard MCP endpoints.

## Testing Your Implementation

Once deployed, test with:

```bash
# Health check
curl https://your-app.azurecontainerapps.io/health

# Server info
curl https://your-app.azurecontainerapps.io/
```

## Tips and Hints

1. **FastAPI Integration**: Look at how FastMCP can integrate with FastAPI applications
2. **Port Configuration**: Azure Container Apps expects port 8000 by default
3. **Health Checks**: Azure needs a `/health` endpoint that returns 200 OK
4. **CORS**: You may need to configure CORS for browser-based clients
5. **Environment Variables**: Use environment variables for configuration

## Success Criteria

Your implementation is complete when:

- ✅ Server runs with HTTP transport (not stdio)
- ✅ `/health` endpoint returns healthy status
- ✅ `/` endpoint provides server information
- ✅ Tools (`get_forecast`, `get_alerts`) work via HTTP
- ✅ Docker container builds and runs successfully
- ✅ Server can be deployed to Azure Container Apps
- ✅ Remote clients can connect and use the tools

## Learning Resources

- [FastAPI Documentation](https://fastapi.tiangolo.com/)
- [FastMCP Documentation](https://github.com/jlowin/fastmcp)
- [Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/)
- [MCP HTTP Transport](https://modelcontextprotocol.io/docs/concepts/transports)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)

## Need Help?

- Check the Coach/Solutions folder for a complete reference implementation
- Review the C# example for similar patterns
- Look at Challenge 02 for stdio transport patterns
- The TODO comments in the code provide specific guidance
