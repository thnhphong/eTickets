using System;
using eTickets.Models.VNPay;
using eTickets.Services.VNPay;
using Microsoft.AspNetCore.Mvc;

namespace eTickets.Controllers
{
    public class VNPayController : Controller
    {
        private readonly IVnPayService _vnPayService;

        public VNPayController(IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        [HttpPost] // Thêm HttpPost
        public IActionResult CreatePaymentUrlVnpay([FromBody] PaymentInformationModel model)
        {
            if (model == null || model.Amount <= 0)
            {
                return BadRequest(new { message = "Thông tin thanh toán không hợp lệ." });
            }

            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Json(new { url }); // Trả về JSON chứa URL thanh toán
        }

        [HttpGet]
        public IActionResult PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            return Json(response);
        }
    }
}
