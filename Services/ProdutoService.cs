using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Omni.Models.Dtos.Produtos;
using Omnimarket.Api.Controllers;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models.Dtos.Produtos;
using Omnimarket.Api.Models.Entidades;

namespace Omni.Services
{
    public class ProdutoService : IProdutoService
    {
        private readonly DataContext _context;

        public ProdutoService(DataContext context)
        {
            _context = context;
        }

        // 🔹 GET ALL
        public async Task<IEnumerable<ProdutoLeituraDto>> GetAllAsync()
        {
            var produtos = await _context.TBL_PRODUTO.ToListAsync();

            return produtos.Select<Produto, ProdutoLeituraDto>(MapToDto);
        }

        // 🔹 GET BY ID
        public async Task<ProdutoLeituraDto?> GetByIdAsync(int id)
        {
            var produto = await _context.TBL_PRODUTO.FindAsync(id);

            return produto == null ? null : MapToDto(produto);
        }

        // 🔹 CREATE
        public async Task<ProdutoLeituraDto> CreateAsync(ProdutoCriacaoDto dto, int usuarioId)
        {
            if (await _context.TBL_PRODUTO.AnyAsync(p => p.Nome == dto.Nome))
                throw new Exception("Já existe um produto com esse nome.");

            var produto = new Produto
            {
                Nome = dto.Nome.Trim(),
                Preco = dto.Preco,
                Estoque = dto.Estoque, // ajuste se renomear
                Descricao = dto.Descricao,
                UsuarioId = usuarioId,
                DtCriacao = DateTimeOffset.UtcNow
            };

            _context.TBL_PRODUTO.Add(produto);
            await _context.SaveChangesAsync();

            return MapToDto(produto);
        }

        // 🔹 UPDATE
        public async Task<bool> UpdateAsync(int id, ProdutoAtualizarDto dto)
        {
            var produto = await _context.TBL_PRODUTO.FindAsync(id);

            if (produto == null)
                return false;

            if (await _context.TBL_PRODUTO.AnyAsync(p => p.Nome == dto.Nome && p.Id != id))
                throw new Exception("Já existe outro produto com esse nome.");

            produto.Nome = dto.Nome.Trim();
            produto.Preco = dto.Preco;
            produto.Estoque = dto.Estoque; // ajuste se renomear
            produto.Descricao = dto.Descricao;
            produto.DtAtualizacao = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // 🔹 DELETE
        public async Task<bool> DeleteAsync(int id)
        {
            var produto = await _context.TBL_PRODUTO.FindAsync(id);

            if (produto == null)
                return false;

            _context.TBL_PRODUTO.Remove(produto);
            await _context.SaveChangesAsync();

            return true;
        }

        // 🔥 PAGINAÇÃO + FILTRO + BUSCA
        public async Task<PageResult<ProdutoLeituraDto>> GetPagedAsync(ProdutoFiltroDto filtro)
        {
            var query = _context.TBL_PRODUTO.AsQueryable();

            // 🔎 Busca por nome
            if (!string.IsNullOrWhiteSpace(filtro.Nome))
            {
                query = query.Where(p =>
                    EF.Functions.Like(p.Nome, $"%{filtro.Nome}%"));
            }

            // 💰 Filtro preço mínimo
            if (filtro.MinPreco.HasValue)
            {
                query = query.Where(p => p.Preco >= filtro.MinPreco.Value);
            }

            // 💰 Filtro preço máximo
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

            var items = produtos.Select(MapToDto);

            return new PageResult<ProdutoLeituraDto>
            {
                Items = items,
                Total = total,
                Page = filtro.Page,
                PageSize = filtro.PageSize
            };
        }

        // 🔧 MÉTODO PRIVADO (REMOVE REPETIÇÃO)
        private static ProdutoLeituraDto MapToDto(Produto p)
        {
            return new ProdutoLeituraDto
            {
                Id = p.Id,
                Nome = p.Nome,
                Preco = p.Preco,
                Estoque = p.Estoque,
                Disponivel = p.Disponivel,
                Descricao = p.Descricao,
                MediaAvaliacao = p.MediaAvaliacao,
                DtCriacao = p.DtCriacao
            };
        }
    }
}
