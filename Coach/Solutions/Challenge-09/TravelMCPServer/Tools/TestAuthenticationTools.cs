using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using TravelMcpServer.Http;

namespace TravelMcpServer.Tools;

/// <summary>
/// Authentication demonstration tools for Amadeus API
/// </summary>
[McpServerToolType]
public static class AuthenticationTools
{
    [McpServerTool, Description("[TEST] Test Amadeus API authentication and get access token information.")]
    public static async Task<string> TestAmadeusAuthentication(AmadeusHttpClient amadeusClient)
    {
        try
        {
            // Get access token
            var accessToken = await amadeusClient.GetAccessTokenAsync();
            var tokenInfo = amadeusClient.GetTokenInfo();

            return $"""
                🔐 AMADEUS API AUTHENTICATION TEST
                
                ✅ Authentication Status: SUCCESS
                
                Token Information:
                • Has Token: {tokenInfo.HasToken}
                • Token Expires: {tokenInfo.TokenExpiry:yyyy-MM-dd HH:mm:ss} UTC
                • Is Expired: {tokenInfo.IsExpired}
                • Time Until Expiry: {tokenInfo.TimeUntilExpiry.TotalMinutes:F1} minutes
                • Token Preview: {accessToken[..Math.Min(20, accessToken.Length)]}...
                
                🌐 API Endpoints Ready:
                • Flight Search: /v2/shopping/flight-offers
                • Hotel Search: /v3/shopping/hotel-offers
                • Location Search: /v1/reference-data/locations
                • Airport Information: /v1/reference-data/locations/airports
                
                💡 The authentication wrapper is now ready for all Amadeus API calls!
                """;
        }
        catch (Exception ex)
        {
            return $"""
                ❌ AMADEUS API AUTHENTICATION TEST FAILED
                
                Error: {ex.Message}
                
                🔧 Troubleshooting Steps:
                1. Verify your Amadeus API credentials in appsettings.json
                2. Check that ClientId and ClientSecret are valid
                3. Ensure network connectivity to test.api.amadeus.com
                4. Confirm your Amadeus API account is active
                
                📝 Configuration should look like this in appsettings.json:
                • "Amadeus:ClientId": "your_client_id_here"
                • "Amadeus:ClientSecret": "your_client_secret_here"  
                • "Amadeus:BaseUrl": "https://test.api.amadeus.com"
                """;
        }
    }

    [McpServerTool, Description("[TEST] Make a test API call to Amadeus reference data endpoint to verify authentication.")]
    public static async Task<string> TestAmadeusApiCall(AmadeusHttpClient amadeusClient)
    {
        try
        {
            // Test with a simple reference data call (airports near Paris)
            var response = await amadeusClient.GetAsync("/v1/reference-data/locations?subType=AIRPORT&keyword=PAR&page[limit]=5");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(content);
                var data = jsonDoc.RootElement.GetProperty("data");

                var airports = new List<string>();
                foreach (var airport in data.EnumerateArray().Take(3))
                {
                    var name = airport.GetProperty("name").GetString();
                    var iataCode = airport.GetProperty("iataCode").GetString();
                    airports.Add($"• {name} ({iataCode})");
                }

                return $"""
                    ✅ AMADEUS API CALL TEST - SUCCESS
                    
                    📡 Endpoint: /v1/reference-data/locations
                    🎯 Query: Airports near Paris (PAR)
                    📊 Status: {response.StatusCode}
                    
                    Sample Results:
                    {string.Join("\n", airports)}
                    
                    🚀 Authentication wrapper is working perfectly!
                    All Amadeus API endpoints are now accessible via AmadeusHttpClient.
                    """;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return $"""
                    ⚠️ AMADEUS API CALL TEST - FAILED
                    
                    📡 Endpoint: /v1/reference-data/locations
                    📊 Status: {response.StatusCode}
                    🔍 Response: {errorContent}
                    
                    🔧 This indicates an issue with API permissions or request format.
                    """;
            }
        }
        catch (Exception ex)
        {
            return $"""
                ❌ AMADEUS API CALL TEST - ERROR
                
                Error: {ex.Message}
                
                💡 This could indicate:
                • Network connectivity issues
                • Invalid API credentials
                • API service temporarily unavailable
                """;
        }
    }

    [McpServerTool, Description("[TEST] Refresh the Amadeus API access token manually.")]
    public static async Task<string> TestRefreshAmadeusToken(AmadeusHttpClient amadeusClient)
    {
        try
        {
            var oldTokenInfo = amadeusClient.GetTokenInfo();
            var newToken = await amadeusClient.RefreshTokenAsync();
            var newTokenInfo = amadeusClient.GetTokenInfo();

            return $"""
                🔄 AMADEUS TOKEN REFRESH - SUCCESS
                
                Previous Token:
                • Had Token: {oldTokenInfo.HasToken}
                • Was Expired: {oldTokenInfo.IsExpired}
                • Expiry: {oldTokenInfo.TokenExpiry:yyyy-MM-dd HH:mm:ss} UTC
                
                New Token:
                • Token Preview: {newToken[..Math.Min(20, newToken.Length)]}...
                • Expires: {newTokenInfo.TokenExpiry:yyyy-MM-dd HH:mm:ss} UTC
                • Valid for: {newTokenInfo.TimeUntilExpiry.TotalMinutes:F1} minutes
                
                ✅ Token refresh completed successfully!
                """;
        }
        catch (Exception ex)
        {
            return $"""
                ❌ AMADEUS TOKEN REFRESH - FAILED
                
                Error: {ex.Message}
                
                🔧 Check your API credentials and network connectivity.
                """;
        }
    }
    
     [McpServerTool, Description("[TEST] Test the Bearer token authentication by making a simple API call to Amadeus.")]
    public static async Task<string> TestBearerTokenCall(AmadeusHttpClient amadeusClient)
    {
        try
        {
            // First, get the current token info
            var tokenInfo = amadeusClient.GetTokenInfo();
            var currentToken = await amadeusClient.GetAccessTokenAsync();

            // Make a simple test call to verify the Bearer token works
            var testEndpoint = "/v1/shopping/flight-destinations";
            var testParameters = new Dictionary<string, string>
            {
                ["origin"] = "PAR",
                ["maxPrice"] = "200"
            };

            using var jsonDocument = await amadeusClient.ReadJsonDocumentAsync(testEndpoint, testParameters);
            var root = jsonDocument.RootElement;

            // Check if we got valid data
            var hasData = root.TryGetProperty("data", out var dataElement);
            var dataCount = hasData ? dataElement.GetArrayLength() : 0;

            return $"""
                ✅ BEARER TOKEN AUTHENTICATION TEST - SUCCESS
                
                🔑 Token Information:
                • Token Preview: {currentToken[..Math.Min(20, currentToken.Length)]}...
                • Token Expires: {tokenInfo.TokenExpiry:yyyy-MM-dd HH:mm:ss} UTC
                • Time Until Expiry: {tokenInfo.TimeUntilExpiry.TotalMinutes:F1} minutes
                • Is Valid: {!tokenInfo.IsExpired}
                
                📡 Test API Call Results:
                • Endpoint: {testEndpoint}
                • Parameters: origin=PAR, maxPrice=200
                • Authorization: Bearer {currentToken[..Math.Min(10, currentToken.Length)]}...
                • Response: {dataCount} destinations returned
                
                🚀 Bearer token is working correctly!
                The HTTP client wrapper automatically adds the Authorization header
                in the format: 'Authorization: Bearer {currentToken[..Math.Min(15, currentToken.Length)]}...'
                
                💡 You can now use any Amadeus API endpoint with automatic authentication!
                """;
        }
        catch (Exception ex)
        {
            return $"""
                ❌ BEARER TOKEN TEST FAILED
                
                Error: {ex.Message}
                
                🔧 This indicates an issue with:
                • Token generation or format
                • API endpoint accessibility  
                • Network connectivity
                • Request authentication headers
                """;
        }
    }

    [McpServerTool, Description("[TEST] Show how the Bearer token is automatically applied to HTTP requests.")]
    public static async Task<string> TestShowAuthenticationHeaders(AmadeusHttpClient amadeusClient)
    {
        try
        {
            // Get the current access token
            var accessToken = await amadeusClient.GetAccessTokenAsync();
            var tokenInfo = amadeusClient.GetTokenInfo();

            // Show how the HTTP client applies the Bearer token
            return $"""
                🔑 AMADEUS API AUTHENTICATION HEADERS
                
                📋 How Bearer Token is Applied:
                
                1️⃣ Original curl command:
                curl 'https://test.api.amadeus.com/v1/shopping/flight-destinations?origin=PAR&maxPrice=200' \
                     -H 'Authorization: Bearer ABCDEFGH12345'
                
                2️⃣ Our HTTP Client Implementation:
                • Endpoint: /v1/shopping/flight-destinations
                • Query Params: origin=PAR&maxPrice=200
                • Header: Authorization: Bearer {accessToken[..Math.Min(12, accessToken.Length)]}...
                
                3️⃣ Automatic Token Management:
                • Current Token: {accessToken[..Math.Min(30, accessToken.Length)]}...
                • Token Status: {(tokenInfo.IsExpired ? "❌ Expired" : "✅ Valid")}
                • Expires: {tokenInfo.TokenExpiry:yyyy-MM-dd HH:mm:ss} UTC
                • Auto-refresh: ✅ Enabled (5 min buffer)
                
                4️⃣ Usage in Tools:
                ```csharp
                // The AmadeusHttpClient automatically adds:
                // Authorization: Bearer [token]
                var response = await amadeusClient.GetAsync("/v1/shopping/flight-destinations?origin=PAR&maxPrice=200");
                
                // Or with parameters:
                var parameters = new Dictionary<string, string>();
                parameters["origin"] = "PAR";
                parameters["maxPrice"] = "200";
                var jsonDoc = await amadeusClient.ReadJsonDocumentAsync("/v1/shopping/flight-destinations", parameters);
                ```
                
                🛡️ Security Features:
                • Token stored securely in memory
                • Automatic refresh before expiry
                • Thread-safe token management
                • Retry logic for 401 responses
                
                🎯 The Bearer token is automatically applied to ALL requests!
                """;
        }
        catch (Exception ex)
        {
            return $"Error showing authentication details: {ex.Message}";
        }
    }
}