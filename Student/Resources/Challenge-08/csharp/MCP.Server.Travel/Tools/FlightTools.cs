using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using TravelMcpServer.Http;

namespace TravelMcpServer.Tools;

/// <summary>
/// MCP tools for comprehensive flight operations using Amadeus APIs
/// </summary>
[McpServerToolType]
[Description("Comprehensive flight operations for search, booking, and management")]
public class FlightTools
{
    #region Flight Search Operations

    [McpServerTool]
    [Description("[FLIGHT] Search for flight offers")]
    public static async Task<object> SearchFlightOffers(
        AmadeusHttpClient httpClient,
        [Description("Origin IATA code (e.g., NYC, LON)")] string originLocationCode,
        [Description("Destination IATA code (e.g., PAR, BKK)")] string destinationLocationCode,
        [Description("Departure date (YYYY-MM-DD format)")] string departureDate,
        [Description("Number of adult travelers (1-9)")] int adults = 1,
        [Description("Return date (YYYY-MM-DD format) - optional for one-way")] string? returnDate = null,
        [Description("Number of children (0-9)")] int children = 0,
        [Description("Number of infants (0-9)")] int infants = 0,
        [Description("Travel class: ECONOMY, PREMIUM_ECONOMY, BUSINESS, FIRST")] string? travelClass = null,
        [Description("Included airline codes (comma-separated IATA codes)")] string? includedAirlineCodes = null,
        [Description("Excluded airline codes (comma-separated IATA codes)")] string? excludedAirlineCodes = null,
        [Description("Non-stop flights only")] bool nonStop = false,
        [Description("Currency code (ISO 4217 format, e.g., USD, EUR)")] string? currencyCode = null,
        [Description("Maximum price per traveler")] int? maxPrice = null,
        [Description("Maximum number of offers to return (default: 250)")] int max = 20)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["originLocationCode"] = originLocationCode,
            ["destinationLocationCode"] = destinationLocationCode,
            ["departureDate"] = departureDate,
            ["adults"] = adults.ToString(),
            ["nonStop"] = nonStop.ToString().ToLower(),
            ["max"] = max.ToString()
        };

        if (!string.IsNullOrEmpty(returnDate))
            queryParams["returnDate"] = returnDate;

        if (children > 0)
            queryParams["children"] = children.ToString();

        if (infants > 0)
            queryParams["infants"] = infants.ToString();

        if (!string.IsNullOrEmpty(travelClass))
            queryParams["travelClass"] = travelClass;

        if (!string.IsNullOrEmpty(includedAirlineCodes))
            queryParams["includedAirlineCodes"] = includedAirlineCodes;

        if (!string.IsNullOrEmpty(excludedAirlineCodes))
            queryParams["excludedAirlineCodes"] = excludedAirlineCodes;

        if (!string.IsNullOrEmpty(currencyCode))
            queryParams["currencyCode"] = currencyCode;

        if (maxPrice.HasValue)
            queryParams["maxPrice"] = maxPrice.Value.ToString();

        var response = await httpClient.GetJsonAsync("/v2/shopping/flight-offers", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[FLIGHT] Get cheapest flight dates")]
    public static async Task<object> CheapestDateSearch(
        AmadeusHttpClient httpClient,
        [Description("Origin IATA code (e.g., NYC, LON)")] string origin,
        [Description("Destination IATA code (e.g., PAR, BKK)")] string destination,
        [Description("Departure date or month (YYYY-MM-DD or YYYY-MM format)")] string departureDate,
        [Description("Return date (YYYY-MM-DD format) - optional")] string? returnDate = null,
        [Description("Number of adults (1-9)")] int adults = 1,
        [Description("Currency code (ISO 4217 format)")] string? currency = null,
        [Description("Maximum price threshold")] int? maxPrice = null,
        [Description("One-way flights only")] bool oneWay = false)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["origin"] = origin,
            ["destination"] = destination,
            ["departureDate"] = departureDate,
            ["adults"] = adults.ToString(),
            ["oneWay"] = oneWay.ToString().ToLower()
        };

        if (!string.IsNullOrEmpty(returnDate))
            queryParams["returnDate"] = returnDate;

        if (!string.IsNullOrEmpty(currency))
            queryParams["currency"] = currency;

        if (maxPrice.HasValue)
            queryParams["maxPrice"] = maxPrice.Value.ToString();

        var response = await httpClient.GetJsonAsync("/v1/shopping/flight-dates", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[FLIGHT] Search for flight inspiration with flexible destinations")]
    public static async Task<object> InspirationSearch(
        AmadeusHttpClient httpClient,
        [Description("Origin IATA code (e.g., NYC, LON)")] string origin,
        [Description("Departure date (YYYY-MM-DD format)")] string departureDate,
        [Description("Return date (YYYY-MM-DD format) - optional")] string? returnDate = null,
        [Description("Number of adults (1-9)")] int adults = 1,
        [Description("Currency code (ISO 4217 format)")] string? currency = null,
        [Description("Maximum price threshold")] int? maxPrice = null,
        [Description("One-way flights only")] bool oneWay = false,
        [Description("Duration range (1-15 for days)")] string? duration = null)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["origin"] = origin,
            ["departureDate"] = departureDate,
            ["adults"] = adults.ToString(),
            ["oneWay"] = oneWay.ToString().ToLower()
        };

        if (!string.IsNullOrEmpty(returnDate))
            queryParams["returnDate"] = returnDate;

        if (!string.IsNullOrEmpty(currency))
            queryParams["currency"] = currency;

        if (maxPrice.HasValue)
            queryParams["maxPrice"] = maxPrice.Value.ToString();

        if (!string.IsNullOrEmpty(duration))
            queryParams["duration"] = duration;

        var response = await httpClient.GetJsonAsync("/v1/shopping/flight-destinations", queryParams);
        return response;
    }

    #endregion

    #region Flight Pricing & Availability

    [McpServerTool]
    [Description("[FLIGHT] Confirm flight offer pricing before booking")]
    public static async Task<object> OffersPrice(
        AmadeusHttpClient httpClient,
        [Description("Flight offer data (JSON string from previous search)")] string flightOfferData)
    {
        try
        {
            var flightOffer = JsonSerializer.Deserialize<object>(flightOfferData);
            var requestBody = new
            {
                data = new
                {
                    type = "flight-offers-pricing",
                    flightOffers = new[] { flightOffer }
                }
            };

            var response = await httpClient.PostJsonAsync("/v1/shopping/flight-offers/pricing", requestBody);
            return response;
        }
        catch (JsonException ex)
        {
            return new { error = "Invalid flight offer data format", details = ex.Message };
        }
    }

    [McpServerTool]
    [Description("[FLIGHT] Search for flight availability")]
    public static async Task<object> AvailabilitySearch(
        AmadeusHttpClient httpClient,
        [Description("Origin IATA code (e.g., NYC, LON)")] string originLocationCode,
        [Description("Destination IATA code (e.g., PAR, BKK)")] string destinationLocationCode,
        [Description("Departure date (YYYY-MM-DD format)")] string departureDate,
        [Description("Travel class: ECONOMY, PREMIUM_ECONOMY, BUSINESS, FIRST")] string travelClass = "ECONOMY",
        [Description("Number of adults (1-9)")] int adults = 1,
        [Description("Return date (YYYY-MM-DD format) - optional")] string? returnDate = null)
    {
        var requestBody = new
        {
            originDestinations = new[]
            {
                new
                {
                    id = "1",
                    originLocationCode,
                    destinationLocationCode,
                    departureDate = new { date = departureDate }
                }
            },
            travelers = Enumerable.Range(1, adults).Select(i => new
            {
                id = i.ToString(),
                travelerType = "ADULT"
            }).ToArray(),
            sources = new[] { "GDS" },
            searchCriteria = new
            {
                flightFilters = new
                {
                    cabinRestrictions = new[]
                    {
                        new
                        {
                            cabin = travelClass.ToUpper(),
                            coverage = "MOST_SEGMENTS",
                            originDestinationIds = new[] { "1" }
                        }
                    }
                }
            }
        };

        var response = await httpClient.PostJsonAsync("/v1/shopping/availability/flight-availabilities", requestBody);
        return response;
    }

    #endregion

     #region Flight Predictions & Analytics

    [McpServerTool]
    [Description("[FLIGHT] Predict flight delays for a specific route")]
    public static async Task<object> DelayPrediction(
        AmadeusHttpClient httpClient,
        [Description("Origin IATA code (e.g., NYC, LON)")] string originLocationCode,
        [Description("Destination IATA code (e.g., PAR, BKK)")] string destinationLocationCode,
        [Description("Departure date (YYYY-MM-DD format)")] string departureDate,
        [Description("Departure time (HH:MM:SS format)")] string departureTime,
        [Description("Arrival date (YYYY-MM-DD format)")] string arrivalDate,
        [Description("Arrival time (HH:MM:SS format)")] string arrivalTime,
        [Description("Aircraft code (IATA format)")] string aircraftCode,
        [Description("Carrier code (IATA format)")] string carrierCode,
        [Description("Flight number")] string flightNumber)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["originLocationCode"] = originLocationCode,
            ["destinationLocationCode"] = destinationLocationCode,
            ["departureDate"] = departureDate,
            ["departureTime"] = departureTime,
            ["arrivalDate"] = arrivalDate,
            ["arrivalTime"] = arrivalTime,
            ["aircraftCode"] = aircraftCode,
            ["carrierCode"] = carrierCode,
            ["flightNumber"] = flightNumber
        };

        var response = await httpClient.GetJsonAsync("/v1/travel/predictions/flight-delay", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[FLIGHT] Get flight choice prediction to help travelers decide")]
    public static async Task<object> ChoicePrediction(
        AmadeusHttpClient httpClient,
        [Description("Flight offers data (JSON string from search results)")] string flightOffersData)
    {
        try
        {
            var flightOffers = JsonSerializer.Deserialize<object[]>(flightOffersData);
            if (flightOffers == null)
            {
                return new { error = "Failed to deserialize flight offers data" };
            }
            
            var requestBody = flightOffers;

            var response = await httpClient.PostJsonAsync("/v2/travel/predictions/flight-choice", requestBody);
            return response;
        }
        catch (JsonException ex)
        {
            return new { error = "Invalid flight offers data format", details = ex.Message };
        }
    }

    #endregion

    #region Flight Booking Management

    [McpServerTool]
    [Description("[FLIGHT] Create a flight booking order")]
    public static async Task<object> CreateOrder(
        AmadeusHttpClient httpClient,
        [Description("Flight offer data (JSON string)")] string flightOfferData,
        [Description("Traveler information (JSON string with traveler details)")] string travelerData,
        [Description("Contact information (JSON string)")] string contactData,
        [Description("Payment information (JSON string) - optional")] string? paymentData = null)
    {
        try
        {
            var flightOffer = JsonSerializer.Deserialize<object>(flightOfferData);
            var travelers = JsonSerializer.Deserialize<object[]>(travelerData);
            var contacts = JsonSerializer.Deserialize<object[]>(contactData);

            var requestBody = new
            {
                data = new
                {
                    type = "flight-order",
                    flightOffers = new[] { flightOffer },
                    travelers,
                    contacts
                }
            };

            var response = await httpClient.PostJsonAsync("/v1/booking/flight-orders", requestBody);
            return response;
        }
        catch (JsonException ex)
        {
            return new { error = "Invalid data format", details = ex.Message };
        }
    }

    [McpServerTool]
    [Description("[FLIGHT] Retrieve flight order details by ID")]
    public static async Task<object> GetOrder(
        AmadeusHttpClient httpClient,
        [Description("Flight order ID")] string orderId)
    {
        var response = await httpClient.GetJsonAsync($"/v1/booking/flight-orders/{orderId}");
        return response;
    }

    [McpServerTool]
    [Description("[FLIGHT] Cancel a flight order")]
    public static async Task<object> FlightCancelOrder(
        AmadeusHttpClient httpClient,
        [Description("Flight order ID")] string orderId)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"/v1/booking/flight-orders/{orderId}");
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                return new { status = "success", message = "Flight order cancelled successfully", orderId };
            }
            else
            {
                return new { status = "error", message = "Failed to cancel flight order", error = responseContent };
            }
        }
        catch (Exception ex)
        {
            return new { status = "error", message = "Exception occurred while cancelling flight order", error = ex.Message };
        }
    }

    #endregion

    #region Seat Maps & Services

    [McpServerTool]
    [Description("[FLIGHT] Get seat map for a specific flight")]
    public static async Task<object> SeatMapDisplay(
        AmadeusHttpClient httpClient,
        [Description("Flight offer data (JSON string)")] string flightOfferData)
    {
        try
        {
            var flightOffer = JsonSerializer.Deserialize<object>(flightOfferData);
            var requestBody = new
            {
                data = new[] { flightOffer }
            };

            var response = await httpClient.PostJsonAsync("/v1/shopping/seatmaps", requestBody);
            return response;
        }
        catch (JsonException ex)
        {
            return new { error = "Invalid flight offer data format", details = ex.Message };
        }
    }

    [McpServerTool]
    [Description("[FLIGHT] Get airline check-in links")]
    public static async Task<object> CheckInLinks(
        AmadeusHttpClient httpClient,
        [Description("Airline IATA code (e.g., AF, LH)")] string airlineCode,
        [Description("Language code (ISO 639-1 format, e.g., EN, FR)")] string language = "EN")
    {
        var queryParams = new Dictionary<string, string>
        {
            ["airlineCode"] = airlineCode,
            ["language"] = language
        };

        var response = await httpClient.GetJsonAsync("/v2/reference-data/urls/checkin-links", queryParams);
        return response;
    }

    #endregion

    #region Flight Status & Tracking

    [McpServerTool]
    [Description("[FLIGHT] Get real-time flight status information")]
    public static async Task<object> FlightStatus(
        AmadeusHttpClient httpClient,
        [Description("Carrier code (IATA format, e.g., AF)")] string carrierCode,
        [Description("Flight number")] string flightNumber,
        [Description("Scheduled departure date (YYYY-MM-DD format)")] string scheduledDepartureDate)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["carrierCode"] = carrierCode,
            ["flightNumber"] = flightNumber,
            ["scheduledDepartureDate"] = scheduledDepartureDate
        };

        var response = await httpClient.GetJsonAsync("/v2/schedule/flights", queryParams);
        return response;
    }

    #endregion
}