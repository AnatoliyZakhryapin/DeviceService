
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NLog;
using System;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService
{
    internal static class RemoteServiceManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void StartServer()
        {
            var builder = WebApplication.CreateBuilder();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "MyDeviceServiceIssuer",
                    ValidAudience = "MyDeviceServiceAudience",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MyKeyPasswordDeviceService123456789!"))
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Logger.Warn("Token di autenticazione non valido: " + context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Logger.Warn("Richiesta non autorizzata. Dettagli: " + context.ErrorDescription);
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGet("/start", async context =>
            {
                Console.WriteLine();
                Logger.Info("Received start command");
                ServiceManager.StartService();
                await context.Response.WriteAsync("Service started.");
                Console.WriteLine();
            }).RequireAuthorization();

            app.MapGet("/stop", async context =>
            {
                Console.WriteLine();
                Logger.Info("Received stop command");
                ServiceManager.StopService();
                await context.Response.WriteAsync("Service stopped.");
                Console.WriteLine();
            }).RequireAuthorization();

            app.MapGet("/restart", async context =>
            {
                Console.WriteLine();
                Logger.Info("Received restart command");
                ServiceManager.RestartService();
                await context.Response.WriteAsync("Service restarted.");
                Console.WriteLine();
            }).RequireAuthorization();

            Task.Run(() => app.Run("https://localhost:5000")); // Start server on port 5000
        }
    }
}
