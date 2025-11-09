using DataAbstractionAPI.Adapters.Csv;
using DataAbstractionAPI.API.Configuration;
using DataAbstractionAPI.API.Middleware;
using DataAbstractionAPI.Core.Interfaces;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure API Key Authentication
builder.Services.Configure<ApiKeyAuthenticationOptions>(
    builder.Configuration.GetSection("ApiKeyAuthentication"));

builder.Services.AddControllers();
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

// Add Global Exception Handler Middleware (should be early in pipeline)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Add API Key Authentication Middleware (before UseAuthorization)
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
