using IndustrialMonitoring.Api.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Temporary test service registration
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IMonitoringService, MonitoringService>(); 
builder.Services.AddScoped<ITagDetailsService, TagDetailsService>();
builder.Services.AddScoped<IAlertsService, AlertsService>();
builder.Services.AddScoped<ITrendsService, TrendsService>();
builder.Services.AddScoped<IOpcUaDiscoveryService, OpcUaDiscoveryService>();

// CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connectionString = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();