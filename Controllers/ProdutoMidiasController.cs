using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Omnimarket.Api.Models.Entidades;
using Omnimarket.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Models.Dtos.Produtos;
using Omnimarket.Api.Models.Dtos.Produtos.Midias;
using Microsoft.AspNetCore.Http.HttpResults;
using Omnimarket.Api.Utils;

namespace Omnimarket.Api.Controllers
{
    public class ProdutoMidiasController : ControllerBase
    {
        private readonly DataContext _context;

        public ProdutoMidiasController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("{id:int}/midias")]
        public async Task<IActionResult> UploadMidias(
        int id,
        [FromForm] List<IFormFile> arquivos)
        {
            if (arquivos is null || arquivos.Count == 0)
                return BadRequest("Envie ao menos 1 arquivo.");

            foreach (var arq in arquivos)
            {
                if (arq.Length == 0) continue;

                // Exemplo: salvar em disco (só para testar)
                var pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", id.ToString());
                Directory.CreateDirectory(pasta);

                var nomeSeguro = Path.GetFileName(arq.FileName); // não confie cegamente no FileName em produção [web:42]
                var caminho = Path.Combine(pasta, nomeSeguro);

                using var stream = System.IO.File.Create(caminho);
                await arq.CopyToAsync(stream); // IFormFile fornece CopyToAsync [web:42]
            }

            return Ok("Arquivos recebidos.");
        }
    }
}