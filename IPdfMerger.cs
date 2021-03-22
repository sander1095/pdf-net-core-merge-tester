using System.Collections.Generic;

namespace pdfmerger
{
    public interface IPdfMerger
    {
        byte[] Create(IEnumerable<(string ContentType, byte[] Content)> blobs);
    }
}
