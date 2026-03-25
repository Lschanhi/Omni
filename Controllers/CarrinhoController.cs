using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omni.Models.Dtos.Carrinho;
using Omni.Models.Entidades;
using Omnimarket.Api.Data;
using Omnimarket.Api.Utils;

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

        // Retorna o carrinho do usuario logado com os produtos ja carregados.
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObterCarrinho()
        {
            var usuarioId = User.GetUserId();

            // Include/ThenInclude traz itens e produto em uma unica consulta.
            var carrinho = await _context.TBL_CARRINHO
                .Include(c => c.Itens)
                .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (carrinho == null)
                return Ok(new { itens = new List<object>() });

            return Ok(carrinho);
        }

        // Adiciona um item ao carrinho atual ou soma a quantidade se ele ja existir.
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AdicionarItem([FromBody] CarrinhoAdicionarDto dto)
        {
            var usuarioId = User.GetUserId();

            // Confere se o produto informado existe.
            var produto = await _context.TBL_PRODUTO
                .FirstOrDefaultAsync(p => p.Id == dto.ProdutoId);

            if (produto == null)
                return NotFound(new { mensagem = "Produto nao encontrado." });

            // Regra de negocio: o vendedor nao pode comprar o proprio produto.
            if (produto.UsuarioId == usuarioId)
                return BadRequest(new { mensagem = "Voce nao pode comprar seu proprio produto." });

            // Cada usuario possui um carrinho; se ainda nao existir, ele e criado.
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
                // Se o item ja existe no carrinho, apenas soma a quantidade.
                item.Quantidade += dto.Quantidade;
            }
            else
            {
                // Caso contrario, cria uma nova linha no carrinho.
                carrinho.Itens.Add(new ItemCarrinho
                {
                    ProdutoId = dto.ProdutoId,
                    Quantidade = dto.Quantidade
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Item adicionado ao carrinho" });
        }

        // Remove um item especifico do carrinho do usuario logado.
        [HttpDelete("{produtoId:int}")]
        [Authorize]
        public async Task<IActionResult> RemoverItem(int produtoId)
        {
            var usuarioId = User.GetUserId();

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
