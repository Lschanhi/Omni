using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models.Dtos.Produtos.Midias;
using Omnimarket.Api.Models.Entidades;
using Omnimarket.Api.Models.Enum;
using Omnimarket.Api.Utils;

namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("api/produtos/{produtoId:int}/midias")]
    public class ProdutoMidiasController : ControllerBase
    {
        private readonly DataContext _context;

        public ProdutoMidiasController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Listar(int produtoId)
        {
            var midias = await _context.ProdutoMidia
                .Where(m => m.ProdutoId == produtoId)
                .OrderBy(m => m.Ordem)
                .Select(m => new ProdutoMidiaLeituraDto
                {
                    Id = m.Id,
                    Tipo = m.Tipo,
                    Url = m.Url,
                    ContentType = m.ContentType,
                    Ordem = m.Ordem
                })
                .ToListAsync();

            return Ok(midias);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadMidias(int produtoId, [FromForm] List<IFormFile> arquivos)
        {
            if (arquivos is null || arquivos.Count == 0)
                return BadRequest("Envie ao menos 1 arquivo.");

            var usuarioId = User.GetUserId();
            var produto = await _context.TBL_PRODUTO
                .Include(p => p.Midias)
                .FirstOrDefaultAsync(p => p.Id == produtoId);

            if (produto == null)
                return NotFound(new { mensagem = "Produto nao encontrado." });

            if (produto.UsuarioId != usuarioId)
                return Forbid();

            var pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", produtoId.ToString());
            Directory.CreateDirectory(pasta);

            var ordemAtual = produto.Midias.Any() ? produto.Midias.Max(m => m.Ordem) + 1 : 0;
            var novasMidias = new List<ProdutoMidia>();

            foreach (var arquivo in arquivos)
            {
                if (arquivo.Length == 0)
                    continue;

                var extensao = Path.GetExtension(arquivo.FileName);
                var nomeSeguro = $"{Guid.NewGuid():N}{extensao}";
                var caminho = Path.Combine(pasta, nomeSeguro);

                using var stream = System.IO.File.Create(caminho);
                await arquivo.CopyToAsync(stream);

                var urlRelativa = $"/uploads/{produtoId}/{nomeSeguro}";
                var tipoMidia = arquivo.ContentType?.StartsWith("video/", StringComparison.OrdinalIgnoreCase) == true
                    ? TipoMidiaProduto.Video
                    : TipoMidiaProduto.Foto;

                novasMidias.Add(new ProdutoMidia
                {
                    ProdutoId = produtoId,
                    Tipo = tipoMidia,
                    Url = urlRelativa,
                    ContentType = arquivo.ContentType,
                    Ordem = ordemAtual++
                });
            }

            if (novasMidias.Count == 0)
                return BadRequest(new { mensagem = "Nenhum arquivo valido foi enviado." });

            await _context.ProdutoMidia.AddRangeAsync(novasMidias);
            await _context.SaveChangesAsync();

            var resposta = novasMidias
                .OrderBy(m => m.Ordem)
                .Select(m => new ProdutoMidiaLeituraDto
                {
                    Id = m.Id,
                    Tipo = m.Tipo,
                    Url = m.Url,
                    ContentType = m.ContentType,
                    Ordem = m.Ordem
                })
                .ToList();

            return Ok(resposta);
        }

        [HttpDelete("{midiaId:int}")]
        [Authorize]
        public async Task<IActionResult> Remover(int produtoId, int midiaId)
        {
            var usuarioId = User.GetUserId();
            var produto = await _context.TBL_PRODUTO
                .FirstOrDefaultAsync(p => p.Id == produtoId);

            if (produto == null)
                return NotFound(new { mensagem = "Produto nao encontrado." });

            if (produto.UsuarioId != usuarioId)
                return Forbid();

            var midia = await _context.ProdutoMidia
                .FirstOrDefaultAsync(m => m.Id == midiaId && m.ProdutoId == produtoId);

            if (midia == null)
                return NotFound(new { mensagem = "Midia nao encontrada." });

            if (midia.Url.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                var caminhoLocal = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    midia.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                if (System.IO.File.Exists(caminhoLocal))
                    System.IO.File.Delete(caminhoLocal);
            }

            _context.ProdutoMidia.Remove(midia);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Midia removida com sucesso." });
        }
    }
}
