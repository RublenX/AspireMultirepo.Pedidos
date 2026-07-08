using Microsoft.EntityFrameworkCore;
using PedidosData.Models;

namespace PedidosData.Context
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Pedido> Pedidos => Set<Pedido>();
    }
}
