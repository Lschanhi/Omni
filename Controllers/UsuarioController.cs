using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models;
using Omnimarket.Api.Models.Dtos.Usuarios;
using Omnimarket.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Omni.Models.Dtos.Usuarios;

namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly RegistrarService _registrarService;
        public UsuarioController(DataContext context, RegistrarService registrarService)
        {
            _context = context;
            _registrarService = registrarService;
        }





        // 🔒 Pegar ID do usuário logado (JWT)
        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        }

        private async Task<bool> EmailExistente(string email) =>
            await _context.TBL_USUARIO.AnyAsync(x => x.Email.ToLower() == email.ToLower());

        private async Task<bool> CpfExistente(string cpf) =>
            await _context.TBL_USUARIO.AnyAsync(x => x.Cpf == cpf);

        // 🔐 PERFIL DO USUÁRIO LOGADO
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetUserId();

            var usuario = await _context.TBL_USUARIO
                .Include(u => u.Telefones)
                .Include(u => u.Enderecos)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (usuario is null) return NotFound();

            return Ok(new
            {
                usuario.Id,
                usuario.Cpf,
                usuario.Nome,
                usuario.Sobrenome,
                usuario.Email,
                Telefones = usuario.Telefones.Select(t => new { t.Id, t.NumeroE164, t.IsPrincipal }),
                Enderecos = usuario.Enderecos.Select(e => new { e.Id, e.TipoLogradouro, e.NomeEndereco, e.Numero, e.Cep, e.Cidade, e.Uf, e.IsPrincipal })
            });
        }

        // 🧾 REGISTRO
        [HttpPost("registrar")]
        public async Task<IActionResult> RegistrarUsuario([FromBody] UsuarioRegistroComContatoDto userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var usuario = await _registrarService.RegistrarUsuario(userDto);

                return Ok(new
                {
                    mensagem = "Usuário registrado com sucesso!",
                    usuario = new
                    {
                        id = usuario.Id,
                        nome = $"{usuario.Nome} {usuario.Sobrenome}",
                        email = usuario.Email
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }
        

        [HttpPut("{id:int}")]
        public async Task<IActionResult> AtualizarUsuario(int id, [FromBody] UsuarioAtualizarDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = await _context.TBL_USUARIO.FindAsync(id);

            if (usuario == null)
                return NotFound(new { mensagem = "Usuário não encontrado." });

            // 🔎 verificar email duplicado (se mudou)
            if (usuario.Email != dto.Email.ToLower())
            {
                var emailExiste = await _context.TBL_USUARIO
                    .AnyAsync(x => x.Email == dto.Email.ToLower());

                if (emailExiste)
                    return BadRequest(new { mensagem = "Email já está em uso." });
            }

            // ✏️ atualizar dados
            usuario.Nome = dto.Nome.Trim();
            usuario.Sobrenome = dto.Sobrenome.Trim();
            usuario.Email = dto.Email.ToLower().Trim();

            // 🔐 atualizar senha (se enviada)
            if (!string.IsNullOrEmpty(dto.Password))
            {
                Criptografia.CriarPasswordHash(dto.Password, out byte[] hash, out byte[] salt);
                usuario.PasswordHash = hash;
                usuario.PasswordSalt = salt;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Usuário atualizado com sucesso!",
                usuario = new
                {
                    usuario.Id,
                    usuario.Nome,
                    usuario.Sobrenome,
                    usuario.Email
                }
            });
        }
    }
}