using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Veryfi.IntegrationTests
{
    [TestClass]
    public class DocumentsTests
    {
        private static async Task ProcessDocumentTestAsync(
            VeryfiApi api,
            Document document,
            CancellationToken cancellationToken)
        {
            document.Should().NotBeNull();

            var documents = await api.GetDocumentsAsync(
                cancellationToken: cancellationToken);

            documents.Should().NotBeNullOrEmpty();

            var deleteStatus = await api.DeleteDocumentAsync(
                document.Id,
                cancellationToken);

            deleteStatus.Should().NotBeNull();
            deleteStatus.Status.Should().Be("ok");
        }

        private static async Task ProcessDocumentTestAsync(
            VeryfiApi api,
            DocumentUploadOptions options,
            CancellationToken cancellationToken)
        {
            var document = await api.ProcessDocumentAsync(
                options,
                cancellationToken);

            await ProcessDocumentTestAsync(api, document, cancellationToken);
        }

        [DataTestMethod]
        [DataRow("invoice1.png")]
        [DataRow("receipt.png")]
        [DataRow("receipt_public.jpg")]
        public async Task ProcessUrlTest(string fileName)
        {
            var url = $"https://raw.githubusercontent.com/HavenDV/veryfi-csharp/master/src/tests/Veryfi.IntegrationTests/Assets/{fileName}";
            
            await BaseTests.ApiTestAsync(async (api, cancellationToken) =>
            {
                await ProcessDocumentTestAsync(
                    api,
                    new DocumentUploadOptions
                    {
                        File_name = fileName,
                        File_url = url,
                    },
                    cancellationToken);
            });
        }

        [DataTestMethod]
        [DataRow("invoice1.png")]
        [DataRow("receipt.png")]
        [DataRow("receipt_public.jpg")]
        public async Task ProcessBase64Test(string fileName)
        {
            var file = new H.Resource(fileName);

            await BaseTests.ApiTestAsync(async (api, cancellationToken) =>
            {
                await ProcessDocumentTestAsync(
                    api,
                    new DocumentUploadOptions
                    {
                        File_name = file.FileName,
                        File_data = Convert.ToBase64String(file.AsBytes()),
                    },
                    cancellationToken);
            });
        }

        [DataTestMethod]
        [DataRow("invoice1.png")]
        [DataRow("receipt.png")]
        [DataRow("receipt_public.jpg")]
        public async Task ProcessFileTest(string fileName)
        {
            var file = new H.Resource(fileName);

            await BaseTests.ApiTestAsync(async (api, cancellationToken) =>
            {
                await ProcessDocumentTestAsync(
                    api,
                    await api.ProcessDocumentFileAsync(
                        file.AsStream(),
                        new DocumentUploadOptions
                        {
                            File_name = file.FileName,
                        },
                        cancellationToken),
                    cancellationToken);
            });
        }
    }
}
