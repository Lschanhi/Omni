using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Omnimarket.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Omnimarket.Api.Services
{
    public class UsuarioService
    {
        private readonly DataContext _context;

        public UsuarioService(DataContext context)
        {
            _context = context;
        }

        //este método serve para diferenciar o usuario(se ele é vendedor ou apenas cliente)
        public async Task<bool> UsuarioVendedor(int usuarioId)
        {
            return await _context.TBL_PRODUTO
                .AnyAsync(p => p.UsuarioId == usuarioId && p.Disponivel);
        }
    }
}