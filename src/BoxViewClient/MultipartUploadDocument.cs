using System.IO;

namespace BoxViewClient
{
    public class MultipartUploadDocument
    {
        public Stream File { get; set; }
        public string Name { get; set; }
        public string[] Thumbnails { get; set; }
        public bool NonSvg { get; set; }
    }
}