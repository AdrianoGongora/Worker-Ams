using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Worker_Ams.Entities;
using Worker_Ams.Repositories.Datos;

namespace Worker_Ams.Services.Kafka;

public class Consumer(IOptions<KafkaSettings> kafkaSettings, IServiceProvider serviceProvider) : BackgroundService
{
    private readonly KafkaSettings _kafkaSettings = kafkaSettings.Value;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly int _batchSize = 100;
    private DateTime _lastMessageTime = DateTime.UtcNow;
    private readonly List<Dato> _datosList = [];

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            GroupId = "test-bobis",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(_kafkaSettings.Topic);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(cancellationToken);
                    var dato = ParseMessage(consumeResult.Message.Value);
                    _datosList.Add(dato);
                    _lastMessageTime = DateTime.UtcNow;

                    if (_datosList.Count >= _batchSize)
                    {
                        await InsertPendingMessagesAsync();
                    }
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Error consumiendo mensaje: {e.Error.Reason}");
                }

                if (DateTime.UtcNow - _lastMessageTime > TimeSpan.FromSeconds(10))
                {
                    await InsertPendingMessagesAsync();
                }
            }
        }
        catch (OperationCanceledException)
        {
            consumer.Close();
        }
    }

    private async Task InsertPendingMessagesAsync()
    {
        if (_datosList.Count > 0)
        {
            // Crear un nuevo scope para el repositorio
            using var scope = _serviceProvider.CreateScope();
            var datosRepository = scope.ServiceProvider.GetRequiredService<IDatosRepository>();

            await datosRepository.BulkInsertDatosAsync(_datosList);
            _datosList.Clear(); // Limpiar la lista después del insert
            Console.WriteLine("Mensajes insertados en la base de datos.");
        }
    }

    private static Dato ParseMessage(string message)
    {
        var dato = JsonSerializer.Deserialize<Dato>(message);
        return dato!;
    }
}
