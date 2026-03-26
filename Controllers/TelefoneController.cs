using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models;
using Omnimarket.Api.Models.Dtos.Telefones;
using Omnimarket.Api.Utils;

namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("api/usuarios/{usuarioId:int}/telefones")]
    public class TelefonesController : ControllerBase
    {
        private readonly DataContext _context;

        public TelefonesController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Listar(int usuarioId)
        {
            var telefones = await _context.TBL_TELEFONE
                .Where(t => t.UsuarioId == usuarioId)
                .Select(t => new { t.Id, numeroE164 = t.NumeroE164, t.IsPrincipal })

                .ToListAsync();

            return Ok(telefones);
        }

        [HttpGet("{telefoneId:int}")]
        public async Task<IActionResult> Obter(int usuarioId, int telefoneId)
        {
            var telefone = await _context.TBL_TELEFONE
                .Where(t => t.UsuarioId == usuarioId && t.Id == telefoneId)
                .Select(t => new { t.Id, numeroE164 = t.NumeroE164, t.IsPrincipal })

                .FirstOrDefaultAsync();

            return telefone is null ? NotFound() : Ok(telefone);
        }

        [HttpPost]
        public async Task<IActionResult> Criar(int usuarioId, [FromBody] UsuarioTelefoneDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuarioExiste = await _context.TBL_USUARIO.AnyAsync(u => u.Id == usuarioId);
            if (!usuarioExiste) return NotFound(new { mensagem = "Usuário não encontrado." });

            var r = ValidadorTelefone.ValidarCelularBr(dto.Ddd, dto.Numero);
            if (!r.Valido) return BadRequest(new { mensagem = "Telefone inválido (apenas celular BR)." });

            var telefone = new Telefone
            {
            
                UsuarioId = usuarioId,
                NumeroE164 = r.E164!,                 // AQUI salva em E164
                IsPrincipal = dto.IsPrincipal ?? false
            };
            await _context.TBL_TELEFONE.AddAsync(telefone);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Obter),
            new { usuarioId, telefoneId = telefone.Id },
            new { telefone.Id, telefone.NumeroE164, telefone.IsPrincipal });
        }

        [HttpPut("{telefoneId:int}")]
        public async Task<IActionResult> Atualizar(int usuarioId, int telefoneId, [FromBody] UsuarioTelefoneDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var telefone = await _context.TBL_TELEFONE
                .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId && t.Id == telefoneId);

           if (telefone is null) return NotFound();

            var r = ValidadorTelefone.ValidarCelularBr(dto.Ddd, dto.Numero);
            if (!r.Valido) return BadRequest(new { mensagem = "Telefone inválido (apenas celular BR)." });

            telefone.NumeroE164 = r.E164!;           // AQUI atualiza em E164
            telefone.IsPrincipal = dto.IsPrincipal ?? telefone.IsPrincipal;

                    await _context.SaveChangesAsync();
                    return NoContent();
        }

        [HttpDelete("{telefoneId:int}")]
        public async Task<IActionResult> Remover(int usuarioId, int telefoneId)
        {
            var telefone = await _context.TBL_TELEFONE
                .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId && t.Id == telefoneId);

            if (telefone is null) return NotFound();

            // Regra: não pode ficar com 0 telefones
            var totalTelefones = await _context.TBL_TELEFONE.CountAsync(t => t.UsuarioId == usuarioId);
            if (totalTelefones <= 1)
                return BadRequest(new { mensagem = "Não é possível remover o último telefone do usuário." });

            _context.TBL_TELEFONE.Remove(telefone);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Opcional: marcar como principal (facilita pro front)
        [HttpPatch("{telefoneId:int}/principal")]
        public async Task<IActionResult> DefinirPrincipal(int usuarioId, int telefoneId)
        {
            var telefone = await _context.TBL_TELEFONE
                .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId && t.Id == telefoneId);

            if (telefone is null) return NotFound();

            // Desmarca todos e marca este
            var telefones = await _context.TBL_TELEFONE.Where(t => t.UsuarioId == usuarioId).ToListAsync();
            foreach (var t in telefones) t.IsPrincipal = (t.Id == telefoneId);

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
