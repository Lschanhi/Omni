using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models;
using Omnimarket.Api.Models.Dtos.Login;
using Omnimarket.Api.Models.Dtos.Usuarios;
using Omnimarket.Api.Utils;

namespace Omnimarket.Api.Services
{
    public class AuthService
    {
        private readonly DataContext _context;
        private readonly TokenService _tokenService;

        public AuthService(DataContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // Registra um novo usuario com telefone e endereco inicial.
        public async Task<Usuario> RegistrarUsuario(UsuarioRegistroComContatoDto userDto)
        {
            if (!CpfValidador.ValidarCpf(userDto.Cpf))
                throw new Exception("CPF invalido.");

            var cpfLimpo = userDto.Cpf.Replace(".", "").Replace("-", "").Trim();

            var existe = await _context.TBL_USUARIO
                .AnyAsync(u => u.Cpf == cpfLimpo || u.Email == userDto.Email);

            if (existe)
                throw new Exception("CPF ou Email ja cadastrado.");

            // A senha nunca e salva em texto puro; apenas hash e salt vao para o banco.
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
                AceitouTermos = userDto.AceitouTermos,
                DataAceiteTermos = DateTime.UtcNow
            };

            // Pelo menos um telefone deve existir no cadastro inicial.
            if (userDto.Telefones == null || !userDto.Telefones.Any())
                throw new Exception("Pelo menos um telefone e obrigatorio.");

            for (int i = 0; i < userDto.Telefones.Count; i++)
            {
                var telefoneDto = userDto.Telefones[i];
                var resultado = ValidadorTelefone.ValidarCelularBr(telefoneDto.Ddd, telefoneDto.Numero);

                if (!resultado.Valido)
                    throw new Exception($"Telefone invalido (item {i + 1})");

                novoUsuario.Telefones.Add(new Telefone
                {
                    NumeroE164 = resultado.E164!,
                    IsPrincipal = telefoneDto.IsPrincipal ?? (i == 0)
                });
            }

            // Enderecos sao opcionais no registro, mas se vierem ja entram vinculados ao usuario.
            if (userDto.Enderecos != null && userDto.Enderecos.Any())
            {
                for (int i = 0; i < userDto.Enderecos.Count; i++)
                {
                    var enderecoDto = userDto.Enderecos[i];

                    novoUsuario.Enderecos.Add(new Endereco
                    {
                        TipoLogradouro = enderecoDto.TipoLogradouro,
                        NomeEndereco = enderecoDto.NomeEndereco,
                        Numero = enderecoDto.Numero,
                        Cep = enderecoDto.Cep,
                        Cidade = enderecoDto.Cidade,
                        Uf = enderecoDto.Uf,
                        IsPrincipal = enderecoDto.IsPrincipal ?? (i == 0)
                    });
                }
            }

            await _context.TBL_USUARIO.AddAsync(novoUsuario);
            await _context.SaveChangesAsync();

            return novoUsuario;
        }

        // Valida as credenciais e, se estiverem corretas, gera o token JWT.
        public async Task<LoginRespostaDto?> Login(LoginDto login)
        {
            var email = login.Email.Trim().ToLower();

            var usuario = await _context.TBL_USUARIO
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (usuario == null ||
                !Criptografia.VerificarPasswordHash(login.Password, usuario.PasswordHash, usuario.PasswordSalt))
                return null;

            var token = _tokenService.GerarToken(usuario);

            return new LoginRespostaDto
            {
                Token = token,
                Nome = $"{usuario.Nome} {usuario.Sobrenome}",
                Email = usuario.Email
            };
        }
    }
}
