using System.Net;
using System.Text.Json;
using DataAbstractionAPI.API.Models.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DataAbstractionAPI.API.Tests.Integration;

/// <summary>
/// Integration tests for the discovery endpoint (GET /api/data/help).
/// </summary>
public class DiscoveryEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DiscoveryEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DiscoveryEndpoint_ReturnsValidJson_WithCorrectStructure()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/data/help");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var discovery = JsonSerializer.Deserialize<DiscoveryResponseDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(discovery);
        Assert.Equal("v1", discovery.ApiVersion);
        Assert.NotEmpty(discovery.BaseUrl);
        Assert.NotNull(discovery.Authentication);
        Assert.NotEmpty(discovery.QuickStart);
    }

    [Fact]
    public async Task DiscoveryEndpoint_IncludesBaseUrl_FromRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/data/help");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        var discovery = JsonSerializer.Deserialize<DiscoveryResponseDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(discovery);
        Assert.Contains("api", discovery.BaseUrl);
    }

    [Fact]
    public async Task DiscoveryEndpoint_InDevelopment_IncludesOpenApiLinks()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
        });
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/data/help");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        var discovery = JsonSerializer.Deserialize<DiscoveryResponseDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(discovery);
        Assert.True(discovery.OpenApiAvailable);
        Assert.NotNull(discovery.OpenApiSpec);
        Assert.NotNull(discovery.SwaggerUi);
        Assert.Contains("swagger", discovery.OpenApiSpec);
        Assert.Contains("swagger", discovery.SwaggerUi);
    }

    [Fact]
    public async Task DiscoveryEndpoint_InProduction_ExcludesOpenApiLinks()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
        });
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/data/help");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        var discovery = JsonSerializer.Deserialize<DiscoveryResponseDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(discovery);
        Assert.False(discovery.OpenApiAvailable);
        Assert.Null(discovery.OpenApiSpec);
        Assert.Null(discovery.SwaggerUi);
    }

    [Fact]
    public async Task DiscoveryEndpoint_ReflectsApiKeyConfiguration_WhenEnabled()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "ApiKeyAuthentication:Enabled", "true" },
                    { "ApiKeyAuthentication:ValidApiKeys:0", "test-key-123" },
                    { "ApiKeyAuthentication:HeaderName", "X-API-Key" }
                });
            });
        });
        var client = factory.CreateClient();
        
        // Add API key header to allow access
        client.DefaultRequestHeaders.Add("X-API-Key", "test-key-123");

        // Act
        var response = await client.GetAsync("/api/data/help");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        var discovery = JsonSerializer.Deserialize<DiscoveryResponseDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(discovery);
        Assert.NotNull(discovery.Authentication);
        Assert.True(discovery.Authentication.Required);
        Assert.Equal("X-API-Key", discovery.Authentication.Header);
        Assert.Equal("api_key", discovery.Authentication.Type);
        Assert.Contains("required", discovery.Authentication.Description?.ToLower() ?? "");
    }

    [Fact]
    public async Task DiscoveryEndpoint_ReflectsApiKeyConfiguration_WhenDisabled()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "ApiKeyAuthentication:Enabled", "false" }
                });
            });
        });
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/data/help");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        var discovery = JsonSerializer.Deserialize<DiscoveryResponseDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(discovery);
        Assert.NotNull(discovery.Authentication);
        Assert.False(discovery.Authentication.Required);
        Assert.Equal("api_key", discovery.Authentication.Type);
    }

    [Fact]
    public async Task DiscoveryEndpoint_IncludesAllQuickStartEndpoints()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/data/help");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        var discovery = JsonSerializer.Deserialize<DiscoveryResponseDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(discovery);
        Assert.NotEmpty(discovery.QuickStart);
        
        var quickStartText = string.Join(" ", discovery.QuickStart);
        Assert.Contains("GET /api/data", quickStartText);
        Assert.Contains("POST /api/data", quickStartText);
        Assert.Contains("PUT /api/data", quickStartText);
        Assert.Contains("DELETE /api/data", quickStartText);
        Assert.Contains("summary", quickStartText);
        Assert.Contains("bulk", quickStartText);
        Assert.Contains("aggregate", quickStartText);
    }

    [Fact]
    public async Task DiscoveryEndpoint_Returns200StatusCode()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/data/help");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task DiscoveryEndpoint_IncludesEndpointsStructure()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/data/help");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        var discovery = JsonSerializer.Deserialize<DiscoveryResponseDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(discovery);
        Assert.NotNull(discovery.Endpoints);
        Assert.NotNull(discovery.Endpoints.Data);
        Assert.NotNull(discovery.Endpoints.Collections);
        Assert.NotNull(discovery.Endpoints.Schema);
        Assert.NotNull(discovery.Endpoints.Upload);
        
        Assert.Equal("GET /api/data/{collection}", discovery.Endpoints.Data.List);
        Assert.Equal("GET /api/data/{collection}/{id}", discovery.Endpoints.Data.Get);
        Assert.Equal("POST /api/data/{collection}", discovery.Endpoints.Data.Create);
        Assert.Equal("PUT /api/data/{collection}/{id}", discovery.Endpoints.Data.Update);
        Assert.Equal("DELETE /api/data/{collection}/{id}", discovery.Endpoints.Data.Delete);
        Assert.NotNull(discovery.Endpoints.Data.Summary);
        Assert.NotNull(discovery.Endpoints.Data.Bulk);
        Assert.NotNull(discovery.Endpoints.Data.Aggregate);
    }
}

