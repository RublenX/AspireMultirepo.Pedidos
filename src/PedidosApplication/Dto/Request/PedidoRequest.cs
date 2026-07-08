namespace PedidosApplication.Dto.Request
{
    public class PedidoRequest
    {
        public int IdCliente { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
    }
}
