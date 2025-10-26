namespace DataAbstractionAPI.Core.Enums;

/// <summary>
/// Defines strategies for generating default values based on different approaches.
/// </summary>
public enum DefaultGenerationStrategy
{
    /// <summary>
    /// Use user-specified default value.
    /// </summary>
    UserSpecified = 0,

    /// <summary>
    /// Use naming pattern to determine default (e.g., "is_active" → false).
    /// </summary>
    PatternMatch = 1,

    /// <summary>
    /// Use existing data context to determine default.
    /// </summary>
    ContextAnalysis = 2,

    /// <summary>
    /// Use type-based defaults (e.g., String → empty string).
    /// </summary>
    TypeBased = 3
}

