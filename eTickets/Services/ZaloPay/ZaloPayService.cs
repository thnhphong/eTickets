namespace eTickets.Services.ZaloPay
{
    using System;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    public class ZaloPayService
    {
        private readonly string _appId;
        private readonly string _key1;
        private readonly string _key2;

        public ZaloPayService(IConfiguration configuration)
        {
            _appId = configuration["ZaloPay:AppId"];
            _key1 = configuration["ZaloPay:Key1"];
            _key2 = configuration["ZaloPay:Key2"];
        }


        public string GetAppId() => _appId;
        public string GetKey1() => _key1;
        public string GetKey2() => _key2;
    }
}