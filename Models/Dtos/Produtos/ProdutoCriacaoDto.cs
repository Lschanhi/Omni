using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Omnimarket.Api.Models.Dtos.Produtos
{
   public class ProdutoCriacaoDto
{
    [Required]
    [StringLength(50)]
    public string Nome { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Preço deve ser maior que 0.")]
    public decimal Preco { get; set; }

    [StringLength(100)]
    public string? Descricao { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Quantidade não pode ser negativa.")]
    public int Estoque { get; set; }

    // Remover e tratar separadamente
    // public IFormFile? Foto { get; set; }
}
}