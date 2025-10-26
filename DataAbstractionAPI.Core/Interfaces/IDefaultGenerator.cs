namespace DataAbstractionAPI.Core.Interfaces;

using DataAbstractionAPI.Core.Enums;

/// <summary>
/// Generates intelligent default values for fields based on naming patterns, context, and type.
/// </summary>
public interface IDefaultGenerator
{
    object GenerateDefault(string fieldName, FieldType fieldType);
}

