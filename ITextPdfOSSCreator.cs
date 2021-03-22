using System.Collections.Generic;
using System.IO;

namespace pdfmerger
{
    public class ITextPdfOSSCreator : IPdfMerger
    {
        public MemoryStream Create(IEnumerable<(string ContentType, byte[] Content)> blobs)
        {
            throw new System.NotImplementedException();
        }
    }
}
