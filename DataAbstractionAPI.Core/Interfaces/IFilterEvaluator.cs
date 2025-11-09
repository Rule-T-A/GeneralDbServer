namespace DataAbstractionAPI.Core.Interfaces;

using DataAbstractionAPI.Core.Models;

/// <summary>
/// Evaluates whether a record matches a filter criteria.
/// </summary>
public interface IFilterEvaluator
{
    /// <summary>
    /// Evaluates whether a record matches the specified filter.
    /// </summary>
    /// <param name="record">The record to evaluate</param>
    /// <param name="filter">The filter criteria (simple, operator-based, or compound)</param>
    /// <returns>True if the record matches the filter, false otherwise</returns>
    bool Evaluate(Record record, Dictionary<string, object> filter);
}

