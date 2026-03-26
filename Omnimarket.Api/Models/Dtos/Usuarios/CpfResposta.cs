using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Omnimarket.Api.Models
{
    public class CpfResposta
    {
         public string Cpf { get; set; }
        public string Nome { get; set; }
        public string Situacao { get; set; } // "Regular", "Pendente", etc.
        public string DataNascimento { get; set; }
        public bool Sucesso { get; set; }
        public string Erro { get; set; }
    }
}