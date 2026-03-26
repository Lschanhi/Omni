using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models.Dtos.Lojas;
using Omnimarket.Api.Models.Entidades;
using Omnimarket.Api.Utils;

namespace Omnimarket.Api.Services
{
    public class LojaService
    {
        private readonly DataContext _context;

        public LojaService(DataContext context)
        {
            _context = context;
        }

        // Retorna a loja vinculada ao usuario autenticado.
        public async Task<LojaLeituraDto?> ObterMinhaLojaAsync(int usuarioId)
        {
            var loja = await _context.TBL_LOJA
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.UsuarioId == usuarioId);

            return loja == null ? null : Mapear(loja);
        }

        // Retorna uma loja publica ativa pelo slug.
        public async Task<LojaLeituraDto?> ObterPorSlugAsync(string slug)
        {
            var slugNormalizado = NormalizarSlug(slug);
            if (string.IsNullOrWhiteSpace(slugNormalizado))
                return null;

            var loja = await _context.TBL_LOJA
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Slug == slugNormalizado && l.Ativa);

            return loja == null ? null : Mapear(loja);
        }

        // Cria a loja do usuario. Cada usuario pode ter apenas uma loja.
        public async Task<LojaLeituraDto> CriarMinhaLojaAsync(int usuarioId, LojaCriacaoDto dto)
        {
            if (await _context.TBL_LOJA.AnyAsync(l => l.UsuarioId == usuarioId))
                throw new InvalidOperationException("Voce ja possui uma loja cadastrada.");

            var usuarioExiste = await _context.TBL_USUARIO.AnyAsync(u => u.Id == usuarioId);
            if (!usuarioExiste)
                throw new InvalidOperationException("Usuario nao encontrado.");

            var slugBase = string.IsNullOrWhiteSpace(dto.Slug) ? dto.NomeFantasia : dto.Slug;

            var loja = new Loja
            {
                UsuarioId = usuarioId,
                NomeFantasia = dto.NomeFantasia.Trim(),
                Slug = await GerarSlugUnicoAsync(slugBase),
                Descricao = LimparOpcional(dto.Descricao),
                EmailContato = LimparOpcional(dto.EmailContato)?.ToLowerInvariant(),
                TelefoneContato = LimparOpcional(dto.TelefoneContato),
                Cidade = LimparOpcional(dto.Cidade),
                Uf = LimparOpcional(dto.Uf)?.ToUpperInvariant(),
                Ativa = dto.Ativa,
                DtCriacao = DateTimeOffset.UtcNow
            };

            await _context.TBL_LOJA.AddAsync(loja);
            await _context.SaveChangesAsync();

            return Mapear(loja);
        }

        // Atualiza os dados da loja do proprio usuario.
        public async Task<LojaLeituraDto?> AtualizarMinhaLojaAsync(int usuarioId, LojaAtualizacaoDto dto)
        {
            var loja = await _context.TBL_LOJA
                .FirstOrDefaultAsync(l => l.UsuarioId == usuarioId);

            if (loja == null)
                return null;

            var slugBase = string.IsNullOrWhiteSpace(dto.Slug) ? dto.NomeFantasia : dto.Slug;

            loja.NomeFantasia = dto.NomeFantasia.Trim();
            loja.Slug = await GerarSlugUnicoAsync(slugBase, loja.Id);
            loja.Descricao = LimparOpcional(dto.Descricao);
            loja.EmailContato = LimparOpcional(dto.EmailContato)?.ToLowerInvariant();
            loja.TelefoneContato = LimparOpcional(dto.TelefoneContato);
            loja.Cidade = LimparOpcional(dto.Cidade);
            loja.Uf = LimparOpcional(dto.Uf)?.ToUpperInvariant();
            loja.Ativa = dto.Ativa;
            loja.DtAtualizacao = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return Mapear(loja);
        }

        // Gera slug unico com sufixo numerico quando necessario.
        private async Task<string> GerarSlugUnicoAsync(string valorBase, int? lojaIdIgnorar = null)
        {
            var slugBase = NormalizarSlug(valorBase);
            if (string.IsNullOrWhiteSpace(slugBase))
                throw new InvalidOperationException("Nao foi possivel gerar um slug valido para a loja.");

            var slug = slugBase;
            var contador = 2;

            while (await _context.TBL_LOJA.AnyAsync(l =>
                       l.Slug == slug &&
                       (!lojaIdIgnorar.HasValue || l.Id != lojaIdIgnorar.Value)))
            {
                slug = $"{slugBase}-{contador}";
                contador++;
            }

            return slug;
        }

        // Converte textos livres para um formato amigavel de URL.
        private static string NormalizarSlug(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            var semAcento = texto.RemoverAcentos().ToLowerInvariant();
            semAcento = Regex.Replace(semAcento, @"[^a-z0-9\s-]", string.Empty);
            semAcento = Regex.Replace(semAcento, @"\s+", "-");
            semAcento = Regex.Replace(semAcento, @"-{2,}", "-");

            return semAcento.Trim('-');
        }

        private static string? LimparOpcional(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            return valor.Trim();
        }

        private static LojaLeituraDto Mapear(Loja loja)
        {
            return new LojaLeituraDto
            {
                Id = loja.Id,
                UsuarioId = loja.UsuarioId,
                NomeFantasia = loja.NomeFantasia,
                Slug = loja.Slug,
                Descricao = loja.Descricao,
                EmailContato = loja.EmailContato,
                TelefoneContato = loja.TelefoneContato,
                Cidade = loja.Cidade,
                Uf = loja.Uf,
                Ativa = loja.Ativa,
                DtCriacao = loja.DtCriacao,
                DtAtualizacao = loja.DtAtualizacao
            };
        }
    }
}
