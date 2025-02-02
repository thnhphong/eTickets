using System;
using System.Text.Json;
using eTickets.Data.Cart;
using eTickets.Models.VNPay;
using eTickets.Services.VNPay;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace eTickets.Controllers
{
    public class VNPayController : Controller
    {

        private readonly IVnPayService _vnPayService;
        public VNPayController(IVnPayService vnPayService)
        {

            _vnPayService = vnPayService;
        }

        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Redirect(url);
        }
        [HttpGet]
        public IActionResult PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            return Json(response);
        }

    }
}
