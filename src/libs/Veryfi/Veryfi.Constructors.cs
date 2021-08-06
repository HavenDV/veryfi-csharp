using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Veryfi
{
    /// <summary>
    /// 
    /// </summary>
    public partial class VeryfiApi
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="apiKey"></param>
        /// <param name="clientId"></param>
        /// <param name="httpClient"></param>
        public VeryfiApi(
            string username,
            string apiKey,
            string clientId, 
            HttpClient httpClient) : 
            this(httpClient)
        {
            username = username ?? throw new ArgumentNullException(nameof(username));
            apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            httpClient.DefaultRequestHeaders.Add("Client-Id", clientId);
            httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("apikey", $"{username}:{apiKey}");
            httpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue(
                    "Veryfi-CSharp", 
                    $"{typeof(VeryfiApi).Assembly.GetName().Version}"));
        }
    }
}