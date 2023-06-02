using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SwishVending.VendingMachineEventHandler
{
    public class MachinePaymentUpdate
    {
        private readonly ILogger _logger;

        public MachinePaymentUpdate(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<MachinePaymentUpdate>();
        }

        [Function("MachinePaymentUpdate")]
        public void Run([ServiceBusTrigger("machine-payment-update", Connection = "")] string myQueueItem)
        {
            _logger.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
        }
    }
}
