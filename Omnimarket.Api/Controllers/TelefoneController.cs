using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models;
using Omnimarket.Api.Models.Dtos.Telefones;
using Omnimarket.Api.Utils;

namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/telefones")]
    public class TelefonesController : ControllerBase
    {
        private readonly DataContext _context;

        public TelefonesController(DataContext context)
        {
            _context = context;
        }

        // Lista os telefones vinculados ao usuario logado.
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

        // Busca um telefone especifico pertencente ao usuario autenticado.
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
                ? NotFound(new { mensagem = "Telefone nao encontrado." })
                : Ok(telefone);
        }

        // Cria um novo telefone validando o formato brasileiro antes de salvar.
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] UsuarioTelefoneDto dto)
        {
            var usuarioId = User.GetUserId();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = ValidadorTelefone.ValidarCelularBr(dto.Ddd, dto.Numero);
            if (!resultado.Valido)
                return BadRequest(new { mensagem = "Telefone invalido." });

            var telefone = new Telefone
            {
                UsuarioId = usuarioId,
                NumeroE164 = resultado.E164!,
                IsPrincipal = dto.IsPrincipal ?? false
            };

            await _context.TBL_TELEFONE.AddAsync(telefone);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Telefone cadastrado com sucesso." });
        }

        // Atualiza o telefone e opcionalmente redefine qual e o principal.
        [HttpPut("{telefoneId:int}")]
        public async Task<IActionResult> Atualizar(int telefoneId, [FromBody] UsuarioTelefoneDto dto)
        {
            var usuarioId = User.GetUserId();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var telefone = await _context.TBL_TELEFONE
                .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId && t.Id == telefoneId);

            if (telefone is null)
                return NotFound(new { mensagem = "Telefone nao encontrado." });

            var resultado = ValidadorTelefone.ValidarCelularBr(dto.Ddd, dto.Numero);
            if (!resultado.Valido)
                return BadRequest(new { mensagem = "Telefone invalido." });

            telefone.NumeroE164 = resultado.E164!;

            if (dto.IsPrincipal == true)
            {
                var telefones = await _context.TBL_TELEFONE
                    .Where(t => t.UsuarioId == usuarioId)
                    .ToListAsync();

                foreach (var telefoneExistente in telefones)
                    telefoneExistente.IsPrincipal = false;

                telefone.IsPrincipal = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Telefone atualizado com sucesso." });
        }

        // Remove um telefone preservando pelo menos um telefone cadastrado.
        [HttpDelete("{telefoneId:int}")]
        public async Task<IActionResult> Remover(int telefoneId)
        {
            var usuarioId = User.GetUserId();

            var telefone = await _context.TBL_TELEFONE
                .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId && t.Id == telefoneId);

            if (telefone is null)
                return NotFound(new { mensagem = "Telefone nao encontrado." });

            var telefones = await _context.TBL_TELEFONE
                .Where(t => t.UsuarioId == usuarioId)
                .ToListAsync();

            if (telefones.Count <= 1)
                return BadRequest(new { mensagem = "Nao pode remover o ultimo telefone." });

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
