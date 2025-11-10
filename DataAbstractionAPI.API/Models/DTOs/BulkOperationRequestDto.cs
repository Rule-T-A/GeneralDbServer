using System.Text.Json.Serialization;

namespace DataAbstractionAPI.API.Models.DTOs;

/// <summary>
/// Data Transfer Object for bulk operation requests.
/// </summary>
public class BulkOperationRequestDto
{
    /// <summary>
    /// The action to perform: "create", "update", or "delete".
    /// </summary>
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// If true, all operations must succeed or all fail (atomic transaction).
    /// If false, operations are performed in best-effort mode.
    /// </summary>
    [JsonPropertyName("atomic")]
    public bool Atomic { get; set; } = false;

    /// <summary>
    /// Array of records to process. For create/update, contains record data.
    /// For delete, contains records with at least an "id" field.
    /// </summary>
    [JsonPropertyName("records")]
    public List<Dictionary<string, object>> Records { get; set; } = new();

    /// <summary>
    /// Optional update data to apply to all records in update operations.
    /// If not provided, each record in Records should contain the update fields.
    /// </summary>
    [JsonPropertyName("update_data")]
    public Dictionary<string, object>? UpdateData { get; set; }
}

