using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omni.Models.Dtos.Carrinho;
using Omni.Models.Entidades;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models.Entidades;
using Omnimarket.Api.Models.Enum;
using Omnimarket.Api.Utils;

namespace Omni.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/carrinho")]
    public class CarrinhoController : ControllerBase
    {
        private readonly DataContext _context;

        public CarrinhoController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ObterCarrinho()
        {
            var usuarioId = User.GetUserId();
            var carrinho = await ObterCarrinhoCompleto(usuarioId);

            return Ok(MontarRespostaCarrinho(carrinho));
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarItem([FromBody] CarrinhoAdicionarDto dto)
        {
            if (dto.Quantidade <= 0)
                return BadRequest(new { mensagem = "Quantidade deve ser maior que zero." });

            var usuarioId = User.GetUserId();
            var produto = await _context.TBL_PRODUTO
                .FirstOrDefaultAsync(p => p.Id == dto.ProdutoId);

            if (produto == null)
                return NotFound(new { mensagem = "Produto nao encontrado." });

            var indisponivel = ValidarProdutoDisponivel(produto, usuarioId);
            if (indisponivel != null)
                return BadRequest(new { mensagem = indisponivel });

            var carrinho = await ObterOuCriarCarrinho(usuarioId);
            var item = carrinho.Itens.FirstOrDefault(i => i.ProdutoId == dto.ProdutoId);
            var quantidadeFinal = (item?.Quantidade ?? 0) + dto.Quantidade;

            if (quantidadeFinal > produto.Estoque)
                return BadRequest(new { mensagem = "Quantidade solicitada excede o estoque disponivel." });

            if (item != null)
            {
                item.Quantidade = quantidadeFinal;
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

            var carrinhoAtualizado = await ObterCarrinhoCompleto(usuarioId);
            return Ok(MontarRespostaCarrinho(carrinhoAtualizado));
        }

        [HttpPut("{produtoId:int}")]
        public async Task<IActionResult> AtualizarQuantidade(int produtoId, [FromBody] CarrinhoAtualizarQuantidadeDto dto)
        {
            if (dto.Quantidade <= 0)
                return BadRequest(new { mensagem = "Quantidade deve ser maior que zero." });

            var usuarioId = User.GetUserId();
            var carrinho = await _context.TBL_CARRINHO
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (carrinho == null)
                return NotFound(new { mensagem = "Carrinho nao encontrado." });

            var item = carrinho.Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
            if (item == null)
                return NotFound(new { mensagem = "Item nao encontrado no carrinho." });

            var produto = await _context.TBL_PRODUTO
                .FirstOrDefaultAsync(p => p.Id == produtoId);

            if (produto == null)
                return NotFound(new { mensagem = "Produto nao encontrado." });

            var indisponivel = ValidarProdutoDisponivel(produto, usuarioId);
            if (indisponivel != null)
                return BadRequest(new { mensagem = indisponivel });

            if (dto.Quantidade > produto.Estoque)
                return BadRequest(new { mensagem = "Quantidade solicitada excede o estoque disponivel." });

            item.Quantidade = dto.Quantidade;
            await _context.SaveChangesAsync();

            var carrinhoAtualizado = await ObterCarrinhoCompleto(usuarioId);
            return Ok(MontarRespostaCarrinho(carrinhoAtualizado));
        }

        [HttpDelete("{produtoId:int}")]
        public async Task<IActionResult> RemoverItem(int produtoId)
        {
            var usuarioId = User.GetUserId();
            var carrinho = await _context.TBL_CARRINHO
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (carrinho == null)
                return NotFound(new { mensagem = "Carrinho nao encontrado." });

            var item = carrinho.Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
            if (item == null)
                return NotFound(new { mensagem = "Item nao encontrado no carrinho." });

            carrinho.Itens.Remove(item);
            await _context.SaveChangesAsync();

            var carrinhoAtualizado = await ObterCarrinhoCompleto(usuarioId);
            return Ok(MontarRespostaCarrinho(carrinhoAtualizado));
        }

        [HttpDelete]
        public async Task<IActionResult> LimparCarrinho()
        {
            var usuarioId = User.GetUserId();
            var carrinho = await _context.TBL_CARRINHO
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (carrinho == null || carrinho.Itens.Count == 0)
                return Ok(new { mensagem = "Carrinho ja esta vazio." });

            _context.TBL_ITEM_CARRINHO.RemoveRange(carrinho.Itens);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Carrinho limpo com sucesso.",
                itens = new List<object>(),
                totalItens = 0,
                valorTotal = 0m
            });
        }

        private async Task<Carrinho> ObterOuCriarCarrinho(int usuarioId)
        {
            var carrinho = await _context.TBL_CARRINHO
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (carrinho != null)
                return carrinho;

            carrinho = new Carrinho
            {
                UsuarioId = usuarioId
            };

            await _context.TBL_CARRINHO.AddAsync(carrinho);
            return carrinho;
        }

        private async Task<Carrinho?> ObterCarrinhoCompleto(int usuarioId)
        {
            return await _context.TBL_CARRINHO
                .Include(c => c.Itens)
                .ThenInclude(i => i.Produto)
                .ThenInclude(p => p.Midias)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
        }

        private object MontarRespostaCarrinho(Carrinho? carrinho)
        {
            if (carrinho == null)
            {
                return new
                {
                    itens = new List<object>(),
                    totalItens = 0,
                    valorTotal = 0m
                };
            }

            var itens = carrinho.Itens
                .Select(i => new
                {
                    i.Id,
                    i.ProdutoId,
                    nome = i.Produto.Nome,
                    sku = i.Produto.Sku,
                    categoria = i.Produto.Categoria,
                    quantidade = i.Quantidade,
                    precoUnitario = i.Produto.Preco,
                    subtotal = i.Produto.Preco * i.Quantidade,
                    estoqueDisponivel = i.Produto.Estoque,
                    statusPublicacao = i.Produto.StatusPublicacao,
                    disponivelParaCompra = i.Produto.StatusPublicacao == StatusProduto.Publicado && i.Produto.Estoque >= i.Quantidade,
                    imagemPrincipal = i.Produto.Midias
                        .OrderBy(m => m.Ordem)
                        .Select(m => m.Url)
                        .FirstOrDefault()
                })
                .ToList();

            return new
            {
                carrinhoId = carrinho.Id,
                itens,
                totalItens = itens.Sum(i => i.quantidade),
                valorTotal = itens.Sum(i => i.subtotal)
            };
        }

        private static string? ValidarProdutoDisponivel(Produto produto, int usuarioId)
        {
            if (produto.UsuarioId == usuarioId)
                return "Voce nao pode comprar seu proprio produto.";

            if (produto.StatusPublicacao != StatusProduto.Publicado)
                return "Produto nao esta disponivel para compra.";

            if (produto.Estoque <= 0)
                return "Produto sem estoque no momento.";

            return null;
        }
    }
}
