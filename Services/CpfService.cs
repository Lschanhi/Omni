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

        // O HttpClient é injetado automaticamente pelo ASP.NET Core
        public CpfService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CpfResposta> ConsultarCpf(string cpf)
        {
            // Remove formatação para enviar apenas números
            string cpfLimpo = cpf.Replace(".", "").Replace("-", "").Trim();

            // EXEMPLO: URL de uma API fictícia. 
            // Você substituirá pela URL do serviço que contratar (ex: BrasilAPI, ReceitaWS)
            string url = $"https://api.exemplo.com/v1/cpf/{cpfLimpo}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var conteudo = await response.Content.ReadAsStringAsync();
                    
                    // Configuração para deserializar JSON case-insensitive
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    return JsonSerializer.Deserialize<CpfResposta>(conteudo, options);
                }

                return new CpfResposta { Sucesso = false, Erro = "CPF não encontrado ou erro na API externa" };
            }
            catch (System.Exception ex)
            {
                return new CpfResposta { Sucesso = false, Erro = $"Falha na conexão: {ex.Message}" };
            }
        }

         private CpfResposta? TesteConsultaCpf(string cpf)
        {
            var cpfLimpo = cpf.Replace(".", "").Replace("-", "").Trim();

            if (cpfLimpo == "12345678909")
            {
                return new CpfResposta
                {
                    Sucesso = true,
                    Nome = "NOME DO PROFESSOR AQUI"
                };
            }

            return null;
        }
        
    }

    
    
}
