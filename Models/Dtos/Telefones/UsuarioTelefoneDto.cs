using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Omnimarket.Api.Models.Dtos.Telefones
{
    public class UsuarioTelefoneDto
    {
         [Required, StringLength(2, MinimumLength = 2)]
        public string Ddd { get; set; } = string.Empty;

        [Required, StringLength(9, MinimumLength = 8)]
        public string Numero { get; set; } = string.Empty;

        [StringLength(30)]
        public string? Tipo { get; set; }

        public bool? IsPrincipal { get; set; }
    }
}