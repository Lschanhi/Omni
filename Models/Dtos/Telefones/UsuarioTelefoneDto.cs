using System.ComponentModel.DataAnnotations;

namespace Omnimarket.Api.Models.Dtos.Telefones
{
    public class UsuarioTelefoneDto
    {
        [Required(ErrorMessage = "DDD é obrigatório.")]
        [RegularExpression(@"^\d{2}$", ErrorMessage = "DDD deve conter 2 dígitos.")]
        public string Ddd { get; set; } = string.Empty;

        [Required(ErrorMessage = "Número é obrigatório.")]
        [RegularExpression(@"^\d{8,9}$", ErrorMessage = "Número deve conter 8 ou 9 dígitos.")]
        public string Numero { get; set; } = string.Empty;

        [StringLength(30)]
        public string? Tipo { get; set; } // ex: "Celular", "Residencial"

        public bool? IsPrincipal { get; set; }
    }
}