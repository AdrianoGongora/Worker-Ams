using System.Text.Json;
using Confluent.Kafka;
using Worker_Ams.Entities;
using Worker_Ams.Repositories.Datos;
using Worker_Ams.Services.Kafka;

namespace Worker.Services.Kafka;

public class Consumer(KafkaSettings kafkaSettings, IDatosRepository datosRepository)
{
    private readonly KafkaSettings kafkaSettings = kafkaSettings;
    private readonly IDatosRepository _datosRepository = datosRepository;
    private readonly int _batchSize = 100;
    private DateTime _lastMessageTime = DateTime.UtcNow;
    private List<Dato> _datosList = [];

    public async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = kafkaSettings.BootstrapServers,
            GroupId = "test-bobis",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(kafkaSettings.Topic);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(cancellationToken);

                    // Procesar el mensaje recibido
                    var dato = ParseMessage(consumeResult.Message.Value);
                    _datosList.Add(dato);
                    _lastMessageTime = DateTime.UtcNow; // Actualizamos el tiempo del último mensaje recibido

                    // Si alcanzamos el límite de batch, hacemos el bulk insert
                    if (_datosList.Count >= _batchSize)
                    {
                        await InsertPendingMessagesAsync();
                    }
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Error consumiendo mensaje: {e.Error.Reason}");
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
            await _datosRepository.BulkInsertDatosAsync(_datosList);
            _datosList.Clear(); // Limpiar la lista después del insert
            Console.WriteLine("Mensajes insertados en la base de datos.");
        }
    }

    private static Dato ParseMessage(string message)
    {
        // Aquí puedes implementar la lógica de parsing según el formato de los datos de tu Excel
        // Por ejemplo, si los datos vienen en formato JSON:
        var dato = JsonSerializer.Deserialize<Dato>(message);
        return dato!;
    }
}
