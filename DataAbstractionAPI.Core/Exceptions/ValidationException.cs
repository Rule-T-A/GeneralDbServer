namespace DataAbstractionAPI.Core.Exceptions;

/// <summary>
/// Exception thrown when data validation fails.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// The name of the field that failed validation.
    /// </summary>
    public string FieldName { get; }

    public ValidationException(string fieldName)
        : this(fieldName, $"Validation failed for field '{fieldName}'")
    {
    }

    public ValidationException(string fieldName, string message)
        : base(message)
    {
        FieldName = fieldName;
    }

    public ValidationException(string fieldName, string message, Exception innerException)
        : base(message, innerException)
    {
        FieldName = fieldName;
    }
}

