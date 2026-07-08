namespace PedidosApplication.Messaging
{
    public class RabbitMqOptions
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
