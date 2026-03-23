using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models;
using Omnimarket.Api.Models.Dtos.Usuarios;
using Omnimarket.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly DataContext _context;

        public UsuarioController(DataContext context)
        {
            _context = context;
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

                // 🔎 Validação CPF
                if (!CpfValidador.ValidarCpf(userDto.Cpf))
                    return BadRequest(new { mensagem = "CPF inválido." });

                string cpfLimpo = userDto.Cpf.Replace(".", "").Replace("-", "").Trim();

                if (await CpfExistente(cpfLimpo))
                    return BadRequest(new { mensagem = "CPF já cadastrado." });

                if (await EmailExistente(userDto.Email))
                    return BadRequest(new { mensagem = "Email já cadastrado." });

                // 🔐 Hash de senha
                Criptografia.CriarPasswordHash(userDto.Password, out byte[] hash, out byte[] salt);

                var novoUsuario = new Usuario
                {
                    Cpf = cpfLimpo,
                    Nome = userDto.Nome.Trim(),
                    Sobrenome = userDto.Sobrenome.Trim(),
                    Email = userDto.Email.ToLower().Trim(),
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    DataCadastro = DateTime.UtcNow,
                    // ✅ TERMOS
                    AceitouTermos = userDto.AceitouTermos,
                    DataAceiteTermos = DateTime.UtcNow
                };

                // 📞 Telefones
                for (int i = 0; i < userDto.Telefones.Count; i++)
                {
                    var t = userDto.Telefones[i];

                    var r = ValidadorTelefone.ValidarCelularBr(t.Ddd, t.Numero);
                    if (!r.Valido)
                        return BadRequest(new { mensagem = $"Telefone inválido (item {i + 1})" });

                    novoUsuario.Telefones.Add(new Telefone
                    {
                        NumeroE164 = r.E164!,
                        IsPrincipal = t.IsPrincipal ?? (i == 0)
                    });
                }

                // 🏠 Endereços (opcional mas recomendado)
                if (userDto.Enderecos != null && userDto.Enderecos.Count > 0)
                {
                    for (int i = 0; i < userDto.Enderecos.Count; i++)
                    {
                        var e = userDto.Enderecos[i];

                        novoUsuario.Enderecos.Add(new Endereco
                        {
                            TipoLogradouro = e.TipoLogradouro,
                            NomeEndereco = e.NomeEndereco,
                            Numero = e.Numero,
                            Cep = e.Cep,
                            Cidade = e.Cidade,
                            Uf = e.Uf,
                            IsPrincipal = e.IsPrincipal ?? (i == 0)
                        });
                    }
                }

                await _context.TBL_USUARIO.AddAsync(novoUsuario);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensagem = "Usuário registrado com sucesso!",
                    usuario = new
                    {
                        id = novoUsuario.Id,
                        nome = $"{novoUsuario.Nome} {novoUsuario.Sobrenome}",
                        email = novoUsuario.Email
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    mensagem = "Erro ao salvar no banco.",
                    detalhes = dbEx.InnerException?.Message ?? dbEx.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensagem = "Erro interno.",
                    detalhes = ex.Message
                });
            }
        }
    }
}