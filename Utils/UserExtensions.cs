using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Omnimarket.Api.Utils
{
    public static class UserExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst("id");

            if (claim == null)
                throw new UnauthorizedAccessException("Usuário não autenticado.");

            return int.Parse(claim.Value);
        }
    }
}