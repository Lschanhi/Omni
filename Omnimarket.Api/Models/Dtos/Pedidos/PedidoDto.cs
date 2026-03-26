using Omnimarket.Api.Models.Dtos.Pedidos.ItemPedido;

namespace Omnimarket.Api.Models.Dtos.Pedidos
{
    public class PedidoDto
    {
        // Legado: mantido por compatibilidade, mas o usuario vem do token.
        public int UsuarioId { get; set; }

        public int? EnderecoId { get; set; }

        public int TipoEntregaId { get; set; }

        // Alias antigo mantido para nao quebrar clientes existentes.
        public int TipoEntrgaId { get; set; }

        public string Observacao { get; set; } = string.Empty;

        public List<ItemPedidoDto> Itens { get; set; } = new();
    }
}
