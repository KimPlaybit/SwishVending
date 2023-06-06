using System.Net;
using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace SwishVending.SwishCallback
{
    public class SwishCallback
    {
        private readonly string serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString") ?? throw new ArgumentNullException();
        private readonly string serviceBusQueueName = "machine-payment-update";

        private const string SecretKey = "<your-secret-key>";

        private readonly ILogger logger;

        public SwishCallback(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<SwishCallback>();
        }

        [Function("SwishCallback")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Verify the request signature
            if (!req.Headers.TryGetValues("x-swish-signature", out var values) || !VerifySignature(requestBody, values.First()))
            {
                logger.LogWarning("Invalid Swish callback signature");
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }
            // Send the Swish callback response to Service Bus
            await SendSwishResponseToServiceBus(requestBody);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            return response;
        }


        private static bool VerifySignature(string requestBody, string signature)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(SecretKey));
            var computedSignature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(requestBody)));

            return string.Equals(signature, computedSignature, StringComparison.OrdinalIgnoreCase);
        }

        private async Task SendSwishResponseToServiceBus(string requestBody)
        {
            await using var client = new ServiceBusClient(serviceBusConnectionString);
            var sender = client.CreateSender(serviceBusQueueName);
            await sender.SendMessageAsync(new ServiceBusMessage(Encoding.UTF8.GetBytes(requestBody)));
            
        }
    }
}
