using DataAbstractionAPI.Adapters.Csv;
using DataAbstractionAPI.Core.Interfaces;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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
app.UseAuthorization();
app.MapControllers();

app.Run();
