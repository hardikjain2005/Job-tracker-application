using System.Text.Json.Serialization;

namespace JobTracker.Api.Models;

// The attribute makes both JSON and the OpenAPI/Swagger schema use names ("Interview") instead of numbers.
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ApplicationStatus
{
    Applied,
    Interview,
    Offer,
    Rejected
}
