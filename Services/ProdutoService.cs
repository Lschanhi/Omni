using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models.Dtos.Produtos;
using Omnimarket.Api.Models.Entidades;
using Omnimarket.Api.Models.Enum;
using Omnimarket.Api.Services.Interfaces;

namespace Omnimarket.Api.Services
{
    public class ProdutoService : IProdutoService
    {
        private readonly DataContext _context;

        public ProdutoService(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProdutoLeituraDto>> GetAllAsync()
        {
            var produtos = await BaseQuery()
                .OrderBy(p => p.Nome)
                .ToListAsync();

            return produtos.Select(MapToDto);
        }

        public async Task<ProdutoLeituraDto?> GetByIdAsync(int id)
        {
            var produto = await BaseQuery()
                .FirstOrDefaultAsync(p => p.Id == id);

            return produto == null ? null : MapToDto(produto);
        }

        public async Task<ProdutoLeituraDto> CreateAsync(ProdutoCriacaoDto dto, int usuarioId)
        {
            var sku = dto.Sku.Trim().ToUpperInvariant();

            if (await _context.TBL_PRODUTO.AnyAsync(p => p.Sku == sku))
                throw new Exception("Ja existe um produto com esse SKU.");

            var produto = new Produto
            {
                Nome = dto.Nome.Trim(),
                Categoria = dto.Categoria.Trim(),
                Sku = sku,
                Preco = dto.Preco,
                Estoque = dto.Estoque,
                Descricao = dto.Descricao?.Trim(),
                StatusPublicacao = dto.StatusPublicacao,
                UsuarioId = usuarioId,
                DtCriacao = DateTimeOffset.UtcNow
            };

            _context.TBL_PRODUTO.Add(produto);
            await _context.SaveChangesAsync();

            return MapToDto(produto);
        }

        public async Task<bool> UpdateAsync(int id, ProdutoAtualizarDto dto, int usuarioId)
        {
            var produto = await _context.TBL_PRODUTO
                .Include(p => p.Midias)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
                return false;

            if (produto.UsuarioId != usuarioId)
                throw new UnauthorizedAccessException("Voce nao pode editar este produto.");

            var sku = dto.Sku.Trim().ToUpperInvariant();

            if (await _context.TBL_PRODUTO.AnyAsync(p => p.Sku == sku && p.Id != id))
                throw new Exception("Ja existe outro produto com esse SKU.");

            produto.Nome = dto.Nome.Trim();
            produto.Categoria = dto.Categoria.Trim();
            produto.Sku = sku;
            produto.Preco = dto.Preco;
            produto.Estoque = dto.Estoque;
            produto.Descricao = dto.Descricao?.Trim();
            produto.StatusPublicacao = dto.StatusPublicacao;
            produto.DtAtualizacao = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id, int usuarioId)
        {
            var produto = await _context.TBL_PRODUTO.FindAsync(id);

            if (produto == null)
                return false;

            if (produto.UsuarioId != usuarioId)
                throw new UnauthorizedAccessException("Voce nao pode excluir este produto.");

            _context.TBL_PRODUTO.Remove(produto);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<PageResult<ProdutoLeituraDto>> GetPagedAsync(ProdutoFiltroDto filtro)
        {
            var query = BaseQuery();

            if (!string.IsNullOrWhiteSpace(filtro.Nome))
            {
                query = query.Where(p => EF.Functions.Like(p.Nome, $"%{filtro.Nome}%"));
            }

            if (!string.IsNullOrWhiteSpace(filtro.Categoria))
            {
                query = query.Where(p => p.Categoria == filtro.Categoria.Trim());
            }

            if (!string.IsNullOrWhiteSpace(filtro.Sku))
            {
                var sku = filtro.Sku.Trim().ToUpperInvariant();
                query = query.Where(p => p.Sku == sku);
            }

            if (filtro.MinPreco.HasValue)
            {
                query = query.Where(p => p.Preco >= filtro.MinPreco.Value);
            }

            if (filtro.MaxPreco.HasValue)
            {
                query = query.Where(p => p.Preco <= filtro.MaxPreco.Value);
            }

            if (filtro.StatusPublicacao.HasValue)
            {
                query = query.Where(p => p.StatusPublicacao == filtro.StatusPublicacao.Value);
            }

            if (filtro.SomenteDisponiveis)
            {
                query = query.Where(p => p.StatusPublicacao == StatusProduto.Publicado && p.Estoque > 0);
            }

            var total = await query.CountAsync();

            var produtos = await query
                .OrderBy(p => p.Nome)
                .Skip((filtro.Page - 1) * filtro.PageSize)
                .Take(filtro.PageSize)
                .ToListAsync();

            return new PageResult<ProdutoLeituraDto>
            {
                Items = produtos.Select(MapToDto),
                Total = total,
                Page = filtro.Page,
                PageSize = filtro.PageSize
            };
        }

        private IQueryable<Produto> BaseQuery()
        {
            return _context.TBL_PRODUTO
                .Include(p => p.Midias)
                .AsQueryable();
        }

        private static ProdutoLeituraDto MapToDto(Produto produto)
        {
            return new ProdutoLeituraDto
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Categoria = produto.Categoria,
                Sku = produto.Sku,
                Preco = produto.Preco,
                Estoque = produto.Estoque,
                Disponivel = produto.Disponivel,
                StatusPublicacao = produto.StatusPublicacao,
                Descricao = produto.Descricao,
                MediaAvaliacao = produto.MediaAvaliacao,
                TotalAvaliacoes = produto.TotalAvaliacoes,
                DtCriacao = produto.DtCriacao,
                DtAtualizacao = produto.DtAtualizacao,
                Imagens = produto.Midias
                    .OrderBy(m => m.Ordem)
                    .Where(m => m.Tipo == TipoMidiaProduto.Foto)
                    .Select(m => m.Url)
                    .ToList()
            };
        }
    }
}
