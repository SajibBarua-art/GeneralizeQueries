// GeneralizeQueries.Api/Program.cs

using System.Text.Json.Serialization;
using GeneralizeQueries.Application;
using GeneralizeQueries.Application.Services.AuditLog;
using GeneralizeQueries.Application.Services.RoleAuthorization;
using GeneralizeQueries.Core.Configuration;
using GeneralizeQueries.Core.Interfaces;
using GeneralizeQueries.Core.Interfaces.RoleAuthorization;
using GeneralizeQueries.Infrastructure.Data;
using GeneralizeQueries.Infrastructure.Data.AuditLog;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Platform.Infrastructure.Authentication;
using Platform.Infrastructure.Core.Extensions;
using Platform.Infrastructure.Host.WebApi.Middlewares;
using Platform.Infrastructure.MassTransit.Bus.RabbitMQ.Extensions;
using Platform.Infrastructure.ServiceRegistry;

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.CSharpLegacy));

var builder = WebApplication.CreateBuilder(args);

// 2. Register dependencies for DI
builder.Services.AddScoped<IServiceRegistryProvider, ServiceRegistryProvider>();
builder.Services.AddScoped<IServiceRegistrationService, ServiceRegistrationService>();

builder.Services.AddJwtBearer(builder.Configuration);

// Register RoleManagement configuration
builder.Services.Configure<RoleManagementSettings>(
    builder.Configuration.GetSection(RoleManagementSettings.SectionName));

// Register the authorization service
builder.Services.AddScoped<IRoleAuthorizationService, RoleAuthorizationService>();

builder.Services.AddScoped<ICollectionRepositoryFactory, CollectionRepositoryFactory>();
builder.Services.AddScoped<ICollectionService, CollectionService>();
// builder.Services.AddScoped<CollectionService>();
// builder.Services.AddScoped<ICollectionRepository, MongoCollectionRepository>();

builder.Services.AddScoped<IQueryTemplateService, QueryTemplateService>();
builder.Services.AddScoped<IQueryTemplateRepositoryFactory, QueryTemplateRepositoryFactory>();
// builder.Services.AddScoped<IQueryTemplateRepository, MongoQueryTemplateRepository>();

// FeatureAggregateRoots services
builder.Services.AddScoped<IFeatureAggregateRootsService, FeatureAggregateRootsService>();
builder.Services.AddScoped<IFeatureAggregateRootsRepositoryFactory, FeatureAggregateRootsRepositoryFactory>();

// FeatureViewModels services
builder.Services.AddScoped<IFeatureViewModelsService, FeatureViewModelsService>();
builder.Services.AddScoped<IFeatureViewModelsRepositoryFactory, FeatureViewModelsRepositoryFactory>();

// RoleFeatureViewModels services
builder.Services.AddScoped<IRoleFeatureViewModelsService, RoleFeatureViewModelsService>();
builder.Services.AddScoped<IRoleFeatureViewModelsRepositoryFactory, RoleFeatureViewModelsRepositoryFactory>();

// FeatureManagement services
builder.Services.AddScoped<IFeatureManagementService, FeatureManagementService>();

// AuditLog services
builder.Services.AddScoped<IAuditLogViewModelsService, AuditLogViewModelsService>();
builder.Services.AddScoped<IAuditLogViewModelsRepositoryFactory, AuditLogViewModelsRepositoryFactory>();

// Register generic services for reusability
builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericMongoRepository<>));

builder.Services.AddInMemoryBusServices();
builder.Services.AddServiceBusProvider(builder.Configuration);

// Register MongoClientFactory as a singleton for managing MongoClient instances
builder.Services.AddSingleton<IMongoClientFactory, MongoClientFactory>();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // This allows enums to be serialized as strings in the JSON response.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();

app.UseRouting();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseCors(corsPolicyBuilder => corsPolicyBuilder
    .AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials()
    .SetPreflightMaxAge(TimeSpan.FromDays(365)));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

var hostLifeTime = app.Services.GetRequiredService<IHostApplicationLifetime>();
hostLifeTime.ApplicationStarted.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Application has started successfully!");
});

app.Run();