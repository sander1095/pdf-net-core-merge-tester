using System.Collections.Generic;
using System.IO;

namespace pdfmerger
{
    public interface IPdfMerger
    {
        MemoryStream Create(IEnumerable<(string ContentType, byte[] Content)> blobs);
    }
}
