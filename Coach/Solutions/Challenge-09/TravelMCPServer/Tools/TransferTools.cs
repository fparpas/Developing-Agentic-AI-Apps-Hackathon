using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using TravelMcpServer.Http;

namespace TravelMcpServer.Tools;

/// <summary>
/// MCP tools for ground transfer and transportation services using Amadeus APIs
/// </summary>
[McpServerToolType]
[Description("Ground transfer and transportation services including private, shared, and taxi transfers")]
public class TransferTools
{
    #region Transfer Search Operations

    [McpServerTool]
    [Description("[TRANSFER] Search for transfer offers between locations")]
    public static async Task<object> TransferSearchOffers(
        AmadeusHttpClient httpClient,
        [Description("Start date and time (YYYY-MM-DDTHH:MM:SS format)")] string startDateTime,
        [Description("Start location IATA code (e.g., CDG, JFK)")] string? startLocationCode = null,
        [Description("Start address line")] string? startAddressLine = null,
        [Description("Start city name")] string? startCityName = null,
        [Description("Start country code (ISO 3166-1 alpha-2)")] string? startCountryCode = null,
        [Description("Start zip/postal code")] string? startZipCode = null,
        [Description("Start geo coordinates (latitude,longitude)")] string? startGeoCode = null,
        [Description("End location IATA code (e.g., CDG, JFK)")] string? endLocationCode = null,
        [Description("End address line")] string? endAddressLine = null,
        [Description("End city name")] string? endCityName = null,
        [Description("End country code (ISO 3166-1 alpha-2)")] string? endCountryCode = null,
        [Description("End zip/postal code")] string? endZipCode = null,
        [Description("End geo coordinates (latitude,longitude)")] string? endGeoCode = null,
        [Description("Number of passengers (default: 1)")] int passengers = 1,
        [Description("Transfer type: PRIVATE, SHARED, TAXI, HOURLY, AIRPORT_EXPRESS, AIRPORT_BUS")] string? transferType = null,
        [Description("Language code (ISO 639-1, e.g., EN, FR)")] string language = "EN",
        [Description("Currency code (ISO 4217, e.g., USD, EUR)")] string? currency = null,
        [Description("Vehicle category: ST (Standard), BU (Business), FC (First Class)")] string? vehicleCategory = null,
        [Description("Vehicle code: CAR, SED, VAN, SUV, LMS, BUS, etc.")] string? vehicleCode = null)
    {
        var requestBody = new Dictionary<string, object>
        {
            ["startDateTime"] = startDateTime,
            ["passengers"] = passengers,
            ["language"] = language
        };

        // Start location information
        if (!string.IsNullOrEmpty(startLocationCode))
            requestBody["startLocationCode"] = startLocationCode;
        if (!string.IsNullOrEmpty(startAddressLine))
            requestBody["startAddressLine"] = startAddressLine;
        if (!string.IsNullOrEmpty(startCityName))
            requestBody["startCityName"] = startCityName;
        if (!string.IsNullOrEmpty(startCountryCode))
            requestBody["startCountryCode"] = startCountryCode;
        if (!string.IsNullOrEmpty(startZipCode))
            requestBody["startZipCode"] = startZipCode;
        if (!string.IsNullOrEmpty(startGeoCode))
            requestBody["startGeoCode"] = startGeoCode;

        // End location information
        if (!string.IsNullOrEmpty(endLocationCode))
            requestBody["endLocationCode"] = endLocationCode;
        if (!string.IsNullOrEmpty(endAddressLine))
            requestBody["endAddressLine"] = endAddressLine;
        if (!string.IsNullOrEmpty(endCityName))
            requestBody["endCityName"] = endCityName;
        if (!string.IsNullOrEmpty(endCountryCode))
            requestBody["endCountryCode"] = endCountryCode;
        if (!string.IsNullOrEmpty(endZipCode))
            requestBody["endZipCode"] = endZipCode;
        if (!string.IsNullOrEmpty(endGeoCode))
            requestBody["endGeoCode"] = endGeoCode;

        // Optional parameters
        if (!string.IsNullOrEmpty(transferType))
            requestBody["transferType"] = transferType;
        if (!string.IsNullOrEmpty(currency))
            requestBody["currency"] = currency;
        if (!string.IsNullOrEmpty(vehicleCategory))
            requestBody["vehicleCategory"] = vehicleCategory;
        if (!string.IsNullOrEmpty(vehicleCode))
            requestBody["vehicleCode"] = vehicleCode;

        var response = await httpClient.PostJsonAsync("/v1/shopping/transfer-offers", requestBody);
        return response;
    }

    [McpServerTool]
    [Description("[TRANSFER] Search for airport transfer options")]
    public static async Task<object> TransferSearchAirportTransfers(
        AmadeusHttpClient httpClient,
        [Description("Airport IATA code (e.g., CDG, JFK)")] string airportCode,
        [Description("Destination address line")] string destinationAddress,
        [Description("Destination city")] string destinationCity,
        [Description("Destination country code (ISO 3166-1 alpha-2)")] string destinationCountryCode,
        [Description("Start date and time (YYYY-MM-DDTHH:MM:SS format)")] string startDateTime,
        [Description("Number of passengers (default: 1)")] int passengers = 1,
        [Description("Transfer type: PRIVATE, SHARED, TAXI, AIRPORT_EXPRESS, AIRPORT_BUS")] string transferType = "PRIVATE",
        [Description("Language code (ISO 639-1, e.g., EN, FR)")] string language = "EN",
        [Description("Currency code (ISO 4217, e.g., USD, EUR)")] string? currency = null)
    {
        var requestBody = new
        {
            startDateTime,
            passengers,
            startLocationCode = airportCode,
            endAddressLine = destinationAddress,
            endCityName = destinationCity,
            endCountryCode = destinationCountryCode,
            transferType,
            language,
            currency
        };

        var response = await httpClient.PostJsonAsync("/v1/shopping/transfer-offers", requestBody);
        return response;
    }

    [McpServerTool]
    [Description("[TRANSFER] Search for hourly transfer services")]
    public static async Task<object> TransferSearchHourly(
        AmadeusHttpClient httpClient,
        [Description("Start location IATA code (e.g., CDG, JFK)")] string startLocationCode,
        [Description("Start date and time (YYYY-MM-DDTHH:MM:SS format)")] string startDateTime,
        [Description("Duration in ISO8601 format (e.g., PT2H30M for 2 hours 30 minutes)")] string duration,
        [Description("Number of passengers (default: 1)")] int passengers = 1,
        [Description("Language code (ISO 639-1, e.g., EN, FR)")] string language = "EN",
        [Description("Currency code (ISO 4217, e.g., USD, EUR)")] string? currency = null,
        [Description("Vehicle category: ST (Standard), BU (Business), FC (First Class)")] string? vehicleCategory = null)
    {
        var requestBody = new
        {
            startDateTime,
            passengers,
            startLocationCode,
            transferType = "HOURLY",
            duration,
            language,
            currency,
            vehicleCategory
        };

        var response = await httpClient.PostJsonAsync("/v1/shopping/transfer-offers", requestBody);
        return response;
    }

    #endregion

    #region Transfer Booking Operations

    [McpServerTool]
    [Description("[TRANSFER] Book a transfer offer")]
    public static async Task<object> TransferBookOffer(
        AmadeusHttpClient httpClient,
        [Description("Transfer offer ID from search results")] string offerId,
        [Description("Primary contact information (JSON: {firstName, lastName, phoneNumber, email})")] string contactInfo,
        [Description("Payment information (JSON: {creditCard: {vendorCode, cardNumber, expiryDate, holderName}})")] string? paymentInfo = null,
        [Description("Special requests or comments")] string? comments = null,
        [Description("Flight information if airport transfer (JSON: {flightNumber, departureTime})")] string? flightInfo = null)
    {
        try
        {
            var contact = JsonSerializer.Deserialize<object>(contactInfo);
            var requestBody = new Dictionary<string, object>
            {
                ["data"] = new
                {
                    offerId,
                    contact,
                    comments
                }
            };

            if (!string.IsNullOrEmpty(paymentInfo))
            {
                var payment = JsonSerializer.Deserialize<object>(paymentInfo);
                ((dynamic)requestBody["data"]).payment = payment;
            }

            if (!string.IsNullOrEmpty(flightInfo))
            {
                var flight = JsonSerializer.Deserialize<object>(flightInfo);
                ((dynamic)requestBody["data"]).flightInfo = flight;
            }

            var response = await httpClient.PostJsonAsync("/v1/booking/transfers", requestBody);
            return response;
        }
        catch (JsonException ex)
        {
            return new { error = "Invalid data format", details = ex.Message };
        }
    }

    [McpServerTool]
    [Description("[TRANSFER] Get transfer booking details")]
    public static async Task<object> TransferGetBooking(
        AmadeusHttpClient httpClient,
        [Description("Transfer booking ID/confirmation number")] string bookingId)
    {
        var response = await httpClient.GetJsonAsync($"/v1/booking/transfers/{bookingId}");
        return response;
    }

    [McpServerTool]
    [Description("[TRANSFER] Cancel a transfer booking")]
    public static async Task<object> TransferCancelBooking(
        AmadeusHttpClient httpClient,
        [Description("Transfer booking ID/confirmation number")] string bookingId,
        [Description("Cancellation reason")] string? reason = null)
    {
        var requestBody = new
        {
            reason
        };

        var response = await httpClient.PostJsonAsync($"/v1/booking/transfers/{bookingId}/cancel", requestBody);
        return response;
    }

    [McpServerTool]
    [Description("[TRANSFER] Modify a transfer booking")]
    public static async Task<object> TransferModifyBooking(
        AmadeusHttpClient httpClient,
        [Description("Transfer booking ID/confirmation number")] string bookingId,
        [Description("Updated booking information (JSON with modified details)")] string updatedBookingData)
    {
        try
        {
            var updateData = JsonSerializer.Deserialize<object>(updatedBookingData);
            var requestBody = new
            {
                data = updateData
            };

            var response = await httpClient.PostJsonAsync($"/v1/booking/transfers/{bookingId}/modify", requestBody);
            return response;
        }
        catch (JsonException ex)
        {
            return new { error = "Invalid booking data format", details = ex.Message };
        }
    }

    #endregion

    #region Transfer Management

    [McpServerTool]
    [Description("[TRANSFER] Get transfer provider information")]
    public static async Task<object> TransferGetProviders(
        AmadeusHttpClient httpClient,
        [Description("Location IATA code to get available providers")] string locationCode)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["locationCode"] = locationCode
        };

        var response = await httpClient.GetJsonAsync("/v1/reference-data/transfer-providers", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[TRANSFER] Get transfer service types available at location")]
    public static async Task<object> TransferGetServiceTypes(
        AmadeusHttpClient httpClient,
        [Description("Location IATA code")] string locationCode)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["locationCode"] = locationCode
        };

        var response = await httpClient.GetJsonAsync("/v1/reference-data/transfer-types", queryParams);
        return response;
    }

    [McpServerTool]
    [Description("[TRANSFER] Get available vehicle types for transfers")]
    public static async Task<object> TransferGetVehicleTypes(
        AmadeusHttpClient httpClient,
        [Description("Location IATA code")] string locationCode,
        [Description("Vehicle category filter: ST, BU, FC")] string? category = null)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["locationCode"] = locationCode
        };

        if (!string.IsNullOrEmpty(category))
            queryParams["category"] = category;

        var response = await httpClient.GetJsonAsync("/v1/reference-data/vehicle-types", queryParams);
        return response;
    }

    #endregion
}