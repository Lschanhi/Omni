using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models;
using Omnimarket.Api.Models.Dtos.Enderecos;
using Omnimarket.Api.Models.Enum;
using Omnimarket.Api.Utils;

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

        // Lista todos os enderecos do usuario logado.
        [HttpGet]
        public async Task<IActionResult> Listar(int usuarioId)
        {
            if (usuarioId != User.GetUserId())
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

        // Busca um endereco especifico do usuario logado.
        [HttpGet("{enderecoId:int}")]
        public async Task<IActionResult> Obter(int usuarioId, int enderecoId)
        {
            if (usuarioId != User.GetUserId())
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

        // Cria um novo endereco para o usuario autenticado.
        [HttpPost]
        public async Task<IActionResult> Criar(int usuarioId, [FromBody] UsuarioEnderecoDto dto)
        {
            var usuarioIdLogado = User.GetUserId();

            if (usuarioId != usuarioIdLogado)
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuarioExiste = await _context.TBL_USUARIO.AnyAsync(u => u.Id == usuarioIdLogado);
            if (!usuarioExiste)
                return NotFound(new { mensagem = "Usuario nao encontrado." });

            // Se o novo endereco for principal, remove a marcacao dos anteriores.
            if (dto.IsPrincipal == true)
            {
                var enderecos = _context.TBL_ENDERECO
                    .Where(e => e.UsuarioId == usuarioIdLogado);

                foreach (var enderecoExistente in enderecos)
                    enderecoExistente.IsPrincipal = false;
            }

            var endereco = new Endereco
            {
                UsuarioId = usuarioIdLogado,
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
                new { usuarioId = usuarioIdLogado, enderecoId = endereco.Id },
                new { endereco.Id });
        }

        // Lista os valores possiveis do enum de tipos de logradouro para o front-end.
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
