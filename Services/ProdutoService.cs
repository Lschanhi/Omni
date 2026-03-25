using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models.Dtos.Produtos;
using Omnimarket.Api.Models.Entidades;
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

        // Converte todas as entidades para DTOs de leitura antes de devolver ao controller.
        public async Task<IEnumerable<ProdutoLeituraDto>> GetAllAsync()
        {
            var produtos = await _context.TBL_PRODUTO.ToListAsync();
            return produtos.Select(MapToDto);
        }

        // Busca um produto por id e retorna null quando ele nao existe.
        public async Task<ProdutoLeituraDto?> GetByIdAsync(int id)
        {
            var produto = await _context.TBL_PRODUTO.FindAsync(id);
            return produto == null ? null : MapToDto(produto);
        }

        // Cria um novo produto e o associa ao usuario autenticado.
        public async Task<ProdutoLeituraDto> CreateAsync(ProdutoCriacaoDto dto, int usuarioId)
        {
            if (await _context.TBL_PRODUTO.AnyAsync(p => p.Nome == dto.Nome))
                throw new Exception("Ja existe um produto com esse nome.");

            var produto = new Produto
            {
                Nome = dto.Nome.Trim(),
                Preco = dto.Preco,
                Estoque = dto.Estoque,
                Descricao = dto.Descricao,
                UsuarioId = usuarioId,
                DtCriacao = DateTimeOffset.UtcNow
            };

            _context.TBL_PRODUTO.Add(produto);
            await _context.SaveChangesAsync();

            return MapToDto(produto);
        }

        // Atualiza somente produtos do proprio usuario para evitar edicao indevida.
        public async Task<bool> UpdateAsync(int id, ProdutoAtualizarDto dto, int usuarioId)
        {
            var produto = await _context.TBL_PRODUTO.FindAsync(id);

            if (produto == null)
                return false;

            if (produto.UsuarioId != usuarioId)
                throw new UnauthorizedAccessException("Voce nao pode editar este produto.");

            if (await _context.TBL_PRODUTO.AnyAsync(p => p.Nome == dto.Nome && p.Id != id))
                throw new Exception("Ja existe outro produto com esse nome.");

            produto.Nome = dto.Nome.Trim();
            produto.Preco = dto.Preco;
            produto.Estoque = dto.Estoque;
            produto.Descricao = dto.Descricao;
            produto.DtAtualizacao = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // Exclui um produto apenas quando o usuario autenticado e o dono do registro.
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

        // Monta uma consulta dinamica com filtros e pagina os resultados.
        public async Task<PageResult<ProdutoLeituraDto>> GetPagedAsync(ProdutoFiltroDto filtro)
        {
            var query = _context.TBL_PRODUTO.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.Nome))
            {
                query = query.Where(p => EF.Functions.Like(p.Nome, $"%{filtro.Nome}%"));
            }

            if (filtro.MinPreco.HasValue)
            {
                query = query.Where(p => p.Preco >= filtro.MinPreco.Value);
            }

            if (filtro.MaxPreco.HasValue)
            {
                query = query.Where(p => p.Preco <= filtro.MaxPreco.Value);
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

        // Mantem a transformacao entidade -> DTO em um unico lugar para evitar repeticao.
        private static ProdutoLeituraDto MapToDto(Produto produto)
        {
            return new ProdutoLeituraDto
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Preco = produto.Preco,
                Estoque = produto.Estoque,
                Disponivel = produto.Disponivel,
                Descricao = produto.Descricao,
                MediaAvaliacao = produto.MediaAvaliacao,
                TotalAvaliacoes = produto.TotalAvaliacoes,
                DtCriacao = produto.DtCriacao,
                DtAtualizacao = produto.DtAtualizacao
            };
        }
    }
}
