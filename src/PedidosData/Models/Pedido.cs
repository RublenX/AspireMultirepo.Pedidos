namespace PedidosData.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public int IdCliente { get; set; }
        public required string Cliente { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
    }
}
