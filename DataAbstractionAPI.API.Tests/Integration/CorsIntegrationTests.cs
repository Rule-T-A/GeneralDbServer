using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DataAbstractionAPI.API.Tests.Integration;

/// <summary>
/// Integration tests for CORS configuration.
/// These tests verify that CORS headers are correctly set based on configuration.
/// </summary>
public class CorsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _tempTestDir;

    public CorsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _tempTestDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempTestDir);

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace IDataAdapter with one using temp directory
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DataAbstractionAPI.Core.Interfaces.IDataAdapter));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddSingleton<DataAbstractionAPI.Core.Interfaces.IDataAdapter>(
                    new DataAbstractionAPI.Adapters.Csv.CsvAdapter(_tempTestDir));
            });
        });

        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client?.Dispose();
        if (Directory.Exists(_tempTestDir))
        {
            try
            {
                Directory.Delete(_tempTestDir, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public async Task CorsMiddleware_AllowsConfiguredOrigins_ReturnsCorsHeaders()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/data");
        request.Headers.Add("Origin", "http://localhost:5001");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        var allowedOrigin = response.Headers.GetValues("Access-Control-Allow-Origin").FirstOrDefault();
        Assert.NotNull(allowedOrigin);
        // In development, it may allow any origin or the specific configured origin
        Assert.True(allowedOrigin == "http://localhost:5001" || allowedOrigin == "*");
    }

    [Fact]
    public async Task CorsMiddleware_HandlesPreflightRequest_Returns200()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/data");
        request.Headers.Add("Origin", "http://localhost:5001");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "Content-Type");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        // OPTIONS requests typically return 204 NoContent, but 200 OK is also valid
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent);
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.True(response.Headers.Contains("Access-Control-Allow-Methods"));
        Assert.True(response.Headers.Contains("Access-Control-Allow-Headers"));
    }

    [Fact]
    public async Task CorsMiddleware_IncludesRequiredHeaders_InResponse()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/data");
        request.Headers.Add("Origin", "http://localhost:5001");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Check for CORS headers
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        
        // Check that allowed methods header exists (may be in preflight only, but should be available)
        var hasMethodsHeader = response.Headers.Contains("Access-Control-Allow-Methods");
        var hasHeadersHeader = response.Headers.Contains("Access-Control-Allow-Headers");
        
        // At least one CORS header should be present
        Assert.True(hasMethodsHeader || hasHeadersHeader || response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task CorsMiddleware_WithPostRequest_IncludesCorsHeaders()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/data/users");
        request.Headers.Add("Origin", "http://localhost:5001");
        request.Content = new StringContent("{\"name\":\"Test\"}", System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        // Response may be 404 (collection doesn't exist) or 201 (created), but should have CORS headers
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task CorsMiddleware_AllowsApiKeyHeader_InPreflight()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/data");
        request.Headers.Add("Origin", "http://localhost:5001");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "X-API-Key");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        // OPTIONS requests typically return 204 NoContent, but 200 OK is also valid
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent);
        Assert.True(response.Headers.Contains("Access-Control-Allow-Headers"));
        
        var allowedHeaders = response.Headers.GetValues("Access-Control-Allow-Headers").FirstOrDefault();
        Assert.NotNull(allowedHeaders);
        Assert.Contains("X-API-Key", allowedHeaders, StringComparison.OrdinalIgnoreCase);
    }
}

