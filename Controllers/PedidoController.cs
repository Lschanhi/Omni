using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Omnimarket.Api.Services;
using Omnimarket.Api.Models.Dtos;
using Omnimarket.Api.Models.Dtos.Pedidos;

namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PedidoController : ControllerBase
    {
        private readonly PedidoService _pedidoService;

        public PedidoController(PedidoService pedidoService)
        {
            _pedidoService = pedidoService;
        }

        [HttpPost]
        public async Task<IActionResult> CriarPedido([FromBody] PedidoDto dto)
        {
            var pedido = await _pedidoService.CriarPedido(dto);
            return Ok(pedido);
        }

        //método par buscar um pedido especifico
        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPedido(int id)
        {
            var pedido = await _pedidoService.BuscarPedido(id);

            if (pedido == null)
                return NotFound();
            
            return Ok(pedido);
        }

        //lista todos os pedidos de um usuario
        [HttpGet("pedidos/{usuarioId}")]
        public async Task<IActionResult> ListarPedidoUsuario(int usuarioId)
        {
            var pedidos = await _pedidoService.ListarPedidosUsuario(usuarioId);
            return Ok(pedidos);
        }

        //cancelar um pedido
        [HttpPut("{id}/cancelar")]
        public async Task<IActionResult> CancelarPedido(int id, int usuarioId)
        {
            try
            {
                var cancelado = await _pedidoService.CancelarPedido(id, usuarioId);

                if(!cancelado)
                    return NotFound();

                return Ok("Pedido cancelado com sucesso");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}