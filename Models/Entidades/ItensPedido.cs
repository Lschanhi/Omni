using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Omnimarket.Api.Models.Entidades
{
    public class ItensPedido
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public int PedidoId { get; set; }
        public int QtdItens { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal ValorSubtotal { get; set; }
        public Pedido Pedido { get; set; }
    }
}