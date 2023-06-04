using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;

namespace SwishVending.SwishRequest
{
    internal class SwishIntegration
    {
        public SwishIntegration()
        {
            
        }

        private const string SwishApiBaseUrl = "https://mss.cpc.getswish.net";
        private const string MerchantCertificatePem = "<your-merchant-certificate-pem>";
        private const string MerchantCertificatePassword = "<your-merchant-certificate-password>";

        public async Task<(string InstructionUuid, string Token)> CreatePaymentRequestAsync(SwishPaymentRequest request)
        {
            try
            {

                var payload = new
                {
                    payeeAlias = request.PayeeAlias,
                    callbackUrl = request.CallBackUrl,
                    amount = request.Amount.ToString("0.00"),
                    currency = "SEK",
                    message = request.Message,
                };

                string payloadJson = Newtonsoft.Json.JsonConvert.SerializeObject(payload);

                using (var httpClient = new HttpClient())
                {
                    // Set the Swish API base URL
                    var instructionUuid = Guid.NewGuid().ToString();
                    httpClient.BaseAddress = new Uri(SwishApiBaseUrl);

                    // Set the merchant certificate
                    var handler = new HttpClientHandler();
                    handler.ClientCertificates.Add(SwishCertificate.GetMerchantCertificate(MerchantCertificatePem, MerchantCertificatePassword));

                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Send the request to create the payment request
                    var response = await httpClient.PutAsJsonAsync(
                        $"https://mss.cpc.getswish.net/swish-cpcapi/api/v2/paymentrequests/{instructionUuid}",
                        payloadJson
                    );

                    if (response.IsSuccessStatusCode)
                    {
                        var paymentRequestToken = response.Headers.GetValues("paymentrequesttoken").FirstOrDefault();
                        return (InstructionUuid: instructionUuid, Token: paymentRequestToken);
                    }
                    else
                    {
                        // Handle the error response
                        var errorData = await response.Content.ReadAsStringAsync();
                        // Handle the error based on the errorData
                        throw new Exception("Failed to create payment request: " + errorData);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
                throw new Exception("An error occurred while creating payment request: " + ex.Message);
            }
        }
        public async Task<byte[]> GetQrCodeAsync(SwishCommerceRequest commerceRequest)
        {
            try
            {
                var requestData = new
                {
                    commerceRequest.Token,
                    size = "300",
                    format = commerceRequest.Format,
                    border = "0"
                };

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsJsonAsync("https://mpc.getswish.net/qrg-swish/api/v1/commerce", requestData);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsByteArrayAsync();
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public class SwishCertificate
    {
        public static X509Certificate2 GetMerchantCertificate(string certificatePem, string certificatePassword)
        {
            // Load the certificate from the PEM file and password
            byte[] certData = Encoding.UTF8.GetBytes(certificatePem);
            X509Certificate2 certificate = new X509Certificate2(certData, certificatePassword, X509KeyStorageFlags.Exportable);

            return certificate;
        }
    }
}
