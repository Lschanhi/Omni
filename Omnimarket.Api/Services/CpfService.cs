using System.Text.Json;
using Omnimarket.Api.Models;

namespace Omnimarket.Api.Services
{
    public interface ICpfService
    {
        Task<CpfResposta> ConsultarCpf(string cpf);
    }

    public class CpfService : ICpfService
    {
        private readonly HttpClient _httpClient;

        public CpfService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Consulta um CPF em um servico externo e devolve um objeto padronizado para a API.
        public async Task<CpfResposta> ConsultarCpf(string cpf)
        {
            var cpfLimpo = cpf.Replace(".", "").Replace("-", "").Trim();

            // Atalho local usado durante desenvolvimento ou demonstracoes.
            var teste = TesteConsultaCpf(cpfLimpo);
            if (teste != null)
                return teste;

            var url = $"https://api.exemplo.com/v1/cpf/{cpfLimpo}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return new CpfResposta
                    {
                        Sucesso = false,
                        Erro = "CPF nao encontrado ou erro na API externa"
                    };
                }

                var conteudo = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var resultado = JsonSerializer.Deserialize<CpfResposta>(conteudo, options);

                if (resultado == null)
                {
                    return new CpfResposta
                    {
                        Sucesso = false,
                        Erro = "Erro ao processar resposta da API"
                    };
                }

                resultado.Sucesso = true;
                return resultado;
            }
            catch (HttpRequestException)
            {
                return new CpfResposta
                {
                    Sucesso = false,
                    Erro = "Erro de conexao com servico externo"
                };
            }
            catch (TaskCanceledException)
            {
                return new CpfResposta
                {
                    Sucesso = false,
                    Erro = "Tempo de requisicao excedido"
                };
            }
            catch (Exception)
            {
                return new CpfResposta
                {
                    Sucesso = false,
                    Erro = "Erro inesperado ao consultar CPF"
                };
            }
        }

        // Mock simples para testes locais sem depender de API externa.
        private CpfResposta? TesteConsultaCpf(string cpf)
        {
            if (cpf == "12345678909")
            {
                return new CpfResposta
                {
                    Sucesso = true,
                    Nome = "USUARIO TESTE TCC"
                };
            }

            return null;
        }
    }
}
