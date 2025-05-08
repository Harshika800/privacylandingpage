using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;

namespace Services
{
    public class MarketplaceResolve
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<string> ResolveSubscriptionAsync(string bearerToken, string marketplaceToken)
        {
            string url = "https://marketplaceapi.microsoft.com/api/saas/subscriptions/resolve?api-version=2018-08-31";

            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            // Required Headers
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            request.Headers.Add("x-ms-marketplace-token", marketplaceToken);
            request.Headers.Add("Accept", "application/json");
            request.Content = new StringContent("", Encoding.UTF8, "application/json");

            // Send request
            var response = await client.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return content;
            }
            else
            {
                throw new Exception($"Resolve failed. Status: {response.StatusCode}, Body: {content}");
            }
        }
    }
}
