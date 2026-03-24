using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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


        public async Task<Usuario> RegistrarUsuario(UsuarioRegistroComContatoDto userDto)
        {
            // 🔎 Validação CPF
            if (!CpfValidador.ValidarCpf(userDto.Cpf))
                throw new Exception("CPF inválido.");

            string cpfLimpo = userDto.Cpf.Replace(".", "").Replace("-", "").Trim();

            var existe = await _context.TBL_USUARIO
                .AnyAsync(u => u.Cpf == cpfLimpo || u.Email == userDto.Email);

            if (existe)
                throw new Exception("CPF ou Email já cadastrado.");

            // 🔐 Hash
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

            // 📞 Telefones
            if (userDto.Telefones == null || !userDto.Telefones.Any())
                throw new Exception("Pelo menos um telefone é obrigatório.");

            for (int i = 0; i < userDto.Telefones.Count; i++)
            {
                var t = userDto.Telefones[i];

                var r = ValidadorTelefone.ValidarCelularBr(t.Ddd, t.Numero);
                if (!r.Valido)
                    throw new Exception($"Telefone inválido (item {i + 1})");

                novoUsuario.Telefones.Add(new Telefone
                {
                    NumeroE164 = r.E164!,
                    IsPrincipal = t.IsPrincipal ?? (i == 0)
                });
            }

            // 🏠 Endereços
            if (userDto.Enderecos != null && userDto.Enderecos.Any())
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

            return novoUsuario;
        }


        //1- buscando o email de um usuario e jogando na variavel(caso não ache, retorna null)
        //2- cria a variavél e chama o método de verificação para codificar a senha
        //3- se for diferente retorna null
        // 🔐 LOGIN
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
