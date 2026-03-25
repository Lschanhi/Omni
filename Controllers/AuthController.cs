using Microsoft.AspNetCore.Mvc;
using Omnimarket.Api.Models.Dtos.Login;
using Omnimarket.Api.Services;

namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // Recebe email e senha, valida as credenciais e devolve o JWT ao cliente.
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.Login(login);

                if (result == null)
                {
                    return Unauthorized(new
                    {
                        mensagem = "Email ou senha incorretos"
                    });
                }

                return Ok(new
                {
                    mensagem = "Login realizado com sucesso",
                    token = result.Token,
                    usuario = new
                    {
                        nome = result.Nome,
                        email = result.Email
                    }
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensagem = "Erro interno ao realizar login"
                });
            }
        }
    }
}
