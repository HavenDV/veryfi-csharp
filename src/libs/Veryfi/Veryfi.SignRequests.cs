using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

#pragma warning disable CA1801 // Review unused parameters
// ReSharper disable UnusedParameterInPartialMethod

namespace Veryfi
{
    public partial class VeryfiApi
    {
        private string ClientSecret { get; }

        partial void PrepareRequest(
            HttpClient client,
            HttpRequestMessage request,
            string url)
        {
            if (string.IsNullOrWhiteSpace(ClientSecret))
            {
                return;
            }

#if NETSTANDARD2_0_OR_GREATER
            var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
#else
            var timestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
#endif
            var signature = GenerateSignature(timestamp);
            
            request.Headers.Add("X-Veryfi-Request-Timestamp", $"{timestamp}");
            request.Headers.Add("X-Veryfi-Request-Signature", signature);
        }

        private string GenerateSignature(long timestamp)
        {
            var dictionary = new Dictionary<string, string>
            {
                ["timestamp"] = $"{timestamp}",
            };

            var payload = string.Join(
                ",",
                dictionary.Select(static pair => $"{pair.Key}:{pair.Value}"));

            var encoding = Encoding.UTF8;
            var secretBytes = encoding.GetBytes(ClientSecret);
            var payloadBytes = encoding.GetBytes(payload);
            using var hmac = new HMACSHA256(secretBytes);
            var signature = hmac.ComputeHash(payloadBytes);
            
            return Convert.ToBase64String(signature);
        }
}
}