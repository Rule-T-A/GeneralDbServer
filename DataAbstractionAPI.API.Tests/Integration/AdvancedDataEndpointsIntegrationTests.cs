using System.Net;
using System.Text.Json;
using DataAbstractionAPI.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DataAbstractionAPI.API.Tests.Integration;

/// <summary>
/// Integration tests for advanced data endpoints (Bulk, Summary, Aggregate).
/// </summary>
public class AdvancedDataEndpointsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _tempTestDir;

    public AdvancedDataEndpointsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _tempTestDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempTestDir);

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
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
        
        // Create test collection with sample data
        var csvPath = Path.Combine(_tempTestDir, "products.csv");
        File.WriteAllText(csvPath, "id,name,category,status,price,quantity\n" +
            "1,Product A,Electronics,active,100.50,10\n" +
            "2,Product B,Electronics,active,200.00,5\n" +
            "3,Product C,Books,inactive,15.99,20\n" +
            "4,Product D,Books,active,25.00,15\n" +
            "5,Product E,Electronics,pending,150.00,8");
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

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkOperation_Create_BestEffort_ReturnsSuccess()
    {
        // Arrange
        var request = new BulkOperationRequestDto
        {
            Action = "create",
            Atomic = false,
            Records = new List<Dictionary<string, object>>
            {
                new() { { "name", "New Product 1" }, { "category", "Electronics" }, { "status", "active" } },
                new() { { "name", "New Product 2" }, { "category", "Books" }, { "status", "active" } }
            }
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/data/products/bulk", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<BulkResponseDto>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(2, result.Succeeded);
        Assert.Equal(0, result.Failed);
        Assert.NotNull(result.Results);
        Assert.Equal(2, result.Results.Count);
        Assert.All(result.Results, r => Assert.True(r.Success));
    }

    [Fact]
    public async Task BulkOperation_Create_Atomic_ReturnsCreated()
    {
        // Arrange
        var request = new BulkOperationRequestDto
        {
            Action = "create",
            Atomic = true,
            Records = new List<Dictionary<string, object>>
            {
                new() { { "name", "Atomic Product 1" }, { "category", "Electronics" } },
                new() { { "name", "Atomic Product 2" }, { "category", "Books" } }
            }
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/data/products/bulk", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<BulkResponseDto>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(2, result.Succeeded);
        Assert.NotNull(result.Ids);
        Assert.Equal(2, result.Ids.Count);
    }

    [Fact]
    public async Task BulkOperation_Update_BestEffort_ReturnsSuccess()
    {
        // Arrange - Get existing record ID
        var listResponse = await _client.GetAsync("/api/data/products?limit=1");
        var listJson = await listResponse.Content.ReadAsStringAsync();
        var listDoc = JsonDocument.Parse(listJson);
        var firstRecord = listDoc.RootElement.GetProperty("d")[0];
        var recordId = firstRecord.GetProperty("id").GetString();

        var request = new BulkOperationRequestDto
        {
            Action = "update",
            Atomic = false,
            Records = new List<Dictionary<string, object>>
            {
                new() { { "id", recordId ?? "" }, { "status", "updated" } }
            }
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/data/products/bulk", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<BulkResponseDto>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(1, result.Succeeded);
    }

    [Fact]
    public async Task BulkOperation_Delete_BestEffort_ReturnsSuccess()
    {
        // Arrange - Get existing record ID
        var listResponse = await _client.GetAsync("/api/data/products?limit=1");
        var listJson = await listResponse.Content.ReadAsStringAsync();
        var listDoc = JsonDocument.Parse(listJson);
        var firstRecord = listDoc.RootElement.GetProperty("d")[0];
        var recordId = firstRecord.GetProperty("id").GetString();

        var request = new BulkOperationRequestDto
        {
            Action = "delete",
            Atomic = false,
            Records = new List<Dictionary<string, object>>
            {
                new() { { "id", recordId ?? "" } }
            }
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/data/products/bulk", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<BulkResponseDto>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(1, result.Succeeded);
    }

    [Fact]
    public async Task BulkOperation_InvalidAction_ReturnsBadRequest()
    {
        // Arrange
        var request = new BulkOperationRequestDto
        {
            Action = "invalid",
            Atomic = false,
            Records = new List<Dictionary<string, object>>()
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/data/products/bulk", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task BulkOperation_EmptyRecords_ReturnsBadRequest()
    {
        // Arrange
        var request = new BulkOperationRequestDto
        {
            Action = "create",
            Atomic = false,
            Records = new List<Dictionary<string, object>>()
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/data/products/bulk", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Summary Tests

    [Fact]
    public async Task GetSummary_ValidField_ReturnsCounts()
    {
        // Act
        var response = await _client.GetAsync("/api/data/products/summary?field=status");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        // Should have counts for each status value
        Assert.True(root.TryGetProperty("active", out var active));
        Assert.True(root.TryGetProperty("inactive", out var inactive));
        Assert.True(root.TryGetProperty("pending", out var pending));
        
        Assert.True(active.GetInt32() > 0);
        Assert.True(inactive.GetInt32() > 0);
        Assert.True(pending.GetInt32() > 0);
    }

    [Fact]
    public async Task GetSummary_CategoryField_ReturnsCounts()
    {
        // Act
        var response = await _client.GetAsync("/api/data/products/summary?field=category");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        // Should have counts for each category
        Assert.True(root.TryGetProperty("Electronics", out var electronics));
        Assert.True(root.TryGetProperty("Books", out var books));
        
        Assert.True(electronics.GetInt32() > 0);
        Assert.True(books.GetInt32() > 0);
    }

    [Fact]
    public async Task GetSummary_MissingField_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/data/products/summary");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetSummary_InvalidCollection_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/data/nonexistent/summary?field=status");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Aggregate Tests

    [Fact]
    public async Task Aggregate_CountByCategory_ReturnsGroupedResults()
    {
        // Arrange
        var request = new AggregateRequestDto
        {
            GroupBy = new[] { "category" },
            Aggregates = new List<AggregateFunctionDto>
            {
                new() { Field = "id", Function = "count", Alias = "count" }
            }
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/data/products/aggregate", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseJson = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(responseJson);
        var data = document.RootElement.GetProperty("d");

        Assert.True(data.GetArrayLength() > 0);
        
        // Should have results for each category
        var categories = new HashSet<string>();
        foreach (var item in data.EnumerateArray())
        {
            Assert.True(item.TryGetProperty("category", out var category));
            Assert.True(item.TryGetProperty("count", out var count));
            categories.Add(category.GetString() ?? "");
            Assert.True(count.GetInt32() > 0);
        }
        
        Assert.Contains("Electronics", categories);
        Assert.Contains("Books", categories);
    }

    [Fact]
    public async Task Aggregate_SumPriceByCategory_ReturnsAggregatedResults()
    {
        // Arrange
        var request = new AggregateRequestDto
        {
            GroupBy = new[] { "category" },
            Aggregates = new List<AggregateFunctionDto>
            {
                new() { Field = "price", Function = "sum", Alias = "total_price" },
                new() { Field = "quantity", Function = "sum", Alias = "total_quantity" }
            }
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/data/products/aggregate", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseJson = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(responseJson);
        var data = document.RootElement.GetProperty("d");

        Assert.True(data.GetArrayLength() > 0);
        
        foreach (var item in data.EnumerateArray())
        {
            Assert.True(item.TryGetProperty("category", out _));
            Assert.True(item.TryGetProperty("total_price", out var totalPrice));
            Assert.True(item.TryGetProperty("total_quantity", out var totalQuantity));
            Assert.True(totalPrice.GetDouble() > 0);
            Assert.True(totalQuantity.GetInt32() > 0);
        }
    }

    [Fact]
    public async Task Aggregate_AvgPriceByCategoryAndStatus_ReturnsMultiGroupedResults()
    {
        // Arrange
        var request = new AggregateRequestDto
        {
            GroupBy = new[] { "category", "status" },
            Aggregates = new List<AggregateFunctionDto>
            {
                new() { Field = "price", Function = "avg", Alias = "avg_price" },
                new() { Field = "id", Function = "count", Alias = "count" }
            }
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/data/products/aggregate", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseJson = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(responseJson);
        var data = document.RootElement.GetProperty("d");

        Assert.True(data.GetArrayLength() > 0);
        
        foreach (var item in data.EnumerateArray())
        {
            Assert.True(item.TryGetProperty("category", out _));
            Assert.True(item.TryGetProperty("status", out _));
            Assert.True(item.TryGetProperty("avg_price", out var avgPrice));
            Assert.True(item.TryGetProperty("count", out var count));
            Assert.True(avgPrice.GetDouble() > 0);
            Assert.True(count.GetInt32() > 0);
        }
    }

    [Fact]
    public async Task Aggregate_WithFilter_ReturnsFilteredResults()
    {
        // Arrange
        var request = new AggregateRequestDto
        {
            GroupBy = new[] { "category" },
            Filter = new Dictionary<string, object> { { "status", "active" } },
            Aggregates = new List<AggregateFunctionDto>
            {
                new() { Field = "id", Function = "count", Alias = "count" }
            }
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/data/products/aggregate", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseJson = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(responseJson);
        var data = document.RootElement.GetProperty("d");

        Assert.True(data.GetArrayLength() > 0);
        
        // All results should be for active status only
        foreach (var item in data.EnumerateArray())
        {
            Assert.True(item.TryGetProperty("count", out var count));
            Assert.True(count.GetInt32() > 0);
        }
    }

    [Fact]
    public async Task Aggregate_NoGroupBy_ReturnsSingleResult()
    {
        // Arrange
        var request = new AggregateRequestDto
        {
            Aggregates = new List<AggregateFunctionDto>
            {
                new() { Field = "price", Function = "sum", Alias = "total_price" },
                new() { Field = "id", Function = "count", Alias = "total_count" }
            }
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/data/products/aggregate", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseJson = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(responseJson);
        var data = document.RootElement.GetProperty("d");

        Assert.Equal(1, data.GetArrayLength());
        
        var result = data[0];
        Assert.True(result.TryGetProperty("total_price", out var totalPrice));
        Assert.True(result.TryGetProperty("total_count", out var totalCount));
        Assert.True(totalPrice.GetDouble() > 0);
        Assert.True(totalCount.GetInt32() > 0);
    }

    [Fact]
    public async Task Aggregate_MinMaxPrice_ReturnsCorrectValues()
    {
        // Arrange
        var request = new AggregateRequestDto
        {
            Aggregates = new List<AggregateFunctionDto>
            {
                new() { Field = "price", Function = "min", Alias = "min_price" },
                new() { Field = "price", Function = "max", Alias = "max_price" }
            }
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/data/products/aggregate", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseJson = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(responseJson);
        var data = document.RootElement.GetProperty("d");

        Assert.Equal(1, data.GetArrayLength());
        
        var result = data[0];
        Assert.True(result.TryGetProperty("min_price", out var minPrice));
        Assert.True(result.TryGetProperty("max_price", out var maxPrice));
        Assert.True(minPrice.GetDouble() > 0);
        Assert.True(maxPrice.GetDouble() > 0);
        Assert.True(maxPrice.GetDouble() >= minPrice.GetDouble());
    }

    [Fact]
    public async Task Aggregate_EmptyAggregates_ReturnsBadRequest()
    {
        // Arrange
        var request = new AggregateRequestDto
        {
            Aggregates = new List<AggregateFunctionDto>()
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/data/products/aggregate", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Aggregate_InvalidCollection_ReturnsNotFound()
    {
        // Arrange
        var request = new AggregateRequestDto
        {
            Aggregates = new List<AggregateFunctionDto>
            {
                new() { Field = "id", Function = "count", Alias = "count" }
            }
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/data/nonexistent/aggregate", content);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion
}

