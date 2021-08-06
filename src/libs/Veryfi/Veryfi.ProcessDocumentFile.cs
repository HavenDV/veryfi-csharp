﻿using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CA2000 // Dispose objects before losing scope

namespace Veryfi
{
    public partial class VeryfiApi
    {
        /// <param name="stream"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        /// </param>
        /// <summary>Process a Document.</summary>
        /// <returns>Document.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<Document> ProcessDocumentFileAsync(
            Stream stream,
            DocumentUploadOptions options,
            CancellationToken cancellationToken = default)
        {
            stream = stream ?? throw new ArgumentNullException(nameof(stream));
            options = options ?? throw new ArgumentNullException(nameof(options));

            var fileName = 
                options.File_name ?? 
                throw new ArgumentException(
                    "options.File_name is required to detect MIME type.", 
                    nameof(options));

            var urlBuilder = new StringBuilder();
            urlBuilder
                .Append(BaseUrl.TrimEnd('/'))
                .Append("/documents");

            var client = _httpClient;
            using var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new MultipartFormDataContent
                {
                    { 
                        new StreamContent(stream)
                        {
                            Headers =
                            {
                                ContentType = MediaTypeHeaderValue.Parse(
#if NETSTANDARD2_0_OR_GREATER
                                    HeyRed.Mime.MimeTypesMap.GetMimeType(fileName)
#else
                                    System.Web.MimeMapping.GetMimeMapping(fileName)
#endif
                                ),
                            },
                        }, 
                        "file",
                        fileName
                    },
                    { new StringContent(fileName), "file_name" },
                    { new StringContent($"{(options.Auto_delete ?? false ? 1 : 0)}"), "auto_delete" },
                    { new StringContent($"{options.Boost_mode ?? 0}"), "boost_mode" },
                    { new StringContent($"{options.Async ?? 0}"), "async" },
                    { new StringContent(options.External_id ?? string.Empty), "external_id" },
                    { new StringContent($"{options.Max_pages_to_process ?? 0}"), "max_pages_to_process" },
                    //{ new StringContent($"[{string.Join(",", options.Categories ?? new List<string>())}]"), "categories" },
                },
                Headers =
                {
                    Accept =
                    {
                        MediaTypeWithQualityHeaderValue.Parse("application/json"),
                    },
                },
            };

            PrepareRequest(client, request, urlBuilder);

            var url = urlBuilder.ToString();
            request.RequestUri = new Uri(url, UriKind.RelativeOrAbsolute);

            PrepareRequest(client, request, url);

            using var response = await client.SendAsync(
                request, 
                HttpCompletionOption.ResponseHeadersRead, 
                cancellationToken).ConfigureAwait(false);
            var headers = response.Headers.ToDictionary(
                static pair => pair.Key,
                static pair => pair.Value);
            if (response.Content is { Headers: { } })
            {
                foreach (var item in response.Content.Headers)
                {
                    headers[item.Key] = item.Value;
                }
            }

            ProcessResponse(client, response);

            var status = (int)response.StatusCode;
            if (status == 201)
            {
                var objectResponse = await ReadObjectResponseAsync<Document>(
                    response, 
                    headers, 
                    cancellationToken).ConfigureAwait(false);
                if (objectResponse.Object == null)
                {
                    throw new ApiException(
                        "Response was null which was not expected.", 
                        status, 
                        objectResponse.Text, 
                        headers, 
                        null);
                }
                return objectResponse.Object;
            }
            else
            {
                var objectResponse = await ReadObjectResponseAsync<OperationStatus>(
                    response, 
                    headers, 
                    cancellationToken).ConfigureAwait(false);
                if (objectResponse.Object == null)
                {
                    throw new ApiException(
                        "Response was null which was not expected.", 
                        status, 
                        objectResponse.Text, 
                        headers, 
                        null);
                }

                throw new ApiException<OperationStatus>(
                    "OperationStatus.", 
                    status, 
                    objectResponse.Text, 
                    headers, 
                    objectResponse.Object, 
                    null);
            }
        }
    }
}