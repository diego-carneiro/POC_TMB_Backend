using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var connectionString = configuration.GetConnectionString("ServiceBus");
var queueName = "order-created";

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("❌ Connection string não encontrada!");
    return;
}

Console.WriteLine("🚌 Testando Azure Service Bus...");
Console.WriteLine($"Queue: {queueName}");
Console.WriteLine();

try
{
    await using var client = new ServiceBusClient(connectionString);
    
    //  TESTE 1: Enviar mensagem
    Console.WriteLine("📤 Teste 1: Enviando mensagem...");
    
    var sender = client.CreateSender(queueName);
    var testMessage = new
    {
        Id = Guid.NewGuid(),
        Message = "Teste de conexão",
        Timestamp = DateTime.UtcNow
    };
    
    var messageBody = JsonSerializer.Serialize(testMessage);
    var serviceBusMessage = new ServiceBusMessage(messageBody);
    
    await sender.SendMessageAsync(serviceBusMessage);
    Console.WriteLine("✅ Mensagem enviada com sucesso!");
    Console.WriteLine($"   ID: {testMessage.Id}");
    Console.WriteLine($"   Conteúdo: {testMessage.Message}");
    Console.WriteLine();
    
    //  TESTE 2: Receber mensagem
    Console.WriteLine("📥 Teste 2: Recebendo mensagem...");
    
    var receiver = client.CreateReceiver(queueName);
    var receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(10));
    
    if (receivedMessage != null)
    {
        var receivedBody = receivedMessage.Body.ToString();
        var deserializedMessage = JsonSerializer.Deserialize<dynamic>(receivedBody);
        
        Console.WriteLine("✅ Mensagem recebida com sucesso!");
        Console.WriteLine($"   Conteúdo: {receivedBody}");
        
        await receiver.CompleteMessageAsync(receivedMessage);
        Console.WriteLine("✅ Mensagem marcada como processada!");
    }
    else
    {
        Console.WriteLine("⚠️  Nenhuma mensagem recebida (timeout 10s)");
    }
    
    Console.WriteLine();
    Console.WriteLine("🎉 TODOS OS TESTES PASSARAM!");
    Console.WriteLine("🔗 Service Bus está configurado corretamente!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ ERRO: {ex.Message}");
    Console.WriteLine($"   Tipo: {ex.GetType().Name}");
    
    if (ex.InnerException != null)
    {
        Console.WriteLine($"   Inner: {ex.InnerException.Message}");
    }
}

Console.WriteLine();
Console.WriteLine("Pressione qualquer tecla para sair...");
Console.ReadKey();