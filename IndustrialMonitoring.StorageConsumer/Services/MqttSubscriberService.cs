using System.Text.Json;
using IndustrialMonitoring.Shared.Models;
using IndustrialMonitoring.StorageConsumer.Configurations;
using IndustrialMonitoring.StorageConsumer.Storage;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace IndustrialMonitoring.StorageConsumer.Services;

public class MqttSubscriberService : IMqttSubscriberService
{
    private readonly MqttSettings _mqttSettings;
    private readonly IReadingRepository _readingRepository;
    private readonly IRedisRepository _redisRepository;
    private readonly ILogger<MqttSubscriberService> _logger;

    public MqttSubscriberService(
        IOptions<MqttSettings> mqttOptions,
        IReadingRepository readingRepository,
        IRedisRepository redisRepository,
        ILogger<MqttSubscriberService> logger)
    {
        _mqttSettings = mqttOptions.Value;
        _readingRepository = readingRepository;
        _redisRepository = redisRepository;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new MqttClientFactory();
        var client = factory.CreateMqttClient();

        client.ApplicationMessageReceivedAsync += async e =>
        {
            try
            {
                string payload = e.ApplicationMessage.ConvertPayloadToString();
                _logger.LogInformation("MQTT message received on topic {Topic}: {Payload}", e.ApplicationMessage.Topic, payload);

                var reading = JsonSerializer.Deserialize<TagReading>(payload);

                if (reading is not null)
                {
                    await _readingRepository.SaveAsync(reading, cancellationToken);
                    await _redisRepository.SaveLatestAsync(reading);

                    _logger.LogInformation("MQTT reading saved to database and Redis for tag: {TagName}", reading.TagName);
                }
                else
                {
                    _logger.LogWarning("Received MQTT payload could not be deserialized into TagReading.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing MQTT message.");
            }
        };

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(_mqttSettings.BrokerHost, _mqttSettings.BrokerPort)
            .WithClientId(_mqttSettings.ClientId)
            .Build();

        await client.ConnectAsync(options, cancellationToken);
        _logger.LogInformation("Connected to MQTT broker at {Host}:{Port}", _mqttSettings.BrokerHost, _mqttSettings.BrokerPort);

        var subscribeOptions = factory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(f => { f.WithTopic(_mqttSettings.Topic); })
            .Build();

        await client.SubscribeAsync(subscribeOptions, cancellationToken);
        _logger.LogInformation("Subscribed to topic: {Topic}", _mqttSettings.Topic);
    }
}