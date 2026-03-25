using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Omnimarket.Api.Models.Dtos.Pedidos;
using Omnimarket.Api.Services;
using Omnimarket.Api.Utils;

namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/pedidos")]
    public class PedidoController : ControllerBase
    {
        private readonly PedidoService _pedidoService;

        public PedidoController(PedidoService pedidoService)
        {
            _pedidoService = pedidoService;
        }

        // Cria um pedido para o usuario autenticado a partir dos itens enviados no body.
        [HttpPost]
        public async Task<IActionResult> CriarPedido([FromBody] PedidoDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var usuarioId = User.GetUserId();
                var pedido = await _pedidoService.CriarPedido(usuarioId, dto);

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

        // Busca um pedido especifico, mas somente se ele pertencer ao usuario logado.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> BuscarPedido(int id)
        {
            var usuarioId = User.GetUserId();
            var pedido = await _pedidoService.BuscarPedido(id, usuarioId);

            if (pedido == null)
                return NotFound(new { mensagem = "Pedido nao encontrado." });

            return Ok(pedido);
        }

        // Lista os pedidos do proprio usuario e bloqueia a consulta de terceiros.
        [HttpGet("usuario/{usuarioId:int}")]
        public async Task<IActionResult> ListarPedidoUsuario(int usuarioId)
        {
            var usuarioIdLogado = User.GetUserId();

            if (usuarioId != usuarioIdLogado)
                return Forbid();

            var pedidos = await _pedidoService.ListarPedidosUsuario(usuarioIdLogado);

            return Ok(pedidos);
        }

        // Cancela um pedido do usuario logado respeitando as regras do servico.
        [HttpPut("{id:int}/cancelar")]
        public async Task<IActionResult> CancelarPedido(int id)
        {
            try
            {
                var usuarioId = User.GetUserId();
                var cancelado = await _pedidoService.CancelarPedido(id, usuarioId);

                if (!cancelado)
                    return NotFound(new { mensagem = "Pedido nao encontrado." });

                return Ok(new { mensagem = "Pedido cancelado com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }
    }
}
