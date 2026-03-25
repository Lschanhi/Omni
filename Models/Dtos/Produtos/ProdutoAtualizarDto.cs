using System.ComponentModel.DataAnnotations;
using Omnimarket.Api.Models.Enum;

namespace Omnimarket.Api.Models.Dtos.Produtos
{
    public class ProdutoAtualizarDto
    {
        [Required]
        [StringLength(50)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Categoria { get; set; } = string.Empty;

        [Required]
        [StringLength(40)]
        public string Sku { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Preco deve ser maior que 0.")]
        public decimal Preco { get; set; }

        [StringLength(500)]
        public string? Descricao { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantidade nao pode ser negativa.")]
        public int Estoque { get; set; }

        public StatusProduto StatusPublicacao { get; set; } = StatusProduto.Publicado;
    }
}
