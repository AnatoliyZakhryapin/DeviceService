using NLog;

namespace DeviceService
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        static async Task Main(string[] args)
        {
            Logger.Info("Starting DeviceService...");

            // Start the remote control server
            RemoteServiceManager.StartServer();

            // Start the service
            ServiceManager.StartService();

            // Block and wait for user input
            await Task.Run(() => Console.ReadLine());

            // Stop the service
            ServiceManager.StopService();
            Logger.Info("Stopping DeviceService...");
        }
    }
}
