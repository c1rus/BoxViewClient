namespace BoxViewClient
{
    public class UrlUploadDocument
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public string[] Thumbnails { get; set; }
        public bool NonSvg { get; set; }
    }
}