using Microsoft.AspNetCore.Mvc;
using Omnimarket.Api.Data;

namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("api/produtos")]
    public class ProdutoMidiasController : ControllerBase
    {
        private readonly DataContext _context;

        public ProdutoMidiasController(DataContext context)
        {
            _context = context;
        }

        // Recebe arquivos de midia de um produto e os salva em disco para testes locais.
        [HttpPost("{id:int}/midias")]
        public async Task<IActionResult> UploadMidias(int id, [FromForm] List<IFormFile> arquivos)
        {
            if (arquivos is null || arquivos.Count == 0)
                return BadRequest("Envie ao menos 1 arquivo.");

            // O contexto continua injetado porque esse fluxo pode evoluir para persistir metadados no banco.
            _ = _context;

            foreach (var arquivo in arquivos)
            {
                if (arquivo.Length == 0)
                    continue;

                var pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", id.ToString());
                Directory.CreateDirectory(pasta);

                // Remove qualquer caminho enviado pelo cliente e preserva apenas o nome do arquivo.
                var nomeSeguro = Path.GetFileName(arquivo.FileName);
                var caminho = Path.Combine(pasta, nomeSeguro);

                using var stream = System.IO.File.Create(caminho);
                await arquivo.CopyToAsync(stream);
            }

            return Ok("Arquivos recebidos.");
        }
    }
}
