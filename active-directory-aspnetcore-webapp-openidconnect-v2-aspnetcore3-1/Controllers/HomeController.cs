using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Models;
using Newtonsoft.Json.Linq;
using Services;
using Models;
using WebApp_OpenIDConnect_DotNet.Models;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AzureAdOptions _azureAdOptions;
        private readonly ILogger<HomeController> _logger;
        private readonly string tenantId;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string authority;

        public HomeController(ILogger<HomeController> logger, IOptions<AzureAdOptions> azureAdOptions)
        {
            _logger = logger;
            _azureAdOptions = azureAdOptions.Value;
            tenantId = _azureAdOptions.TenantId;
            clientId = _azureAdOptions.ClientId;
            clientSecret = _azureAdOptions.ClientSecret;
            authority = $"{_azureAdOptions.Instance}{tenantId}";
            _logger.LogInformation($"ClientId: {_azureAdOptions.ClientId}");
            _logger.LogInformation($"ClientSecret: {_azureAdOptions.ClientSecret}");
            _logger.LogInformation($"TenantId: {_azureAdOptions.TenantId}");
            _logger.LogInformation($"Instance: {_azureAdOptions.Instance}");




        }

        public async Task<IActionResult> Index() // Marked method as async and changed return type to Task<IActionResult>
        {
            // Read token from query string
            string tokenFromQuery = Request.Query["token"];

            // Read token from header
            string tokenFromHeader = Request.Headers["x-ms-marketplace-token"];

            // Display results in ViewBag
            if (!string.IsNullOrEmpty(tokenFromQuery))
            {
                ViewBag.QueryTokenMessage = $"Token from query string: {tokenFromQuery}";
            }
            else
            {
                ViewBag.QueryTokenMessage = "Query string token is missing.";
            }

            if (!string.IsNullOrEmpty(tokenFromHeader))
            {
                ViewBag.HeaderTokenMessage = $"Token from header: {tokenFromHeader}";
            }
            else
            {
                ViewBag.HeaderTokenMessage = "Header token is missing.";
            }


            //tokenFromQuery = "MTBhYzRlMDMtZjQ2YS00ZWEwLWM5ZmYtMjI3ZWI5NjAwZjcwLDE3NDY3NzQxOTYyMjIscEZVeFhjYTlUdlZ1T1FDdGFvRFlCK0FodWYxOXNTRnoyQ1pyNTArMVQyYWFwMHhjbHR3M1JKTVhJOFAxMUt5UzhJNU0zVk00VXJzMGU4bWFZMVdiUGcyWVc4RkhjOFFEUllPcVhSNGZYN3ZVUlFpQlB4NTJsZ0JMaDBZdERLV2hEM1BEY0ZVVjkvd3lFdWxOZVlCMnVudTk3SW9rNXpFOXh5N29GaUgwTnljcnFTM0psZ2Voa1Y4TDViNEdVQjUzaXgwK1hkNnBpN0ZKdzhrcEx0a3JFcURXTVZ0aDBHdTJaSGF1bWFIS1paNjhHZGRGaXZxWWwrU2tkd2ZoTHdaTEhTZkIzbUNaSDY2a3k1Sk9vM2FpYUlOdy9jVWJ6OElMd0ZJVktYaWVxQWVNYVFod0RiM0txTmJ5SWhCRVFBYXVMRHNxYWFabzhKQ3VpV3BsSm0zQnlRPT0%3D";
            if (tokenFromQuery == null)
            {
                tokenFromQuery = "MGNjOThmZjctNTRhMC00ZWEzLWQzMGUtYTgzMGJmOTNlMjRjLDE3NDY3NTgyNzg5MjcsQ1B6VVFkbHNBUkErSzNlZkdOMFA5b0dHTGkwblAwSXVva0dpdEZtbWE4d2FRcmhlQWxHcS9qT3QrOGxqL3JsUHBzazNUYzVBUHBhdi9WYjNhZDJIU3pKOHMvSHVqRTl0VEdqak5kTDdIL1dFbTJMUnluRU9lSW9uQWxNN1hYUnh4alA1RVR0VXpwaDM3S09jdDJQQkV5aGFuUUp0SU9lQTd6MDlvc1VCL1dIamRrZEkvOGNIZzN3eWJEemtmb3dYWi8rYVVTejRvL21TRzFIMTQwSDY1V09VZ09tb3pYNkRVYVdjNFVROHVGQ29vR1p6aGlsd0ljbTQzTUNFeVpvR1FnYzhNYXdDMm0ybng1Wlp5MHY4T0JtYWpuQ21Ca20yTXR0Y3F2cHNCZUpZQUdLblRDREJ2azhtRnd4QXUydHJuZjB1eDlVcEZpQmJuUlA2Wkl5emtRPT0%3D";
            }
            // URL-decode the token first
            string urlDecodedToken = Uri.UnescapeDataString(tokenFromQuery);

            //this we don't want
            //string decodedToken = DecodeBase64Token(urlDecodedToken);
            //string decodedToken = DecodeBase64Token(tokenFromQuery);
            ViewBag.DecodedQueryToken = urlDecodedToken;

            var bearerToken = await GetTokenAsync();
            ViewBag.BearerToken = bearerToken;
            var resolveClient = new MarketplaceResolve();
            string jsonResult = await resolveClient.ResolveSubscriptionAsync(bearerToken, urlDecodedToken);

            JObject resultObj = JObject.Parse(jsonResult);
            ViewBag.SubscriptionName = resultObj["subscriptionName"]?.ToString();
            ViewBag.OfferId = resultObj["offerId"]?.ToString();
            ViewBag.PlanId = resultObj["planId"]?.ToString();
            ViewBag.SubscriptionStatus = resultObj["subscription"]["saasSubscriptionStatus"].ToString();
            ViewBag.EmailId = resultObj["subscription"]?["purchaser"]?["emailId"]?.ToString();
            ViewBag.TenantId = resultObj["subscription"]?["purchaser"]?["tenantId"]?.ToString();
            ViewBag.AutoRenew = resultObj["subscription"]?["autoRenew"]?.ToString();
            ViewBag.SubscriptionId = resultObj["subscription"]?["subscriptionId"]?.ToString();
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<string> GetTokenAsync()
        {
            var app = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(new Uri(authority))
                .Build();
            // var result = await app.AcquireTokenForClient(new[] { $"{resource}/.default" }).ExecuteAsync();
            var result = await app.AcquireTokenForClient(new[] { $"20e940b3-4c77-4b0b-9a53-9e16a1b010a7/.default" }).ExecuteAsync();
            return result.AccessToken;
        }
    }
}
