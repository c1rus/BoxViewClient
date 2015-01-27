using System.IO;

namespace BoxViewClient
{
    public class MultipartUploadDocumentRequest
    {
        /// <summary>
        /// The filename used to detect the content type of the file stream.
        /// </summary>
        public string Filename { get; set; }
        
        /// <summary>
        /// The file stream.
        /// </summary>
        public Stream File { get; set; }
        
        /// <summary>
        /// The name of this document.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Comma-separated list of thumbnail dimensions of the format {width}x{height} e.g. 128×128,256×256 – width can be between 16 and 1024, height between 16 and 768.
        /// </summary>
        public string Thumbnails { get; set; }

        /// <summary>
        /// Whether to also create the non-svg version of the document (default=false).
        /// </summary>
        public bool? NonSvg { get; set; }
    }
}