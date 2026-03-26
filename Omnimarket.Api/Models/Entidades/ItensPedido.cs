using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Omnimarket.Api.Models.Entidades
{
    public class ItensPedido
    {
        [Key]
        public int Id { get; set; }

        // 🔗 Pedido
        [Required]
        public int PedidoId { get; set; }

        [ForeignKey("PedidoId")]
        public Pedido Pedido { get; set; } = null!;

        // 🔗 Produto
        [Required(ErrorMessage = "Produto é obrigatório.")]
        public int ProdutoId { get; set; }

        [ForeignKey("ProdutoId")]
        public Produto Produto { get; set; } = null!;

        // 🔢 Quantidade
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantidade deve ser maior que 0.")]
        public int Quantidade { get; set; }

        // 💰 Preço unitário no momento da compra
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Preço inválido.")]
        public decimal PrecoUnitario { get; set; }

        // 💰 Total do item
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal ValorTotal { get; set; }
    }
}