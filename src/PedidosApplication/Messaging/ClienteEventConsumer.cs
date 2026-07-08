using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PedidosApplication.Events;
using PedidosApplication.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PedidosApplication.Messaging
{
    // Escucha los eventos de Cliente publicados por el microservicio de Clientes y actualiza
    // o elimina los pedidos afectados. Se registra como hosted service desde PedidosApi.
    public class ClienteEventConsumer(
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<ClienteEventConsumer> logger) : BackgroundService
    {
        private readonly RabbitMqOptions _options = options.Value;
        private IConnection? _connection;
        private IChannel? _channel;

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.User,
                Password = _options.Password
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.ExchangeDeclareAsync(
                exchange: ClienteEventContract.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: ClienteEventContract.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await _channel.QueueBindAsync(
                queue: ClienteEventContract.QueueName,
                exchange: ClienteEventContract.ExchangeName,
                routingKey: ClienteEventContract.RoutingKeyPattern,
                cancellationToken: cancellationToken);

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channel is null) return;

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var evento = JsonSerializer.Deserialize<ClienteEventEnvelope>(json);

                    if (evento is not null)
                    {
                        using var scope = scopeFactory.CreateScope();
                        var pedidoService = scope.ServiceProvider.GetRequiredService<IPedidoService>();

                        if (evento.EventType == ClienteEventTypes.Actualizado && evento.Nombre is not null)
                        {
                            await pedidoService.ActualizarNombreClienteAsync(evento.IdCliente, evento.Nombre);
                            logger.LogInformation("Nombre del cliente {IdCliente} actualizado en los pedidos", evento.IdCliente);
                        }
                        else if (evento.EventType == ClienteEventTypes.Eliminado)
                        {
                            await pedidoService.EliminarPedidosPorClienteAsync(evento.IdCliente);
                            logger.LogInformation("Pedidos del cliente {IdCliente} eliminados", evento.IdCliente);
                        }
                    }

                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error al procesar el evento de cliente recibido desde RabbitMQ");
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: ClienteEventContract.QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null)
                await _channel.CloseAsync();

            if (_connection is not null)
                await _connection.CloseAsync();

            await base.StopAsync(cancellationToken);
        }
    }
}
