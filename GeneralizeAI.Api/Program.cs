// GeneralizeAI.Api/Program.cs
using GeneralizeAI.Application;
using GeneralizeAI.Core.Interfaces;
using GeneralizeAI.Infrastructure;
using GeneralizeAI.Infrastructure.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure settings using the Options Pattern
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDb"));

// 2. Register dependencies for DI
builder.Services.AddScoped<IQueryTemplateService, QueryTemplateService>();
builder.Services.AddScoped<IQueryTemplateRepository, MongoQueryTemplateRepository>();


// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // This allows enums to be serialized as strings in the JSON response.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.WebHost.UseUrls("http://0.0.0.0:5259", "https://0.0.0.0:5260");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();