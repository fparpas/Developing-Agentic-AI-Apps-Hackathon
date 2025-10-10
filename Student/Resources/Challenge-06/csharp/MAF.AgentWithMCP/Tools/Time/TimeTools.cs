using System.ComponentModel;

// Define the tool function
public static class TimeTools
{
    [Description("Returns the current system time in UTC.")]
    public static string GetCurrentTimeInUTC()
    {
        return $"The current time in UTC is {DateTime.UtcNow}";
    }
}