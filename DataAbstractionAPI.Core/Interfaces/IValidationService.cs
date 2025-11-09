namespace DataAbstractionAPI.Core.Interfaces;

using DataAbstractionAPI.Core.Models;

/// <summary>
/// Validates records against collection schemas.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates a record against a collection schema.
    /// </summary>
    /// <param name="record">The record data to validate</param>
    /// <param name="schema">The schema to validate against</param>
    /// <exception cref="ValidationException">Thrown when validation fails</exception>
    void Validate(Dictionary<string, object> record, CollectionSchema schema);
}

