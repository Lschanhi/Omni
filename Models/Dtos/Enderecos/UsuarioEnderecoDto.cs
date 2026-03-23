using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Omnimarket.Api.Models.Enum;

namespace Omnimarket.Api.Models.Dtos.Enderecos
{
    public class UsuarioEnderecoDto
    {
        [Required(ErrorMessage = "Número do CEP é obrigatório.")]
        [StringLength(10)]
        public string Cep { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Tipo de logradouro é obrigatório.")]
        public TiposLogradouroBR TipoLogradouro { get; set; }

        [Required(ErrorMessage = "Nome do Endereço é obrigatório.")]
        [StringLength(200)]
        public string NomeEndereco { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Numero { get; set; } = string.Empty;

        [StringLength(80)]
        public string? Complemento { get; set; }

        [Required, StringLength(120)]
        public string Cidade { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string Uf { get; set; } = string.Empty;

        public bool? IsPrincipal { get; set; }
    }
}