using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Omnimarket.Api.Models.Enum;

namespace Omnimarket.Api.Models.Entidades
{
    public class Pedido
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }  //apenas o id do usuario pois n tem diferenciação de comprador nem vendedor
        public int TipoEntregaId { get; set; }
        public StatusPedido StatusPedidosId { get; set; }    //tipos de status de um pedido em Id(entregue, a caminho, cancelado, etc)
        public decimal ValorTotalProdutos { get; set; }     //valor total dos produtos
        public decimal ValorFrete { get; set; }
        public decimal ValorTotalPedido { get; set; }   //valor dos produtos mais o frete
        public DateTime DataPedido { get; set; } = DateTime.Now;   //Data feita do pedido
        public string Observacao { get; set; } = string.Empty;
        public List<ItensPedido> Itens { get; set; } = new();
    }
}