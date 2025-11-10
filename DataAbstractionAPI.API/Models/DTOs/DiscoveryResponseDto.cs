using System.Text.Json.Serialization;

namespace DataAbstractionAPI.API.Models.DTOs;

/// <summary>
/// Discovery endpoint response providing machine-readable information about the API.
/// This endpoint is always available in both Development and Production environments,
/// unlike Swagger which is development-only.
/// </summary>
public class DiscoveryResponseDto
{
    /// <summary>
    /// API version identifier.
    /// </summary>
    /// <example>v1</example>
    [JsonPropertyName("api_version")]
    public string ApiVersion { get; set; } = "v1";

    /// <summary>
    /// Base URL of the API.
    /// </summary>
    /// <example>http://localhost:5012/api</example>
    [JsonPropertyName("base_url")]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL to the OpenAPI JSON specification (only available in Development).
    /// </summary>
    /// <example>http://localhost:5012/swagger/v1/swagger.json</example>
    [JsonPropertyName("openapi_spec")]
    public string? OpenApiSpec { get; set; }

    /// <summary>
    /// URL to the Swagger UI (only available in Development).
    /// </summary>
    /// <example>http://localhost:5012/swagger</example>
    [JsonPropertyName("swagger_ui")]
    public string? SwaggerUi { get; set; }

    /// <summary>
    /// Whether OpenAPI documentation is available (typically only in Development).
    /// </summary>
    /// <example>true</example>
    [JsonPropertyName("openapi_available")]
    public bool OpenApiAvailable { get; set; }

    /// <summary>
    /// Authentication information.
    /// </summary>
    [JsonPropertyName("authentication")]
    public AuthenticationInfoDto Authentication { get; set; } = new();

    /// <summary>
    /// Quick start guide with common API operations.
    /// </summary>
    [JsonPropertyName("quick_start")]
    public List<string> QuickStart { get; set; } = new();

    /// <summary>
    /// Detailed endpoint information (optional, can be expanded later).
    /// </summary>
    [JsonPropertyName("endpoints")]
    public EndpointsInfoDto? Endpoints { get; set; }
}

/// <summary>
/// Authentication information for the API.
/// </summary>
public class AuthenticationInfoDto
{
    /// <summary>
    /// Type of authentication used.
    /// </summary>
    /// <example>api_key</example>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "api_key";

    /// <summary>
    /// HTTP header name for the API key.
    /// </summary>
    /// <example>X-API-Key</example>
    [JsonPropertyName("header")]
    public string Header { get; set; } = "X-API-Key";

    /// <summary>
    /// Whether authentication is required.
    /// </summary>
    /// <example>false</example>
    [JsonPropertyName("required")]
    public bool Required { get; set; }

    /// <summary>
    /// Description of the authentication mechanism.
    /// </summary>
    /// <example>Optional API key authentication. Configured via appsettings.json</example>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>
/// Information about available API endpoints.
/// </summary>
public class EndpointsInfoDto
{
    /// <summary>
    /// Collection listing endpoint information.
    /// </summary>
    [JsonPropertyName("collections")]
    public EndpointInfoDto? Collections { get; set; }

    /// <summary>
    /// Data operation endpoints information.
    /// </summary>
    [JsonPropertyName("data")]
    public DataEndpointsInfoDto? Data { get; set; }

    /// <summary>
    /// Schema operation endpoints information.
    /// </summary>
    [JsonPropertyName("schema")]
    public EndpointInfoDto? Schema { get; set; }

    /// <summary>
    /// File upload endpoint information.
    /// </summary>
    [JsonPropertyName("upload")]
    public EndpointInfoDto? Upload { get; set; }
}

/// <summary>
/// Information about data operation endpoints.
/// </summary>
public class DataEndpointsInfoDto
{
    /// <summary>
    /// List records endpoint.
    /// </summary>
    /// <example>GET /api/data/{collection}</example>
    [JsonPropertyName("list")]
    public string List { get; set; } = "GET /api/data/{collection}";

    /// <summary>
    /// Get single record endpoint.
    /// </summary>
    /// <example>GET /api/data/{collection}/{id}</example>
    [JsonPropertyName("get")]
    public string Get { get; set; } = "GET /api/data/{collection}/{id}";

    /// <summary>
    /// Create record endpoint.
    /// </summary>
    /// <example>POST /api/data/{collection}</example>
    [JsonPropertyName("create")]
    public string Create { get; set; } = "POST /api/data/{collection}";

    /// <summary>
    /// Update record endpoint.
    /// </summary>
    /// <example>PUT /api/data/{collection}/{id}</example>
    [JsonPropertyName("update")]
    public string Update { get; set; } = "PUT /api/data/{collection}/{id}";

    /// <summary>
    /// Delete record endpoint.
    /// </summary>
    /// <example>DELETE /api/data/{collection}/{id}</example>
    [JsonPropertyName("delete")]
    public string Delete { get; set; } = "DELETE /api/data/{collection}/{id}";

    /// <summary>
    /// Summary/aggregation endpoint.
    /// </summary>
    /// <example>GET /api/data/{collection}/summary?field={fieldName}</example>
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    /// <summary>
    /// Bulk operations endpoint.
    /// </summary>
    /// <example>POST /api/data/{collection}/bulk</example>
    [JsonPropertyName("bulk")]
    public string? Bulk { get; set; }

    /// <summary>
    /// Complex aggregation endpoint.
    /// </summary>
    /// <example>POST /api/data/{collection}/aggregate</example>
    [JsonPropertyName("aggregate")]
    public string? Aggregate { get; set; }
}

/// <summary>
/// Information about a single endpoint.
/// </summary>
public class EndpointInfoDto
{
    /// <summary>
    /// Endpoint path and method.
    /// </summary>
    /// <example>GET /api/data</example>
    [JsonPropertyName("endpoint")]
    public string? Endpoint { get; set; }

    /// <summary>
    /// Description of what the endpoint does.
    /// </summary>
    /// <example>List all available collections</example>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

