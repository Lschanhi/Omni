using Omnimarket.Api.Models.Enum;

namespace Omnimarket.Api.Models.Dtos.Produtos
{
    public class ProdutoFiltroDto
    {
        public string? Nome { get; set; }
        public string? Categoria { get; set; }
        public string? Sku { get; set; }
        public decimal? MinPreco { get; set; }
        public decimal? MaxPreco { get; set; }
        public StatusProduto? StatusPublicacao { get; set; }
        public bool SomenteDisponiveis { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
