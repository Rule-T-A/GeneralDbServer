namespace DataAbstractionAPI.Services;

using DataAbstractionAPI.Core.Interfaces;
using DataAbstractionAPI.Core.Models;
using System.Globalization;

/// <summary>
/// Evaluates whether records match filter criteria, supporting simple, operator-based, and compound filters.
/// </summary>
public class FilterEvaluator : IFilterEvaluator
{
    private static readonly HashSet<string> CompoundFilterKeys = new() { "and", "or" };
    private static readonly HashSet<string> OperatorFilterKeys = new() { "field", "operator", "value" };

    public bool Evaluate(Record record, Dictionary<string, object> filter)
    {
        if (filter == null || filter.Count == 0)
        {
            return true; // Empty filter matches all records
        }

        // Check for compound filters (AND/OR)
        if (HasCompoundFilter(filter))
        {
            return EvaluateCompoundFilter(record, filter);
        }

        // Check for operator-based filter
        if (IsOperatorFilter(filter))
        {
            return EvaluateOperatorFilter(record, filter);
        }

        // Otherwise, treat as simple filter (multiple conditions are ANDed)
        return EvaluateSimpleFilter(record, filter);
    }

    private bool HasCompoundFilter(Dictionary<string, object> filter)
    {
        return filter.Keys.Any(key => CompoundFilterKeys.Contains(key.ToLowerInvariant()));
    }

    private bool IsOperatorFilter(Dictionary<string, object> filter)
    {
        // Operator filter has exactly "field", "operator", and "value" keys
        var keys = new HashSet<string>(filter.Keys.Select(k => k.ToLowerInvariant()));
        return keys.Contains("field") && keys.Contains("operator") && keys.Contains("value");
    }

    private bool EvaluateSimpleFilter(Record record, Dictionary<string, object> filter)
    {
        // All conditions must match (AND logic)
        foreach (var kvp in filter)
        {
            if (!record.Data.ContainsKey(kvp.Key))
            {
                return false;
            }

            var recordValue = record.Data[kvp.Key];
            var filterValue = kvp.Value;

            if (!ValuesMatch(recordValue, filterValue))
            {
                return false;
            }
        }

        return true;
    }

    private bool EvaluateOperatorFilter(Record record, Dictionary<string, object> filter)
    {
        var fieldName = filter["field"]?.ToString() ?? string.Empty;
        var operatorName = filter["operator"]?.ToString()?.ToLowerInvariant() ?? string.Empty;
        var filterValue = filter["value"];

        if (!record.Data.ContainsKey(fieldName))
        {
            return false; // Field doesn't exist
        }

        var recordValue = record.Data[fieldName];

        return CompareValues(recordValue, operatorName, filterValue);
    }

    private bool EvaluateCompoundFilter(Record record, Dictionary<string, object> filter)
    {
        // Check for AND
        if (filter.ContainsKey("and"))
        {
            var andFilters = filter["and"] as List<Dictionary<string, object>>;
            if (andFilters == null || andFilters.Count == 0)
            {
                return true; // Empty AND matches all
            }

            // All conditions must match
            foreach (var subFilter in andFilters)
            {
                if (!Evaluate(record, subFilter))
                {
                    return false;
                }
            }

            return true;
        }

        // Check for OR
        if (filter.ContainsKey("or"))
        {
            var orFilters = filter["or"] as List<Dictionary<string, object>>;
            if (orFilters == null || orFilters.Count == 0)
            {
                return false; // Empty OR matches nothing
            }

            // At least one condition must match
            foreach (var subFilter in orFilters)
            {
                if (Evaluate(record, subFilter))
                {
                    return true;
                }
            }

            return false;
        }

        // If we get here, it's malformed but treat as simple filter
        return EvaluateSimpleFilter(record, filter);
    }

    private bool CompareValues(object recordValue, string operatorName, object filterValue)
    {
        return operatorName switch
        {
            "eq" => ValuesMatch(recordValue, filterValue),
            "ne" => !ValuesMatch(recordValue, filterValue),
            "gt" => CompareNumeric(recordValue, filterValue) > 0,
            "gte" => CompareNumeric(recordValue, filterValue) >= 0,
            "lt" => CompareNumeric(recordValue, filterValue) < 0,
            "lte" => CompareNumeric(recordValue, filterValue) <= 0,
            "in" => ValueInArray(recordValue, filterValue),
            "nin" => !ValueInArray(recordValue, filterValue),
            "contains" => StringContains(recordValue, filterValue),
            "startswith" => StringStartsWith(recordValue, filterValue),
            "endswith" => StringEndsWith(recordValue, filterValue),
            _ => throw new ArgumentException($"Unsupported operator: {operatorName}")
        };
    }

    private bool ValuesMatch(object? value1, object? value2)
    {
        // Handle null values
        if (value1 == null && value2 == null)
        {
            return true;
        }

        if (value1 == null || value2 == null)
        {
            return false;
        }

        // Try direct equality first
        if (value1.Equals(value2))
        {
            return true;
        }

        // Try string comparison
        var str1 = value1.ToString() ?? string.Empty;
        var str2 = value2.ToString() ?? string.Empty;

        return str1.Equals(str2, StringComparison.OrdinalIgnoreCase);
    }

    private int CompareNumeric(object? value1, object? value2)
    {
        // Try to convert both to numbers for comparison
        var num1 = ConvertToDouble(value1);
        var num2 = ConvertToDouble(value2);

        if (num1 == null || num2 == null)
        {
            // If conversion fails, try string comparison
            var str1 = value1?.ToString() ?? string.Empty;
            var str2 = value2?.ToString() ?? string.Empty;
            return string.Compare(str1, str2, StringComparison.OrdinalIgnoreCase);
        }

        return num1.Value.CompareTo(num2.Value);
    }

    private double? ConvertToDouble(object? value)
    {
        if (value == null)
        {
            return null;
        }

        // Try direct conversion
        if (value is double d)
        {
            return d;
        }

        if (value is int i)
        {
            return i;
        }

        if (value is float f)
        {
            return f;
        }

        if (value is decimal dec)
        {
            return (double)dec;
        }

        // Try parsing from string
        if (value is string str)
        {
            if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
        }

        // Try converting to string and parsing
        var stringValue = value.ToString() ?? string.Empty;
        if (double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private bool ValueInArray(object? recordValue, object? filterValue)
    {
        if (filterValue == null)
        {
            return false;
        }

        // Handle array/list
        if (filterValue is System.Collections.IEnumerable enumerable && !(filterValue is string))
        {
            foreach (var item in enumerable)
            {
                if (ValuesMatch(recordValue, item))
                {
                    return true;
                }
            }

            return false;
        }

        // Single value - treat as array with one element
        return ValuesMatch(recordValue, filterValue);
    }

    private bool StringContains(object? recordValue, object? filterValue)
    {
        var str1 = recordValue?.ToString() ?? string.Empty;
        var str2 = filterValue?.ToString() ?? string.Empty;

        return str1.Contains(str2, StringComparison.OrdinalIgnoreCase);
    }

    private bool StringStartsWith(object? recordValue, object? filterValue)
    {
        var str1 = recordValue?.ToString() ?? string.Empty;
        var str2 = filterValue?.ToString() ?? string.Empty;

        return str1.StartsWith(str2, StringComparison.OrdinalIgnoreCase);
    }

    private bool StringEndsWith(object? recordValue, object? filterValue)
    {
        var str1 = recordValue?.ToString() ?? string.Empty;
        var str2 = filterValue?.ToString() ?? string.Empty;

        return str1.EndsWith(str2, StringComparison.OrdinalIgnoreCase);
    }
}

