using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models;
using Omnimarket.Api.Models.Dtos.Login;
using Omnimarket.Api.Utils;

namespace Omnimarket.Api.Services
{
    public class AuthService
    {
        private readonly DataContext _context;

        public AuthService(DataContext context)
        {
            _context = context;
        }

        //1- buscando o email de um usuario e jogando na variavel(caso não ache, retorna null)
        //2- cria a variavél e chama o método de verificação para codificar a senha
        //3- se for diferente retorna null
        public async Task<Usuario?> ValidarLogin(LoginDto login)
        {
            //1
            var usuario = await _context.TBL_USUARIO.FirstOrDefaultAsync(u => u.Email == login.Email);

            if (usuario == null)
            {
                return null;
            }

            //2
            bool senhaValida = Criptografia.VerificarPasswordHash(login.Password, usuario.PasswordHash, usuario.PasswordSalt);

            //3
            if (!senhaValida)
                return null;

            return usuario;
        }
    }
}