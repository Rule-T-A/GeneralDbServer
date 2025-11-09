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

    [Fact]
    public void FilterEvaluator_WithNullFilter_ReturnsTrue()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "status", "active" } });
        Dictionary<string, object>? filter = null;

        // Act
        var result = _evaluator.Evaluate(record, filter!);

        // Assert
        Assert.True(result); // Null filter should match all
    }

    #endregion

    #region Compound Filter Edge Cases

    [Fact]
    public void FilterEvaluator_CompoundFilter_EmptyAnd_ReturnsTrue()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "status", "active" } });
        var filter = new Dictionary<string, object>
        {
            { "and", new List<Dictionary<string, object>>() }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert
        Assert.True(result); // Empty AND matches all
    }

    [Fact]
    public void FilterEvaluator_CompoundFilter_EmptyOr_ReturnsFalse()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "status", "active" } });
        var filter = new Dictionary<string, object>
        {
            { "or", new List<Dictionary<string, object>>() }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert
        Assert.False(result); // Empty OR matches nothing
    }

    [Fact]
    public void FilterEvaluator_CompoundFilter_NullAndList_ReturnsTrue()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "status", "active" } });
        var filter = new Dictionary<string, object>
        {
            { "and", null! }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert - Should handle null gracefully, treat as empty
        Assert.True(result);
    }

    [Fact]
    public void FilterEvaluator_CompoundFilter_DeeplyNested_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object>
        {
            { "status", "active" },
            { "age", 25 },
            { "role", "admin" }
        });
        var filter = new Dictionary<string, object>
        {
            { "or", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
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
                    }
                }
            }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Null Value Handling Tests

    [Fact]
    public void FilterEvaluator_OperatorFilter_Eq_WithNullValues_Matches()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "field", null! } });
        var filter = new Dictionary<string, object>
        {
            { "field", "field" },
            { "operator", "eq" },
            { "value", null! }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_Ne_WithNullValues_DoesNotMatch()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "field", null! } });
        var filter = new Dictionary<string, object>
        {
            { "field", "field" },
            { "operator", "ne" },
            { "value", "value" }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert
        Assert.True(result); // null != "value"
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_Gt_WithNullValue_HandlesGracefully()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "age", 25 } });
        var filter = new Dictionary<string, object>
        {
            { "field", "age" },
            { "operator", "gt" },
            { "value", null! }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert - Null converts to 0, so 25 > 0 is true
        Assert.True(result);
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_Contains_WithNullValue_HandlesGracefully()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "name", "John" } });
        var filter = new Dictionary<string, object>
        {
            { "field", "name" },
            { "operator", "contains" },
            { "value", null! }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert - Null.ToString() returns empty string, so "John".Contains("") is true
        Assert.True(result);
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_In_WithNullValue_HandlesGracefully()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "status", "active" } });
        var filter = new Dictionary<string, object>
        {
            { "field", "status" },
            { "operator", "in" },
            { "value", null! }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Type Coercion Edge Cases

    [Fact]
    public void FilterEvaluator_OperatorFilter_Gt_WithStringNumber_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "age", "30" } });
        var filter = new Dictionary<string, object>
        {
            { "field", "age" },
            { "operator", "gt" },
            { "value", 25 }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_Gt_WithInvalidString_HandlesGracefully()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "age", "not-a-number" } });
        var filter = new Dictionary<string, object>
        {
            { "field", "age" },
            { "operator", "gt" },
            { "value", 25 }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert - Falls back to string comparison, "not-a-number" > "25" is true (lexicographically)
        Assert.True(result);
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_Gt_WithDecimal_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "price", 19.99m } });
        var filter = new Dictionary<string, object>
        {
            { "field", "price" },
            { "operator", "gt" },
            { "value", 10.0 }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_Gt_WithFloat_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "price", 19.99f } });
        var filter = new Dictionary<string, object>
        {
            { "field", "price" },
            { "operator", "gt" },
            { "value", 10.0 }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_Gt_WithLong_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "count", 100L } });
        var filter = new Dictionary<string, object>
        {
            { "field", "count" },
            { "operator", "gt" },
            { "value", 50 }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region String Operator Edge Cases

    [Fact]
    public void FilterEvaluator_OperatorFilter_Contains_WithEmptyString_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "name", "John" } });
        var filter = new Dictionary<string, object>
        {
            { "field", "name" },
            { "operator", "contains" },
            { "value", "" }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert - Empty string should match (contains empty string)
        Assert.True(result);
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_StartsWith_WithEmptyString_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "name", "John" } });
        var filter = new Dictionary<string, object>
        {
            { "field", "name" },
            { "operator", "startswith" },
            { "value", "" }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert - Empty string should match (starts with empty string)
        Assert.True(result);
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_EndsWith_WithEmptyString_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "name", "John" } });
        var filter = new Dictionary<string, object>
        {
            { "field", "name" },
            { "operator", "endswith" },
            { "value", "" }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert - Empty string should match (ends with empty string)
        Assert.True(result);
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_Contains_WithSpecialCharacters_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "email", "user@example.com" } });
        var filter = new Dictionary<string, object>
        {
            { "field", "email" },
            { "operator", "contains" },
            { "value", "@" }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Array/Collection Edge Cases

    [Fact]
    public void FilterEvaluator_OperatorFilter_In_WithEmptyArray_ReturnsFalse()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "status", "active" } });
        var filter = new Dictionary<string, object>
        {
            { "field", "status" },
            { "operator", "in" },
            { "value", new string[0] }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_In_WithSingleElementArray_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "status", "active" } });
        var filter = new Dictionary<string, object>
        {
            { "field", "status" },
            { "operator", "in" },
            { "value", new[] { "active" } }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_In_WithList_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "status", "active" } });
        var filter = new Dictionary<string, object>
        {
            { "field", "status" },
            { "operator", "in" },
            { "value", new List<string> { "active", "pending" } }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_In_WithHashSet_Works()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "status", "active" } });
        var filter = new Dictionary<string, object>
        {
            { "field", "status" },
            { "operator", "in" },
            { "value", new HashSet<string> { "active", "pending" } }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_In_WithString_DoesNotTreatAsEnumerable()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "name", "John" } });
        var filter = new Dictionary<string, object>
        {
            { "field", "name" },
            { "operator", "in" },
            { "value", "John" } // String should be treated as single value, not enumerable
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert - Should match as single value
        Assert.True(result);
    }

    #endregion

    #region Malformed Filter Tests

    [Fact]
    public void FilterEvaluator_OperatorFilter_WithMissingFieldKey_TreatsAsSimpleFilter()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "status", "active" } });
        var filter = new Dictionary<string, object>
        {
            { "operator", "eq" },
            { "value", "active" }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert - Should treat as simple filter, won't match
        Assert.False(result);
    }

    [Fact]
    public void FilterEvaluator_OperatorFilter_WithMissingOperatorKey_TreatsAsSimpleFilter()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "status", "active" } });
        var filter = new Dictionary<string, object>
        {
            { "field", "status" },
            { "value", "active" }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert - Without "operator" key, it's not an operator filter, so treated as simple filter
        // Simple filter requires "field" key to match, but record has "status" not "field"
        Assert.False(result);
    }

    [Fact]
    public void FilterEvaluator_CompoundFilter_Malformed_FallsBackToSimpleFilter()
    {
        // Arrange
        var record = CreateRecord("1", new Dictionary<string, object> { { "status", "active" } });
        var filter = new Dictionary<string, object>
        {
            { "and", "not-a-list" }, // Invalid type
            { "status", "active" }
        };

        // Act
        var result = _evaluator.Evaluate(record, filter);

        // Assert - Should fall back to simple filter
        Assert.True(result);
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

