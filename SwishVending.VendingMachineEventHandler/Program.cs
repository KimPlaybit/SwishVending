using Microsoft.Extensions.Hosting;
using SwishVending.VendingMachineEventHandler;

var pythonExecution = new PythonExecution();
pythonExecution.ExecutePythonProgram("1");

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .Build();

host.Run();
