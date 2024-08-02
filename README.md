# DeviceService

## Project Description

The **DeviceService** project is a microservice designed to manage operations on remote devices. This microservice serves as a support for device control, providing secure and reliable APIs to enable client applications to interact with various devices. Currently, the service retrieves data from a CSV file containing temperature sensor data, processes this data using specific coefficients, and sends it to a central server for further processing.

## Objectives

The primary goals of the **DeviceService** project are:

- Facilitate remote control of devices by providing APIs to manage services.
- Ensure secure communication with devices using JWT authentication.
- Log device operations and handle errors effectively.
- Retrieve, process, and transmit sensor data reliably and efficiently.

## Features

The **DeviceService** project offers the following features:

- **CSV Reading:** The service reads temperature sensor data from a CSV file and processes it using specific transformation coefficients.
- **Data Transmission:** The processed data is sent to a central server for storage and analysis.
- **Remote Commands:** Currently, three remote commands can be executed by the service as the project is still under development:
  - **Start Service:** Starts the remote device control service.
  - **Stop Service:** Stops the remote device control service.
  - **Restart Service:** Restarts the remote device control service.

## Technologies Used

- **C# & .NET Core:** Core application logic and RESTful services.
- **NLog:** Logging framework for monitoring and recording.
- **JWT:** Secure authentication using JSON Web Tokens.
- **ASP.NET Core:** Web framework for building APIs.

## Configuration and Architecture

### Dependency Injection (DI)

The project utilizes Dependency Injection (DI) to manage class instances and enhance the modularity and testability of the code. Here's how it's configured:

- **Service Registration**: Services such as `RemoteServiceManager` and `ServiceManager` are registered in the DI container using the `ServiceCollection` class. This registration allows for the automatic creation and management of these class instances with the necessary configuration.

- **Service Injection**: When a service is needed, it is retrieved from the DI container. This approach avoids manual instance creation and automatically manages their lifecycle and dependencies.

- **Example Configuration**: Here's how services are registered and used:
  ```csharp
  private static void CreateServices()
  {
      var serviceCollection = new ServiceCollection();

      // Registering services
      serviceCollection.AddSingleton(_serviceConfig);
      serviceCollection.AddSingleton(_jwtSettings);
      serviceCollection.AddSingleton<RemoteServiceManager>();
      serviceCollection.AddSingleton<ServiceManager>();

      _serviceProvider = serviceCollection.BuildServiceProvider();
  }

  // Retrieving services from the DI container
   var remoteServiceManager = _serviceProvider.GetRequiredService<RemoteServiceManager>();
   var serviceManager = _serviceProvider.GetRequiredService<ServiceManager>();

   // Using services
   remoteServiceManager.StartServer();
   serviceManager.StartService();
   ```
This approach keeps the code modular, facilitates unit testing, and ensures centralized management of configurations and dependencies.

##  Installation and Usage
1. **Clone the repository DeviceServiceServer for remote contorll:**
   ```bash
   git clone https://github.com/AnatoliyZakhryapin/DeviceServiceServer.git 
   cd DeviceServiceServer\DeviceServiceServer

 2. **Run the project:**  
   ```bash
   dotnet run

3. **Clone the repository DeviceService:**
   ```bash
   git clone https://github.com/AnatoliyZakhryapin/DeviceService.git 
   cd DeviceService\DeviceService

4. **Run the project:**  
     ```bash
   dotnet run
   
## Configuration

### Data CSV File
- Ensure that the `SensorData.csv` file is located in the `Data` directory within the project structure. The service uses this file to retrieve temperature sensor data.

### NLog Configuration
- Update the `NLog.config` file to specify the log file path and the desired logging level.

### Remote Server
- For remote control, you can use the following server:
[DeviceServiceServer](https://github.com/AnatoliyZakhryapin/DeviceServiceServer)

### Configurazione **`appsettings.json`**
Make sure to properly configure the `appsettings.json` file for JWT authentication settings and other service configurations. Here's an example configuration:

```json
{
  "ServiceConfig": {
    "Username": "user@user.com",
    "Password": "Password123456789?!",
    "Interval": "5000",
    "DataEndpoint": "https://localhost:7031/api/data",
    "LoginEndpoint": "https://localhost:7031/api/auth/login",
    "ServiceURL": "http://localhost",
    "ServicePort": 5000
  },
  "JwtSettings": {
    "Issuer": "MyDeviceServiceIssuer",
    "Audience": "MyDeviceServiceAudience",
    "SecretKey": "MyKeyPasswordDeviceService123456789!",
    "TokenExpiryInMinutes": 60
  }
}
```

### Explanation:

- **ServiceConfig:**
    - **Username and Password:** Must match the credentials of your account on DeviceServiceServer.
    - **Interval:** You can set the interval (in milliseconds) for sending data to the server.
    - **DataEndpoint and LoginEndpoint:** Endpoints of the DeviceServiceServer for sending data and logging in. Modify according to the address of the DeviceServiceServer.
    - **ServiceURL and ServicePort:** URL and port of the service.

- **JwtSettings:** 
    - **Issuer:** Specifies the issuer of the JWT tokens.
    - **Audience:** Specifies the audience of the JWT tokens.
    - **SecretKey:** Secret key used to sign the JWT tokens. Must match the values on the DeviceServiceServer.
    - **TokenExpiryInMinutes:** Defines the validity duration of a JWT token.

## Contributions

Contributions are welcome! Feel free to fork the project and submit a pull request with your improvements or bug fixes.

## License

This project is licensed under the MIT License.
