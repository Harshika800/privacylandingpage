using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(ILogger<WebhookController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveWebhook()
        {
            _logger.LogInformation("Webhook received.");

            // 🔐 Step 1: Extract and validate the JWT token
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader) ||
                !AuthenticationHeaderValue.TryParse(authHeader, out var headerValue) ||
                string.IsNullOrEmpty(headerValue.Parameter))
            {
                _logger.LogWarning("Missing or invalid Authorization header.");
                return Unauthorized();
            }

            var token = headerValue.Parameter;
            if (!await ValidateTokenAsync(token))
            {
                _logger.LogWarning("JWT validation failed.");
                return Unauthorized();
            }

            // 📥 Step 2: Read and parse the body
            string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            var payload = JsonConvert.DeserializeObject<SaaSWebhookPayload>(requestBody);
            if (payload == null || string.IsNullOrEmpty(payload.Action))
            {
                _logger.LogWarning("Invalid payload.");
                return BadRequest("Invalid webhook payload.");
            }

            // 🔄 Step 3: Handle each action
            switch (payload.Action.ToLowerInvariant())
            {
                case "unsubscribe":
                    _logger.LogInformation($"Unsubscribe action for Subscription ID: {payload.SubscriptionId}");
                    // TODO: Mark user as unsubscribed
                    break;

                case "changeplan":
                    _logger.LogInformation($"ChangePlan action for Subscription ID: {payload.SubscriptionId}, Plan: {payload.PlanId}");
                    // TODO: Call SaaS Fulfillment API to approve plan change
                    break;

                case "suspend":
                    _logger.LogInformation($"Suspend action for Subscription ID: {payload.SubscriptionId}");
                    // TODO: Restrict access
                    break;

                case "reinstate":
                    _logger.LogInformation($"Reinstate action for Subscription ID: {payload.SubscriptionId}");
                    // TODO: Re-enable access
                    break;

                default:
                    _logger.LogWarning($"Unknown action: {payload.Action}");
                    break;
            }

            return Ok(new { message = "Webhook processed successfully" });
        }

        private async Task<bool> ValidateTokenAsync(string token)
        {
            const string issuer = "https://login.microsoftonline.com";
            const string discoveryEndpoint = "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration";

            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                discoveryEndpoint,
                new OpenIdConnectConfigurationRetriever());

            var config = await configManager.GetConfigurationAsync();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = new[] { issuer },
                ValidateAudience = false, // or provide your App ID URI
                ValidateLifetime = true,
                IssuerSigningKeys = config.SigningKeys
            };

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation failed.");
                return false;
            }
        }
    }

    // 📦 Model for the webhook payload
    public class SaaSWebhookPayload
    {
        public string Action { get; set; }
        public string SubscriptionId { get; set; }
        public string OfferId { get; set; }
        public string PlanId { get; set; }
        public string PublisherId { get; set; }
        public string OperationId { get; set; }
    }
}
