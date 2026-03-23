using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Omnimarket.Api.Models.Enum;

namespace Omnimarket.Api.Models.Entidades
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        // 🔗 Usuário (comprador)
        [Required(ErrorMessage = "Usuário é obrigatório.")]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; } = null!;

        // 🚚 Tipo de entrega
        [Required(ErrorMessage = "Tipo de entrega é obrigatório.")]
        public int TipoEntregaId { get; set; }

        // 📦 Status do pedido
        [Required]
        public StatusPedido StatusPedidosId { get; set; }

        // 💰 Valores
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Valor dos produtos inválido.")]
        public decimal ValorTotalProdutos { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Valor do frete inválido.")]
        public decimal ValorFrete { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Valor total inválido.")]
        public decimal ValorTotalPedido { get; set; }

        // 📅 Data
        public DateTime DataPedido { get; set; } = DateTime.UtcNow;

        // 📝 Observação
        [StringLength(500)]
        public string Observacao { get; set; } = string.Empty;

        // 📦 Itens do pedido
        [MinLength(1, ErrorMessage = "O pedido deve ter pelo menos 1 item.")]
        public List<ItensPedido> Itens { get; set; } = new();
    }
}