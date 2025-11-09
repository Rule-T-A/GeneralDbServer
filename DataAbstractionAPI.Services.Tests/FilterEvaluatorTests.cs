namespace DataAbstractionAPI.Services.Tests;

using DataAbstractionAPI.Core.Interfaces;
using DataAbstractionAPI.Core.Models;
using DataAbstractionAPI.Services;
using DataAbstractionAPI.Adapters.Csv;

public class FilterEvaluatorTests
{
    private readonly IFilterEvaluator _evaluator;

    public FilterEvaluatorTests()
    {
        _evaluator = new FilterEvaluator();
    }

    private Record CreateRecord(string id, Dictionary<string, object> data)
    {
        return new Record { Id = id, Data = data };
    }

    #region Simple Filters

    [Fact]
    public void FilterEvaluator_SimpleFilter_Equals_ReturnsMatches()
    {
        // Arrange
        var records = new List<Record>
        {
            CreateRecord("1", new Dictionary<string, object> { { "status", "active" } }),
            CreateRecord("2", new Dictionary<string, object> { { "status", "inactive" } }),
            CreateRecord("3", new Dictionary<string, object> { { "status", "active" } })
        };
        var filter = new Dictionary<string, object> { { "status", "active" } };

        // Act & Assert
        Assert.True(_evaluator.Evaluate(records[0], filter));
        Assert.False(_evaluator.Evaluate(records[1], filter));
        Assert.True(_evaluator.Evaluate(records[2], filter));
    }

    [Fact]
    public void FilterEvaluator_SimpleFilter_WithMultipleFields_RequiresAllMatch()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> 
        { 
            { "status", "active" },
            { "age", 25 }
        });
        
        var matchingFilter = new Dictionary<string, object> 
        { 
            { "status", "active" },
            { "age", 25 }
        };
        
        var nonMatchingFilter = new Dictionary<string, object> 
        { 
            { "status", "active" },
            { "age", 30 }
        };

        // Act & Assert
        Assert.True(_evaluator.Evaluate(record, matchingFilter));
        Assert.False(_evaluator.Evaluate(record, nonMatchingFilter));
    }

    [Fact]
    public void FilterEvaluator_SimpleFilter_WithMissingField_ReturnsFalse()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "status", "active" } });
        var filter = new Dictionary<string, object> { { "nonexistent", "value" } };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void FilterEvaluator_SimpleFilter_WithNullValue_HandlesCorrectly()
    {
        // Arrange
        var recordData = new Dictionary<string, object> { { "field", null! } };
        var record = CreateRecord("1", recordData);
        var filter = new Dictionary<string, object> { { "field", null! } };

        // Act & Assert
        Assert.True(_evaluator.Evaluate(record, filter));
    }

    [Fact]
    public void FilterEvaluator_WithEmptyFilter_ReturnsTrue()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "status", "active" } });
        var filter = new Dictionary<string, object>();

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void FilterEvaluator_SimpleFilter_WithNumericValues_MatchesCorrectly()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "age", 25 } });
        var matchingFilter = new Dictionary<string, object> { { "age", 25 } };
        var nonMatchingFilter = new Dictionary<string, object> { { "age", 30 } };

        // Act & Assert
        Assert.True(_evaluator.Evaluate(record, matchingFilter));
        Assert.False(_evaluator.Evaluate(record, nonMatchingFilter));
    }

    #endregion

    #region Operator-Based Filters - Comparison Operators

    [Fact]
    public void FilterEvaluator_OperatorFilter_GreaterThan_Works()
    {
        // Arrange
        var records = new List<Record>
        {
            CreateRecord("1", new Dictionary<string, object> { { "age", 15 } }),
            CreateRecord("2", new Dictionary<string, object> { { "age", 20 } }),
            CreateRecord("3", new Dictionary<string, object> { { "age", 25 } }),
            CreateRecord("4", new Dictionary<string, object> { { "age", 30 } })
        };
        var filter = new Dictionary<string, object>
        {
            { "field", "age" },
            { "operator", "gt" },
            { "value", 18 }
        };

        // Act & Assert
        Assert.False(_evaluator.Evaluate(records[0], filter)); // 15 <= 18
        Assert.True(_evaluator.Evaluate(records[1], filter));  // 20 > 18
        Assert.True(_evaluator.Evaluate(records[2], filter));  // 25 > 18
        Assert.True(_evaluator.Evaluate(records[3], filter));  // 30 > 18
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_GreaterThanOrEqual_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "age", 18 } });
        var filter = new Dictionary<string, object>
        {
            { "field", "age" },
            { "operator", "gte" },
            { "value", 18 }
        };

        // Act & Assert
        Assert.True(_evaluator.Evaluate(record, filter));
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_LessThan_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "age", 15 } });
        var filter = new Dictionary<string, object>
        {
            { "field", "age" },
            { "operator", "lt" },
            { "value", 18 }
        };

        // Act & Assert
        Assert.True(_evaluator.Evaluate(record, filter));
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_LessThanOrEqual_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "age", 18 } });
        var filter = new Dictionary<string, object>
        {
            { "field", "age" },
            { "operator", "lte" },
            { "value", 18 }
        };

        // Act & Assert
        Assert.True(_evaluator.Evaluate(record, filter));
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_Equal_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "status", "active" } });
        var filter = new Dictionary<string, object>
        {
            { "field", "status" },
            { "operator", "eq" },
            { "value", "active" }
        };

        // Act & Assert
        Assert.True(_evaluator.Evaluate(record, filter));
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_NotEqual_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "status", "active" } });
        var filter = new Dictionary<string, object>
        {
            { "field", "status" },
            { "operator", "ne" },
            { "value", "inactive" }
        };

        // Act & Assert
        Assert.True(_evaluator.Evaluate(record, filter));
    }

    #endregion

    #region Operator-Based Filters - Membership Operators

    [Fact]
    public void FilterEvaluator_OperatorFilter_In_Works()
    {
        // Arrange
        var records = new List<Record>
        {
            CreateRecord("1", new Dictionary<string, object> { { "status", "active" } }),
            CreateRecord("2", new Dictionary<string, object> { { "status", "pending" } }),
            CreateRecord("3", new Dictionary<string, object> { { "status", "inactive" } })
        };
        var filter = new Dictionary<string, object>
        {
            { "field", "status" },
            { "operator", "in" },
            { "value", new[] { "active", "pending" } }
        };

        // Act & Assert
        Assert.True(_evaluator.Evaluate(records[0], filter));
        Assert.True(_evaluator.Evaluate(records[1], filter));
        Assert.False(_evaluator.Evaluate(records[2], filter));
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_NotIn_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "status", "inactive" } });
        var filter = new Dictionary<string, object>
        {
            { "field", "status" },
            { "operator", "nin" },
            { "value", new[] { "active", "pending" } }
        };

        // Act & Assert
        Assert.True(_evaluator.Evaluate(record, filter));
    }

    #endregion

    #region Operator-Based Filters - String Operators

    [Fact]
    public void FilterEvaluator_OperatorFilter_Contains_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "name", "John Doe" } });
        var matchingFilter = new Dictionary<string, object>
        {
            { "field", "name" },
            { "operator", "contains" },
            { "value", "John" }
        };
        var nonMatchingFilter = new Dictionary<string, object>
        {
            { "field", "name" },
            { "operator", "contains" },
            { "value", "Jane" }
        };

        // Act & Assert
        Assert.True(_evaluator.Evaluate(record, matchingFilter));
        Assert.False(_evaluator.Evaluate(record, nonMatchingFilter));
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_StartsWith_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "email", "john@example.com" } });
        var filter = new Dictionary<string, object>
        {
            { "field", "email" },
            { "operator", "startswith" },
            { "value", "john" }
        };

        // Act & Assert
        Assert.True(_evaluator.Evaluate(record, filter));
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_EndsWith_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "email", "john@example.com" } });
        var filter = new Dictionary<string, object>
        {
            { "field", "email" },
            { "operator", "endswith" },
            { "value", ".com" }
        };

        // Act & Assert
        Assert.True(_evaluator.Evaluate(record, filter));
    }

    #endregion

    #region Compound Filters

    [Fact]
    public void FilterEvaluator_CompoundFilter_And_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> 
        { 
            { "status", "active" },
            { "age", 25 }
        });
        var matchingFilter = new Dictionary<string, object>
        {
            { "and", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "status", "active" } },
                    new Dictionary<string, object>
                    {
                        { "field", "age" },
                        { "operator", "gte" },
                        { "value", 18 }
                    }
                }
            }
        };
        var nonMatchingFilter = new Dictionary<string, object>
        {
            { "and", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "status", "active" } },
                    new Dictionary<string, object>
                    {
                        { "field", "age" },
                        { "operator", "gte" },
                        { "value", 30 }
                    }
                }
            }
        };

        // Act & Assert
        Assert.True(_evaluator.Evaluate(record, matchingFilter));
        Assert.False(_evaluator.Evaluate(record, nonMatchingFilter));
    }

    [Fact]
    public void FilterEvaluator_CompoundFilter_Or_Works()
    {
        // Arrange
        var records = new List<Record>
        {
            CreateRecord("1", new Dictionary<string, object> { { "status", "active" } }),
            CreateRecord("2", new Dictionary<string, object> { { "status", "pending" } }),
            CreateRecord("3", new Dictionary<string, object> { { "status", "inactive" } })
        };
        var filter = new Dictionary<string, object>
        {
            { "or", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "status", "active" } },
                    new Dictionary<string, object> { { "status", "pending" } }
                }
            }
        };

        // Act & Assert
        Assert.True(_evaluator.Evaluate(records[0], filter));
        Assert.True(_evaluator.Evaluate(records[1], filter));
        Assert.False(_evaluator.Evaluate(records[2], filter));
    }

    [Fact]
    public void FilterEvaluator_CompoundFilter_NestedAndOr_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> 
        { 
            { "status", "active" },
            { "age", 25 }
        });
        var filter = new Dictionary<string, object>
        {
            { "and", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "status", "active" } },
                    new Dictionary<string, object>
                    {
                        { "or", new List<Dictionary<string, object>>
                            {
                                new Dictionary<string, object>
                                {
                                    { "field", "age" },
                                    { "operator", "gte" },
                                    { "value", 18 }
                                },
                                new Dictionary<string, object> { { "role", "admin" } }
                            }
                        }
                    }
                }
            }
        };

        // Act & Assert
        Assert.True(_evaluator.Evaluate(record, filter));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void FilterEvaluator_WithTypeCoercion_WorksCorrectly()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "age", "25" } }); // String
        var filter = new Dictionary<string, object>
        {
            { "field", "age" },
            { "operator", "gt" },
            { "value", 20 } // Integer
        };

        // Act & Assert
        Assert.True(_evaluator.Evaluate(record, filter));
    }

    [Fact]
    public void FilterEvaluator_WithInvalidOperator_ThrowsException()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "age", 25 } });
        var filter = new Dictionary<string, object>
        {
            { "field", "age" },
            { "operator", "invalid_operator" },
            { "value", 20 }
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _evaluator.Evaluate(record, filter));
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_WithMissingField_ReturnsFalse()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "status", "active" } });
        var filter = new Dictionary<string, object>
        {
            { "field", "nonexistent" },
            { "operator", "eq" },
            { "value", "value" }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task FilterEvaluator_WithCsvAdapter_WorksCorrectly()
    {
        // This test verifies that FilterEvaluator can be used with CsvAdapter
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create a test CSV file
            var csvPath = Path.Combine(tempDir, "test.csv");
            File.WriteAllText(csvPath, "id,name,age,status\n1,Alice,25,active\n2,Bob,30,inactive\n3,Charlie,20,active\n");

            // Create CsvAdapter with FilterEvaluator
            var filterEvaluator = new FilterEvaluator();
            var adapter = new CsvAdapter(tempDir, filterEvaluator: filterEvaluator);

            // Create QueryOptions with operator-based filter
            var options = new DataAbstractionAPI.Core.Models.QueryOptions
            {
                Filter = new Dictionary<string, object>
                {
                    { "field", "age" },
                    { "operator", "gte" },
                    { "value", 25 }
                },
                Limit = 10
            };

            // Act
            var result = await adapter.ListAsync("test", options);

            // Assert
            Assert.Equal(2, result.Data.Count); // Should return Alice (25) and Bob (30)
            Assert.Contains(result.Data, r => r.Id == "1"); // Alice
            Assert.Contains(result.Data, r => r.Id == "2"); // Bob
            Assert.DoesNotContain(result.Data, r => r.Id == "3"); // Charlie (age 20 < 25)
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    #endregion
}

