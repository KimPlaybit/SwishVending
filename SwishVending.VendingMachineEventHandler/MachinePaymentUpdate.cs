using System;
using System.Diagnostics;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SwishVending.VendingMachineEventHandler
{
    public class MachinePaymentUpdate
    {
        private readonly ILogger logger;
        private readonly PythonExecution pythonExecution;

        public MachinePaymentUpdate(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<MachinePaymentUpdate>();
            pythonExecution = new PythonExecution();
            pythonExecution.ExecutePythonProgram("1");
        }

        [Function("MachinePaymentUpdate")]
        public void Run([ServiceBusTrigger("machine-payment-update", Connection = "")] string myQueueItem)
        {
            logger.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            pythonExecution.ExecutePythonProgram(myQueueItem);
        }

    }
    public class PythonExecution
    {
        public void ExecutePythonProgram(string parameter)
        {
            try
            {
                // Create a new process to execute the Python program
                Process process = new Process();

                // Set the path to the Python interpreter
                process.StartInfo.FileName = "py";

                // Set the arguments for the Python script and the parameter
                process.StartInfo.Arguments = $"engine-control.py {parameter}";

                // Redirect the standard output and error streams
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                // Enable reading the standard output and error
                process.StartInfo.UseShellExecute = false;

                // Start the process
                process.Start();

                // Read the output and error streams
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                // Wait for the process to exit
                process.WaitForExit();

                // Display the output and error messages
                Console.WriteLine("Output:");
                Console.WriteLine(output);

                Console.WriteLine("Error:");
                Console.WriteLine(error);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}
