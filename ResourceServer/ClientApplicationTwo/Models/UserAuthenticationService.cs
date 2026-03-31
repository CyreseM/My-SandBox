using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ClientApplicationTwo.Models
{
    public class UserAuthenticationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _authServerUrl;

        public UserAuthenticationService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _authServerUrl = _configuration["AuthenticationServer:BaseUrl"] ?? "https://localhost:7088";
        }

        public async Task<HttpResponseMessage> LoginUserAsync(LoginViewModel model)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_authServerUrl}/api/Authentication/Login";
            var jsonContent = JsonSerializer.Serialize(model);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            return await client.PostAsync(url, content);
        }

        public async Task<HttpResponseMessage> GenerateSSOTokenAsync(string jwtToken)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_authServerUrl}/api/Authentication/GenerateSSOToken";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            // Add the JWT token in the Authorization header.
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            // For this endpoint, no additional JSON payload is needed.
            return await client.SendAsync(request);
        }
    }
}