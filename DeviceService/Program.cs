using DeviceService.config;
using NLog;

namespace DeviceService
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static IConfigurationRoot _configuration;
        private static ServiceConfig _serviceConfig;
        private static JwtSettings _jwtSettings;
        private static ServiceProvider _serviceProvider;
        static async Task Main(string[] args)
        {
            Logger.Info("Starting DeviceService...");

            Configure();

            CreateServices();

            var remoteServiceManager = _serviceProvider.GetRequiredService<RemoteServiceManager>();
            var serviceManager = _serviceProvider.GetRequiredService<ServiceManager>();

            // Start the remote control server
            remoteServiceManager.StartServer();

            // Start the service
            await serviceManager.StartService();

            // Block and wait for user input
            await Task.Run(() => Console.ReadLine());

            // Stop the service
            serviceManager.StopService();
            Logger.Info("Stopping DeviceService...");
        }
        private static void Configure()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _serviceConfig = _configuration.GetSection("ServiceConfig").Get<ServiceConfig>();
            _jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>();
        }

        private static void CreateServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(_serviceConfig);
            serviceCollection.AddSingleton(_jwtSettings);
            serviceCollection.AddSingleton<RemoteServiceManager>();
            serviceCollection.AddSingleton<ServiceManager>();

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
