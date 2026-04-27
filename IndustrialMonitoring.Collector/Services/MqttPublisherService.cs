using System.Text;
using System.Text.Json;
using IndustrialMonitoring.Collector.Configurations;
using IndustrialMonitoring.Shared.Models;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace IndustrialMonitoring.Collector.Services;

public class MqttPublisherService : IMqttPublisherService
{
    private readonly MqttSettings _mqttSettings;
    private readonly ILogger<MqttPublisherService> _logger;

    public MqttPublisherService(
        IOptions<MqttSettings> mqttOptions,
        ILogger<MqttPublisherService> logger)
    {
        _mqttSettings = mqttOptions.Value;
        _logger = logger;
    }

    public async Task PublishAsync(TagReading reading, CancellationToken cancellationToken)
    {
        var factory = new MqttClientFactory();
        using var client = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(_mqttSettings.BrokerHost, _mqttSettings.BrokerPort)
            .WithClientId(_mqttSettings.ClientId)
            .Build();

        await client.ConnectAsync(options, cancellationToken);

        string payload = JsonSerializer.Serialize(reading);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(_mqttSettings.Topic)
            .WithPayload(Encoding.UTF8.GetBytes(payload))
            .Build();

        await client.PublishAsync(message, cancellationToken);

        _logger.LogInformation(
            "Published MQTT message for tag {TagName} to topic {Topic}",
            reading.TagName,
            _mqttSettings.Topic);
    }
}