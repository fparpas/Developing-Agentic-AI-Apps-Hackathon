using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using TravelMcpServer.Http;

namespace TravelMcpServer.Tools;

/// <summary>
/// MCP tools for comprehensive hotel operations using Amadeus APIs
/// </summary>
[McpServerToolType]
[Description("Comprehensive hotel operations including search, booking, and reviews")]
public class HotelTools
{
    #region Hotel Search Operations

    [McpServerTool]
    [Description("[HOTEL] Search for hotels in a specific city with filters for amenities, ratings, and other preferences")]
    public static async Task<object> HotelSearchHotelsByCity(
        AmadeusHttpClient httpClient,
        [Description("3-letter IATA city code (e.g., PAR for Paris, NYC for New York)")] string cityCode,
        [Description("Search radius in kilometers (1-300, default: 5)")] int radius = 5,
        [Description("Radius unit: KM or MILE (default: KM)")] string radiusUnit = "KM",
        [Description("Array of 2-letter hotel chain codes (e.g., ['AC', 'HI'])")] string[]? chainCodes = null,
        [Description("Array of amenities: SWIMMING_POOL, SPA, FITNESS_CENTER, AIR_CONDITIONING, RESTAURANT, PARKING, PETS_ALLOWED, AIRPORT_SHUTTLE, BUSINESS_CENTER, DISABLED_FACILITIES, WIFI, MEETING_ROOMS, NO_KID_ALLOWED, TENNIS, GOLF, KITCHEN, ANIMAL_WATCHING, BABY-SITTING, BEACH, CASINO, JACUZZI, SAUNA, SOLARIUM, MASSAGE, VALET_PARKING, BAR_or_LOUNGE, KIDS_WELCOME, NO_PORN_FILMS, MINIBAR, TELEVISION, WI-FI_IN_ROOM, ROOM_SERVICE, GUARDED_PARKG, SERV_SPEC_MENU")] string[]? amenities = null,
        [Description("Array of hotel star ratings (1-5)")] string[]? ratings = null,
        [Description("Hotel source: BEDBANK (aggregators), DIRECTCHAIN (GDS), ALL (both) - default: ALL")] string hotelSource = "ALL")
    {
        var queryParams = new Dictionary<string, string>
        {
            ["cityCode"] = cityCode,
            ["radius"] = radius.ToString(),
            ["radiusUnit"] = radiusUnit,
            ["hotelSource"] = hotelSource
        };

        if (chainCodes?.Length > 0)
            queryParams["chainCodes"] = string.Join(",", chainCodes);
        
        if (amenities?.Length > 0)
            queryParams["amenities"] = string.Join(",", amenities);
        
        if (ratings?.Length > 0)
            queryParams["ratings"] = string.Join(",", ratings);

        var response = await httpClient.GetJsonAsync("/v1/reference-data/locations/hotels/by-city", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[HOTEL] Search for hotels by geographical coordinates (latitude/longitude) with comprehensive filtering options")]
    public static async Task<object> HotelSearchHotelsByGeocode(
        AmadeusHttpClient httpClient,
        [Description("Latitude (-90 to 90 degrees)")] double latitude,
        [Description("Longitude (-180 to 180 degrees)")] double longitude,
        [Description("Search radius (1-300, default: 5)")] int radius = 5,
        [Description("Radius unit: KM or MILE (default: KM)")] string radiusUnit = "KM",
        [Description("Array of 2-letter hotel chain codes")] string[]? chainCodes = null,
        [Description("Array of amenities (up to 3)")] string[]? amenities = null,
        [Description("Array of hotel star ratings (1-5)")] string[]? ratings = null,
        [Description("Hotel source: BEDBANK, DIRECTCHAIN, or ALL")] string hotelSource = "ALL")
    {
        var queryParams = new Dictionary<string, string>
        {
            ["latitude"] = latitude.ToString(),
            ["longitude"] = longitude.ToString(),
            ["radius"] = radius.ToString(),
            ["radiusUnit"] = radiusUnit,
            ["hotelSource"] = hotelSource
        };

        if (chainCodes?.Length > 0)
            queryParams["chainCodes"] = string.Join(",", chainCodes);
        
        if (amenities?.Length > 0)
            queryParams["amenities"] = string.Join(",", amenities);
        
        if (ratings?.Length > 0)
            queryParams["ratings"] = string.Join(",", ratings);

        var response = await httpClient.GetJsonAsync("/v1/reference-data/locations/hotels/by-geocode", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[HOTEL] Retrieve detailed information for specific hotels using their Amadeus Property Codes")]
    public static async Task<object> HotelGetHotelsByIds(
        AmadeusHttpClient httpClient,
        [Description("Array of Amadeus Property Codes (8-character hotel IDs, e.g., ['ACPAR419', 'HLPAR123'])")] string[] hotelIds)
    {
        if (hotelIds == null || hotelIds.Length == 0)
            throw new ArgumentException("At least one hotel ID is required");

        var queryParams = new Dictionary<string, string>
        {
            ["hotelIds"] = string.Join(",", hotelIds)
        };

        var response = await httpClient.GetJsonAsync("/v1/reference-data/locations/hotels/by-hotels", queryParams);
        return response;
    }

    #endregion

    #region Hotel Availability & Pricing

    [McpServerTool]
    [Description("[HOTEL] Search for hotel offers with pricing and availability for specific dates")]
    public static async Task<object> HotelSearchHotelOffers(
        AmadeusHttpClient httpClient,
        [Description("Array of Amadeus hotel IDs (up to 3 hotels)")] string[] hotelIds,
        [Description("Check-in date (YYYY-MM-DD format)")] string checkInDate,
        [Description("Check-out date (YYYY-MM-DD format)")] string checkOutDate,
        [Description("Number of adult guests (1-9, default: 1)")] int adults = 1,
        [Description("Number of child guests (0-9, default: 0)")] int childAges = 0,
        [Description("Number of rooms (1-9, default: 1)")] int roomQuantity = 1,
        [Description("Price currency (3-letter ISO code, e.g., USD, EUR)")] string? currency = null,
        [Description("Payment policy: GUARANTEE, DEPOSIT, NONE")] string? paymentPolicy = null,
        [Description("Board type: ROOM_ONLY, BREAKFAST, HALF_BOARD, FULL_BOARD, ALL_INCLUSIVE")] string? boardType = null,
        [Description("Include closed hotels in results")] bool includeClosed = false,
        [Description("Best rate only")] bool bestRateOnly = true,
        [Description("Preferred language code (e.g., EN, FR, ES)")] string? lang = null)
    {

        var queryParams = new Dictionary<string, string>
        {
            ["hotelIds"] = string.Join(",", hotelIds),
            ["checkInDate"] = checkInDate,
            ["checkOutDate"] = checkOutDate,
            ["adults"] = adults.ToString(),
            ["roomQuantity"] = roomQuantity.ToString(),
            ["includeClosed"] = includeClosed.ToString().ToLower(),
            ["bestRateOnly"] = bestRateOnly.ToString().ToLower()
        };

        if (childAges > 0)
            queryParams["childAges"] = childAges.ToString();
        
        if (!string.IsNullOrEmpty(currency))
            queryParams["currency"] = currency;
        
        if (!string.IsNullOrEmpty(paymentPolicy))
            queryParams["paymentPolicy"] = paymentPolicy;
        
        if (!string.IsNullOrEmpty(boardType))
            queryParams["boardType"] = boardType;
        
        if (!string.IsNullOrEmpty(lang))
            queryParams["lang"] = lang;

        var response = await httpClient.GetJsonAsync("/v3/shopping/hotel-offers", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[HOTEL] Get detailed information for a specific hotel offer including terms, conditions, and policies")]
    public static async Task<object> HotelGetHotelOffer(
        AmadeusHttpClient httpClient,
        [Description("Unique offer ID from hotel search results")] string offerId,
        [Description("Preferred language code (e.g., EN, FR, ES)")] string? lang = null)
    {
        var queryParams = new Dictionary<string, string>();
        
        if (!string.IsNullOrEmpty(lang))
            queryParams["lang"] = lang;

        var response = await httpClient.GetJsonAsync($"/v3/shopping/hotel-offers/{offerId}", queryParams);
        return response;
    }

    #endregion

    #region Hotel Bookings

    [McpServerTool]
    [Description("[HOTEL] Create a hotel booking reservation with guest details and payment information")]
    public static async Task<object> HotelCreateHotelBooking(
        AmadeusHttpClient httpClient,
        [Description("Hotel offer data from previous search (JSON string)")] string offerData,
        [Description("Primary guest information (JSON: {firstName, lastName, phone, email})")] string guestInfo,
        [Description("Additional guests information (JSON array)")] string? additionalGuests = null,
        [Description("Payment information (JSON: {creditCard: {vendorCode, cardNumber, expiryDate, holderName}})")] string? paymentInfo = null,
        [Description("Special requests or comments")] string? comments = null)
    {
        try
        {
            var offer = JsonSerializer.Deserialize<object>(offerData);
            var guest = JsonSerializer.Deserialize<object>(guestInfo);
            
            var bookingRequest = new
            {
                data = new
                {
                    type = "hotel-booking",
                    hotelOffer = offer,
                    guests = new[] { guest },
                    payments = !string.IsNullOrEmpty(paymentInfo) ? 
                        new[] { JsonSerializer.Deserialize<object>(paymentInfo) } : null,
                    rooms = new[]
                    {
                        new
                        {
                            guestIds = new[] { 1 },
                            paymentId = !string.IsNullOrEmpty(paymentInfo) ? 1 : (int?)null,
                            specialRequest = comments
                        }
                    }
                }
            };

            var response = await httpClient.PostJsonAsync("/v2/booking/hotel-bookings", bookingRequest);
            return response;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON format in request data: {ex.Message}");
        }
    }

    [McpServerTool]
    [Description("[HOTEL] Retrieve details of an existing hotel booking by confirmation number")]
    public static async Task<object> HotelGetHotelBooking(
        AmadeusHttpClient httpClient,
        [Description("Hotel booking confirmation ID/locator")] string bookingId)
    {
        var response = await httpClient.GetJsonAsync($"/v1/booking/hotel-bookings/{bookingId}");
        return response;
    }

    #endregion

    #region Hotel Ratings & Reviews

    [McpServerTool]
    [Description("[HOTEL] Get hotel sentiment analysis and ratings based on guest reviews")]
    public static async Task<object> HotelGetHotelRatings(
        AmadeusHttpClient httpClient,
        [Description("Array of Amadeus hotel IDs to get ratings for")] string[] hotelIds)
    {
        if (hotelIds == null || hotelIds.Length == 0)
            throw new ArgumentException("At least one hotel ID is required");

        var queryParams = new Dictionary<string, string>
        {
            ["hotelIds"] = string.Join(",", hotelIds)
        };

        var response = await httpClient.GetJsonAsync("/v2/e-reputation/hotel-sentiments", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[HOTEL] Analyze hotel sentiment with detailed breakdown by category (service, location, comfort, etc.)")]
    public static async Task<object> HotelAnalyzeHotelSentiment(
        AmadeusHttpClient httpClient,
        [Description("Single Amadeus hotel ID for detailed sentiment analysis")] string hotelId)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["hotelIds"] = hotelId
        };

        var response = await httpClient.GetJsonAsync("/v2/e-reputation/hotel-sentiments", queryParams);
        
        // Extract and format sentiment data for better readability
        if (response.RootElement.TryGetProperty("data", out var dataArray))
        {
            var hotels = new List<object>();
            foreach (var hotel in dataArray.EnumerateArray())
            {
                if (hotel.TryGetProperty("hotelId", out var id) && 
                    hotel.TryGetProperty("sentiments", out var sentiments))
                {
                    var sentimentAnalysis = new
                    {
                        HotelId = id.GetString(),
                        OverallRating = hotel.TryGetProperty("overallRating", out var rating) ? rating.GetInt32() : (int?)null,
                        NumberOfRatings = hotel.TryGetProperty("numberOfRatings", out var count) ? count.GetInt32() : (int?)null,
                        SentimentBreakdown = sentiments.EnumerateObject().ToDictionary(
                            prop => prop.Name,
                            prop => prop.Value.GetInt32()
                        )
                    };
                    hotels.Add(sentimentAnalysis);
                }
            }
            
            return new { data = hotels };
        }

        return response;
    }

    #endregion

    #region Hotel Information & Details

    [McpServerTool]
    [Description("[HOTEL] Get comprehensive hotel information including amenities, description, and facilities")]
    public static async Task<object> HotelGetHotelDetails(
        AmadeusHttpClient httpClient,
        [Description("Amadeus hotel ID")] string hotelId,
        [Description("Preferred language code (e.g., EN, FR, ES)")] string? lang = null)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["hotelIds"] = hotelId
        };

        if (!string.IsNullOrEmpty(lang))
            queryParams["lang"] = lang;

        var response = await httpClient.GetJsonAsync("/v1/reference-data/locations/hotels/by-hotels", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[HOTEL] Advanced hotel search with multiple criteria and sorting options")]
    public static async Task<object> HotelSearchHotelsAdvanced(
        AmadeusHttpClient httpClient,
        [Description("Search criteria: 'city' for city search, 'geocode' for coordinate search")] string searchType,
        [Description("City code (for city search) or latitude (for geocode search)")] string primaryParam,
        [Description("Longitude (required for geocode search, ignored for city search)")] double? longitude = null,
        [Description("Search radius in kilometers")] int radius = 5,
        [Description("Minimum star rating (1-5)")] int? minRating = null,
        [Description("Maximum star rating (1-5)")] int? maxRating = null,
        [Description("Required amenities (comma-separated)")] string? requiredAmenities = null,
        [Description("Preferred hotel chains (comma-separated 2-letter codes)")] string? preferredChains = null,
        [Description("Sort results by: distance, rating, price")] string sortBy = "distance")
    {
        var queryParams = new Dictionary<string, string>
        {
            ["radius"] = radius.ToString(),
            ["radiusUnit"] = "KM"
        };

        // Determine search endpoint and add location parameters
        string endpoint;
        if (searchType.ToLower() == "city")
        {
            endpoint = "/v1/reference-data/locations/hotels/by-city";
            queryParams["cityCode"] = primaryParam;
        }
        else if (searchType.ToLower() == "geocode")
        {
            if (!longitude.HasValue)
                throw new ArgumentException("Longitude is required for geocode search");
            
            endpoint = "/v1/reference-data/locations/hotels/by-geocode";
            queryParams["latitude"] = primaryParam;
            queryParams["longitude"] = longitude.Value.ToString();
        }
        else
        {
            throw new ArgumentException("SearchType must be 'city' or 'geocode'");
        }

        // Add rating filter
        if (minRating.HasValue || maxRating.HasValue)
        {
            var ratings = new List<string>();
            for (int i = minRating ?? 1; i <= (maxRating ?? 5); i++)
            {
                ratings.Add(i.ToString());
            }
            queryParams["ratings"] = string.Join(",", ratings);
        }

        // Add amenities filter
        if (!string.IsNullOrEmpty(requiredAmenities))
        {
            queryParams["amenities"] = requiredAmenities;
        }

        // Add chain codes filter
        if (!string.IsNullOrEmpty(preferredChains))
        {
            queryParams["chainCodes"] = preferredChains;
        }

        var response = await httpClient.GetJsonAsync(endpoint, queryParams);
        
        // Note: Amadeus API doesn't support server-side sorting by rating/price for location search
        // Sorting would need to be implemented client-side if needed
        
        return response;
    }

    #endregion
}