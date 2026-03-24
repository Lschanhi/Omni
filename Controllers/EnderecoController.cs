using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models;
using Omnimarket.Api.Models.Enum;
using Omnimarket.Api.Utils;
using Omnimarket.Api.Models.Dtos.Enderecos;
using Omnimarket.Api.Models.Dtos.Telefones;


namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("api/usuarios/{usuarioId:int}/enderecos")]
    [Authorize]
    public class EnderecosController : ControllerBase
    {
        private readonly DataContext _context;

        public EnderecosController(DataContext context)
        {
            _context = context;
        }

        // 🔐 Pegar ID do usuário logado
        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }

        // 📋 LISTAR
        [HttpGet]
        public async Task<IActionResult> Listar(int usuarioId)
        {
            if (usuarioId != GetUserId())
                return Forbid();

            var enderecos = await _context.TBL_ENDERECO
                .Where(e => e.UsuarioId == usuarioId)
                .Select(e => new
                {
                    e.Id,
                    TipoLogradouro = EnumExtensions.GetDisplayName(e.TipoLogradouro),
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

        // 🔍 OBTER POR ID
        [HttpGet("{enderecoId:int}")]
        public async Task<IActionResult> Obter(int usuarioId, int enderecoId)
        {
            if (usuarioId != GetUserId())
                return Forbid();

            var endereco = await _context.TBL_ENDERECO
                .Where(e => e.UsuarioId == usuarioId && e.Id == enderecoId)
                .Select(e => new
                {
                    e.Id,
                    TipoLogradouro = EnumExtensions.GetDisplayName(e.TipoLogradouro),
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

        // ➕ CRIAR
        [HttpPost]
        public async Task<IActionResult> Criar( [FromBody] UsuarioEnderecoDto dto)
        {
            var usuarioId = int.Parse(User.FindFirst("id")!.Value);

            if (usuarioId != GetUserId())
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuarioExiste = await _context.TBL_USUARIO.AnyAsync(u => u.Id == usuarioId);
            if (!usuarioExiste)
                return NotFound(new { mensagem = "Usuário não encontrado." });

            // ⭐ Garantir apenas 1 principal
            if (dto.IsPrincipal == true)
            {
                var enderecos = _context.TBL_ENDERECO
                    .Where(e => e.UsuarioId == usuarioId);

                foreach (var e in enderecos)
                    e.IsPrincipal = false;
            }

            var endereco = new Endereco
            {
                UsuarioId = usuarioId,
                Cep = dto.Cep.Replace("-", "").Trim(),
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

            return CreatedAtAction(nameof(Obter),
                new { usuarioId, enderecoId = endereco.Id },
                new { endereco.Id });
        }

        // 📚 ENUM TIPOS LOGRADOURO
        [HttpGet("tipos-logradouro")]
        public IActionResult GetTiposLogradouro()
        {
            var itens = Enum.GetValues<TiposLogradouroBR>()
                .Select(v => new
                {
                    codigo = v.ToString(),
                    descricao = EnumExtensions.GetDisplayName(v)
                })
                .ToList();

            return Ok(itens);
        }
    }
}