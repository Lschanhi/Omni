using Microsoft.AspNetCore.Mvc;
using Omnimarket.Api.Services;
using Omnimarket.Api.Models.Dtos.Pedidos;

namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("api/pedidos")]
    public class PedidoController : ControllerBase
    {
        private readonly PedidoService _pedidoService;

        public PedidoController(PedidoService pedidoService)
        {
            _pedidoService = pedidoService;
        }

        // 🧾 CRIAR PEDIDO
        [HttpPost]
        public async Task<IActionResult> CriarPedido([FromBody] PedidoDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var pedido = await _pedidoService.CriarPedido(dto);

                return Ok(new
                {
                    mensagem = "Pedido criado com sucesso!",
                    pedidoId = pedido.Id,
                    valorTotal = pedido.ValorTotalPedido
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        // 🔍 BUSCAR PEDIDO POR ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> BuscarPedido(int id)
        {
            var pedido = await _pedidoService.BuscarPedido(id);

            if (pedido == null)
                return NotFound(new { mensagem = "Pedido não encontrado." });

            return Ok(pedido);
        }

        // 📦 LISTAR PEDIDOS DE UM USUÁRIO
        [HttpGet("usuario/{usuarioId:int}")]
        public async Task<IActionResult> ListarPedidoUsuario(int usuarioId)
        {
            var pedidos = await _pedidoService.ListarPedidosUsuario(usuarioId);

            return Ok(pedidos);
        }

        // ❌ CANCELAR PEDIDO
        [HttpPut("{id:int}/cancelar")]
        public async Task<IActionResult> CancelarPedido(
            int id,
            [FromQuery] int usuarioId // 🔥 vem pela URL ?usuarioId=1
        )
        {
            try
            {
                var cancelado = await _pedidoService.CancelarPedido(id, usuarioId);

                if (!cancelado)
                    return NotFound(new { mensagem = "Pedido não encontrado." });

                return Ok(new { mensagem = "Pedido cancelado com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }
    }
}