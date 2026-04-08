using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SDP.Application.Dtos;
using SDP.Infrastructure.Persistence;

namespace SDP.IntegrationTests;

public sealed class ApiLifecycleTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public ApiLifecycleTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ProjectCrud_ShouldCreateAndFetchProject()
    {
        using var client = _factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/projects", new UpsertProjectRequest("Test", "Demo"));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var project = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();
        project.Should().NotBeNull();

        var getResponse = await client.GetAsync($"/api/projects/{project!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SimulationEndpoint_ShouldReturnResults()
    {
        using var client = _factory.CreateClient();

        var projects = await client.GetFromJsonAsync<List<ProjectDto>>("/api/projects");
        projects.Should().NotBeNullOrEmpty();
        var projectId = projects![0].Id;

        var services = await client.GetFromJsonAsync<List<ServiceNodeDto>>($"/api/projects/{projectId}/services");
        services.Should().NotBeNullOrEmpty();

        var scenarioRequest = new UpsertTrafficScenarioRequest("Burst", "integration test", services![0].Id, 120);
        var createScenarioResponse = await client.PostAsJsonAsync($"/api/projects/{projectId}/scenarios", scenarioRequest);
        createScenarioResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var scenario = await createScenarioResponse.Content.ReadFromJsonAsync<TrafficScenarioDto>();
        scenario.Should().NotBeNull();

        var simResponse = await client.PostAsync($"/api/scenarios/{scenario!.Id}/simulate", null);
        simResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await simResponse.Content.ReadFromJsonAsync<SimulationResultDto>();
        result.Should().NotBeNull();
        result!.ServiceResults.Should().NotBeEmpty();
    }
}

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<SdpDbContext>>();

            services.AddDbContext<SdpDbContext>(options =>
            {
                options.UseInMemoryDatabase("sdp-tests");
            });
        });
    }
}
