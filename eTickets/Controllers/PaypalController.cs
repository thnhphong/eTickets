using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using eTickets.Data.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Ini;
using Microsoft.Extensions.Options;

namespace eTickets.Controllers
{
    public class PaypalController : Controller
    {
        private readonly ShoppingCart _shoppingCart;
        private readonly IConfiguration _configuration;
        private string PayPalClientID { get; set; } = "";
        private string PayPalSecret { get; set; } = "";
        private string PayPalUrl { get; set; } = "";

        public PaypalController(IConfiguration configure, ShoppingCart shoppingCart)
        {
            _configuration = configure;
            _shoppingCart = shoppingCart;

            PayPalClientID = configure["PayPalSettings:ClientId"];
            PayPalSecret = configure["PayPalSettings:Secret"];
            PayPalUrl = configure["PayPalSettings:Url"];
        }
        public IActionResult Index()
        {
            ViewBag.PayPalClientID = PayPalClientID;
            ViewBag.PayPalSecret = PayPalSecret;
            ViewBag.PayPalUrl = PayPalUrl;

            var shoppingCartTotal = _shoppingCart.GetShoppingCartTotal(); // Lấy tổng tiền từ giỏ hàng
            ViewBag.TotalAmount = shoppingCartTotal;
            return View();
        }

        //public async Task<string> Token()
        //{
        //    return await GetPayPalAccessToken();
        //}
        private async Task<string> GetPayPalAccessToken()
        {
            string accessToken = "";
            string url = PayPalUrl + "/v1/oauth2/token";

            using (var client = new HttpClient())
            {
                string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{PayPalClientID}:{PayPalSecret}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                var requestContent = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });

                var httpResponse = await client.PostAsync(url, requestContent);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var strResponse = await httpResponse.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);
                    if (jsonResponse != null)
                    {
                        accessToken = jsonResponse["access_token"]?.ToString() ?? "";
                    }
                }
            }
            return accessToken;
        }

        [HttpPost]
        public async Task<JsonResult> CreateOrder([FromBody] JsonObject data)
        {
            // 1️⃣ Kiểm tra dữ liệu đầu vào
            var totalAmount = data?["amount"]?.ToString();
            if (!decimal.TryParse(totalAmount, out decimal amountValue) || amountValue <= 0)
            {
                return Json(new { status = "error", message = "Invalid amount format" });
            }

            // 2️⃣ Tạo JSON request cho PayPal
            JsonObject createOrderRequest = new JsonObject
    {
        { "intent", "CAPTURE" }
    };

            JsonObject amount = new JsonObject
    {
        { "currency_code", "USD" },
        { "value", totalAmount }
    };

            JsonObject purchaseUnit = new JsonObject
    {
        { "amount", amount }
    };

            createOrderRequest.Add("purchase_units", new JsonArray { purchaseUnit });

            // 3️⃣ Lấy Access Token từ PayPal
            string accessToken = await GetPayPalAccessToken();
            string url = PayPalUrl + "/v2/checkout/orders";

            // 4️⃣ Gửi request tạo đơn hàng đến PayPal
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(createOrderRequest.ToString(), Encoding.UTF8, "application/json")
                };

                var httpResponse = await client.SendAsync(requestMessage);
                var strResponse = await httpResponse.Content.ReadAsStringAsync();

                Console.WriteLine("PayPal Response: " + strResponse); // Debug phản hồi từ PayPal

                if (httpResponse.IsSuccessStatusCode)
                {
                    var jsonResponse = JsonNode.Parse(strResponse);
                    if (jsonResponse != null)
                    {
                        string paypalOrderId = jsonResponse["id"]?.ToString();
                        return Json(new { Id = paypalOrderId });
                    }
                }

                // 5️⃣ Nếu có lỗi, trả về lỗi
                return Json(new { status = "error", message = "Failed to create PayPal order", details = strResponse });
            }
        }

        [HttpPost]
        public async Task<JsonResult> CompleteOrder([FromBody] JsonObject data)
        {
            var orderId = data?["orderId"]?.ToString();
            if(orderId == null)
            {
                return new JsonResult("error");
            }

            string accessToken = await GetPayPalAccessToken();

            string url = PayPalUrl + $"/v2/checkout/orders/" +orderId+"/capture";

            using (var client = new HttpClient()){
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent("", null, "application/json");

                var httpResponse = await client.SendAsync(requestMessage);

                if(httpResponse.IsSuccessStatusCode)
                {
                    var strResponse = await httpResponse.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);
                    if (jsonResponse != null)
                    {
                        string status = jsonResponse["status"]?.ToString() ?? "";
                        if (status == "COMPLETED")
                        {
                            return new JsonResult("success");
                        }
                    }
                }
            }


            return new JsonResult("error");
        }

    }
}
