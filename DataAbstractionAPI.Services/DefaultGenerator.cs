namespace DataAbstractionAPI.Services;

using DataAbstractionAPI.Core.Interfaces;
using DataAbstractionAPI.Core.Models;
using DataAbstractionAPI.Core.Enums;
using Microsoft.Extensions.Logging;

/// <summary>
/// Generates intelligent default values for fields based on naming patterns, context, and type.
/// </summary>
public class DefaultGenerator : IDefaultGenerator
{
    private readonly ILogger<DefaultGenerator>? _logger;

    public DefaultGenerator(ILogger<DefaultGenerator>? logger = null)
    {
        _logger = logger;
    }

    public object GenerateDefault(string fieldName, FieldType fieldType, DefaultGenerationContext context)
    {
        _logger?.LogDebug("Generating default value for field '{FieldName}' of type {FieldType} in collection '{Collection}'", 
            fieldName, fieldType, context?.CollectionName ?? "unknown");

        var strategy = DetermineStrategy(fieldName, fieldType);
        _logger?.LogDebug("Determined strategy: {Strategy} for field '{FieldName}'", strategy, fieldName);

        var result = strategy switch
        {
            DefaultGenerationStrategy.PatternMatch => GeneratePatternBasedDefault(fieldName, fieldType),
            DefaultGenerationStrategy.TypeBased => GenerateTypeBasedDefault(fieldType),
            DefaultGenerationStrategy.ContextAnalysis => GenerateContextBasedDefault(fieldName, fieldType, context),
            _ => (object?)null
        };

        _logger?.LogDebug("Generated default value '{Value}' for field '{FieldName}' using strategy {Strategy}", 
            result, fieldName, strategy);

        return result;
    }

    public DefaultGenerationStrategy DetermineStrategy(string fieldName, FieldType fieldType)
    {
        // Check for pattern-based strategies first
        if (HasPattern(fieldName, fieldType))
        {
            return DefaultGenerationStrategy.PatternMatch;
        }

        // Fall back to type-based defaults
        return DefaultGenerationStrategy.TypeBased;
    }

    private bool HasPattern(string fieldName, FieldType fieldType)
    {
        var lowerName = fieldName.ToLowerInvariant();

        // Boolean patterns: is_*, has_*, can_*
        if (fieldType == FieldType.Boolean)
        {
            return lowerName.StartsWith("is_") || 
                   lowerName.StartsWith("has_") || 
                   lowerName.StartsWith("can_");
        }

        // DateTime patterns: *_at, *_date, created_*, updated_*, deleted_*
        if (fieldType == FieldType.DateTime || fieldType == FieldType.Date)
        {
            return lowerName.EndsWith("_at") || 
                   lowerName.EndsWith("_date") || 
                   lowerName.StartsWith("created_") || 
                   lowerName.StartsWith("updated_") || 
                   lowerName.StartsWith("deleted_");
        }

        // ID patterns: *_id, *_key
        if (lowerName.EndsWith("_id") || lowerName.EndsWith("_key"))
        {
            return true;
        }

        // Count patterns: *_count, *_total, num_*
        if (lowerName.EndsWith("_count") || 
            lowerName.EndsWith("_total") || 
            lowerName.StartsWith("num_"))
        {
            return true;
        }

        return false;
    }

    private object GeneratePatternBasedDefault(string fieldName, FieldType fieldType)
    {
        var lowerName = fieldName.ToLowerInvariant();

        // Boolean patterns
        if (fieldType == FieldType.Boolean && (lowerName.StartsWith("is_") || lowerName.StartsWith("has_") || lowerName.StartsWith("can_")))
        {
            return false;
        }

        // DateTime/Date patterns
        if ((fieldType == FieldType.DateTime || fieldType == FieldType.Date) && 
            (lowerName.EndsWith("_at") || lowerName.EndsWith("_date") || 
             lowerName.StartsWith("created_") || lowerName.StartsWith("updated_") || lowerName.StartsWith("deleted_")))
        {
            return DateTime.UtcNow;
        }

        // ID patterns - return null
        if (lowerName.EndsWith("_id") || lowerName.EndsWith("_key"))
        {
            return (object?)null;
        }

        // Count patterns - return 0
        if (lowerName.EndsWith("_count") || lowerName.EndsWith("_total") || lowerName.StartsWith("num_"))
        {
            return 0;
        }

        // Default fallback to type-based
        return GenerateTypeBasedDefault(fieldType);
    }

    private object GenerateTypeBasedDefault(FieldType fieldType)
    {
        return fieldType switch
        {
            FieldType.String => string.Empty,
            FieldType.Integer => 0,
            FieldType.Float => 0.0,
            FieldType.Boolean => false,
            FieldType.DateTime => DateTime.UtcNow,
            FieldType.Date => DateTime.UtcNow.Date,
            FieldType.Array => Array.Empty<object>(),
            FieldType.Object => new Dictionary<string, object>(),
            _ => (object?)null
        };
    }

    private object GenerateContextBasedDefault(string fieldName, FieldType fieldType, DefaultGenerationContext context)
    {
        // TODO: Implement context-based default generation in a future step
        // For now, fall back to pattern-based or type-based
        if (HasPattern(fieldName, fieldType))
        {
            return GeneratePatternBasedDefault(fieldName, fieldType);
        }
        
        return GenerateTypeBasedDefault(fieldType);
    }
}

