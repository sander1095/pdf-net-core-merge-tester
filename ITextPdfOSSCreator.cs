using iText.IO.Source;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using Document = iTextSharp.text.Document;
using Image = iTextSharp.text.Image;
using PdfReader = iTextSharp.text.pdf.PdfReader;

namespace pdfmerger
{
    public class ITextPdfOSSCreator : IPdfMerger
    {
        // https://github.com/VahidN/iTextSharp.LGPLv2.Core
        // https://stackoverflow.com/a/6056801/3013479
        // https://stackoverflow.com/a/26111677/3013479
        // https://stackoverflow.com/a/4933889/3013479
        public byte[] Create(IEnumerable<(string ContentType, byte[] Content)> blobs)
        {
            var document = new Document();
            using var memoryStream = new MemoryStream();
            var copy = new PdfCopy(document, memoryStream);

            document.Open();

            //var firstIteration = true;
            foreach (var blob in blobs)
            {
                //TODO: Determine is this is needed? Perhaps you need to use this to prevent overlapping files on 1 page?
                //if (!firstIteration)
                //{
                //    // Add an empty page
                //    copy.AddPage(new Rectangle(0, 0), 0);
                //}

                // TODO: Make the image be on the page correctly
                if (blob.ContentType.StartsWith("image/"))
                {
                    ReadImage(copy, blob.Content);
                }
                else if (blob.ContentType.StartsWith("application/pdf"))
                {
                    ReadPdf(copy, blob.Content);
                }

                //firstIteration = false;
            }

            // step 5: we close the document and writer
            copy.Close();
            document.Close();

            return memoryStream.ToArray();
        }

        private static void ReadPdf(PdfCopy copy, byte[] content)
        {
            var reader = new PdfReader(content);

            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                var page = copy.GetImportedPage(reader, i);
                copy.AddPage(page);
            }

            var form = reader.AcroForm;
            if (form != null)
            {
                copy.CopyAcroForm(reader);
            }
            //TODO: DEtermine if this is necessary.
            // For some reason this fails sometiems?
            //else
            //{
            //    reader.Close();
            //    throw new Exception("AcroForm is null");
            //}

            reader.Close();
        }

        private static void ReadImage(PdfCopy copy, byte[] content)
        {
            var document = new Document();
            using var stream = new ByteArrayOutputStream();
            var writer = PdfWriter.GetInstance(document, stream);

            document.Open();
            if (document.NewPage())
            {
                var image = Image.GetInstance(content);

                var pageWidth = copy.PageSize.Width - (10 + 10);
                var pageHeight = copy.PageSize.Height - (10 + 10);
                image.SetAbsolutePosition(10, 10);
                image.ScaleToFit(pageWidth, pageHeight);

                if (!document.Add(image))
                {
                    document.Close();
                    writer.Close();
                    throw new Exception("Unable to add image to page!");
                }

                document.Close();
                writer.Close();

                var reader = new PdfReader(stream.ToArray());

                copy.AddPage(copy.GetImportedPage(reader, 1));

                reader.Close();
            }
            else
            {
                document.Close();
                throw new Exception("New page could not be created");
            }
        }
    }
}