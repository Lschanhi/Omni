using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Omnimarket.Api.Models.Dtos.Produtos
{
    public class ProdutoAtualizarDto
    {
        [Required, StringLength(50)]
        public string Nome { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Preço deve ser maior que 0.")]
        public decimal Preco { get; set; }

        [StringLength(100)]
        public string? Descricao { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantidade não pode ser negativa.")]
        public int QtdProdutos { get; set; }

        // Se você quiser permitir atualizar foto aqui, mantenha; senão, remova e crie endpoint próprio.
        public byte[]? Foto { get; set; }
    }
}