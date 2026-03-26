using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Omnimarket.Api.Models.Dtos.Enderecos;
using Omnimarket.Api.Models.Dtos.Telefones;


namespace Omnimarket.Api.Models.Dtos.Usuarios
{
    public class UsuarioRegistroDto
    {
        [Required(ErrorMessage = "CPF é obrigatório")]
        [StringLength(14, ErrorMessage = "CPF deve ter no máximo 14 caracteres")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Nome deve ter entre 2 e 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sobrenome é obrigatório")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Sobrenome deve ter entre 2 e 100 caracteres")]
        public string Sobrenome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
        [Compare("Password", ErrorMessage = "As senhas não coincidem")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class UsuarioRegistroComContatoDto : UsuarioRegistroDto
    {
        [Required(ErrorMessage = "Informe pelo menos 1 telefone.")]
        [MinLength(1, ErrorMessage = "Informe pelo menos 1 telefone.")]
        public List<UsuarioTelefoneDto> Telefones { get; set; } = new();

        public List<UsuarioEnderecoDto>? Enderecos { get; set; }
    }

   
}