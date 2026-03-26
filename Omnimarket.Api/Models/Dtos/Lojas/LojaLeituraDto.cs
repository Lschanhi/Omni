namespace Omnimarket.Api.Models.Dtos.Lojas
{
    public class LojaLeituraDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string NomeFantasia { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? EmailContato { get; set; }
        public string? TelefoneContato { get; set; }
        public string? Cidade { get; set; }
        public string? Uf { get; set; }
        public bool Ativa { get; set; }
        public DateTimeOffset DtCriacao { get; set; }
        public DateTimeOffset? DtAtualizacao { get; set; }
    }
}
