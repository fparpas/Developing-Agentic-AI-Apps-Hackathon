using ModelContextProtocol.Server;
using System.ComponentModel;
using TravelMcpServer.Http;

namespace TravelMcpServer.Tools;

/// <summary>
/// MCP tools for tours and activities search using Amadeus APIs
/// </summary>
[McpServerToolType]
[Description("Tours and activities search and discovery services")]
public class ActivitiesTools
{
    #region Activities Search Operations

    [McpServerTool]
    [Description("[ACTIVITIES] Search for activities around a specific location")]
    public static async Task<object> ActivitiesSearchByLocation(
        AmadeusHttpClient httpClient,
        [Description("Latitude (decimal coordinates)")] double latitude,
        [Description("Longitude (decimal coordinates)")] double longitude,
        [Description("Search radius in kilometers (0-20, default: 1)")] int radius = 1)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["latitude"] = latitude.ToString(),
            ["longitude"] = longitude.ToString(),
            ["radius"] = radius.ToString()
        };

        var response = await httpClient.GetJsonAsync("/v1/shopping/activities", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[ACTIVITIES] Search for activities within a bounding box area")]
    public static async Task<object> ActivitiesSearchByArea(
        AmadeusHttpClient httpClient,
        [Description("Northern latitude of bounding box (decimal coordinates)")] double north,
        [Description("Western longitude of bounding box (decimal coordinates)")] double west,
        [Description("Southern latitude of bounding box (decimal coordinates)")] double south,
        [Description("Eastern longitude of bounding box (decimal coordinates)")] double east)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["north"] = north.ToString(),
            ["west"] = west.ToString(),
            ["south"] = south.ToString(),
            ["east"] = east.ToString()
        };

        var response = await httpClient.GetJsonAsync("/v1/shopping/activities/by-square", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[ACTIVITIES] Get detailed information about a specific activity")]
    public static async Task<object> ActivitiesGetDetails(
        AmadeusHttpClient httpClient,
        [Description("Activity ID from search results")] string activityId)
    {
        var response = await httpClient.GetJsonAsync($"/v1/shopping/activities/{activityId}");
        return response;
    }

    [McpServerTool]
    [Description("[ACTIVITIES] Search for activities in a city by name")]
    public static async Task<object> ActivitiesSearchByCity(
        AmadeusHttpClient httpClient,
        [Description("City name (e.g., Paris, London, New York)")] string cityName,
        [Description("Country code (ISO 3166-1 alpha-2, e.g., FR, GB, US)")] string countryCode,
        [Description("Search radius in kilometers (0-20, default: 5)")] int radius = 5)
    {
        // This would typically require geocoding the city first, but for demonstration
        // we'll use a simplified approach with major city coordinates
        var cityCoordinates = GetCityCoordinates(cityName, countryCode);
        
        if (cityCoordinates == null)
        {
            return new { error = "City not found or coordinates not available" };
        }

        var queryParams = new Dictionary<string, string>
        {
            ["latitude"] = cityCoordinates.Value.latitude.ToString(),
            ["longitude"] = cityCoordinates.Value.longitude.ToString(),
            ["radius"] = radius.ToString()
        };

        var response = await httpClient.GetJsonAsync("/v1/shopping/activities", queryParams);
        return response;
    }

    #endregion

    #region Helper Methods

    private static (double latitude, double longitude)? GetCityCoordinates(string cityName, string countryCode)
    {
        // Simplified city coordinates lookup - in a real implementation, 
        // you might use a geocoding service or comprehensive city database
        var cityKey = $"{cityName.ToLower()}_{countryCode.ToLower()}";
        
        var cityCoordinates = new Dictionary<string, (double, double)>
        {
            ["paris_fr"] = (48.8566, 2.3522),
            ["london_gb"] = (51.5074, -0.1278),
            ["new york_us"] = (40.7128, -74.0060),
            ["nyc_us"] = (40.7128, -74.0060),
            ["madrid_es"] = (40.4168, -3.7038),
            ["barcelona_es"] = (41.3851, 2.1734),
            ["rome_it"] = (41.9028, 12.4964),
            ["amsterdam_nl"] = (52.3676, 4.9041),
            ["berlin_de"] = (52.5200, 13.4050),
            ["tokyo_jp"] = (35.6762, 139.6503),
            ["sydney_au"] = (-33.8688, 151.2093),
            ["dubai_ae"] = (25.2048, 55.2708),
            ["singapore_sg"] = (1.3521, 103.8198),
            ["bangkok_th"] = (13.7563, 100.5018),
            ["hong kong_hk"] = (22.3193, 114.1694),
            ["los angeles_us"] = (34.0522, -118.2437),
            ["chicago_us"] = (41.8781, -87.6298),
            ["toronto_ca"] = (43.6532, -79.3832),
            ["vancouver_ca"] = (49.2827, -123.1207),
            ["mexico city_mx"] = (19.4326, -99.1332),
            ["mumbai_in"] = (19.0760, 72.8777),
            ["delhi_in"] = (28.7041, 77.1025)
        };

        if (cityCoordinates.TryGetValue(cityKey, out var coordinates))
        {
            return coordinates;
        }

        return null;
    }

    #endregion

    #region Points of Interest

    [McpServerTool]
    [Description("[ACTIVITIES] Search for points of interest near a location")]
    public static async Task<object> ActivitiesSearchPointsOfInterest(
        AmadeusHttpClient httpClient,
        [Description("Latitude (decimal coordinates)")] double latitude,
        [Description("Longitude (decimal coordinates)")] double longitude,
        [Description("Search radius in kilometers (default: 1)")] int radius = 1,
        [Description("POI category filter (e.g., SIGHTS, RESTAURANT, SHOPPING)")] string? category = null)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["latitude"] = latitude.ToString(),
            ["longitude"] = longitude.ToString(),
            ["radius"] = radius.ToString()
        };

        if (!string.IsNullOrEmpty(category))
            queryParams["category"] = category;

        var response = await httpClient.GetJsonAsync("/v1/reference-data/locations/pois", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[ACTIVITIES] Search for points of interest within a bounding box")]
    public static async Task<object> ActivitiesSearchPointsOfInterestByArea(
        AmadeusHttpClient httpClient,
        [Description("Northern latitude of bounding box")] double north,
        [Description("Western longitude of bounding box")] double west,
        [Description("Southern latitude of bounding box")] double south,
        [Description("Eastern longitude of bounding box")] double east,
        [Description("POI category filter (e.g., SIGHTS, RESTAURANT, SHOPPING)")] string? category = null)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["north"] = north.ToString(),
            ["west"] = west.ToString(),
            ["south"] = south.ToString(),
            ["east"] = east.ToString()
        };

        if (!string.IsNullOrEmpty(category))
            queryParams["category"] = category;

        var response = await httpClient.GetJsonAsync("/v1/reference-data/locations/pois/by-square", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[ACTIVITIES] Get detailed information about a specific point of interest")]
    public static async Task<object> ActivitiesGetPointOfInterest(
        AmadeusHttpClient httpClient,
        [Description("POI ID from search results")] string poiId)
    {
        var response = await httpClient.GetJsonAsync($"/v1/reference-data/locations/pois/{poiId}");
        return response;
    }

    #endregion

    #region Travel Recommendations

    [McpServerTool]
    [Description("[ACTIVITIES] Get travel recommendations for a destination")]
    public static async Task<object> ActivitiesGetTravelRecommendations(
        AmadeusHttpClient httpClient,
        [Description("Origin city IATA code (e.g., NYC, LON)")] string origin,
        [Description("Destination city IATA code (e.g., PAR, ROM)")] string destination,
        [Description("Travel date (YYYY-MM-DD format)")] string travelDate,
        [Description("Number of travelers")] int travelers = 1)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["origin"] = origin,
            ["destination"] = destination,
            ["travelDate"] = travelDate,
            ["travelers"] = travelers.ToString()
        };

        var response = await httpClient.GetJsonAsync("/v1/travel/recommendations", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[ACTIVITIES] Get safe place information for a destination")]
    public static async Task<object> ActivitiesGetSafePlaceInfo(
        AmadeusHttpClient httpClient,
        [Description("Latitude (decimal coordinates)")] double latitude,
        [Description("Longitude (decimal coordinates)")] double longitude,
        [Description("Search radius in kilometers (default: 1)")] int radius = 1)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["latitude"] = latitude.ToString(),
            ["longitude"] = longitude.ToString(),
            ["radius"] = radius.ToString()
        };

        var response = await httpClient.GetJsonAsync("/v1/safety/safety-places", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[ACTIVITIES] Get location score and insights for a destination")]
    public static async Task<object> ActivitiesGetLocationScore(
        AmadeusHttpClient httpClient,
        [Description("Latitude (decimal coordinates)")] double latitude,
        [Description("Longitude (decimal coordinates)")] double longitude,
        [Description("Score category: SAFETY, TOURISM, BUSINESS")] string category = "TOURISM")
    {
        var queryParams = new Dictionary<string, string>
        {
            ["latitude"] = latitude.ToString(),
            ["longitude"] = longitude.ToString(),
            ["category"] = category
        };

        var response = await httpClient.GetJsonAsync("/v1/location/analytics/category-rated-areas", queryParams);
        return response;
    }

    #endregion
}