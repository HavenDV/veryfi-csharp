using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Veryfi.IntegrationTests
{
    [TestClass]
    public class DocumentsTests
    {
        [TestMethod]
        public async Task ProcessUpdateDeleteTest() => await BaseTests.ApiTestAsync(async (api, cancellationToken) =>
        {
            var file = H.Resources.receipt_public_jpg;
            var document = await api.ProcessDocumentAsync(
                new DocumentUploadOptions
                {
                    File_name = file.FileName,
                    File_data = Convert.ToBase64String(file.AsBytes()),
                },
                cancellationToken);

            document.Should().NotBeNull();

            var documents = await api.GetDocumentsAsync(
                cancellationToken: cancellationToken);

            documents.Should().NotBeNullOrEmpty();

            var deleteStatus = await api.DeleteDocumentAsync(
                document.Id,
                cancellationToken);

            deleteStatus.Should().NotBeNull();
            deleteStatus.Status.Should().Be("ok");
        });
    }
}
