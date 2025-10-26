namespace DataAbstractionAPI.Core.Interfaces;

using DataAbstractionAPI.Core.Enums;

/// <summary>
/// Converts values between different types using various conversion strategies.
/// </summary>
public interface ITypeConverter
{
    object Convert(object value, FieldType fromType, FieldType toType);
}

