namespace OrderManagement.Core.Interfaces;

public interface IMessageService
{
    Task SendMessageAsync<T>(T message, string queueName) where T : class;
    Task<T?> ReceiveMessageAsync<T>(string queueName) where T : class;
    Task StartProcessingAsync<T>(string queueName, Func<T, Task> messageHandler) where T : class;
}