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

        public int UsuarioId { get; set; }  //O produto tem q ter o id de seu usuario(seu vendedor)

        [Required, StringLength(50)]
        public string Nome { get; set; } = string.Empty;

        public decimal Preco { get; set; }

        public bool Disponivel { get; set; } = true;

        [StringLength(100)]
        public string? Descricao { get; set; }

        public int QtdProdutos { get; set; }

        public double MediaAvaliacao { get; set; }  // depois vale migrar p/ cálculo via entidade Avaliacao
        public int Avaliacao { get; set; }          // idem

        public DateTimeOffset DtCriacao { get; set; } = DateTimeOffset.UtcNow;

        public string? Comentarios { get; set; }

        public ICollection<ProdutoMidia> Midias { get; set; } = new List<ProdutoMidia>();
    }
}