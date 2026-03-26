using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Omnimarket.Api.Utils
{
    public class CpfValidador
    {
         public static bool ValidarCpf(string cpf)
        {
            try
            {
                // Remove formatação (pontos, traços e espaços)
                cpf = LimparFormatacao(cpf);

                // Validações básicas
                if (string.IsNullOrEmpty(cpf) || cpf.Length != 11)
                    return false;

                // Verifica se todos os dígitos são iguais (ex: 111.111.111-11)
                if (TodosDigitosIguais(cpf))
                    return false;

                // Calcula e valida os dígitos verificadores
                return ValidarDigitosVerificadores(cpf);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Remove pontos, traços e espaços do CPF
        /// </summary>
        private static string LimparFormatacao(string cpf)
        {
            return cpf.Trim()
                      .Replace(".", "")
                      .Replace("-", "")
                      .Replace(" ", "");
        }

        /// <summary>
        /// Verifica se todos os dígitos são iguais
        /// </summary>
        private static bool TodosDigitosIguais(string cpf)
        {
            return cpf.All(c => c == cpf[0]);
        }

        /// <summary>
        /// Valida os dois dígitos verificadores usando o algoritmo módulo 11
        /// </summary>
        private static bool ValidarDigitosVerificadores(string cpf)
        {
            // Arrays de multiplicadores para cada dígito verificador
            int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            // Extrai os 9 primeiros dígitos
            string tempCpf = cpf.Substring(0, 9);

            // Calcula o primeiro dígito verificador
            int soma = 0;
            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            int primeiroDigito = resto < 2 ? 0 : 11 - resto;

            // Adiciona o primeiro dígito verificador
            tempCpf += primeiroDigito.ToString();

            // Calcula o segundo dígito verificador
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            int segundoDigito = resto < 2 ? 0 : 11 - resto;

            // Verifica se os dígitos calculados correspondem aos dígitos informados
            return cpf.EndsWith($"{primeiroDigito}{segundoDigito}");
        }

        /// <summary>
        /// Formata um CPF válido no padrão XXX.XXX.XXX-XX
        /// </summary>
        /// <param name="cpf">CPF sem formatação</param>
        /// <returns>CPF formatado ou string vazia se inválido</returns>
        public static string FormatarCpf(string cpf)
        {
            cpf = LimparFormatacao(cpf);
            
            if (cpf.Length != 11)
                return string.Empty;

            return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
        }
    }
}