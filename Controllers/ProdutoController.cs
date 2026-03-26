using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Omnimarket.Api.Models.Entidades;
using Omnimarket.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Models.Dtos.Produtos;
using Omnimarket.Api.Services;


namespace Omnimarket.Api.Controllers
{
   [ApiController]
    [Route("api/produtos")]
    public class ProdutoController : ControllerBase
    {
        private readonly DataContext _context;
        //private readonly UsuarioService _usuarioService;

        public ProdutoController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoLeituraDto>>> GetProduto()
        {
            var produtos = await _context.TBL_PRODUTO
                .Where(p => p.Disponivel)
                .Select(p => new ProdutoLeituraDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Preco = p.Preco,
                    Disponivel = p.Disponivel,
                    Descricao = p.Descricao,
                    QtdProdutos = p.QtdProdutos,
                    MediaAvaliacao = p.MediaAvaliacao,
                    DtCriacao = p.DtCriacao
                })
                .ToListAsync();

                //a linha abaixo serve para saber se o user pode ser clasificado como vendedor
                //bool ehVendedor = await _usuarioService.UsuarioVendedor(usuarioId); //método do service usuario para conferir se tem um produto criado e se está disponivel

            return Ok(produtos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProdutoLeituraDto>> GetProduto(int id)
        {
            var produto = await _context.TBL_PRODUTO
                .Where(p => p.Id == id && p.Disponivel)
                .Select(p => new ProdutoLeituraDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Preco = p.Preco,
                    Disponivel = p.Disponivel,
                    Descricao = p.Descricao,
                    QtdProdutos = p.QtdProdutos,
                    MediaAvaliacao = p.MediaAvaliacao,
                    DtCriacao = p.DtCriacao
                })
                .FirstOrDefaultAsync();

            if (produto is null) return NotFound();
            return Ok(produto);
        }

        [HttpPost]
        public async Task<ActionResult<ProdutoLeituraDto>> CriarProduto([FromBody] ProdutoCriacaoDto dto)
        {
            var produto = new Produto
            {
                Nome = dto.Nome,
                Preco = dto.Preco,
                Descricao = dto.Descricao,
                QtdProdutos = dto.QtdProdutos,

                // Regras do servidor (cliente não controla)
                Disponivel = true,
                DtCriacao = DateTimeOffset.UtcNow
            };

            _context.TBL_PRODUTO.Add(produto);
            await _context.SaveChangesAsync();

            var readDto = new ProdutoLeituraDto
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Preco = produto.Preco,
                Disponivel = produto.Disponivel,
                Descricao = produto.Descricao,
                QtdProdutos = produto.QtdProdutos,
                MediaAvaliacao = produto.MediaAvaliacao,
                DtCriacao = produto.DtCriacao
            };

            return CreatedAtAction(
                actionName: nameof(GetProduto),
                routeValues: new { id = produto.Id },
                value: readDto);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> AtualizarProduto(int id, [FromBody] ProdutoAtualizarDto dto)
        {
            var produto = await _context.TBL_PRODUTO.FindAsync(id);
            if (produto is null) return NotFound();

            produto.Nome =dto.Nome ;
            produto.Preco = dto.Preco;
            produto.Descricao = dto.Descricao;
            produto.QtdProdutos = dto.QtdProdutos;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("desativar/{id:int}")]
        public async Task<IActionResult> DesativarProduto(int id)
        {
            var produto = await _context.TBL_PRODUTO.FindAsync(id);
            if (produto is null) return NotFound();

            produto.Disponivel = false;
            await _context.SaveChangesAsync();

            return Ok("Produto marcado como indisponível para compra.");
        }

        [HttpPut("disponibilizar/{id:int}")]
        public async Task<IActionResult> Disponibilizar(int id)
        {
            var produto = await _context.TBL_PRODUTO.FindAsync(id);
            if (produto is null) return NotFound();

            produto.Disponivel = true;
            await _context.SaveChangesAsync();

            return Ok("Produto reativado.");
        }
    }
}
