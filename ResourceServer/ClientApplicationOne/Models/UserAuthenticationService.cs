using System.Text;
using System.Text.Json;

namespace ClientApplicationOne.Models
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
            _authServerUrl = _configuration["AuthenticationServer:BaseUrl"] ?? "https://localhost:7125";
        }

        public async Task<HttpResponseMessage> RegisterUserAsync(RegisterViewModel model)
        {
            var client = _httpClientFactory.CreateClient();

            var url = $"{_authServerUrl}/api/Authentication/Register";
            var jsonContent = JsonSerializer.Serialize(model);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            return await client.PostAsync(url, content);
        }

        public async Task<HttpResponseMessage> LoginUserAsync(LoginViewModel model)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_authServerUrl}/api/Authentication/Login";
            var jsonContent = JsonSerializer.Serialize(model);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            return await client.PostAsync(url, content);
        }

        public async Task<HttpResponseMessage> ValidateSSOTokenAsync(ValidateSSOTokenModel model)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_authServerUrl}/api/Authentication/ValidateSSOToken";
            var jsonContent = JsonSerializer.Serialize(model);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            return await client.PostAsync(url, content);
        }
    }
}