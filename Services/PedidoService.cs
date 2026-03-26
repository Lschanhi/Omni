using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Models;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models.Entidades;
using Omnimarket.Api.Models.Dtos.Pedidos;
using Omnimarket.Api.Models.Enum;

namespace Omnimarket.Api.Services
{
    public class PedidoService
    {
        private readonly DataContext _context;

        public PedidoService(DataContext context)
        {
            _context = context;
        }

        public async Task<Pedido> CriarPedido(PedidoDto dto)
        {
            var pedido = new Pedido
            {
                UsuarioId = dto.UsuarioId,
                TipoEntregaId = dto.TipoEntrgaId,
                Observacao = dto.Observacao,
                StatusPedidosId = StatusPedido.Pendente
            };

            await _context.TBL_PEDIDO.AddAsync(pedido);  
            await _context.SaveChangesAsync();  

            foreach (var item in dto.Itens)
            {
                //procura no banco o id informado pelo user para retornar o produto ou null e evitar criar um pedido com produtos inexistntes
                var produto = await _context.TBL_PRODUTO.FirstOrDefaultAsync(p => p.Id == item.ProdutoId);
  
                if (produto == null)
                    throw new Exception($"Produto {item.ProdutoId} não encontrado.");


                var itemPedido = new ItensPedido
                {
                    ProdutoId = item.ProdutoId,
                    QtdItens = item.QtdItens,
                    ValorUnitario = produto.Preco,
                    ValorSubtotal = item.QtdItens * produto.Preco
                };

                //impede que o cliente add 0 produtos no pedido
                if(item.QtdItens <= 0)
                    throw new Exception($"Impossível comprar {item.QtdItens} desse produto.");
        
                pedido.Itens.Add(itemPedido);
            }

            pedido.ValorTotalProdutos = pedido.Itens.Sum(x => x.ValorSubtotal);
            pedido.ValorFrete = 0;  //valor fixo do frete (terá q mudar para quando calcular a distancia)
            pedido.ValorTotalPedido = pedido.ValorTotalProdutos + pedido.ValorFrete;
            pedido.StatusPedidosId = StatusPedido.Pendente; //formas de status que um pedido pode ter em formato de enum

            _context.TBL_PEDIDO.Add(pedido);

            await _context.SaveChangesAsync();

            return pedido;
        }

        public async Task<Pedido?> BuscarPedido(int id)
        {
            return await _context.TBL_PEDIDO
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Pedido>> ListarPedidosUsuario(int usuarioId)
        {
            return await _context.TBL_PEDIDO.Where(p => p.UsuarioId == usuarioId).Include(p => p.Itens).ToListAsync();
        }

        //regras para o endpoit de cancelar um pedido (sem o risco de cancelar os outros)
        public async Task<bool> CancelarPedido(int pedidoId, int usuarioId)
        {
            var pedido = await _context.TBL_PEDIDO.FirstOrDefaultAsync(p => p.Id == pedidoId);

            if(pedido == null)
                return false;

            if(pedido.UsuarioId != usuarioId)
                throw new Exception("Você não pode cancelar pedidos que não são seus.");

            if(pedido.StatusPedidosId == StatusPedido.Cancelado)
                throw new Exception("Este pedido já está cancelado.");

            pedido.StatusPedidosId = StatusPedido.Cancelado;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}