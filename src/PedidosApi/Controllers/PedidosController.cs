using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidosApplication.Dto.Request;
using PedidosApplication.Services;
using PedidosData.Models;

namespace PedidosApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController(IPedidoService pedidoService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetAll()
        {
            var pedidos = await pedidoService.GetAllAsync();
            return Ok(pedidos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Pedido>> Get(int id)
        {
            var pedido = await pedidoService.GetAsync(id);
            if (pedido == null) return NotFound();
            return Ok(pedido);
        }

        [HttpPost]
        public async Task<ActionResult<Pedido>> Create(PedidoRequest pedidoRequest)
        {
            var pedido = await pedidoService.CreateAsync(pedidoRequest);
            return CreatedAtAction(nameof(Get), new { id = pedido.Id }, pedido);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PedidoRequest pedidoRequest)
        {
            try
            {
                await pedidoService.UpdateAsync(id, pedidoRequest);
            }
            catch (DbUpdateConcurrencyException)
            {
                var pedidoGet = await pedidoService.GetAsync(id);
                if (pedidoGet == null) return NotFound();
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var pedido = await pedidoService.GetAsync(id);
            if (pedido == null) return NotFound();
            await pedidoService.DeleteAsync(pedido.Id);
            return NoContent();
        }
    }
}
