using DataAbstractionAPI.Adapters.Csv;
using DataAbstractionAPI.API.Configuration;
using DataAbstractionAPI.API.Middleware;
using DataAbstractionAPI.Core.Interfaces;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure API Key Authentication
builder.Services.Configure<ApiKeyAuthenticationOptions>(
    builder.Configuration.GetSection("ApiKeyAuthentication"));

// Configure CORS
builder.Services.Configure<CorsOptions>(
    builder.Configuration.GetSection("Cors"));

var corsOptions = builder.Configuration.GetSection("Cors").Get<CorsOptions>() 
    ?? new CorsOptions();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        if (corsOptions.AllowedOrigins.Length > 0)
        {
            policy.WithOrigins(corsOptions.AllowedOrigins);
        }
        else
        {
            // Fallback: allow all origins in development only
            if (builder.Environment.IsDevelopment())
            {
                policy.AllowAnyOrigin();
            }
        }

        if (corsOptions.AllowedMethods.Length > 0)
        {
            policy.WithMethods(corsOptions.AllowedMethods);
        }
        else
        {
            policy.WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS");
        }

        if (corsOptions.AllowedHeaders.Length > 0)
        {
            policy.WithHeaders(corsOptions.AllowedHeaders);
        }
        else
        {
            policy.WithHeaders("Content-Type", "X-API-Key", "Authorization");
        }

        if (corsOptions.AllowCredentials)
        {
            policy.AllowCredentials();
        }

        if (corsOptions.PreflightMaxAge.HasValue)
        {
            policy.SetPreflightMaxAge(TimeSpan.FromSeconds(corsOptions.PreflightMaxAge.Value));
        }
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Use [JsonPropertyName] attributes instead
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()); // Serialize enums as strings
        options.JsonSerializerOptions.WriteIndented = false; // Compact JSON for efficiency
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Data Abstraction API",
        Version = "v1",
        Description = "API for managing data collections with CSV storage"
    });
    
    // Add API Key authentication to Swagger
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key authentication using the X-API-Key header",
        Name = "X-API-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    // Apply API Key requirement to all endpoints
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                },
                In = ParameterLocation.Header
            },
            Array.Empty<string>()
        }
    });
    
    // Map Dictionary<string, object> to a generic object schema
    c.MapType<Dictionary<string, object>>(() => new OpenApiSchema
    {
        Type = "object",
        AdditionalProperties = new OpenApiSchema
        {
            Type = "object"
        }
    });
    
    // Map IFormFile to binary schema for file uploads
    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
    
    // Ignore model properties that might cause issues
    c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    
    // Configure to handle Dictionary<string, object> properly
    c.UseInlineDefinitionsForEnums();
    
    // Suppress schema generation errors
    c.IgnoreObsoleteActions();
    c.IgnoreObsoleteProperties();
});

// Register CsvAdapter - use relative path to testdata directory
var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "testdata");
builder.Services.AddSingleton<IDataAdapter>(new CsvAdapter(dataPath));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add CORS middleware (before UseAuthorization)
app.UseCors("DefaultPolicy");

// Add Global Exception Handler Middleware (should be early in pipeline)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Add API Key Authentication Middleware (before UseAuthorization)
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
