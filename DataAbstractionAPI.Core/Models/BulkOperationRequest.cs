namespace DataAbstractionAPI.Core.Models;

/// <summary>
/// Request model for bulk operations (create, update, delete).
/// </summary>
public class BulkOperationRequest
{
    /// <summary>
    /// The action to perform: "create", "update", or "delete".
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// If true, all operations must succeed or all fail (atomic transaction).
    /// If false, operations are performed in best-effort mode.
    /// </summary>
    public bool Atomic { get; set; } = false;

    /// <summary>
    /// Array of records to process. For create/update, contains record data.
    /// For delete, contains records with at least an "id" field.
    /// </summary>
    public List<Dictionary<string, object>> Records { get; set; } = new();

    /// <summary>
    /// Optional update data to apply to all records in update operations.
    /// If not provided, each record in Records should contain the update fields.
    /// </summary>
    public Dictionary<string, object>? UpdateData { get; set; }
}

