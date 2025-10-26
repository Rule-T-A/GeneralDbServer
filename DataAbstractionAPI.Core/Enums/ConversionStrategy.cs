namespace DataAbstractionAPI.Core.Enums;

/// <summary>
/// Defines strategies for converting values between different types.
/// </summary>
public enum ConversionStrategy
{
    /// <summary>
    /// Direct cast conversion (e.g., "123" → 123).
    /// </summary>
    Cast = 0,

    /// <summary>
    /// Truncate value to fit target type (e.g., very long string → first N chars).
    /// </summary>
    Truncate = 1,

    /// <summary>
    /// Throw exception if conversion fails.
    /// </summary>
    FailOnError = 2,

    /// <summary>
    /// Set value to null if conversion fails.
    /// </summary>
    SetNull = 3
}

