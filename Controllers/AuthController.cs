using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Services;
using Omnimarket.Api.Models.Dtos.Login;

namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly TokenService _tokenService;

        public AuthController(AuthService authService, TokenService tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
        }

        //a loginDTO pedirá o email e senha para o user e dps virá para esse método.
        //o método abaixo é para retornar apenas o id, nome, sobrenome, email e o token do user dps do login.
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto login)
        {
            var usuario = await _authService.ValidarLogin(login);

            if (usuario == null)
            {
                return Unauthorized("Email ou senha incorretos!");
            }
            
            var token =  _tokenService.GerarToken(usuario.Email, usuario.Id.ToString());

            return Ok(new 
            {
                id = usuario.Id,
                nome = usuario.Nome,
                sobrenome = usuario.Sobrenome,
                email = usuario.Email,
                token = token
            });
        }
    }
}