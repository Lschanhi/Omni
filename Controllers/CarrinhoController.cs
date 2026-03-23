using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omni.Models.Dtos.Carrinho;
using Omni.Models.Entidades;
using Omnimarket.Api.Data;

namespace Omni.Controllers
{
    [ApiController]
    [Route("api/carrinho")]
    public class CarrinhoController : ControllerBase
    {
        private readonly DataContext _context;

        public CarrinhoController(DataContext context)
        {
            _context = context;
        }

        // 🛒 VER CARRINHO
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObterCarrinho()
        {
            var usuarioId = int.Parse(User.FindFirst("id")!.Value);

            var carrinho = await _context.TBL_CARRINHO
                .Include(c => c.Itens)
                .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (carrinho == null)
                return Ok(new { itens = new List<object>() });

            return Ok(carrinho);
        }

        // ➕ ADICIONAR ITEM
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AdicionarItem([FromBody] CarrinhoAdicionarDto dto)
        {
            var usuarioId = int.Parse(User.FindFirst("id")!.Value);

            // 🔥 BUSCAR PRODUTO
            var produto = await _context.TBL_PRODUTO
                .FirstOrDefaultAsync(p => p.Id == dto.ProdutoId);

            if (produto == null)
                return NotFound(new { mensagem = "Produto não encontrado." });

            // 🚫 REGRA: NÃO PODE COMPRAR O PRÓPRIO PRODUTO
            if (produto.UsuarioId == usuarioId)
                return BadRequest(new { mensagem = "Você não pode comprar seu próprio produto." });

            // 🛒 BUSCAR OU CRIAR CARRINHO
            var carrinho = await _context.TBL_CARRINHO
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (carrinho == null)
            {
                carrinho = new Carrinho
                {
                    UsuarioId = usuarioId
                };

                await _context.TBL_CARRINHO.AddAsync(carrinho);
            }

            var item = carrinho.Itens.FirstOrDefault(i => i.ProdutoId == dto.ProdutoId);

            if (item != null)
            {
                item.Quantidade += dto.Quantidade;
            }
            else
            {
                carrinho.Itens.Add(new ItemCarrinho
                {
                    ProdutoId = dto.ProdutoId,
                    Quantidade = dto.Quantidade
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Item adicionado ao carrinho" });
        }
        // ❌ REMOVER ITEM
        [HttpDelete("{produtoId:int}")]
        [Authorize]
        public async Task<IActionResult> RemoverItem(int produtoId)
        {
            var usuarioId = int.Parse(User.FindFirst("id")!.Value);

            var carrinho = await _context.TBL_CARRINHO
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (carrinho == null)
                return NotFound();

            var item = carrinho.Itens.FirstOrDefault(i => i.ProdutoId == produtoId);

            if (item == null)
                return NotFound();

            carrinho.Itens.Remove(item);

            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Item removido" });
        }
    }
}