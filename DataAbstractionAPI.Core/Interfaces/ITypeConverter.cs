namespace DataAbstractionAPI.Core.Interfaces;

using DataAbstractionAPI.Core.Enums;

/// <summary>
/// Converts values between different types using various conversion strategies.
/// </summary>
public interface ITypeConverter
{
    /// <summary>
    /// Converts a value from one type to another using the specified strategy.
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <param name="fromType">The source type</param>
    /// <param name="toType">The target type</param>
    /// <param name="strategy">The conversion strategy to use</param>
    /// <returns>The converted value</returns>
    /// <exception cref="ConversionException">Thrown when conversion fails</exception>
    object Convert(object value, FieldType fromType, FieldType toType, ConversionStrategy strategy);
}

