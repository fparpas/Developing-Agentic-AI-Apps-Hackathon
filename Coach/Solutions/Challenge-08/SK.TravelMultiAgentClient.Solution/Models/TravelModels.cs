using System.Text.Json.Serialization;

namespace TravelMultiAgentClient.Models;

/// <summary>
/// Represents a travel request from a customer
/// </summary>
public class TravelRequest
{
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int Adults { get; set; } = 1;
    public int Children { get; set; } = 0;
    public int Infants { get; set; } = 0;
    public string? Budget { get; set; }
    public string? Preferences { get; set; }
    public string? SpecialRequests { get; set; }
    public TripType TripType { get; set; } = TripType.Leisure;
}

/// <summary>
/// Type of trip being planned
/// </summary>
public enum TripType
{
    Leisure,
    Business,
    Family,
    Romantic,
    Adventure,
    Cultural,
    Beach,
    City,
    Nature
}

/// <summary>
/// Represents a complete travel plan with all components
/// </summary>
public class TravelPlan
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public TravelRequest Request { get; set; } = new();
    public List<FlightOption> FlightOptions { get; set; } = new();
    public List<HotelOption> HotelOptions { get; set; } = new();
    public List<ActivityOption> ActivityOptions { get; set; } = new();
    public List<TransferOption> TransferOptions { get; set; } = new();
    public decimal? EstimatedTotalCost { get; set; }
    public string? Summary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Flight option within a travel plan
/// </summary>
public class FlightOption
{
    public string Id { get; set; } = string.Empty;
    public string Airline { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string Duration { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string Class { get; set; } = "Economy";
    public bool IsDirectFlight { get; set; }
    public List<string> Stops { get; set; } = new();
}

/// <summary>
/// Hotel option within a travel plan
/// </summary>
public class HotelOption
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int StarRating { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string PriceUnit { get; set; } = "per night";
    public List<string> Amenities { get; set; } = new();
    public double? ReviewScore { get; set; }
    public string? Description { get; set; }
    public double? DistanceToCenter { get; set; }
}

/// <summary>
/// Activity option within a travel plan
/// </summary>
public class ActivityOption
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string Duration { get; set; } = string.Empty;
    public double? Rating { get; set; }
    public string Location { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Transfer option within a travel plan
/// </summary>
public class TransferOption
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Private, Shared, Taxi, etc.
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string Duration { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public List<string> Features { get; set; } = new();
}