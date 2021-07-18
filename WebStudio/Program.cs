using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebStudio.Models;
using WebStudio.Services;

namespace WebStudio
{
    public class Program
    {
        private static readonly Func<string> SetConnection = () =>
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var appConfig = builder.Build();
            return appConfig.GetConnectionString("DefaultConnection");
        };
        private static readonly Func<string> SetPath = () =>
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var appConfig = builder.Build();
            return appConfig.GetValue<string>("PathToFiles:DefaultPath");
        };
        
        public static readonly string DefaultConnection = SetConnection();
        public static readonly string PathToFiles = SetPath();

        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var userManager = services.GetRequiredService<UserManager<User>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var context = services.GetRequiredService<WebStudioContext>();
                await RoleInitializer.Initializer(roleManager, userManager);
            }
            catch (Exception exception)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(exception, "Возникло исключение при инициализации ролей");
            }
            
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}