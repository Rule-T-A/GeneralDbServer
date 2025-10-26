using DataAbstractionAPI.Adapters.Csv;
using DataAbstractionAPI.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
