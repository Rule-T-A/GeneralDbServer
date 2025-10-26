namespace DataAbstractionAPI.Core.Interfaces;

using DataAbstractionAPI.Core.Enums;
using DataAbstractionAPI.Core.Models;

/// <summary>
/// Generates intelligent default values for fields based on naming patterns, context, and type.
/// </summary>
public interface IDefaultGenerator
{
    /// <summary>
    /// Generates a default value for a field.
    /// </summary>
    /// <param name="fieldName">The name of the field</param>
    /// <param name="fieldType">The type of the field</param>
    /// <param name="context">The context for generating the default value</param>
    /// <returns>The generated default value</returns>
    object GenerateDefault(string fieldName, FieldType fieldType, DefaultGenerationContext context);

    /// <summary>
    /// Determines the strategy to use for generating a default value.
    /// </summary>
    /// <param name="fieldName">The name of the field</param>
    /// <param name="fieldType">The type of the field</param>
    /// <returns>The strategy to use</returns>
    DefaultGenerationStrategy DetermineStrategy(string fieldName, FieldType fieldType);
}

