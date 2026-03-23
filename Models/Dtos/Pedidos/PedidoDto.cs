using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Omnimarket.Api.Models.Entidades;
using Omnimarket.Api.Models.Dtos.Pedidos.ItemPedido; 

namespace Omnimarket.Api.Models.Dtos.Pedidos
{
    public class PedidoDto
    {
        public int UsuarioId { get; set; }
        public int TipoEntrgaId { get; set; }
        public string Observacao { get; set; } = string.Empty;

        public List<ItemPedidoDto> Itens { get; set; } = new();
    }
}