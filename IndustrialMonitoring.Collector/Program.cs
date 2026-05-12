using IndustrialMonitoring.Collector.Configurations;
using IndustrialMonitoring.Collector.OpcUa;
using IndustrialMonitoring.Collector.Services;
using IndustrialMonitoring.Collector.Storage;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<OpcUaSettings>(builder.Configuration.GetSection("OpcUa"));
builder.Services.Configure<CollectorSettings>(builder.Configuration.GetSection("Collector"));
builder.Services.Configure<MqttSettings>(builder.Configuration.GetSection("Mqtt"));

builder.Services.AddSingleton<OpcUaDiscoveryPersistenceService>();
builder.Services.AddSingleton<EnabledTagsProvider>();

builder.Services.AddSingleton<IOpcUaClient, OpcUaClientService>();
builder.Services.AddSingleton<IMqttPublisherService, MqttPublisherService>();

builder.Services.AddHostedService<OpcUaDiscoveryWorker>();
builder.Services.AddHostedService<CollectorWorker>();
builder.Services.AddSingleton<OpcUaHierarchyPersistenceService>();

var host = builder.Build();
host.Run();