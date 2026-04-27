using IndustrialMonitoring.StorageConsumer.Configurations;
using IndustrialMonitoring.StorageConsumer.Services;
using IndustrialMonitoring.StorageConsumer.Storage;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));
builder.Services.Configure<MqttSettings>(builder.Configuration.GetSection("Mqtt"));
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("Redis"));

builder.Services.AddSingleton<IReadingRepository, TimescaleRepository>();
builder.Services.AddSingleton<IRedisRepository, RedisRepository>();
builder.Services.AddSingleton<IMqttSubscriberService, MqttSubscriberService>();
builder.Services.AddHostedService<StorageConsumerWorker>();

var host = builder.Build();
host.Run();