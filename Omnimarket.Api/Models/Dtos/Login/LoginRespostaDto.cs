using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Omnimarket.Api.Models.Dtos.Login
{
    public class LoginRespostaDto
    {
        public string Token { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
    }
}