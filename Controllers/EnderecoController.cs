using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models;
using Omnimarket.Api.Models.Enum;
using Omnimarket.Api.Utils;
using Omnimarket.Api.Models.Dtos.Enderecos;

namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("api/usuarios/{usuarioId:int}/enderecos")]
    public class EnderecosController : ControllerBase
    {
        private readonly DataContext _context;

        public EnderecosController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Listar(int usuarioId)
        {
            var enderecos = await _context.TBL_ENDERECO
                .Where(e => e.UsuarioId == usuarioId)
                .Select(e => new
                {
                    e.Id,
                    e.TipoLogradouro,
                    e.NomeEndereco,
                    e.Numero,
                    e.Complemento,
                    e.Cep,
                    e.Cidade,
                    e.Uf,
                    e.IsPrincipal
                })
                .ToListAsync();

            return Ok(enderecos);
        }

        [HttpGet("{enderecoId:int}")]
        public async Task<IActionResult> Obter(int usuarioId, int enderecoId)
        {
            var endereco = await _context.TBL_ENDERECO
                .Where(e => e.UsuarioId == usuarioId && e.Id == enderecoId)
                .Select(e => new
                {
                    e.Id,
                    e.TipoLogradouro,
                    e.NomeEndereco,
                    e.Numero,
                    e.Complemento,
                    e.Cep,
                    e.Cidade,
                    e.Uf,
                    e.IsPrincipal
                })
                .FirstOrDefaultAsync();

            return endereco is null ? NotFound() : Ok(endereco);
        }

        [HttpPost]
        public async Task<IActionResult> Criar(int usuarioId, [FromBody] UsuarioEnderecoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuarioExiste = await _context.TBL_USUARIO.AnyAsync(u => u.Id == usuarioId);
            if (!usuarioExiste) return NotFound(new { mensagem = "Usuário não encontrado." });

            var endereco = new Endereco
            {
                UsuarioId = usuarioId,
                Cep = dto.Cep.Trim(),
                TipoLogradouro = dto.TipoLogradouro,
                NomeEndereco = dto.NomeEndereco.Trim(),
                Numero = dto.Numero.Trim(),
                Complemento = dto.Complemento?.Trim(),
                Cidade = dto.Cidade.Trim(),
                Uf = dto.Uf.Trim(),
                IsPrincipal = dto.IsPrincipal ?? false
            };

            await _context.TBL_ENDERECO.AddAsync(endereco);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Obter), new { usuarioId, enderecoId = endereco.Id }, new { endereco.Id });
        }

        [HttpPut("{enderecoId:int}")]
        public async Task<IActionResult> Atualizar(int usuarioId, int enderecoId, [FromBody] UsuarioEnderecoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var endereco = await _context.TBL_ENDERECO
                .FirstOrDefaultAsync(e => e.UsuarioId == usuarioId && e.Id == enderecoId);

            if (endereco is null) return NotFound();

            endereco.Cep = dto.Cep.Trim();
            endereco.TipoLogradouro = dto.TipoLogradouro;
            endereco.NomeEndereco = dto.NomeEndereco.Trim();
            endereco.Numero = dto.Numero.Trim();
            endereco.Complemento = dto.Complemento?.Trim();
            endereco.Cidade = dto.Cidade.Trim();
            endereco.Uf = dto.Uf.Trim();
            endereco.IsPrincipal = dto.IsPrincipal ?? endereco.IsPrincipal;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{enderecoId:int}")]
        public async Task<IActionResult> Remover(int usuarioId, int enderecoId)
        {
            var endereco = await _context.TBL_ENDERECO
                .FirstOrDefaultAsync(e => e.UsuarioId == usuarioId && e.Id == enderecoId);

            if (endereco is null) return NotFound();

            _context.TBL_ENDERECO.Remove(endereco);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("tipos-logradouro")]
        public IActionResult GetTiposLogradouro()
        {
             var itens = Enum.GetValues<TiposLogradouroBR>()
            .Select(v => new
            {
                codigo = v.ToString(),
                descricao = v.GetDisplayName()
            })
            .ToList();

        return Ok(itens);
        }
    }
}
