using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ExcusesService.Helpers
{
    public class ExcuseGeneratorClient
    {
        private readonly HttpClient _httpClient;

        public ExcuseGeneratorClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public class ExcuseRequest
        {
            public string Nome { get; set; }
            public string Motivo { get; set; }
        }

        public class ExcuseResponse
        {
            public string Desculpa { get; set; }
            public string DataGeracao { get; set; }
        }

        public async Task<ExcuseResponse> GerarDesculpaAsync(string nome, string motivo)
        {
            var req = new ExcuseRequest { Nome = nome, Motivo = motivo };
            var resp = await _httpClient.PostAsJsonAsync("/gerar", req);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ExcuseResponse>();
        }
    }
}
