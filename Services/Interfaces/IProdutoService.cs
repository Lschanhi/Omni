using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Omni.Models.Dtos.Produtos;
using Omni.Services;
using Omnimarket.Api.Models.Dtos.Produtos;
using Omnimarket.Api.Models.Entidades;

namespace Omnimarket.Api.Services.Interfaces
{
    public interface IProdutoService
    {
        Task<IEnumerable<ProdutoLeituraDto>> GetAllAsync();
        Task<ProdutoLeituraDto?> GetByIdAsync(int id);
        Task<ProdutoLeituraDto> CreateAsync(ProdutoCriacaoDto dto, int usuarioId);
        Task<bool> UpdateAsync(int id, ProdutoAtualizarDto dto);
        Task<bool> DeleteAsync(int id);

        Task<PageResult<ProdutoLeituraDto>> GetPagedAsync(ProdutoFiltroDto filtro);
    }
}