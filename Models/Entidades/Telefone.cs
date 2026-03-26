using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace Omnimarket.Api.Models
{
   public class Telefone
   {
      public int Id { get; set; } 

      [Required]
      public int UsuarioId { get; set; }

      public Usuario Usuario { get; set; } = null!;

      [Required, StringLength(20)]
      public string NumeroE164 { get; set; } = string.Empty; // ex: +5511912345678

      public bool IsPrincipal { get; set; } = false;
   }

}