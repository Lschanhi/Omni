using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Omnimarket.Api.Models.Enum;

namespace Omnimarket.Api.Models
{
    public class Endereco
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        public Usuario Usuario { get; set; } = null!;

        [Required, StringLength(200)]
        public TiposLogradouroBR TipoLogradouro { get; set; }// ex: "AV", "R", "AL"
        public string NomeEndereco { get; set; } = string.Empty; // ex: "Paulista"

        [Required, StringLength(20)]
        public string Numero { get; set; } = string.Empty;

        [StringLength(80)]
        public string? Complemento { get; set; }

        [Required, StringLength(10)]
        public string Cep { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string Cidade { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string Uf { get; set; } = string.Empty;

        public bool IsPrincipal { get; set; } = false;
    }
}