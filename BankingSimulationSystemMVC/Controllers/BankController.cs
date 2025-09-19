using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using BankingSimulationSystemMVC.Models;

namespace BankingSimulationSystemMVC.Controllers
{
    public class BankController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;
        private readonly JsonSerializerOptions _jsonOptions;

        public BankController(IHttpClientFactory clientFactory, IConfiguration config)
        {
            _clientFactory = clientFactory;
            _config = config;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"));

        private HttpClient CreateClient()
        {
            var client = _clientFactory.CreateClient();
            var baseUrl = _config["ApiSettings:BaseUrl"]?.TrimEnd('/') ?? throw new InvalidOperationException("ApiSettings:BaseUrl not configured.");
    
            client.BaseAddress = new Uri(baseUrl + "/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        // GET: /Bank/Index
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var client = CreateClient();
            HttpResponseMessage response;

            try
            {
                response = await client.GetAsync("bank/account");
            }
            catch (HttpRequestException ex)
            {
                ViewBag.Error = "Unable to contact API: " + ex.Message;
                return View(new Account());
            }

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = $"Unable to fetch account details. Status: {response.StatusCode}";
                return View(new Account());
            }

            var json = await response.Content.ReadAsStringAsync();

            Account? account;
            try
            {
                account = JsonSerializer.Deserialize<Account>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error reading account data: " + ex.Message;
                return View(new Account());
            }

            return View(account ?? new Account());
        }


        [HttpGet]
        public IActionResult Deposit()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            return View(new TransactionRequest());
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(TransactionRequest model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
                return View(model);

            var client = CreateClient();
            var json = JsonSerializer.Serialize(model, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync("bank/deposit", content);
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError("", "API connection error: " + ex.Message);
                return View(model);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (!response.IsSuccessStatusCode)
            {
                var err = await SafeReadContent(response);
                ModelState.AddModelError("", $"Deposit failed: {err}");
                return View(model);
            }

            TempData["Success"] = "Deposit successful.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Withdraw()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            return View(new TransactionRequest());
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw(TransactionRequest model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
                return View(model);

            var client = CreateClient();
            var json = JsonSerializer.Serialize(model, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync("bank/withdraw", content);
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError("", "API connection error: " + ex.Message);
                return View(model);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (!response.IsSuccessStatusCode)
            {
                var err = await SafeReadContent(response);
                ModelState.AddModelError("", $"Withdrawal failed: {err}");
                return View(model);
            }

            TempData["Success"] = "Withdrawal successful.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> TransactionHistory()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var client = CreateClient();

            // ✅ Corrected URL
            var response = await client.GetAsync("Bank/transactions");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = $"Unable to fetch transactions. Status: {response.StatusCode}";
                return View(new List<Transaction>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var transactions = JsonSerializer.Deserialize<List<Transaction>>(json, _jsonOptions);

            return View(transactions ?? new List<Transaction>());
        }




        // helper: read content safely
        private static async Task<string> SafeReadContent(HttpResponseMessage response)
        {
            try
            {
                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return response.ReasonPhrase ?? response.StatusCode.ToString();
            }
        }

        // helper: detect if JSON text is an array
        private static bool IsJsonArray(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return false;
            var s = json.TrimStart();
            return s.StartsWith("[");
        }
    }
}
