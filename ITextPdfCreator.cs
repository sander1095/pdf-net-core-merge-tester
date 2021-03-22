using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Document = iText.Layout.Document;

namespace pdfmerger
{
    public class ITextPdfCreator : IPdfMerger
    {
        public async Task<byte[]> CreateAsync(IEnumerable<(string ContentType, byte[] Content)> blobs)
        {
            await using var memoryStream = new MemoryStream();
            await using var writer = new PdfWriter(memoryStream);

            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);
            var firstIteration = true;

            foreach (var blob in blobs)
            {
                if (!firstIteration)
                {
                    document.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));
                }

                if (blob.ContentType.StartsWith("image/"))
                {
                    var content = new Image(ImageDataFactory.Create(blob.Content));
                    document.Add(content);
                }
                else if (blob.ContentType.StartsWith("application/pdf"))
                {
                    await using var stream = new MemoryStream(blob.Content);
                    var d = new PdfDocument(new PdfReader(stream));
                    d.CopyPagesTo(1, d.GetNumberOfPages(), pdf, pdf.GetNumberOfPages() + 1);
                }

                firstIteration = false;
            }

            document.Close();

            return memoryStream.ToArray();
        }
    }
}
