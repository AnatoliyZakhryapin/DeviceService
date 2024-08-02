using Azure;
using CsvHelper;
using CsvHelper.Configuration;
using DeviceService.config;
using NLog;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace DeviceService
{
    internal class ServiceManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static Timer _timer;
        private readonly ServiceConfig _serviceConfig;
        private readonly JwtSettings _jwtSettings;
        private static bool _serviceRunning;
        private static string _csvFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Data", "SensorData.csv");
        private static string _token;
        private static DateTime _tokenExpiry;

        public ServiceManager(ServiceConfig serviceConfig, JwtSettings jwtSettings)
        {
            _serviceConfig = serviceConfig;
            _jwtSettings = jwtSettings;
        }

        public async Task StartService()
        {
            if (_serviceRunning)
            {
                Logger.Info("Service is already running.");
                return;
            }

            Logger.Info("Starting service...");
            Console.WriteLine(); 

            // First authentication
            if (!await AuthenticateAsync())
            {
                Logger.Error("Initial authentication failed. Service cannot start without a valid token.");
                return;
            }

            Logger.Info("Initial authentication successful.");

            _timer = new Timer(_serviceConfig.Interval);
            _timer.Elapsed += async (sender, e) => await OnTimedEvent();
            _timer.AutoReset = true;
            _timer.Enabled = true;

            _serviceRunning = true;
        }

        public async Task StopService()
        {
            if (!_serviceRunning)
            {
                Logger.Info("Service is not running.");
                return;
            }

            Logger.Info("Stopping service...");

            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;

            _serviceRunning = false;
        }

        public async Task RestartService()
        {
            Logger.Info("Service restarted");
            StopService();
            StartService();
        }
        private List<SensorData> ReadCsvFile(string filePath)
        {

            List<SensorData> sensorDataList = new List<SensorData>();

            using (StreamReader fileStream = new StreamReader(filePath))
            {
                string headerLine = fileStream.ReadLine();

                while (!fileStream.EndOfStream)
                {
                    string line = fileStream.ReadLine();

                    if (line != null)
                    {
                        string[] lineVulues = line.Split(';');

                        if (lineVulues.Length == 5)
                        {
                            SensorData sensorData = new SensorData
                            {
                                SensorId =Convert.ToInt32(lineVulues[0]),
                                Timestamp = ConvertTimestampToDateTime(long.Parse(lineVulues[1])),
                                Date = lineVulues[2],
                                Time = lineVulues[3],
                                Value = double.Parse(lineVulues[4], CultureInfo.InvariantCulture)
                            };

                            sensorDataList.Add(sensorData);
                        }

                    }
                }
            }

            return sensorDataList;
        }

        private DateTime ConvertTimestampToDateTime(long timestamp)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(timestamp);
        }

        private void TransformSensorDataList(List<SensorData> sensorDataList)
        {
            foreach (SensorData sensorData in sensorDataList)
            {
                sensorData.Value = CalibrationCoefficient.TransformValue(sensorData.Value);
            }
        }

        private async Task OnTimedEvent()
        {
            Console.WriteLine();
            List<SensorData> sensorDataList = null;

            try
            {
                Logger.Info("Start read file");
                // Create list of Sensor Data from File CSV
                sensorDataList = ReadCsvFile(_csvFilePath);

                Logger.Info("Read file success.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during reading the CSV file");
                return; // Exit the method if reading CSV fails
            }

            try
            {
                Logger.Info("Start transform sensor data");

                // Transform data
                TransformSensorDataList(sensorDataList);

                Logger.Info("Finish transform sensor data");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during transform sensor data");
                return; // Exit the method if data transformation fails
            }

            try
            {
                Logger.Info("Start sending data to the Server");

                // Send data to Serer
                if (await EnsureValidTokenAsync())
                {
                    await SendDataToServer(sensorDataList);
                }

                Logger.Info("The data sent successfully");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during sending data to Server");
                return; // Exit the method if sending data fails
            }

            Console.WriteLine();
            Console.WriteLine("Press [Enter] to stop service");
        }

        private async Task<bool> AuthenticateAsync()
        {
            using (var client = new HttpClient())
            {
                var loginModel = new
                {
                    Email = _serviceConfig.Username,
                    Password = _serviceConfig.Password
                };

                var content = new StringContent(JsonSerializer.Serialize(loginModel), Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync(_serviceConfig.LoginEndpoint, content);
                    response.EnsureSuccessStatusCode();

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var jsonDocument = JsonDocument.Parse(responseBody);

                    _token = jsonDocument.RootElement.GetProperty("token").GetString();
                    _tokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpiryInMinutes);

                    Console.WriteLine();
                    Logger.Info("Authentication successful, token obtained.");
                    return true;
                }
                catch (HttpRequestException ex)
                {
                    Logger.Error(ex, "Authentication failed.");
                    return false;
                }
            }
        }

        private async Task<bool> EnsureValidTokenAsync()
        {
            if (string.IsNullOrEmpty(_token) || DateTime.UtcNow >= _tokenExpiry)
            {
                Logger.Warn("Token is expired or not available. Attempting to authenticate.");

                return await AuthenticateAsync();
            }

            return true;
        }

        private async Task SendDataToServer(List<SensorData> data)
        {
            int maxRetries = 3;
            int delay = 1000;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                for (int attempt = 0; attempt < maxRetries; attempt++)
                {
                    try
                    {
                        var response = await client.PostAsync(_serviceConfig.DataEndpoint, content);
                        if (response.IsSuccessStatusCode)
                        {
                            return;
                        }

                        Logger.Warn($"Failed to send data (Attempt {attempt + 1}/{maxRetries}): {response.StatusCode}");
                    }
                    catch (HttpRequestException ex)
                    {
                        Logger.Error(ex, $"Error sending data (Attempt {attempt + 1}/{maxRetries})");
                    }

                    await Task.Delay(delay);
                }

                throw new Exception($"Error sending data after {maxRetries} attempts.");
            }
        }
    }
}
