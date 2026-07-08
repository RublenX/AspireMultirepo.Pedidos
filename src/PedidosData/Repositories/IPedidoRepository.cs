using PedidosData.Models;

namespace PedidosData.Repositories
{
    public interface IPedidoRepository
    {
        Task<IEnumerable<Pedido>> GetAllAsync();
        Task<Pedido?> GetAsync(int id);
        Task<IEnumerable<Pedido>> GetByClienteAsync(int idCliente);
        Task AddAsync(Pedido pedido);
        Task UpdateAsync(Pedido pedido);
        Task DeleteAsync(int id);
    }
}
