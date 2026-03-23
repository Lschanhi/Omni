using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Omnimarket.Api.Models.Entidades
{
    public class Produto
{
    public int Id { get; set; }

    [Required]
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; }

    [Required, StringLength(50)]
    public string Nome { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Preco { get; set; }

    [Range(0, int.MaxValue)]
    public int Estoque { get; set; }

    public bool Disponivel => Estoque > 0;

    [StringLength(100)]
    public string? Descricao { get; set; }

    public double MediaAvaliacao { get; set; }
    public int TotalAvaliacoes { get; set; }

    public DateTimeOffset DtCriacao { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DtAtualizacao { get; set; }

    public ICollection<ProdutoMidia> Midias { get; set; } = new List<ProdutoMidia>();

    // Futuro ideal
    // public ICollection<Avaliacao> Avaliacoes { get; set; }
    // public ICollection<Comentario> Comentarios { get; set; }
}
}