using Microsoft.EntityFrameworkCore;
using PedidosData.Context;
using PedidosData.Models;

namespace PedidosData.Repositories
{
    public class PedidoRepository(AppDbContext db) : IPedidoRepository
    {
        private readonly AppDbContext _db = db;

        public async Task<IEnumerable<Pedido>> GetAllAsync() => await _db.Pedidos.ToListAsync();

        public async Task<Pedido?> GetAsync(int id) => await _db.Pedidos.FindAsync(id);

        public async Task<IEnumerable<Pedido>> GetByClienteAsync(int idCliente) =>
            await _db.Pedidos.Where(p => p.IdCliente == idCliente).ToListAsync();

        public async Task AddAsync(Pedido pedido)
        {
            _db.Pedidos.Add(pedido);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Pedido pedido)
        {
            _db.Entry(pedido).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var p = await _db.Pedidos.FindAsync(id);
            if (p == null) return;
            _db.Pedidos.Remove(p);
            await _db.SaveChangesAsync();
        }
    }
}
