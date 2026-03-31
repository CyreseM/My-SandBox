using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ClientApplicationOne.Models;

namespace ClientApplicationOne.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _resourceServerUrl;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _resourceServerUrl = _configuration["ResourceServer:BaseUrl"] ?? "https://localhost:7206";
            _httpClientFactory = httpClientFactory;
        }

        // GET: /Home/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Home/GetPublicData
        [HttpGet]
        public async Task<IActionResult> GetPublicData()
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_resourceServerUrl}/api/demo/public-data";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                try
                {
                    // Deserialize the response into a DataResponseModel.
                    var dataResponse = JsonSerializer.Deserialize<DataResponseModel>(responseContent);

                    if (dataResponse != null)
                    {
                        ViewBag.PublicData = dataResponse.Message;
                    }
                    else
                    {
                        ViewBag.PublicData = "No message found in the response.";
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error parsing the response: " + ex.Message;
                }
                return View();
            }

            ViewBag.Error = "Failed to retrieve public data.";
            return View();
        }

        // GET: /Home/GetProtectedData
        [HttpGet]
        public async Task<IActionResult> GetProtectedData()
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient();
            var url = $"{_resourceServerUrl}/api/demo/protected-data";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                try
                {
                    // Deserialize the response into a DataResponseModel.
                    var dataResponse = JsonSerializer.Deserialize<DataResponseModel>(responseContent);

                    if (dataResponse != null)
                    {
                        ViewBag.ProtectedData = dataResponse.Message;
                    }
                    else
                    {
                        ViewBag.ProtectedData = "No message found in the response.";
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error parsing the response: " + ex.Message;
                }
                return View();
            }

            ViewBag.Error = "Failed to retrieve protected data or unauthorized.";
            return View();
        }
    }
}