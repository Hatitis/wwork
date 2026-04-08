using SDP.Application.Dtos;

namespace SDP.Api.Extensions;

public static class ValidationExtensions
{
    public static IDictionary<string, string[]> Validate(this UpsertProjectRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["name"] = ["Name is required."];
        }

        return errors;
    }

    public static IDictionary<string, string[]> Validate(this UpsertServiceNodeRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["name"] = ["Name is required."];
        }

        if (request.CapacityRps <= 0)
        {
            errors["capacityRps"] = ["Capacity must be greater than 0."];
        }

        if (request.BaseLatencyMs < 0)
        {
            errors["baseLatencyMs"] = ["Base latency cannot be negative."];
        }

        if (request.ErrorRatePct is < 0 or > 100)
        {
            errors["errorRatePct"] = ["Error rate must be between 0 and 100."];
        }

        return errors;
    }

    public static IDictionary<string, string[]> Validate(this UpsertServiceLinkRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.FromServiceId == request.ToServiceId)
        {
            errors["toServiceId"] = ["Self links are not allowed."];
        }

        if (request.LinkLatencyMs < 0)
        {
            errors["linkLatencyMs"] = ["Link latency cannot be negative."];
        }

        return errors;
    }

    public static IDictionary<string, string[]> Validate(this UpsertTrafficScenarioRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["name"] = ["Name is required."];
        }

        if (request.EntryServiceId == Guid.Empty)
        {
            errors["entryServiceId"] = ["Entry service is required."];
        }

        if (request.IncomingRps <= 0)
        {
            errors["incomingRps"] = ["Incoming RPS must be greater than 0."];
        }

        return errors;
    }
}
