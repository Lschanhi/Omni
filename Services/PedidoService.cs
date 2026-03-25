using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models.Dtos.Pedidos;
using Omnimarket.Api.Models.Entidades;
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

        // Monta e persiste um novo pedido usando o usuario autenticado como dono da compra.
        public async Task<Pedido> CriarPedido(int usuarioId, PedidoDto dto)
        {
            // Antes de criar o pedido, garante que o usuario existe no banco.
            var usuarioExiste = await _context.TBL_USUARIO
                .AnyAsync(u => u.Id == usuarioId);

            if (!usuarioExiste)
                throw new Exception("Usuario nao encontrado.");

            if (dto.Itens == null || dto.Itens.Count == 0)
                throw new Exception("Pedido deve conter pelo menos 1 item.");

            var pedido = new Pedido
            {
                UsuarioId = usuarioId,
                TipoEntregaId = dto.TipoEntrgaId,
                Observacao = dto.Observacao,
                StatusPedidosId = StatusPedido.Pendente,
                DataPedido = DateTime.UtcNow
            };

            // Cada item do DTO e validado e convertido para a entidade persistida.
            foreach (var item in dto.Itens)
            {
                var produto = await _context.TBL_PRODUTO
                    .FirstOrDefaultAsync(p => p.Id == item.ProdutoId);

                if (produto == null)
                    throw new Exception($"Produto {item.ProdutoId} nao encontrado.");

                if (item.QtdItens <= 0)
                    throw new Exception($"Quantidade invalida para o produto {item.ProdutoId}.");

                var itemPedido = new ItensPedido
                {
                    ProdutoId = produto.Id,
                    Quantidade = item.QtdItens,
                    PrecoUnitario = produto.Preco,
                    ValorTotal = item.QtdItens * produto.Preco
                };

                pedido.Itens.Add(itemPedido);
            }

            // Os totais sao calculados no servidor para evitar manipulacao pelo cliente.
            pedido.ValorTotalProdutos = pedido.Itens.Sum(i => i.ValorTotal);
            pedido.ValorFrete = 0;
            pedido.ValorTotalPedido = pedido.ValorTotalProdutos + pedido.ValorFrete;

            await _context.TBL_PEDIDO.AddAsync(pedido);
            await _context.SaveChangesAsync();

            return pedido;
        }

        // So devolve o pedido se ele pertencer ao usuario autenticado.
        public async Task<Pedido?> BuscarPedido(int id, int usuarioId)
        {
            return await _context.TBL_PEDIDO
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId);
        }

        // Recupera o historico de pedidos do usuario.
        public async Task<List<Pedido>> ListarPedidosUsuario(int usuarioId)
        {
            return await _context.TBL_PEDIDO
                .Where(p => p.UsuarioId == usuarioId)
                .Include(p => p.Itens)
                .ToListAsync();
        }

        // Cancela um pedido existente validando dono e status atual.
        public async Task<bool> CancelarPedido(int pedidoId, int usuarioId)
        {
            var pedido = await _context.TBL_PEDIDO
                .FirstOrDefaultAsync(p => p.Id == pedidoId);

            if (pedido == null)
                return false;

            if (pedido.UsuarioId != usuarioId)
                throw new Exception("Voce nao pode cancelar pedidos que nao sao seus.");

            if (pedido.StatusPedidosId == StatusPedido.Cancelado)
                throw new Exception("Este pedido ja esta cancelado.");

            if (pedido.StatusPedidosId == StatusPedido.Entregue)
                throw new Exception("Pedido ja entregue nao pode ser cancelado.");

            pedido.StatusPedidosId = StatusPedido.Cancelado;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
