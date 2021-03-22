using System.Collections.Generic;
using System.Threading.Tasks;

namespace pdfmerger
{
    public interface IPdfMerger
    {
        Task<byte[]> CreateAsync(IEnumerable<(string ContentType, byte[] Content)> blobs);
    }
}
