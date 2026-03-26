using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Models.Entidades;

namespace Omnimarket.Api.Models
{

    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Cpf), IsUnique = true)]
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

        // 🔐 Segurança (CORRETO)
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

        // 📷 Opcional (pode virar URL no futuro)
        public byte[]? Foto { get; set; }

        public DateTime? DataAcesso { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

        // ❌ NÃO SALVAR SENHA EM STRING NO BANCO
        [NotMapped]
        public string PasswordString { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        // ❌ Token não deve ficar no banco
        [NotMapped]
        public string Token { get; set; } = string.Empty;

        public bool AceitouTermos { get; set; }
        public DateTime? DataAceiteTermos { get; set; }

        public string Role { get; set; } = "User";

        // 📞 Relacionamentos
        public List<Telefone> Telefones { get; set; } = new();
        public List<Endereco> Enderecos { get; set; } = new();

        // 🛍️ RELAÇÃO COM PRODUTOS (IMPORTANTE PRA VOCÊ)
        public List<Produto> Produtos { get; set; } = new();

        // 🧾 RELAÇÃO COM PEDIDOS
        public List<Pedido> Pedidos { get; set; } = new();

        // Loja unica para o mesmo usuario atuar como vendedor sem outra conta.
        public Loja? Loja { get; set; }
    }
}
