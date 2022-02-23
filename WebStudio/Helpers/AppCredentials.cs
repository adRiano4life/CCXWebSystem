using System;
using Microsoft.Extensions.Configuration;

namespace WebStudio.Helpers
{
    public static class AppCredentials
    {
        
        public static readonly Func<string> SetPath = () =>
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var appConfig = builder.Build();
            return appConfig.GetValue<string>("PathToFiles:DefaultPath"); // сервер 
            //return @$"C:\Users\user\Desktop\files"; // Саня Т.
            //return @$"E:\csharp\ESDP\Download Files"; // Саня Ф.
            //return "D:/csharp/esdp/app/WebStudio/wwwroot/Files"; // Гульжан
        };

        public static readonly Func<string> SetConnection = () =>
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var appConfig = builder.Build();
            return appConfig.GetConnectionString("DefaultConnection");
        };

        public static readonly Func<string> SetEmailName = () =>
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var appConfig = builder.Build();
            return appConfig.GetValue<string>("EmailCredentials:EmailOffice");
        };

        public static readonly Func<string> SetAdminEmailName = () =>
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var appConfig = builder.Build();
            return appConfig.GetValue<string>("EmailCredentials:AdminEmailOffice");
        };
        
        public static readonly Func<string> SetAdminEmailName = () =>
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var appConfig = builder.Build();
            return appConfig.GetValue<string>("EmailCredentials:AdminEmail");
        };

        public static readonly Func<string> SetEmailPassword = () =>
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var appConfig = builder.Build();
            return appConfig.GetValue<string>("EmailCredentials:Password");
        };

        public static readonly string DefaultConnection = SetConnection();
        public static readonly string PathToFiles = SetPath();
        public static readonly string EmailName = SetEmailName();
        public static readonly string AdminEmailName = SetAdminEmailName();
        public static readonly string EmailPassword = SetEmailPassword();
    }
}