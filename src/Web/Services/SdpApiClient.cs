using System.Net.Http.Json;
using SDP.Application.Dtos;

namespace SDP.Web.Services;

public sealed class SdpApiClient
{
    private readonly HttpClient _httpClient;

    public SdpApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ProjectDto>> GetProjectsAsync(CancellationToken cancellationToken)
        => await _httpClient.GetFromJsonAsync<IReadOnlyList<ProjectDto>>("/api/projects", cancellationToken) ?? [];

    public async Task<ProjectDto> CreateProjectAsync(UpsertProjectRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/projects", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProjectDto>(cancellationToken))!;
    }

    public async Task<IReadOnlyList<ServiceNodeDto>> GetServicesAsync(Guid projectId, CancellationToken cancellationToken)
        => await _httpClient.GetFromJsonAsync<IReadOnlyList<ServiceNodeDto>>($"/api/projects/{projectId}/services", cancellationToken) ?? [];

    public async Task<ServiceNodeDto> CreateServiceAsync(Guid projectId, UpsertServiceNodeRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync($"/api/projects/{projectId}/services", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ServiceNodeDto>(cancellationToken))!;
    }

    public async Task<ServiceNodeDto> UpdateServiceAsync(Guid projectId, Guid serviceId, UpsertServiceNodeRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/projects/{projectId}/services/{serviceId}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ServiceNodeDto>(cancellationToken))!;
    }

    public async Task<IReadOnlyList<ServiceLinkDto>> GetLinksAsync(Guid projectId, CancellationToken cancellationToken)
        => await _httpClient.GetFromJsonAsync<IReadOnlyList<ServiceLinkDto>>($"/api/projects/{projectId}/links", cancellationToken) ?? [];

    public async Task<ServiceLinkDto> CreateLinkAsync(Guid projectId, UpsertServiceLinkRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync($"/api/projects/{projectId}/links", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ServiceLinkDto>(cancellationToken))!;
    }

    public async Task<IReadOnlyList<TrafficScenarioDto>> GetScenariosAsync(Guid projectId, CancellationToken cancellationToken)
        => await _httpClient.GetFromJsonAsync<IReadOnlyList<TrafficScenarioDto>>($"/api/projects/{projectId}/scenarios", cancellationToken) ?? [];

    public async Task<TrafficScenarioDto> CreateScenarioAsync(Guid projectId, UpsertTrafficScenarioRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync($"/api/projects/{projectId}/scenarios", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TrafficScenarioDto>(cancellationToken))!;
    }

    public async Task<SimulationResultDto> SimulateAsync(Guid scenarioId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsync($"/api/scenarios/{scenarioId}/simulate", null, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SimulationResultDto>(cancellationToken))!;
    }
}
