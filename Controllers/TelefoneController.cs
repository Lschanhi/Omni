using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models;
using Omnimarket.Api.Models.Dtos.Telefones;
using Omnimarket.Api.Utils;

namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("api/telefones")]
    public class TelefonesController : ControllerBase
    {
        private readonly DataContext _context;

        public TelefonesController(DataContext context)
        {
            _context = context;
        }

        // 📄 LISTAR
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var usuarioId = User.GetUserId();

            var telefones = await _context.TBL_TELEFONE
                .Where(t => t.UsuarioId == usuarioId)
                .Select(t => new
                {
                    t.Id,
                    numero = t.NumeroE164,
                    t.IsPrincipal
                })
                .ToListAsync();

            return Ok(telefones);
        }

        // 🔍 OBTER
        [HttpGet("{telefoneId:int}")]
        public async Task<IActionResult> Obter(int telefoneId)
        {
            var usuarioId = User.GetUserId();

            var telefone = await _context.TBL_TELEFONE
                .Where(t => t.UsuarioId == usuarioId && t.Id == telefoneId)
                .Select(t => new
                {
                    t.Id,
                    numero = t.NumeroE164,
                    t.IsPrincipal
                })
                .FirstOrDefaultAsync();

            return telefone is null
                ? NotFound(new { mensagem = "Telefone não encontrado." })
                : Ok(telefone);
        }

        // ➕ CRIAR
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] UsuarioTelefoneDto dto)
        {
            var usuarioId = User.GetUserId();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var r = ValidadorTelefone.ValidarCelularBr(dto.Ddd, dto.Numero);
            if (!r.Valido)
                return BadRequest(new { mensagem = "Telefone inválido." });

            var telefone = new Telefone
            {
                UsuarioId = usuarioId,
                NumeroE164 = r.E164!,
                IsPrincipal = dto.IsPrincipal ?? false
            };

            await _context.TBL_TELEFONE.AddAsync(telefone);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Telefone cadastrado com sucesso." });
        }

        // 🔄 ATUALIZAR
        [HttpPut("{telefoneId:int}")]
        public async Task<IActionResult> Atualizar(int telefoneId, [FromBody] UsuarioTelefoneDto dto)
        {
            var usuarioId = User.GetUserId();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var telefone = await _context.TBL_TELEFONE
                .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId && t.Id == telefoneId);

            if (telefone is null)
                return NotFound(new { mensagem = "Telefone não encontrado." });

            var r = ValidadorTelefone.ValidarCelularBr(dto.Ddd, dto.Numero);
            if (!r.Valido)
                return BadRequest(new { mensagem = "Telefone inválido." });

            telefone.NumeroE164 = r.E164!;

            // ⭐ Apenas 1 principal
            if (dto.IsPrincipal == true)
            {
                var telefones = await _context.TBL_TELEFONE
                    .Where(t => t.UsuarioId == usuarioId)
                    .ToListAsync();

                foreach (var t in telefones)
                    t.IsPrincipal = false;

                telefone.IsPrincipal = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Telefone atualizado com sucesso." });
        }

        // ❌ REMOVER
        [HttpDelete("{telefoneId:int}")]
        public async Task<IActionResult> Remover(int telefoneId)
        {
            var usuarioId = User.GetUserId();

            var telefone = await _context.TBL_TELEFONE
                .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId && t.Id == telefoneId);

            if (telefone is null)
                return NotFound(new { mensagem = "Telefone não encontrado." });

            var telefones = await _context.TBL_TELEFONE
                .Where(t => t.UsuarioId == usuarioId)
                .ToListAsync();

            if (telefones.Count <= 1)
                return BadRequest(new { mensagem = "Não pode remover o último telefone." });

            if (telefone.IsPrincipal)
            {
                var novoPrincipal = telefones.FirstOrDefault(t => t.Id != telefoneId);
                if (novoPrincipal != null)
                    novoPrincipal.IsPrincipal = true;
            }

            _context.TBL_TELEFONE.Remove(telefone);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Telefone removido com sucesso." });
        }
    }
}