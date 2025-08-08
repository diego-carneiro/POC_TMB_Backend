using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderManagement.Core.Interfaces;
using System.Text.Json;

namespace OrderManagement.Infrastructure.Services;

public class AzureServiceBusService : IMessageService, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<AzureServiceBusService> _logger;
    private ServiceBusProcessor? _processor;

    public AzureServiceBusService(IConfiguration configuration, ILogger<AzureServiceBusService> logger)
    {
        var connectionString = configuration.GetConnectionString("ServiceBus");
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Service Bus connection string não encontrada!");

        _client = new ServiceBusClient(connectionString);
        _logger = logger;
        
        _logger.LogInformation("Azure Service Bus client inicializado");
    }

    public async Task SendMessageAsync<T>(T message, string queueName) where T : class
    {
        try
        {
            var sender = _client.CreateSender(queueName);
            var messageBody = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(messageBody);
            
            await sender.SendMessageAsync(serviceBusMessage);
            _logger.LogInformation("Message sent to queue {QueueName}: {MessageType}", 
                queueName, typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to queue {QueueName}", queueName);
            throw;
        }
    }

    public async Task<T?> ReceiveMessageAsync<T>(string queueName) where T : class
    {
        try
        {
            var receiver = _client.CreateReceiver(queueName);
            var message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(10));
            
            if (message != null)
            {
                var messageBody = message.Body.ToString();
                var deserializedMessage = JsonSerializer.Deserialize<T>(messageBody);
                await receiver.CompleteMessageAsync(message);
                
                _logger.LogInformation("Message received from queue {QueueName}: {MessageType}", 
                    queueName, typeof(T).Name);
                return deserializedMessage;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving message from queue {QueueName}", queueName);
            throw;
        }
    }

    public async Task StartProcessingAsync<T>(string queueName, Func<T, Task> messageHandler) where T : class
    {
        try
        {
            _processor = _client.CreateProcessor(queueName);
            
            _processor.ProcessMessageAsync += async args =>
            {
                try
                {
                    var messageBody = args.Message.Body.ToString();
                    var message = JsonSerializer.Deserialize<T>(messageBody);
                    
                    if (message != null)
                    {
                        await messageHandler(message);
                        _logger.LogInformation("Message processed successfully from queue {QueueName}", queueName);
                    }
                    
                    await args.CompleteMessageAsync(args.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from queue {QueueName}", queueName);
                    await args.AbandonMessageAsync(args.Message);
                }
            };

            _processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception, "Service Bus processing error in queue {QueueName}", queueName);
                return Task.CompletedTask;
            };

            await _processor.StartProcessingAsync();
            _logger.LogInformation("Started processing messages from queue {QueueName}", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting message processing for queue {QueueName}", queueName);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_processor != null)
        {
            await _processor.DisposeAsync();
        }
        await _client.DisposeAsync();
        _logger.LogInformation("Azure Service Bus client disposed");
    }
}