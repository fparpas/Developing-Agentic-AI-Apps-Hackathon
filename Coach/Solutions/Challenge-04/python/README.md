# Remote MCP Weather Server - Challenge 04 Solution (Python)

This is the Python solution for Challenge 04 of the Developing Agentic AI Apps Hackathon. This project implements a remote MCP server using FastAPI that can be deployed to Azure Container Apps or Azure Functions.

## Features

- **HTTP-based MCP Transport**: Unlike Challenge 02's stdio transport, this server uses HTTP for remote access
- **FastAPI Framework**: Modern Python web framework optimized for async operations
- **Azure-Ready Deployment**: Includes Docker configuration for Azure Container Apps
- **Health Check Endpoint**: Monitoring support for Azure deployments
- **Same Tools as Challenge 02**: get_forecast and get_alerts from National Weather Service

## Architecture

```
Challenge 02 (Local):           Challenge 04 (Remote):
┌─────────────────┐            ┌──────────────┐
│  Local Client   │            │  Remote      │
│  (stdio)        │            │  Client      │
└────────┬────────┘            └──────┬───────┘
         │                            │
         ▼                            ▼ HTTP
   ┌──────────┐               ┌──────────────┐
   │  Server  │               │  Azure       │
   │  stdio   │               │  Container   │
   └──────────┘               │  App Server  │
                              └──────────────┘
```

## Project Structure

```
weather_remote_server/
├── weather_remote_server.py  # FastAPI + MCP server implementation
├── requirements.txt          # Python dependencies
├── Dockerfile                # Docker configuration for Azure
├── .dockerignore              # Docker build optimization
├── .env.example              # Environment variable template
└── README.md                 # This file
```

## Dependencies

- **fastapi**: Modern async web framework
- **uvicorn**: ASGI server for FastAPI
- **mcp[cli]**: MCP SDK with CLI support
- **httpx**: Async HTTP client
- **azure-identity**: Azure authentication (for deployment)

## Setup and Deployment

### Local Development

**For faster dependency management, consider using `uv`:** [`uv` is an extremely fast Python package installer and resolver](https://docs.astral.sh/uv/). It's significantly faster than `pip` (10-100x in many cases) and handles dependency resolution more efficiently. You can install it from https://docs.astral.sh/uv/getting-started/installation/.

**Using `uv` (recommended for performance):**
```bash
# Create virtual environment
uv venv .venv
source .venv/bin/activate  # Windows: .venv\Scripts\activate

# Install dependencies
uv pip install -r requirements.txt

# Run locally
python weather_remote_server.py
```

**Or using standard `pip`:**
```bash
# Create virtual environment
python -m venv .venv
source .venv/bin/activate  # Windows: .venv\Scripts\activate

# Install dependencies
pip install -r requirements.txt

# Run locally
python weather_remote_server.py
```

Server will be available at: http://localhost:8000

### Docker Build

```bash
# Build Docker image
docker build -t weather-mcp-server:latest .

# Run container locally
docker run -p 8000:8000 weather-mcp-server:latest
```

### Azure Container Apps Deployment

```bash
# Login to Azure
az login

# Create resource group
az group create --name weather-rg --location eastus

# Create container registry (if needed)
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

### Azure Functions Deployment

```bash
# Create function app
az functionapp create \
  --resource-group weather-rg \
  --consumption-plan-location eastus \
  --runtime python \
  --functions-version 4 \
  --name weather-mcp-function

# Deploy code
func azure functionapp publish weather-mcp-function
```

## API Endpoints

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

### MCP Endpoints
```
POST /mcp/v1/initialize
POST /mcp/v1/messages
GET /mcp/v1/resources
```

## Testing the Server

Once deployed, test with a simple HTTP request:

```bash
# Health check
curl https://your-app.azurecontainerapps.io/health

# Server info
curl https://your-app.azurecontainerapps.io/

# In your client code
import httpx

client = httpx.Client(base_url="https://your-app.azurecontainerapps.io")
response = client.get("/health")
print(response.json())
```

## Key Implementation Notes

### HTTP Transport
The server uses FastAPI with async operations to handle HTTP-based MCP protocol:
```python
@app.get("/")
async def root():
    return {"status": "ready"}
```

### MCP Integration
The MCP server is integrated with FastAPI using decorators:
```python
@mcp.tool()
async def get_alerts(state: str) -> str:
    # Tool implementation
```

### Scalability
Azure Container Apps automatically scales based on:
- HTTP request volume
- CPU and memory usage
- Custom metrics (if configured)

### Cost Optimization
For minimal cost:
- Container Apps scale to zero when idle
- Azure Functions charges only for execution time
- Both support reserved instances for predictable load

## Monitoring

### Azure Portal
- View logs and metrics in Container Apps resource
- Set up alerts for error rates or latency

### Health Checks
The `/health` endpoint enables Azure to verify server availability:
```
HEALTHCHECK --interval=30s --timeout=3s
```

## Environment Variables

For production deployments, consider:
```env
# Logging
LOG_LEVEL=INFO

# Rate limiting
RATE_LIMIT=100

# Cache settings
CACHE_TTL=300
```

## Success Criteria Met

✅ MCP server runs over HTTP transport
✅ Provides get_forecast and get_alerts tools
✅ Deployable to Azure Container Apps
✅ Deployable to Azure Functions
✅ Includes health check endpoint
✅ Documented with deployment examples
✅ Scales automatically with demand
✅ Production-ready error handling

## Learning Resources

- [FastAPI Documentation](https://fastapi.tiangolo.com/)
- [Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/)
- [Azure Functions Python Guide](https://learn.microsoft.com/en-us/azure/azure-functions/functions-reference-python)
- [MCP HTTP Transport](https://modelcontextprotocol.io/docs/sdk)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
