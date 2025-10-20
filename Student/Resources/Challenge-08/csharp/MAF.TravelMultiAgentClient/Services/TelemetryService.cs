using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace TravelMultiAgentClient.Services;

public class TelemetryService
{
    private const string SourceName = "WorkflowSample";
    private const string ServiceName = "AgentOpenTelemetry";
    private const int MaxBodyCaptureBytes = 4096;

    public TracerProvider CreateTracerProvider(string otlpEndpoint, string appInsightsConnectionString)
    {
        var tracerProviderBuilder = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(ServiceName, serviceVersion: "1.0.0"))
            .AddSource(SourceName) // Our custom activity source
            .AddSource("*Microsoft.Agents.AI") // Agent Framework telemetry
            .AddSource("Microsoft.Agents.AI*") // Agent Framework telemetry
            .AddSource("*Microsoft.Extensions.AI") // Listen to the Experimental.Microsoft.Extensions.AI source for chat client telemetry
                                                   // Listen to the Experimental.Microsoft.Extensions.Agents source for agent telemetry
            .AddHttpClientInstrumentation(options =>
            {
                options.EnrichWithHttpRequestMessage = (activity, request) =>
                {
                    // Capture full URL
                    activity.SetTag("http.request.url", request.RequestUri?.ToString() ?? string.Empty);

                    // Capture query string
                    var queryString = request.RequestUri?.Query;
                    if (!string.IsNullOrEmpty(queryString))
                    {
                        activity.SetTag("http.request.query_string", queryString);

                        // Parse individual query parameters
                        try
                        {
                            var queryParams = queryString.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
                            foreach (var param in queryParams)
                            {
                                var parts = param.Split('=', 2);
                                if (parts.Length >= 1)
                                {
                                    var key = Uri.UnescapeDataString(parts[0]);
                                    var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
                                    activity.SetTag($"http.request.query.{key}", value);
                                }
                            }
                        }
                        catch { /* Ignore parsing errors */ }
                    }

                    // Capture HTTP method
                    activity.SetTag("http.request.method", request.Method.Method);

                    // Capture request headers (excluding sensitive ones)
                    foreach (var header in request.Headers)
                    {
                        if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ||
                            header.Key.Equals("api-key", StringComparison.OrdinalIgnoreCase))
                        {
                            activity.SetTag($"http.request.header.{header.Key}", "(redacted)");
                        }
                        else
                        {
                            activity.SetTag($"http.request.header.{header.Key}", string.Join(",", header.Value));
                        }
                    }

                    // Capture Content-Type
                    var contentType = request.Content?.Headers?.ContentType?.MediaType;
                    if (!string.IsNullOrEmpty(contentType))
                    {
                        activity.SetTag("http.request.content_type", contentType);
                    }

                    // Capture request body
                    var requestBody = SafeReadContent(request.Content, MaxBodyCaptureBytes);
                    if (!string.IsNullOrEmpty(requestBody))
                    {
                        activity.SetTag("http.request.body", requestBody);
                        activity.SetTag("http.request.body_length", requestBody.Length);
                    }
                };

                options.EnrichWithHttpResponseMessage = (activity, response) =>
                {
                    // Capture status code
                    activity.SetTag("http.response.status_code", (int)response.StatusCode);
                    activity.SetTag("http.response.status_text", response.ReasonPhrase ?? string.Empty);

                    // Capture response headers
                    foreach (var header in response.Headers)
                    {
                        activity.SetTag($"http.response.header.{header.Key}", string.Join(",", header.Value));
                    }

                    // Capture Content-Type
                    var contentType = response.Content?.Headers?.ContentType?.MediaType;
                    if (!string.IsNullOrEmpty(contentType))
                    {
                        activity.SetTag("http.response.content_type", contentType);
                    }

                    // Capture response body
                    var responseBody = SafeReadContent(response.Content, MaxBodyCaptureBytes);
                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        activity.SetTag("http.response.body", responseBody);
                        activity.SetTag("http.response.body_length", responseBody.Length);
                    }
                };
            }) // Capture HTTP calls to OpenAI
            .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint))
            .AddAzureMonitorTraceExporter(options => options.ConnectionString = appInsightsConnectionString);


        return tracerProviderBuilder.Build();
    }

    private static string SafeReadContent(HttpContent? content, int maxBytes)
    {
        if (content == null) return string.Empty;
        try
        {
            var contentString = content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (string.IsNullOrWhiteSpace(contentString)) return string.Empty;
            if (contentString.Length > maxBytes)
            {
                return contentString.Substring(0, maxBytes) + "...(truncated)";
            }
            return contentString;
        }
        catch (Exception ex)
        {
            return $"(error reading content: {ex.Message})";
        }
    }

    public static ActivitySource CreateActivitySource()
    {
        return new ActivitySource(SourceName);
    }

    public static string GetServiceName() => ServiceName;
}
