using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Omnimarket.Api.Models.Dtos.Pedidos.ItemPedido
{
    public class ItemPedidoDto
    {
        public int ProdutoId { get; set; }
        public int QtdItens { get; set; }
    }
}