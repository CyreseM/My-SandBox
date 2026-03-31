using ClientApplicationTwo.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ClientApplicationTwo.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserAuthenticationService _userAuthenticationService;
        private readonly IConfiguration _configuration;

        public AccountController(UserAuthenticationService userAuthenticationService, IConfiguration configuration)
        {
            _userAuthenticationService = userAuthenticationService;
            _configuration = configuration;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _userAuthenticationService.LoginUserAsync(model);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                try
                {
                    // Deserialize into LoginResponseModel.
                    var loginResponse = JsonSerializer.Deserialize<LoginResponseModel>(responseContent);

                    if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                    {
                        // Store the JWT token and username in session.
                        HttpContext.Session.SetString("JWT", loginResponse.Token);
                        HttpContext.Session.SetString("Username", model.Username);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Token not found in the response.");
                        return View();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error parsing response: " + ex.Message);
                    ModelState.AddModelError(string.Empty, "Failed to parse the response.");
                    return View();
                }
            }

            ModelState.AddModelError(string.Empty, "Login failed.");
            return View();
        }

        // GET: /Account/GenerateSSOToken
        // This action calls the User Server to generate an SSO token and then redirects to Client Application One.
        [HttpGet]
        public async Task<IActionResult> GenerateSSOToken()
        {
            // Ensure the user is logged in.
            var jwtToken = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(jwtToken))
            {
                return RedirectToAction("Login");
            }

            var response = await _userAuthenticationService.GenerateSSOTokenAsync(jwtToken);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                try
                {
                    // Deserialize the SSO response.
                    // We expect a JSON with an "SSOToken" property.
                    var ssoResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);

                    if (ssoResponse != null && ssoResponse.ContainsKey("SSOToken"))
                    {
                        var ssoToken = ssoResponse["SSOToken"];

                        // Get the base URL for Client Application One from configuration.
                        var clientAppOneUrl = _configuration["ClientApplicationOne:BaseUrl"];

                        // Redirect the user to Client Application One with the SSO token as a query parameter.
                        return Redirect($"{clientAppOneUrl}/validate-sso?ssoToken={ssoToken}");
                    }
                    else
                    {
                        ViewBag.Error = "SSO token not found in the response.";
                        return View("Error");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error parsing SSO response: " + ex.Message);
                    ViewBag.Error = "Failed to parse the SSO response.";
                    return View("Error");
                }
            }

            ViewBag.Error = "Failed to generate SSO token.";
            return View("Error");
        }

        // POST: /Account/Logout
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JWT");
            HttpContext.Session.Remove("Username");
            return RedirectToAction("Index", "Home");
        }
    }
}