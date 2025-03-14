using System.Threading.Tasks;
using System;
using eTickets.Services.ZaloPay;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace eTickets.Controllers
{
    public class ZaloPayController : Controller
    {
        private readonly IConfiguration _configuration;

        public ZaloPayController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.AppId = _configuration["ZaloPay:AppId"];
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment()
        {
            var appId = _configuration["ZaloPay:AppId"];
            var key1 = _configuration["ZaloPay:Key1"];
            var endpoint = "https://sb-openapi.zalopay.vn/v2/create";

            var orderData = new
            {
                app_id = appId,
                app_user = "demo",
                app_time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                amount = 50000, // Example amount
                description = "Payment for order #123",
                embed_data = "{}",
                item = "[]",
                bank_code = "zalopayapp",
                callback_url = "https://yourdomain.com/ZaloPay/Callback",
            };

            using (var client = new HttpClient())
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(orderData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(responseJson);

                return Json(new
                {
                    return_code = jsonResponse["return_code"],
                    return_message = jsonResponse["return_message"],
                    order_url = jsonResponse["order_url"]
                });
            }
        }
    }
}