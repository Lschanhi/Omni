using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Models;

namespace Omnimarket.Api.Models.Entidades
{
    [Index(nameof(UsuarioId), IsUnique = true)]
    [Index(nameof(Slug), IsUnique = true)]
    public class Loja
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public Usuario Usuario { get; set; } = null!;

        [Required]
        [StringLength(120)]
        public string NomeFantasia { get; set; } = string.Empty;

        [Required]
        [StringLength(160)]
        public string Slug { get; set; } = string.Empty;

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

        public DateTimeOffset DtCriacao { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? DtAtualizacao { get; set; }
    }
}
