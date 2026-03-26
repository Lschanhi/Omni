using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Omnimarket.Api.Models.Dtos.Produtos
{
    public class ProdutoLeituraDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public bool Disponivel { get; set; }
        public string? Descricao { get; set; }
        public int QtdProdutos { get; set; }
        public double MediaAvaliacao { get; set; }
        public DateTimeOffset DtCriacao { get; set; }
    }
}