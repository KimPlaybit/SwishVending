using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.IO;

namespace SwishVending.SwishRequest
{
    public static class RequestSwish
    {
        [FunctionName("RequestSwish")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<SwishTransactionRequest>(requestBody);

            //Map Object for Swish payer Request
            var paymentRequest = new SwishPaymentRequest("+46733507354", 100, "SEK", "", data.Message);
            //Send paymentRequest to Swish
            var token = SendSwishPaymentRequest(paymentRequest);

            //Map Token from Swish 
            var commerceRequest = new SwishCommerceRequest(token.Token, "jpg", 300);

            //Send to /api/v1/commerce
            var qr = GetQRCode(commerceRequest);

            return new OkObjectResult(qr.ArrayBuffer);
        }

        private static SwishPaymentResponse SendSwishPaymentRequest(SwishPaymentRequest request)
        {
            return new SwishPaymentResponse { Token = Guid.NewGuid().ToString() };
        }

        private static SwishCommerceResponse GetQRCode(SwishCommerceRequest request)
        {
            return new SwishCommerceResponse { ArrayBuffer = ImageToByteArray() };
        }
        public static FileStream ImageToByteArray()
        {
            return File.OpenRead("./response.png");
        }
    }
}

public class SwishTransactionRequest
{
    public string Message { get; set; }
    public int Price { get; set; }
}

public class SwishCommerceResponse
{
    public FileStream ArrayBuffer { get; set; }
}

public class SwishPaymentResponse
{
    public string Token { get; set; }
}

public record SwishPaymentRequest(string PayeeAlias, int Amount, string Currency, string CallBackUrl, string Message)
{
    public string PayeePaymentReference { get; init; }
}

public record SwishCommerceRequest(string Token, string Format, int Size)
{
    public int Border { get; init; }
    public bool Transparent { get; init; }
}