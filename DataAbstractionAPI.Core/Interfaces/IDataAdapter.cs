namespace DataAbstractionAPI.Core.Interfaces;

using DataAbstractionAPI.Core.Models;

/// <summary>
/// Core interface for data storage adapters.
/// Provides a unified interface for interacting with data across different storage backends.
/// </summary>
public interface IDataAdapter
{
    // Data Operations
    Task<ListResult> ListAsync(string collection, QueryOptions options, CancellationToken ct = default);
    Task<Record> GetAsync(string collection, string id, CancellationToken ct = default);
    Task<CreateResult> CreateAsync(string collection, Dictionary<string, object> data, CancellationToken ct = default);
    Task UpdateAsync(string collection, string id, Dictionary<string, object> data, CancellationToken ct = default);
    Task DeleteAsync(string collection, string id, CancellationToken ct = default);
    
    // Schema Operations
    Task<CollectionSchema> GetSchemaAsync(string collection, CancellationToken ct = default);
    Task<string[]> ListCollectionsAsync(CancellationToken ct = default);
    
    // Advanced Data Operations
    Task<BulkResult> BulkOperationAsync(string collection, BulkOperationRequest request, CancellationToken ct = default);
    Task<SummaryResult> GetSummaryAsync(string collection, string field, CancellationToken ct = default);
    Task<AggregateResult> AggregateAsync(string collection, AggregateRequest request, CancellationToken ct = default);
}

