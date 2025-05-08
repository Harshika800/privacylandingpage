//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Azure.Identity;
//using Azure.Extensions.AspNetCore.Configuration.Secrets;


//namespace WebApp_OpenIDConnect_DotNet
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            CreateHostBuilder(args).Build().Run();
//        }

//        public static IHostBuilder CreateHostBuilder(string[] args) =>
//            Host.CreateDefaultBuilder(args)
//                .ConfigureAppConfiguration((context, config) =>
//                {
//                    var builtConfig = config.Build();
//                    config.AddAzureKeyVault(
//                        new Uri($"https://{builtConfig["KeyVaultName"]}.vault.azure.net/"),
//                        new DefaultAzureCredential());
//                })
//                .ConfigureWebHostDefaults(webBuilder =>
//                {
//                    webBuilder.UseStartup<Startup>();
//                });
//    }
//}
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;

namespace WebApp_OpenIDConnect_DotNet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // 🔹 Step 1: Load appsettings.json first to access KeyVaultName
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                    var builtConfig = config.Build();
                    var keyVaultName = builtConfig["KeyVaultName"];
                    var keyVaultUri = $"https://{keyVaultName}.vault.azure.net/";

                    // 🔍 Debug log
                    Console.WriteLine($" Accessing Azure Key Vault: {keyVaultUri}");

                    // 🔹 Step 2: Add Azure Key Vault to configuration
                    config.AddAzureKeyVault(
                        new Uri(keyVaultUri),
                        new DefaultAzureCredential());

                    // 🔹 Step 3 (Optional): Check if a key like AzureAd:ClientId is available
                    var testConfig = config.Build();
                    var testClientId = testConfig["AzureAd:ClientId"];
                    Console.WriteLine($" ClientId from Key Vault: {(string.IsNullOrEmpty(testClientId) ? "❌ not loaded" : "✅ loaded")}");
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
