using ModelContextProtocol.Server;
using System.ComponentModel;
using TravelMcpServer.Http;

namespace TravelMcpServer.Tools;

/// <summary>
/// MCP tools for travel reference data using Amadeus APIs
/// </summary>
[McpServerToolType]
[Description("Travel reference data including airports, airlines, cities, and other travel information")]
public class ReferenceDataTools
{
    #region Airport and Location Search

    [McpServerTool]
    [Description("[REFERENCE] Search for airports and cities by keyword")]
    public static async Task<object> ReferenceSearchAirportsAndCities(
        AmadeusHttpClient httpClient,
        [Description("Search keyword (city name, airport name, or IATA code)")] string keyword,
        [Description("Subtype filter: AIRPORT, CITY (optional)")] string? subType = null,
        [Description("Country code filter (ISO 3166-1 alpha-2)")] string? countryCode = null,
        [Description("Maximum number of results")] int pageLimit = 10)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["keyword"] = keyword,
            ["page[limit]"] = pageLimit.ToString()
        };

        if (!string.IsNullOrEmpty(subType))
            queryParams["subType"] = subType;

        if (!string.IsNullOrEmpty(countryCode))
            queryParams["countryCode"] = countryCode;

        var response = await httpClient.GetJsonAsync("/v1/reference-data/locations", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[REFERENCE] Find nearest relevant airports to a location")]
    public static async Task<object> ReferenceGetNearestAirports(
        AmadeusHttpClient httpClient,
        [Description("Latitude (decimal coordinates)")] double latitude,
        [Description("Longitude (decimal coordinates)")] double longitude,
        [Description("Search radius in kilometers (default: 500)")] int radius = 500,
        [Description("Maximum number of results")] int pageLimit = 10,
        [Description("Sort order: relevance, distance, analytics.travelers.score")] string sort = "relevance")
    {
        var queryParams = new Dictionary<string, string>
        {
            ["latitude"] = latitude.ToString(),
            ["longitude"] = longitude.ToString(),
            ["radius"] = radius.ToString(),
            ["page[limit]"] = pageLimit.ToString(),
            ["sort"] = sort
        };

        var response = await httpClient.GetJsonAsync("/v1/reference-data/locations/airports", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[REFERENCE] Search for cities by keyword")]
    public static async Task<object> ReferenceSearchCities(
        AmadeusHttpClient httpClient,
        [Description("Search keyword (city name)")] string keyword,
        [Description("Country code filter (ISO 3166-1 alpha-2)")] string? countryCode = null,
        [Description("Maximum number of results")] int pageLimit = 10,
        [Description("Include airports served by this city")] bool include = false)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["keyword"] = keyword,
            ["page[limit]"] = pageLimit.ToString()
        };

        if (!string.IsNullOrEmpty(countryCode))
            queryParams["countryCode"] = countryCode;

        if (include)
            queryParams["include"] = "AIRPORTS";

        var response = await httpClient.GetJsonAsync("/v1/reference-data/locations/cities", queryParams);
        return response;
    }

    #endregion

    #region Airline Information

    [McpServerTool]
    [Description("[REFERENCE] Look up airline information by IATA or ICAO code")]
    public static async Task<object> ReferenceGetAirlineInfo(
        AmadeusHttpClient httpClient,
        [Description("Airline IATA code (2 characters, e.g., AF) or ICAO code (3 characters, e.g., AFR)")] string airlineCode)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["airlineCodes"] = airlineCode
        };

        var response = await httpClient.GetJsonAsync("/v1/reference-data/airlines", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[REFERENCE] Get multiple airlines information")]
    public static async Task<object> ReferenceGetMultipleAirlines(
        AmadeusHttpClient httpClient,
        [Description("Comma-separated airline codes (IATA or ICAO)")] string airlineCodes,
        [Description("Maximum number of results")] int pageLimit = 10)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["airlineCodes"] = airlineCodes,
            ["page[limit]"] = pageLimit.ToString()
        };

        var response = await httpClient.GetJsonAsync("/v1/reference-data/airlines", queryParams);
        return response;
    }

    #endregion

    #region Route Information

    [McpServerTool]
    [Description("[REFERENCE] Get airline routes from specific airport")]
    public static async Task<object> ReferenceGetAirlineRoutes(
        AmadeusHttpClient httpClient,
        [Description("Airline IATA code (e.g., AF, LH)")] string airlineCode,
        [Description("Departure airport IATA code (e.g., CDG, JFK)")] string? departureAirportCode = null,
        [Description("Maximum number of results")] int pageLimit = 10)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["airlineCode"] = airlineCode,
            ["page[limit]"] = pageLimit.ToString()
        };

        if (!string.IsNullOrEmpty(departureAirportCode))
            queryParams["departureAirportCode"] = departureAirportCode;

        var response = await httpClient.GetJsonAsync("/v1/reference-data/routes", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[REFERENCE] Get airport routes and destinations")]
    public static async Task<object> ReferenceGetAirportRoutes(
        AmadeusHttpClient httpClient,
        [Description("Departure airport IATA code (e.g., CDG, JFK)")] string departureAirportCode,
        [Description("Maximum number of results")] int pageLimit = 10)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["departureAirportCode"] = departureAirportCode,
            ["page[limit]"] = pageLimit.ToString()
        };

        var response = await httpClient.GetJsonAsync("/v1/reference-data/routes", queryParams);
        return response;
    }

    #endregion

    #region Airport Performance and Analytics

    [McpServerTool]
    [Description("[REFERENCE] Get airport on-time performance statistics")]
    public static async Task<object> ReferenceGetAirportPerformance(
        AmadeusHttpClient httpClient,
        [Description("Airport IATA code (e.g., CDG, JFK)")] string airportCode,
        [Description("Date for performance data (YYYY-MM-DD format)")] string date)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["airportCode"] = airportCode,
            ["date"] = date
        };

        var response = await httpClient.GetJsonAsync("/v1/airport/predictions/on-time", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[REFERENCE] Get most traveled destinations from an airport")]
    public static async Task<object> ReferenceGetMostTraveledDestinations(
        AmadeusHttpClient httpClient,
        [Description("Origin airport IATA code (e.g., CDG, JFK)")] string originAirportCode,
        [Description("Time period: YEAR, MONTH")] string period = "YEAR",
        [Description("Maximum number of results")] int max = 10)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["originAirportCode"] = originAirportCode,
            ["period"] = period,
            ["max"] = max.ToString()
        };

        var response = await httpClient.GetJsonAsync("/v1/travel/analytics/air-traffic/traveled", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[REFERENCE] Get most booked destinations from an airport")]
    public static async Task<object> ReferenceGetMostBookedDestinations(
        AmadeusHttpClient httpClient,
        [Description("Origin airport IATA code (e.g., CDG, JFK)")] string originAirportCode,
        [Description("Time period: YEAR, MONTH")] string period = "YEAR",
        [Description("Maximum number of results")] int max = 10)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["originAirportCode"] = originAirportCode,
            ["period"] = period,
            ["max"] = max.ToString()
        };

        var response = await httpClient.GetJsonAsync("/v1/travel/analytics/air-traffic/booked", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[REFERENCE] Get busiest traveling period for a route")]
    public static async Task<object> ReferenceGetBusiestTravelingPeriod(
        AmadeusHttpClient httpClient,
        [Description("Origin city/airport IATA code (e.g., NYC, CDG)")] string originCityCode,
        [Description("Destination city/airport IATA code (e.g., PAR, JFK)")] string destinationCityCode,
        [Description("Direction: ARRIVING, DEPARTING")] string direction = "DEPARTING",
        [Description("Maximum number of results")] int max = 10)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["cityCode"] = originCityCode,
            ["direction"] = direction,
            ["max"] = max.ToString()
        };

        if (direction == "ARRIVING")
        {
            queryParams["cityCode"] = destinationCityCode;
        }

        var response = await httpClient.GetJsonAsync("/v1/travel/analytics/air-traffic/busiest-period", queryParams);
        return response;
    }

    #endregion

    #region Trip and Travel Purpose

    [McpServerTool]
    [Description("[REFERENCE] Predict trip purpose for a journey")]
    public static async Task<object> ReferenceGetTripPurposePrediction(
        AmadeusHttpClient httpClient,
        [Description("Origin airport IATA code (e.g., CDG, JFK)")] string originLocationCode,
        [Description("Destination airport IATA code (e.g., PAR, BKK)")] string destinationLocationCode,
        [Description("Departure date (YYYY-MM-DD format)")] string departureDate,
        [Description("Return date (YYYY-MM-DD format) - optional")] string? returnDate = null,
        [Description("Search date (YYYY-MM-DD format) - when the search was made")] string? searchDate = null)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["originLocationCode"] = originLocationCode,
            ["destinationLocationCode"] = destinationLocationCode,
            ["departureDate"] = departureDate
        };

        if (!string.IsNullOrEmpty(returnDate))
            queryParams["returnDate"] = returnDate;

        if (!string.IsNullOrEmpty(searchDate))
            queryParams["searchDate"] = searchDate;

        var response = await httpClient.GetJsonAsync("/v1/travel/predictions/trip-purpose", queryParams);
        return response;
    }

    #endregion

    #region Hotel Name Autocomplete

    [McpServerTool]
    [Description("[REFERENCE] Get hotel name suggestions for autocomplete")]
    public static async Task<object> ReferenceGetHotelNameAutocomplete(
        AmadeusHttpClient httpClient,
        [Description("Hotel name keyword for autocomplete")] string keyword,
        [Description("Subtype filter: HOTEL_LEISURE, HOTEL_GDS")] string subType = "HOTEL_LEISURE",
        [Description("Country code filter (ISO 3166-1 alpha-2)")] string? countryCode = null,
        [Description("Maximum number of results")] int pageLimit = 10)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["keyword"] = keyword,
            ["subType"] = subType,
            ["page[limit]"] = pageLimit.ToString()
        };

        if (!string.IsNullOrEmpty(countryCode))
            queryParams["countryCode"] = countryCode;

        var response = await httpClient.GetJsonAsync("/v1/reference-data/locations/hotel", queryParams);
        return response;
    }

    #endregion

    #region Authentication and System

    [McpServerTool]
    [Description("[REFERENCE] Test Amadeus API authentication and connectivity")]
    public static async Task<object> ReferenceTestAuthentication(
        AmadeusHttpClient httpClient)
    {
        try
        {
            // Simple test call to verify authentication
            var queryParams = new Dictionary<string, string>
            {
                ["keyword"] = "test",
                ["page[limit]"] = "1"
            };

            var response = await httpClient.GetJsonAsync("/v1/reference-data/locations", queryParams);
            return new { status = "success", message = "Authentication successful", data = response };
        }
        catch (Exception ex)
        {
            return new { status = "error", message = "Authentication failed", error = ex.Message };
        }
    }

    #endregion
}