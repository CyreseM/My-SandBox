using Microsoft.AspNetCore.Mvc;
using ClientApplicationOne.Models;
using System.Text.Json;

namespace ClientApplicationOne.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserAuthenticationService _userAuthenticationService;

        public AccountController(UserAuthenticationService userAuthenticationService)
        {
            _userAuthenticationService = userAuthenticationService;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _userAuthenticationService.RegisterUserAsync(model);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, "Registration Failed.");
            return View();
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
                    // Deserialize the response into a LoginResponseModel.
                    var loginResponse = JsonSerializer.Deserialize<LoginResponseModel>(responseContent);

                    if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                    {
                        // Store the token and username in session.
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

        // POST: /Account/Logout
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JWT");
            HttpContext.Session.Remove("Username");
            return RedirectToAction("Index", "Home");
        }

        // GET: /validate-sso?ssoToken=...
        // Validates the SSO token received from another application.
        [HttpGet("validate-sso")]
        public async Task<IActionResult> ValidateSSO([FromQuery] string ssoToken)
        {
            if (string.IsNullOrEmpty(ssoToken))
            {
                return RedirectToAction("Login");
            }

            ValidateSSOTokenModel model = new ValidateSSOTokenModel()
            {
                SSOToken = ssoToken
            };

            var response = await _userAuthenticationService.ValidateSSOTokenAsync(model);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                try
                {
                    // Deserialize the response into a ValidateSSOResponseModel.
                    var ssoResponse = JsonSerializer.Deserialize<ValidateSSOResponseModel>(responseContent);

                    if (ssoResponse != null)
                    {
                        if (!string.IsNullOrEmpty(ssoResponse.Token))
                        {
                            HttpContext.Session.SetString("JWT", ssoResponse.Token);
                        }

                        if (ssoResponse.UserDetails != null && !string.IsNullOrEmpty(ssoResponse.UserDetails.Username))
                        {
                            HttpContext.Session.SetString("Username", ssoResponse.UserDetails.Username);
                        }

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Login");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error parsing response: " + ex.Message);
                    return RedirectToAction("Login");
                }
            }

            return RedirectToAction("Login");
        }
    }
}