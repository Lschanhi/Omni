using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
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

        public async Task<CpfResposta> ConsultarCpf(string cpf)
        {
            string cpfLimpo = cpf.Replace(".", "").Replace("-", "").Trim();

            // 🔥 TESTE LOCAL (TCC)
            var teste = TesteConsultaCpf(cpfLimpo);
            if (teste != null)
                return teste;

            string url = $"https://api.exemplo.com/v1/cpf/{cpfLimpo}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return new CpfResposta
                    {
                        Sucesso = false,
                        Erro = "CPF não encontrado ou erro na API externa"
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
                    Erro = "Erro de conexão com serviço externo"
                };
            }
            catch (TaskCanceledException)
            {
                return new CpfResposta
                {
                    Sucesso = false,
                    Erro = "Tempo de requisição excedido"
                };
            }
            catch (System.Exception)
            {
                return new CpfResposta
                {
                    Sucesso = false,
                    Erro = "Erro inesperado ao consultar CPF"
                };
            }
        }

        // 🔥 MOCK PARA TCC / TESTE
        private CpfResposta? TesteConsultaCpf(string cpf)
        {
            if (cpf == "12345678909")
            {
                return new CpfResposta
                {
                    Sucesso = true,
                    Nome = "USUÁRIO TESTE TCC"
                };
            }

            return null;
        }
    }
}