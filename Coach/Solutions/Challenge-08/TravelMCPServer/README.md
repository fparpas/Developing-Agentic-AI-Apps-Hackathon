# Travel MCP Server

A comprehensive Model Context Protocol (MCP) server that provides travel-related tools and information for AI assistants, with built-in Amadeus API authentication and 40+ specialized travel tools.

## 🚀 Features Overview

This MCP server provides **40+ travel tools** across 5 specialized categories, plus built-in Amadeus API authentication:

### 🔐 **Amadeus API Authentication** (NEW!)
- **AmadeusAuthService**: OAuth2 client credentials flow for Amadeus API
- **AmadeusHttpClient**: HTTP client wrapper with automatic token management
- **AuthenticationTools**: Test and verify Amadeus API authentication
- Automatic token refresh and error handling
- Production-ready authentication service

### ✈️ **Flight Tools** (18 APIs)
- Flight search, booking, price analysis
- Delay predictions, seat maps, status tracking
- Airport performance, travel trends, disruption monitoring

### 🏨 **Hotel Tools** (6 APIs)  
- Hotel search, booking, availability checking
- Ratings, sentiment analysis, chain search

### 📍 **Location Tools** (8 APIs)
- Airport/city search, POI discovery
- Safety information, distance calculations, recommendations

### � **Transportation Tools** (3 APIs)
- Ground transfer search, booking, management

### 🎯 **Service Tools** (5 APIs)
- AI recommendations, trip purpose prediction
- Tours/activities, branded fare upselling

### 🌍 **Utility Tools**
- **GetCountryInfo**: Country information (capital, population, region)
- **GetTimeZoneInfo**: Current time for any timezone or city  
- **GetExchangeRates**: Real-time currency exchange rates
- **GetGeneralTravelRecommendations**: Curated travel recommendations
- **CalculateDistance**: Distance calculations between major cities

## 🛠️ Setup

### Prerequisites

- .NET 9.0 SDK
- Amadeus API Developer Account (for production use)

### 1. Configuration

Create or update your `appsettings.json` with your Amadeus API credentials:

```json
{
  "Transport": {
    "Mode": "stdio",
    "HttpHost": "localhost", 
    "HttpPort": 8080
  },
  "Amadeus": {
    "ClientId": "your_amadeus_client_id",
    "ClientSecret": "your_amadeus_client_secret", 
    "BaseUrl": "https://test.api.amadeus.com"
  }
}
```

### 2. Running the Server

**Option A: Quick Start Scripts**

```bash
# Run in stdio mode (default)
./run-server.ps1

# Run in HTTP mode (remote accessible)
./run-server.ps1 -Transport http

# Run in HTTP mode on custom port
./run-server.ps1 -Transport http -Port 9000

# Run accessible from other machines
./run-server.ps1 -Transport http -Host 0.0.0.0 -Port 8080
```

**Option B: Manual Build and Run**

```bash
# Build the project
dotnet build

# Run in stdio mode (pipe communication)
dotnet run

# Run in HTTP mode (network accessible)
dotnet run -- --Transport:Mode=http --Transport:HttpHost=localhost --Transport:HttpPort=8080
```

**Option C: Batch File (Windows)**

```batch
# stdio mode
run-server.bat

# HTTP mode  
run-server.bat http
```

### 3. Testing Remote Connection

```bash
# Test the HTTP server
./test-client.ps1

# Test custom server URL
./test-client.ps1 -ServerUrl http://yourserver:8080
```

## 🌐 Remote Server Configuration

### Transport Modes

**📡 Stdio Mode** (Default):
- Communication via stdin/stdout pipes
- Best for local MCP clients
- Lower latency
- Process-to-process communication

**🌐 HTTP Mode** (Remote):
- Communication via HTTP/REST API
- Network accessible
- Can serve multiple clients
- Firewall/proxy friendly
- Standard web protocols

### Configuration Options

| Setting | Description | Default | Example |
|---------|-------------|---------|---------|
| `Transport:Mode` | Transport type | `stdio` | `http`, `stdio` |
| `Transport:HttpHost` | HTTP bind address | `localhost` | `0.0.0.0`, `127.0.0.1` |
| `Transport:HttpPort` | HTTP port number | `8080` | `9000`, `3000` |

### Environment Variables

```bash
# Set transport mode
export Transport__Mode=http
export Transport__HttpHost=0.0.0.0  
export Transport__HttpPort=8080

# Set Amadeus credentials
export Amadeus__ClientId=your_client_id
export Amadeus__ClientSecret=your_client_secret
```

## 🔑 Amadeus API Setup

1. **Register**: Create an account at [Amadeus for Developers](https://developers.amadeus.com/)
2. **Create App**: Create a new application to get your Client ID and Secret
3. **Configure**: Add credentials to `appsettings.json` (or environment variables)
4. **Test**: Use `TestAmadeusAuthentication` tool to verify setup

### Environment Variables (Alternative)
```bash
export Amadeus__ClientId="your_client_id"
export Amadeus__ClientSecret="your_client_secret"
export Amadeus__BaseUrl="https://test.api.amadeus.com"
```

## 🧪 Testing Authentication

Use the built-in authentication tools:

```csharp
// Test authentication
TestAmadeusAuthentication() 

// Test API call
TestAmadeusApiCall()

// Refresh token manually  
RefreshAmadeusToken()
```

## Usage

This server is designed to be used with MCP-compatible AI assistants. The server exposes tools that can be called to provide comprehensive travel-related information and services.

### Example Tool Calls

**Authentication & Setup:**
- Test Amadeus authentication: `TestAmadeusAuthentication()`
- Test API connectivity: `TestAmadeusApiCall()`
- Get overview of all tools: `GetTravelToolsOverview()`

**Flight Operations:**
- Search flights: `SearchFlightOffers("NYC", "LON", "2024-06-15")`
- Analyze prices: `GetFlightPriceAnalysis("NYC", "LON")`
- Predict delays: `PredictFlightDelay("AA123", "2024-06-15")`

**Hotel Operations:**
- Search hotels: `SearchHotels("London", "2024-06-15", "2024-06-17")`
- Get ratings: `GetHotelRatings("HOTEL123")`

**Location Services:**
- Find airports: `SearchAirportsAndCities("London")`
- Get safety info: `GetLocationSafety("Paris")`
- Search POIs: `SearchPointsOfInterest("Tokyo", "museum")`

**Utility Tools:**
- Get country info: `GetCountryInfo("Japan")`
- Check timezone: `GetTimeZoneInfo("Asia/Tokyo")`
- Exchange rates: `GetExchangeRates("USD", "EUR,GBP,JPY")`
- Distance calculation: `CalculateDistance("New York, NY", "London, UK")`

## 📦 Dependencies

- .NET 9.0
- Microsoft.Extensions.Hosting 9.0.8
- Microsoft.Extensions.Configuration 9.0.8
- Microsoft.Extensions.Configuration.Json 9.0.8
- Microsoft.Extensions.Configuration.EnvironmentVariables 9.0.8
- ModelContextProtocol 0.3.0-preview.4

## 🔗 API Dependencies

### Production APIs (Amadeus)
- [Amadeus Travel APIs](https://developers.amadeus.com/) - Comprehensive travel data
  - Requires API credentials
  - OAuth2 authentication included
  - 40+ API specifications supported

### Public APIs (Free Tier)
- [REST Countries API](https://restcountries.com/) - Country information
- [World Time API](http://worldtimeapi.org/) - Time zone data  
- [Exchange Rate API](https://api.exchangerate-api.com/) - Currency rates

## 🏗️ Architecture

### Core Components

**Authentication Layer:**
- `Services/AmadeusAuthService.cs` - OAuth2 authentication service
- `Http/AmadeusHttpClient.cs` - HTTP client wrapper with auth
- Automatic token management and refresh

**MCP Server Framework:**
- `Program.cs` - Main application entry point and DI setup
- `HttpClientExt.cs` - HTTP client extensions for JSON handling

**Tool Categories:**
- `Tools/AuthenticationTools.cs` - Authentication testing and management
- `Tools/FlightTools.cs` - Flight search, booking, predictions (18 tools)
- `Tools/HotelTools.cs` - Hotel search, booking, ratings (6 tools)
- `Tools/LocationTools.cs` - Location search, POI, safety (8 tools)
- `Tools/TransportationTools.cs` - Ground transfers (3 tools)
- `Tools/ServiceTools.cs` - AI services, recommendations (5 tools)
- `Tools/TravelTools.cs` - Utility tools and overview (6 tools)

### Design Patterns

- **Dependency Injection**: All services registered in DI container
- **Configuration Pattern**: Settings from appsettings.json and environment
- **Authentication Middleware**: Automatic token management
- **Tool Discovery**: MCP framework automatic tool registration
- **Error Handling**: Comprehensive exception handling throughout

Each tool is decorated with `[McpServerTool]` and `[Description]` attributes to provide metadata for the MCP framework.

## 🚀 Production Deployment

1. **Configure Production Credentials**: Use production Amadeus API credentials
2. **Environment Variables**: Set credentials via environment variables for security
3. **Logging**: Configure appropriate logging levels
4. **Monitoring**: Implement health checks and monitoring
5. **Rate Limiting**: Handle API rate limits appropriately
6. **Caching**: Implement caching for frequently requested data

This comprehensive travel MCP server is ready for production use with proper configuration!