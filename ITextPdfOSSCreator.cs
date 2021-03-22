using iText.IO.Source;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using PdfReader = iTextSharp.text.pdf.PdfReader;

namespace pdfmerger
{
    public class ITextPdfOSSCreator : IPdfMerger
    {
        public MemoryStream Create(IEnumerable<(string ContentType, byte[] Content)> blobs)
        {
            return Method1(blobs);
        }

        // https://stackoverflow.com/a/6056801/3013479
        public static MemoryStream Method1(IEnumerable<(string ContentType, byte[] Content)> blobs)
        {
            // step 1: creation of a document-object
            Document document = new Document();
            //create newFileStream object which will be disposed at the end
            using var memoryStream = new MemoryStream();
            // step 2: we create a writer that listens to the document
            PdfCopy copy = new PdfCopy(document, memoryStream);
            if (copy == null)
            {
                return null;
            }

            // step 3: we open the document
            document.Open();

            foreach (var blob in blobs)
            {
                // https://stackoverflow.com/a/26111677/3013479
                if (blob.ContentType.StartsWith("image/"))
                {
                    ReadImage(copy, blob.Content);
                }
                else if (blob.ContentType.StartsWith("application/pdf"))
                {
                    ReadPdf(copy, blob.Content);
                }
            }

            // step 5: we close the document and writer
            copy.Close();
            document.Close();

            return memoryStream;

            static void ReadImage(PdfCopy copy, byte[] content)
            {
                // https://stackoverflow.com/a/26111677/3013479

                Document imageDocument = new Document();
                ByteArrayOutputStream imageDocumentOutputStream = new ByteArrayOutputStream();
                PdfWriter imageDocumentWriter = PdfWriter.GetInstance(imageDocument, imageDocumentOutputStream);

                imageDocument.Open();
                if (imageDocument.NewPage())
                {
                    var image = Image.GetInstance(content);

                    if (!imageDocument.Add(image))
                    {
                        throw new Exception("Unable to add image to page!");
                    }

                    imageDocument.Close();
                    imageDocumentWriter.Close();

                    PdfReader imageDocumentReader = new PdfReader(imageDocumentOutputStream.ToArray());

                    copy.AddPage(copy.GetImportedPage(imageDocumentReader, 1));

                    imageDocumentReader.Close();
                }
            }

            static void ReadPdf(PdfCopy copy, byte[] content)
            {
                // we create a reader for a certain document
                PdfReader reader = new PdfReader(content);
                reader.ConsolidateNamedDestinations();

                // step 4: we add content
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    PdfImportedPage page = copy.GetImportedPage(reader, i);
                    copy.AddPage(page);
                }

                PRAcroForm form = reader.AcroForm;
                if (form != null)
                {
                    copy.CopyAcroForm(reader);
                }

                reader.Close();
            }
        }
    }
}