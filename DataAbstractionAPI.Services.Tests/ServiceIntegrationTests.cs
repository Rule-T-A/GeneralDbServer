namespace DataAbstractionAPI.Services.Tests;

using DataAbstractionAPI.Core.Interfaces;
using DataAbstractionAPI.Core.Models;
using DataAbstractionAPI.Core.Enums;
using DataAbstractionAPI.Core.Exceptions;
using DataAbstractionAPI.Services;
using DataAbstractionAPI.Adapters.Csv;

/// <summary>
/// Integration tests for service composition - testing all services working together with CsvAdapter.
/// </summary>
public class ServiceIntegrationTests : IDisposable
{
    private readonly string _tempTestDir;
    private readonly IDefaultGenerator _defaultGenerator;
    private readonly ITypeConverter _typeConverter;
    private readonly IFilterEvaluator _filterEvaluator;
    private readonly IValidationService _validationService;
    private readonly CsvAdapter _adapterWithAllServices;
    private readonly CsvAdapter _adapterWithoutServices;

    public ServiceIntegrationTests()
    {
        _tempTestDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempTestDir);

        // Initialize all services
        _defaultGenerator = new DefaultGenerator();
        _typeConverter = new TypeConverter();
        _filterEvaluator = new FilterEvaluator();
        _validationService = new ValidationService();

        // Create adapter with all services
        _adapterWithAllServices = new CsvAdapter(
            _tempTestDir,
            defaultGenerator: _defaultGenerator,
            typeConverter: _typeConverter,
            filterEvaluator: _filterEvaluator
        );

        // Create adapter without services (backward compatibility)
        _adapterWithoutServices = new CsvAdapter(_tempTestDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempTestDir))
        {
            Directory.Delete(_tempTestDir, true);
        }
    }

    #region All Services Integration

    [Fact]
    public async Task CsvAdapter_WithAllServices_WorksCorrectly()
    {
        // Arrange - Create test CSV
        var csvPath = Path.Combine(_tempTestDir, "users.csv");
        File.WriteAllText(csvPath, "id,name,age,status\n1,Alice,25,active\n2,Bob,30,inactive\n3,Charlie,20,active\n");

        // Act - Use FilterEvaluator for complex filtering
        var options = new QueryOptions
        {
            Filter = new Dictionary<string, object>
            {
                { "field", "age" },
                { "operator", "gte" },
                { "value", 25 }
            },
            Limit = 10
        };

        var result = await _adapterWithAllServices.ListAsync("users", options);

        // Assert
        Assert.Equal(2, result.Data.Count);
        Assert.Contains(result.Data, r => r.Id == "1"); // Alice
        Assert.Contains(result.Data, r => r.Id == "2"); // Bob
    }

    [Fact]
    public async Task CsvAdapter_WithAllServices_BackwardCompatible_WithoutServices()
    {
        // Arrange - Create test CSV
        var csvPath = Path.Combine(_tempTestDir, "products.csv");
        File.WriteAllText(csvPath, "id,name,price\n1,Widget,10.50\n2,Gadget,20.00\n");

        // Act - Use adapter without services (should still work)
        var options = new QueryOptions
        {
            Filter = new Dictionary<string, object> { { "name", "Widget" } }, // Simple filter
            Limit = 10
        };

        var result = await _adapterWithoutServices.ListAsync("products", options);

        // Assert - Should work with simple filters
        Assert.Single(result.Data);
        Assert.Equal("1", result.Data[0].Id);
    }

    #endregion

    #region FilterEvaluator Integration

    [Fact]
    public async Task CsvAdapter_WithFilterEvaluator_SupportsOperatorBasedFilters()
    {
        // Arrange
        var csvPath = Path.Combine(_tempTestDir, "orders.csv");
        File.WriteAllText(csvPath, "id,amount,status\n1,100.50,completed\n2,50.25,pending\n3,200.00,completed\n");

        var options = new QueryOptions
        {
            Filter = new Dictionary<string, object>
            {
                { "field", "amount" },
                { "operator", "gt" },
                { "value", 75.0 }
            },
            Limit = 10
        };

        // Act
        var result = await _adapterWithAllServices.ListAsync("orders", options);

        // Assert
        Assert.Equal(2, result.Data.Count); // Should return orders with amount > 75
        Assert.Contains(result.Data, r => r.Id == "1");
        Assert.Contains(result.Data, r => r.Id == "3");
    }

    [Fact]
    public async Task CsvAdapter_WithFilterEvaluator_SupportsCompoundFilters()
    {
        // Arrange
        var csvPath = Path.Combine(_tempTestDir, "users.csv");
        File.WriteAllText(csvPath, "id,name,age,status\n1,Alice,25,active\n2,Bob,30,inactive\n3,Charlie,20,active\n");

        var options = new QueryOptions
        {
            Filter = new Dictionary<string, object>
            {
                { "and", new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "status", "active" } },
                        new Dictionary<string, object>
                        {
                            { "field", "age" },
                            { "operator", "gte" },
                            { "value", 20 }
                        }
                    }
                }
            },
            Limit = 10
        };

        // Act
        var result = await _adapterWithAllServices.ListAsync("users", options);

        // Assert
        Assert.Equal(2, result.Data.Count); // Alice and Charlie (both active and age >= 20)
        Assert.Contains(result.Data, r => r.Id == "1");
        Assert.Contains(result.Data, r => r.Id == "3");
    }

    #endregion

    #region ValidationService Integration (Manual Testing)

    [Fact]
    public void ValidationService_WithTypeConverter_ValidatesAndConvertsTypes()
    {
        // Arrange
        var schema = new CollectionSchema
        {
            Name = "users",
            Fields = new List<FieldDefinition>
            {
                new FieldDefinition { Name = "name", Type = FieldType.String, Nullable = false },
                new FieldDefinition { Name = "age", Type = FieldType.Integer, Nullable = false }
            }
        };

        // Record with string age that should be valid (type coercion)
        var record = new Dictionary<string, object>
        {
            { "name", "Alice" },
            { "age", "25" } // String that can be coerced to int
        };

        // Act & Assert - Should pass validation (type coercion allowed)
        _validationService.Validate(record, schema);
    }

    [Fact]
    public void ValidationService_WithTypeConverter_RejectsInvalidTypes()
    {
        // Arrange
        var schema = new CollectionSchema
        {
            Name = "users",
            Fields = new List<FieldDefinition>
            {
                new FieldDefinition { Name = "age", Type = FieldType.Integer, Nullable = false }
            }
        };

        var record = new Dictionary<string, object>
        {
            { "age", "not-a-number" } // Cannot be coerced to int
        };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _validationService.Validate(record, schema));
        Assert.Equal("age", exception.FieldName);
    }

    #endregion

    #region Service Interaction Tests

    [Fact]
    public void DefaultGenerator_And_TypeConverter_WorkTogether()
    {
        // Arrange
        var context = new DefaultGenerationContext();
        var fieldName = "is_active";
        var fieldType = FieldType.Boolean;

        // Act - Generate default
        var defaultValue = _defaultGenerator.GenerateDefault(fieldName, fieldType, context);

        // Convert to string and back
        var asString = _typeConverter.Convert(defaultValue, FieldType.Boolean, FieldType.String, ConversionStrategy.Cast);
        var backToBool = _typeConverter.Convert(asString, FieldType.String, FieldType.Boolean, ConversionStrategy.Cast);

        // Assert
        Assert.False((bool)defaultValue);
        Assert.Equal("False", asString);
        Assert.False((bool)backToBool);
    }

    [Fact]
    public void FilterEvaluator_And_TypeConverter_WorkTogether()
    {
        // Arrange
        var record = new Record
        {
            Id = "1",
            Data = new Dictionary<string, object> { { "age", "25" } } // String value
        };

        // Filter expects numeric comparison
        var filter = new Dictionary<string, object>
        {
            { "field", "age" },
            { "operator", "gt" },
            { "value", 20 } // Integer
        };

        // Act
        var result = _filterEvaluator.Evaluate(record, filter);

        // Assert - FilterEvaluator should handle type coercion
        Assert.True(result);
    }

    #endregion

    #region End-to-End Scenarios

    [Fact]
    public async Task EndToEnd_CreateAndQuery_WithAllServices()
    {
        // Arrange - Create empty collection
        var csvPath = Path.Combine(_tempTestDir, "products.csv");
        File.WriteAllText(csvPath, "id,name,price,is_active\n");

        // Act 1 - Create record
        var newRecord = new Dictionary<string, object>
        {
            { "name", "Test Product" },
            { "price", "19.99" },
            { "is_active", "true" }
        };

        var createResult = await _adapterWithAllServices.CreateAsync("products", newRecord);

        // Act 2 - Query with filter
        var queryOptions = new QueryOptions
        {
            Filter = new Dictionary<string, object>
            {
                { "field", "price" },
                { "operator", "gt" },
                { "value", 10.0 }
            },
            Limit = 10
        };

        var queryResult = await _adapterWithAllServices.ListAsync("products", queryOptions);

        // Assert
        Assert.NotNull(createResult.Id);
        Assert.Single(queryResult.Data);
        Assert.Equal(createResult.Id, queryResult.Data[0].Id);
    }

    [Fact]
    public async Task EndToEnd_ComplexFiltering_WithMultipleOperators()
    {
        // Arrange
        var csvPath = Path.Combine(_tempTestDir, "orders.csv");
        File.WriteAllText(csvPath, "id,amount,status,customer\n1,100.50,completed,Alice\n2,50.25,pending,Bob\n3,200.00,completed,Charlie\n4,75.00,pending,Alice\n");

        // Complex filter: (amount > 75 AND status = completed) OR customer = Alice
        var options = new QueryOptions
        {
            Filter = new Dictionary<string, object>
            {
                { "or", new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            { "and", new List<Dictionary<string, object>>
                                {
                                    new Dictionary<string, object>
                                    {
                                        { "field", "amount" },
                                        { "operator", "gt" },
                                        { "value", 75.0 }
                                    },
                                    new Dictionary<string, object> { { "status", "completed" } }
                                }
                            }
                        },
                        new Dictionary<string, object> { { "customer", "Alice" } }
                    }
                }
            },
            Limit = 10
        };

        // Act
        var result = await _adapterWithAllServices.ListAsync("orders", options);

        // Assert
        // Should return: order 1 (amount > 75 AND completed), order 3 (amount > 75 AND completed), order 4 (customer = Alice)
        Assert.Equal(3, result.Data.Count); // Multiple items, so Assert.Equal is appropriate
        Assert.Contains(result.Data, r => r.Id == "1");
        Assert.Contains(result.Data, r => r.Id == "3");
        Assert.Contains(result.Data, r => r.Id == "4");
    }

    #endregion

    #region Backward Compatibility Tests

    [Fact]
    public async Task CsvAdapter_WithoutServices_StillWorks_WithSimpleFilters()
    {
        // Arrange
        var csvPath = Path.Combine(_tempTestDir, "users.csv");
        File.WriteAllText(csvPath, "id,name,status\n1,Alice,active\n2,Bob,inactive\n3,Charlie,active\n");

        var options = new QueryOptions
        {
            Filter = new Dictionary<string, object> { { "status", "active" } },
            Limit = 10
        };

        // Act
        var result = await _adapterWithoutServices.ListAsync("users", options);

        // Assert - Should work with simple equality filters
        Assert.Equal(2, result.Data.Count);
        Assert.Contains(result.Data, r => r.Id == "1");
        Assert.Contains(result.Data, r => r.Id == "3");
    }

    [Fact]
    public async Task CsvAdapter_WithoutServices_DoesNotSupport_OperatorFilters()
    {
        // Arrange
        var csvPath = Path.Combine(_tempTestDir, "users.csv");
        File.WriteAllText(csvPath, "id,name,age\n1,Alice,25\n2,Bob,30\n");

        var options = new QueryOptions
        {
            Filter = new Dictionary<string, object>
            {
                { "field", "age" },
                { "operator", "gt" },
                { "value", 20 }
            },
            Limit = 10
        };

        // Act
        var result = await _adapterWithoutServices.ListAsync("users", options);

        // Assert - Without FilterEvaluator, operator filters won't work
        // The simple filter logic will treat this as a simple filter and won't match
        // This is expected behavior - operator filters require FilterEvaluator
        Assert.Empty(result.Data);
    }

    #endregion
}

