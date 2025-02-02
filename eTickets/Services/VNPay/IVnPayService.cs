using eTickets.Models.VNPay;
using Microsoft.AspNetCore.Http;

namespace eTickets.Services.VNPay
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
