using PedidosApplication.Dto.Request;
using PedidosData.Models;

namespace PedidosApplication.Services
{
    public interface IPedidoService
    {
        Task<Pedido> CreateAsync(PedidoRequest pedidoInsert);
        Task<bool> DeleteAsync(int id);
        Task<Pedido?> GetAsync(int id);
        Task<IEnumerable<Pedido>> GetAllAsync();
        Task<Pedido> UpdateAsync(int id, PedidoRequest pedidoUpdate);
        Task ActualizarNombreClienteAsync(int idCliente, string nombreCliente);
        Task EliminarPedidosPorClienteAsync(int idCliente);
    }
}