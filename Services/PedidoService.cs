using Microsoft.EntityFrameworkCore;
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
            // 🔎 Validar usuário
            var usuarioExiste = await _context.TBL_USUARIO
                .AnyAsync(u => u.Id == dto.UsuarioId);

            if (!usuarioExiste)
                throw new Exception("Usuário não encontrado.");

            if (dto.Itens == null || dto.Itens.Count == 0)
                throw new Exception("Pedido deve conter pelo menos 1 item.");

            var pedido = new Pedido
            {
                UsuarioId = dto.UsuarioId,
                TipoEntregaId = dto.TipoEntrgaId,
                Observacao = dto.Observacao,
                StatusPedidosId = StatusPedido.Pendente,
                DataPedido = DateTime.UtcNow
            };

            foreach (var item in dto.Itens)
            {
                var produto = await _context.TBL_PRODUTO
                    .FirstOrDefaultAsync(p => p.Id == item.ProdutoId);

                if (produto == null)
                    throw new Exception($"Produto {item.ProdutoId} não encontrado.");

                if (item.QtdItens <= 0)
                    throw new Exception($"Quantidade inválida para o produto {item.ProdutoId}.");

                var itemPedido = new ItensPedido
                {
                    ProdutoId = produto.Id,
                    Quantidade = item.QtdItens,
                    PrecoUnitario = produto.Preco,
                    ValorTotal = item.QtdItens * produto.Preco
                };

                pedido.Itens.Add(itemPedido);
            }

            // 💰 Cálculo automático (regra de negócio)
            pedido.ValorTotalProdutos = pedido.Itens.Sum(i => i.ValorTotal);
            pedido.ValorFrete = 0; // pode melhorar depois
            pedido.ValorTotalPedido = pedido.ValorTotalProdutos + pedido.ValorFrete;

            // 💾 Salva UMA vez só
            await _context.TBL_PEDIDO.AddAsync(pedido);
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
            return await _context.TBL_PEDIDO
                .Where(p => p.UsuarioId == usuarioId)
                .Include(p => p.Itens)
                .ToListAsync();
        }

        public async Task<bool> CancelarPedido(int pedidoId, int usuarioId)
        {
            var pedido = await _context.TBL_PEDIDO
                .FirstOrDefaultAsync(p => p.Id == pedidoId);

            if (pedido == null)
                return false;

            if (pedido.UsuarioId != usuarioId)
                throw new Exception("Você não pode cancelar pedidos que não são seus.");

            if (pedido.StatusPedidosId == StatusPedido.Cancelado)
                throw new Exception("Este pedido já está cancelado.");

            if (pedido.StatusPedidosId == StatusPedido.Entregue)
                throw new Exception("Pedido já entregue não pode ser cancelado.");

            pedido.StatusPedidosId = StatusPedido.Cancelado;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}