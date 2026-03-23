using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 
namespace Omnimarket.Api.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(11)]
        public string Cpf { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Sobrenome { get; set; } = string.Empty;
        
        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }
        public byte[]? Foto { get; set; }
        public DateTime? DataAcesso { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [NotMapped]
        public string PasswordString { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [NotMapped]
        public string Token { get; set; } = string.Empty;


        public List<Telefone> Telefones { get; set; } = new();
        public List<Endereco> Enderecos { get; set; } = new();
    }
}