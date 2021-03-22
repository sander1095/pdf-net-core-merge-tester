using iTextSharp.text;
using iTextSharp.text.pdf;
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
            return Method2(blobs);
        }

        // https://stackoverflow.com/a/6056801/3013479
        public static MemoryStream Method1(IEnumerable<(string ContentType, byte[] Content)> blobs)
        {
            // step 1: creation of a document-object
            Document document = new Document();
            //create newFileStream object which will be disposed at the end
            using var memoryStream = new MemoryStream();
            // step 2: we create a writer that listens to the document
            PdfCopy copier = new PdfCopy(document, memoryStream);
            if (copier == null)
            {
                return null;
            }

            // step 3: we open the document
            document.Open();

            foreach (var blob in blobs)
            {
                if (blob.ContentType.StartsWith("image/"))
                {
                    ReadImage(copier, blob.Content);
                }
                else if (blob.ContentType.StartsWith("application/pdf"))
                {
                    ReadPdf(copier, blob.Content);
                }
            }

            // step 5: we close the document and writer
            copier.Close();
            document.Close();

            return memoryStream;

            static void ReadImage(PdfCopy copier, byte[] content)
            {
                iTextSharp.text.Image pic = iTextSharp.text.Image.GetInstance(content);

                if (pic.Height > pic.Width)
                {
                    //Maximum height is 800 pixels.
                    float percentage = 0.0f;
                    percentage = 700 / pic.Height;
                    pic.ScalePercent(percentage * 100);
                }
                else
                {
                    //Maximum width is 600 pixels.
                    float percentage = 0.0f;
                    percentage = 540 / pic.Width;
                    pic.ScalePercent(percentage * 100);
                }

                //pic.Border = iTextSharp.text.Rectangle.BOX;
                //pic.BorderColor = iTextSharp.text.BaseColor.BLACK;
                //pic.BorderWidth = 3f;
                copier.Add(pic);
            }

            static void ReadPdf(PdfCopy copier, byte[] content)
            {
                // we create a reader for a certain document
                PdfReader reader = new PdfReader(content);
                reader.ConsolidateNamedDestinations();

                // step 4: we add content
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    PdfImportedPage page = copier.GetImportedPage(reader, i);
                    copier.AddPage(page);
                    copier.AddPage()
                }

                PRAcroForm form = reader.AcroForm;
                if (form != null)
                {
                    copier.CopyAcroForm(reader);
                }

                reader.Close();
            }
        }


        // https://stackoverflow.com/a/26883360/3013479
        private static MemoryStream Method2(IEnumerable<(string ContentType, byte[] Content)> blobs)
        {
            using (var memoryStream = new MemoryStream())
            {
                Document document = new Document();
                PdfCopy pdf = new PdfCopy(document, memoryStream);
                PdfReader reader = null;
                try
                {
                    document.Open();
                    foreach (var blob in blobs)
                    {
                        reader = new PdfReader(blob.Content);
                        // TODO: Apparently you sohuldnt use this acro method?
                        pdf.CopyAcroForm(reader);
                        reader.Close();
                    }

                    return memoryStream;
                }
                finally
                {
                    document?.Close();
                    reader?.Close();
                    pdf?.Close();
                }
            }
        }
    }

}
