namespace PedidosApplication.Events
{
    // Debe coincidir exactamente con el exchange y las routing keys declarados por el
    // publicador de eventos en el microservicio de Clientes.
    public static class ClienteEventContract
    {
        public const string ExchangeName = "clientes-exchange";
        public const string RoutingKeyPattern = "cliente.*";
        public const string QueueName = "pedidos.cliente-events-queue";
    }

    // Tipos de evento posibles dentro del envoltorio ClienteEventEnvelope.
    public static class ClienteEventTypes
    {
        public const string Actualizado = "ClienteActualizado";
        public const string Eliminado = "ClienteEliminado";
    }

    // Envoltorio deserializado desde el cuerpo JSON del mensaje de RabbitMQ.
    public class ClienteEventEnvelope
    {
        public string EventType { get; set; } = string.Empty;
        public int IdCliente { get; set; }
        public string? Nombre { get; set; }
    }
}
