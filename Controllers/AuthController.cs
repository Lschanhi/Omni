using Microsoft.AspNetCore.Mvc;
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

        // 🔐 LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            try
            {
                // 🔎 Validação básica
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var usuario = await _authService.ValidarLogin(login);

                if (usuario == null)
                {
                    return Unauthorized(new
                    {
                        mensagem = "Email ou senha incorretos"
                    });
                }

                var token = _tokenService.GerarToken(
                    usuario.Email,
                    usuario.Id.ToString()
                );

                return Ok(new
                {
                    mensagem = "Login realizado com sucesso",
                    usuario = new
                    {
                        id = usuario.Id,
                        nome = usuario.Nome,
                        sobrenome = usuario.Sobrenome,
                        email = usuario.Email
                    },
                    token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensagem = "Erro interno ao realizar login",
                    detalhes = ex.Message
                });
            }
        }
    }
}