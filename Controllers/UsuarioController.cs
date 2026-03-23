using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models;
using Omnimarket.Api.Services;
using Omnimarket.Api.Utils;
using Omnimarket.Api.Models.Enum;
using Omnimarket.Api.Models.Dtos.Usuarios;



namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ICpfService _cpfService;

        /*public UsuarioController(DataContext context, ICpfService cpfService)
        {
            _context = context;
            _cpfService = cpfService;
        }*/

        public UsuarioController(DataContext context)
        {
            _context = context;
        }

        /*public UsuarioController(DataContext context, UsuarioService usuarioService)
        {
            _context = context;
            _usuarioService = usuarioService;
        }*/

        private async Task<bool> EmailExistente(string email) =>
            await _context.TBL_USUARIO.AnyAsync(x => x.Email.ToLower() == email.ToLower());

        private async Task<bool> CpfExistente(string cpf)
        {
            string cpfLimpo = cpf.Replace(".", "").Replace("-", "").Trim();
            return await _context.TBL_USUARIO.AnyAsync(x => x.Cpf == cpfLimpo);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var usuario = await _context.TBL_USUARIO
                .Include(u => u.Telefones)
                .Include(u => u.Enderecos)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario is null) return NotFound();

            return Ok(new
            {
                usuario.Id,
                usuario.Cpf,
                usuario.Nome,
                usuario.Sobrenome,
                usuario.Email,
                Telefones = usuario.Telefones.Select(t => new { t.Id, numeroE164 = t.NumeroE164, t.IsPrincipal }),
                Enderecos = usuario.Enderecos.Select(e => new { e.Id, e.TipoLogradouro, e.NomeEndereco, e.Numero, e.Cep, e.Cidade, e.Uf, e.IsPrincipal })
            });
        }


        [HttpPost("Registrar")]
        public async Task<IActionResult> RegistrarUsuario([FromBody] UsuarioRegistroComContatoDto userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var erros = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new { mensagem = "Dados inválidos", erros });
                }

                if (userDto.Telefones is null || userDto.Telefones.Count < 1)
                    return BadRequest(new { mensagem = "Informe pelo menos 1 telefone." });

                if (!CpfValidador.ValidarCpf(userDto.Cpf))
                    return BadRequest(new { mensagem = "CPF inválido." });
             /*
                var dadosReceita = await _cpfService.ConsultarCpf(userDto.Cpf);
                if (!dadosReceita.Sucesso)
                    return BadRequest("Não foi possível validar o CPF na Receita Federal. Tente novamente.");

                // (opcional) anti-fraude que você fez
                string nomeCompletoUsuario = $"{userDto.Nome} {userDto.Sobrenome}";
                if (!StringExtensions.NomeCompativel(nomeCompletoUsuario, dadosReceita.Nome))
                {
                    return BadRequest(new
                    {
                        mensagem = "Divergência de titularidade.",
                        detalhes = $"O nome informado não confere com o CPF. O nome registrado na Receita Federal começa com: {EsconderNome(dadosReceita.Nome)}"
                    });
                }
            */
                

                // Normalização
                string cpfLimpo = userDto.Cpf.Replace(".", "").Replace("-", "").Trim();

                if (await CpfExistente(cpfLimpo))
                    return BadRequest(new { mensagem = "CPF já cadastrado." });

                if (await EmailExistente(userDto.Email))
                    return BadRequest(new { mensagem = "Email já cadastrado." });

                Criptografia.CriarPasswordHash(userDto.Password, out byte[] hash, out byte[] salt);

                var novoUsuario = new Usuario
                {
                
                    Cpf = cpfLimpo,
                    Nome = userDto.Nome.Trim(),
                    Sobrenome = userDto.Sobrenome.Trim(),
                    Email = userDto.Email.ToLower().Trim(),
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    DataCadastro = DateTime.Now,
                    DataAcesso = null
                };

                // Telefones
                for (int i = 0; i < userDto.Telefones.Count; i++)
                {
                        var t = userDto.Telefones[i];

                        var r = ValidadorTelefone.ValidarCelularBr(t.Ddd, t.Numero);
                        if (!r.Valido)
                            return BadRequest(new { mensagem = $"Telefone inválido (apenas celular BR). Item {i + 1}." });

                        novoUsuario.Telefones.Add(new Telefone
                        {
                            
                            NumeroE164 = r.E164!,               // salva E164
                            IsPrincipal = t.IsPrincipal ?? (i == 0)
                        });
                }


                await _context.TBL_USUARIO.AddAsync(novoUsuario);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensagem = "Usuário registrado com sucesso!",
                    usuario = new
                    {
                        id = novoUsuario.Id,
                        cpf = novoUsuario.Cpf,
                        email = novoUsuario.Email,
                        nomeCompleto = $"{novoUsuario.Nome} {novoUsuario.Sobrenome}"
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    mensagem = "Erro ao salvar no banco de dados.",
                    detalhes = dbEx.InnerException?.Message ?? dbEx.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensagem = "Erro interno ao registrar usuário.",
                    detalhes = ex.Message
                });
            }
        }

        private string EsconderNome(string nome)
        {
            if (string.IsNullOrEmpty(nome) || nome.Length < 3) return "***";
            return nome.Substring(0, 3) + "***";
        }


    }
}
