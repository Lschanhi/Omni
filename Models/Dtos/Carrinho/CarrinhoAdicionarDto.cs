using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Omni.Models.Entidades;

namespace Omni.Models.Dtos.Carrinho
{
    public class CarrinhoAdicionarDto
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }
}