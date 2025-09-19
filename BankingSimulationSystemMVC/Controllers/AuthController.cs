using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using BankingSimulationSystemMVC.Models;

public class AuthController : Controller
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IConfiguration _config;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthController(IHttpClientFactory clientFactory, IConfiguration config)
    {
        _clientFactory = clientFactory;
        _config = config;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var client = _clientFactory.CreateClient();
        client.BaseAddress = new Uri(_config["ApiSettings:BaseUrl"]);

        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("auth/login", content);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Login failed.");
            return View(model);
        }

        var resultJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<LoginResponse>(resultJson, _jsonOptions);

        if (result == null || string.IsNullOrEmpty(result.Token))
        {
            ModelState.AddModelError("", "Invalid response from API.");
            return View(model);
        }

        HttpContext.Session.SetString("JwtToken", result.Token);
        return RedirectToAction("Index", "Bank");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var client = _clientFactory.CreateClient();
        client.BaseAddress = new Uri(_config["ApiSettings:BaseUrl"]);

        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("auth/register", content);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Registration failed.");
            return View(model);
        }

        return RedirectToAction("Login");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
