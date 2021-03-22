﻿using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using System.Collections.Generic;
using System.IO;
using Document = iText.Layout.Document;

namespace pdfmerger
{
    public class ITextPdfCreator : IPdfMerger
    {
        public byte[] Create(IEnumerable<(string ContentType, byte[] Content)> blobs)
        {
            using var memoryStream = new MemoryStream();
            using (PdfWriter writer = new PdfWriter(memoryStream))
            {
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
                        using Stream stream = new MemoryStream(blob.Content);
                        var d = new PdfDocument(new PdfReader(stream));
                        d.CopyPagesTo(1, d.GetNumberOfPages(), pdf, pdf.GetNumberOfPages() + 1);
                    }

                    firstIteration = false;
                }
                document.Close();
            }

            return memoryStream.ToArray();
        }
    }
}
