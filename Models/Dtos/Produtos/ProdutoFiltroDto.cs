namespace Omnimarket.Api.Models.Dtos.Produtos
{
    public class ProdutoFiltroDto
    {
        public string? Nome { get; set; }
        public decimal? MinPreco { get; set; }
        public decimal? MaxPreco { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
