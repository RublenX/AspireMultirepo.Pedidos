using PedidosApplication.Dto.Request;
using PedidosData.Models;
using PedidosData.Repositories;
using System.Text.Json;

namespace PedidosApplication.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly HttpClient _clientesClient;
        private readonly IPedidoRepository _pedidoRepository;

        public PedidoService(IHttpClientFactory factory, IPedidoRepository pedidoRepository)
        {
            _clientesClient = factory.CreateClient("Clientes");
            _pedidoRepository = pedidoRepository;
        }

        public async Task<IEnumerable<Pedido>> GetAllAsync()
        {
            var pedidos = await _pedidoRepository.GetAllAsync();
            return pedidos;
        }

        public async Task<Pedido?> GetAsync(int id)
        {
            var pedido = await _pedidoRepository.GetAsync(id);
            if (pedido == null) return null;
            return pedido;
        }

        public async Task<Pedido> CreateAsync(PedidoRequest pedidoInsert)
        {
            // Recupera el nombre del cliente desde la API de clientes
            string? nombreCliente = await GetNombreCliente(pedidoInsert.IdCliente);

            // Si no obtiene el nombre del cliente, levanta excepción
            if (nombreCliente == null)
            {
                throw new ArgumentException($"No se pudo obtener el nombre del cliente con Id {pedidoInsert.IdCliente}");
            }

            // Crea un nuevo objeto Pedido con los datos del DTO y el nombre del cliente
            var pedido = new Pedido
            {
                IdCliente = pedidoInsert.IdCliente,
                Cliente = nombreCliente,
                Fecha = pedidoInsert.Fecha,
                Total = pedidoInsert.Total
            };
            await _pedidoRepository.AddAsync(pedido);
            return pedido;
        }

        public async Task<Pedido> UpdateAsync(int id, PedidoRequest pedidoUpdate)
        {
            var pedido = await _pedidoRepository.GetAsync(id);

            // Si no encuentra el pedido, levanta excepción
            if (pedido == null)
            {
                throw new ArgumentException($"No se encontró el pedido con Id {id}");
            }

            // Comprueba si el Cliente ha cambiado y obtiene el nombre del cliente actualizado si es necesario
            if (pedido.IdCliente != pedidoUpdate.IdCliente)
            {
                string? nombreCliente = await GetNombreCliente(pedidoUpdate.IdCliente);
                if (nombreCliente == null)
                {
                    throw new ArgumentException($"No se pudo obtener el nombre del cliente con Id {pedidoUpdate.IdCliente}");
                }
                pedido.IdCliente = pedidoUpdate.IdCliente;
                pedido.Cliente = nombreCliente;
            }

            pedido.Fecha = pedidoUpdate.Fecha;
            pedido.Total = pedidoUpdate.Total;

            await _pedidoRepository.UpdateAsync(pedido);
            return pedido;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var pedido = await _pedidoRepository.GetAsync(id);
            if (pedido == null) return false;
            await _pedidoRepository.DeleteAsync(pedido.Id);
            return true;
        }

        // Actualiza el nombre de cliente almacenado en todos los pedidos del cliente indicado.
        // Se invoca al recibir el evento ClienteActualizado desde el bus de mensajería.
        public async Task ActualizarNombreClienteAsync(int idCliente, string nombreCliente)
        {
            var pedidos = await _pedidoRepository.GetByClienteAsync(idCliente);
            foreach (var pedido in pedidos)
            {
                pedido.Cliente = nombreCliente;
                await _pedidoRepository.UpdateAsync(pedido);
            }
        }

        // Elimina todos los pedidos del cliente indicado.
        // Se invoca al recibir el evento ClienteEliminado desde el bus de mensajería.
        public async Task EliminarPedidosPorClienteAsync(int idCliente)
        {
            var pedidos = await _pedidoRepository.GetByClienteAsync(idCliente);
            foreach (var pedido in pedidos)
            {
                await _pedidoRepository.DeleteAsync(pedido.Id);
            }
        }

        private async Task<string?> GetNombreCliente(int idCliente)
        {
            var response = await _clientesClient.GetAsync($"/api/cliente/{idCliente}");
            response.EnsureSuccessStatusCode();

            // Recupera el parámetro Nombre del cliente desde la respuesta de la API
            using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);

            string? nombre = document.RootElement
                .GetProperty("nombre")
                .GetString();

            return nombre;
        }
    }
}
