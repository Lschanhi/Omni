using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Omnimarket.Api.Models.Dtos.Usuarios.Login
{
    public class LoginDto
    {
        public string Email { get; set; }
        public string Senha { get; set; }
    }
}