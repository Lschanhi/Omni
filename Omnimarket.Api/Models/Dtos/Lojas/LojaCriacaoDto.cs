using System.ComponentModel.DataAnnotations;

namespace Omnimarket.Api.Models.Dtos.Lojas
{
    public class LojaCriacaoDto
    {
        [Required]
        [StringLength(120, MinimumLength = 3)]
        public string NomeFantasia { get; set; } = string.Empty;

        [StringLength(160)]
        public string? Slug { get; set; }

        [StringLength(500)]
        public string? Descricao { get; set; }

        [EmailAddress]
        [StringLength(120)]
        public string? EmailContato { get; set; }

        [StringLength(20)]
        public string? TelefoneContato { get; set; }

        [StringLength(80)]
        public string? Cidade { get; set; }

        [StringLength(2, MinimumLength = 2)]
        public string? Uf { get; set; }

        public bool Ativa { get; set; } = true;
    }
}
