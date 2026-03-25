using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models;
using Omnimarket.Api.Models.Dtos.Pedidos;
using Omnimarket.Api.Models.Entidades;
using Omnimarket.Api.Models.Enum;
using Omnimarket.Api.Utils;

namespace Omnimarket.Api.Services
{
    public class PedidoService
    {
        private readonly DataContext _context;

        public PedidoService(DataContext context)
        {
            _context = context;
        }

        public async Task<Pedido> CriarPedido(int usuarioId, PedidoDto dto)
        {
            var usuarioExiste = await _context.TBL_USUARIO
                .AnyAsync(u => u.Id == usuarioId);

            if (!usuarioExiste)
                throw new Exception("Usuario nao encontrado.");

            if (dto.Itens == null || dto.Itens.Count == 0)
                throw new Exception("Pedido deve conter pelo menos 1 item.");

            var tipoEntregaId = dto.TipoEntregaId > 0 ? dto.TipoEntregaId : dto.TipoEntrgaId;
            if (tipoEntregaId <= 0)
                throw new Exception("Tipo de entrega invalido.");

            var enderecoEntrega = await ResolverEnderecoEntrega(usuarioId, dto.EnderecoId);

            var itensAgrupados = dto.Itens
                .GroupBy(i => i.ProdutoId)
                .Select(g => new
                {
                    ProdutoId = g.Key,
                    Quantidade = g.Sum(x => x.QtdItens)
                })
                .ToList();

            if (itensAgrupados.Any(i => i.Quantidade <= 0))
                throw new Exception("Quantidade invalida em um ou mais itens do pedido.");

            var produtoIds = itensAgrupados
                .Select(i => i.ProdutoId)
                .Distinct()
                .ToList();

            var produtos = await _context.TBL_PRODUTO
                .Where(p => produtoIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            var pedido = new Pedido
            {
                UsuarioId = usuarioId,
                TipoLogradouroEntrega = EnumExtensions.GetDisplayName(enderecoEntrega.TipoLogradouro),
                NomeEnderecoEntrega = enderecoEntrega.NomeEndereco,
                NumeroEntrega = enderecoEntrega.Numero,
                ComplementoEntrega = enderecoEntrega.Complemento,
                CepEntrega = enderecoEntrega.Cep,
                CidadeEntrega = enderecoEntrega.Cidade,
                UfEntrega = enderecoEntrega.Uf,
                TipoEntregaId = tipoEntregaId,
                Observacao = dto.Observacao,
                StatusPedidosId = StatusPedido.Pendente,
                DataPedido = DateTime.UtcNow
            };

            foreach (var item in itensAgrupados)
            {
                if (!produtos.TryGetValue(item.ProdutoId, out var produto))
                    throw new Exception($"Produto {item.ProdutoId} nao encontrado.");

                if (produto.UsuarioId == usuarioId)
                    throw new Exception($"Voce nao pode comprar o proprio produto {item.ProdutoId}.");

                if (produto.StatusPublicacao != StatusProduto.Publicado)
                    throw new Exception($"Produto {item.ProdutoId} nao esta publicado para venda.");

                if (produto.Estoque < item.Quantidade)
                    throw new Exception($"Estoque insuficiente para o produto {item.ProdutoId}.");

                produto.Estoque -= item.Quantidade;

                pedido.Itens.Add(new ItensPedido
                {
                    ProdutoId = produto.Id,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = produto.Preco,
                    ValorTotal = item.Quantidade * produto.Preco
                });
            }

            pedido.ValorTotalProdutos = pedido.Itens.Sum(i => i.ValorTotal);
            pedido.ValorFrete = 0;
            pedido.ValorTotalPedido = pedido.ValorTotalProdutos + pedido.ValorFrete;

            await _context.TBL_PEDIDO.AddAsync(pedido);
            await _context.SaveChangesAsync();

            return pedido;
        }

        public async Task<Pedido?> BuscarPedido(int id, int usuarioId)
        {
            return await _context.TBL_PEDIDO
                .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId);
        }

        public async Task<List<Pedido>> ListarPedidosUsuario(int usuarioId)
        {
            return await _context.TBL_PEDIDO
                .Where(p => p.UsuarioId == usuarioId)
                .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
                .OrderByDescending(p => p.DataPedido)
                .ToListAsync();
        }

        public async Task<bool> CancelarPedido(int pedidoId, int usuarioId)
        {
            var pedido = await _context.TBL_PEDIDO
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);

            if (pedido == null)
                return false;

            if (pedido.UsuarioId != usuarioId)
                throw new Exception("Voce nao pode cancelar pedidos que nao sao seus.");

            if (pedido.StatusPedidosId == StatusPedido.Cancelado)
                throw new Exception("Este pedido ja esta cancelado.");

            if (pedido.StatusPedidosId == StatusPedido.Entregue)
                throw new Exception("Pedido ja entregue nao pode ser cancelado.");

            var produtoIds = pedido.Itens
                .Select(i => i.ProdutoId)
                .Distinct()
                .ToList();

            var produtos = await _context.TBL_PRODUTO
                .Where(p => produtoIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var item in pedido.Itens)
            {
                if (produtos.TryGetValue(item.ProdutoId, out var produto))
                    produto.Estoque += item.Quantidade;
            }

            pedido.StatusPedidosId = StatusPedido.Cancelado;
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<Endereco> ResolverEnderecoEntrega(int usuarioId, int? enderecoId)
        {
            var query = _context.TBL_ENDERECO
                .AsNoTracking()
                .Where(e => e.UsuarioId == usuarioId);

            if (enderecoId.HasValue && enderecoId.Value > 0)
            {
                var enderecoSelecionado = await query.FirstOrDefaultAsync(e => e.Id == enderecoId.Value);

                if (enderecoSelecionado == null)
                    throw new Exception("Endereco de entrega nao encontrado.");

                return enderecoSelecionado;
            }

            var enderecoPadrao = await query
                .OrderByDescending(e => e.IsPrincipal)
                .ThenBy(e => e.Id)
                .FirstOrDefaultAsync();

            if (enderecoPadrao == null)
                throw new Exception("Nenhum endereco de entrega foi encontrado para o usuario.");

            return enderecoPadrao;
        }
    }
}
