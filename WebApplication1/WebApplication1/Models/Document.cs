namespace WebApplication1.Models
{
    public class Document
    {
        public int DocumentId { get; set; }
        public int ClaimId { get; set; }
        public string FileName { get; set; } = string.Empty;   // original file name (display)
        public string StoredFileName { get; set; } = string.Empty; // encrypted file on disk
        public DateTime UploadDate { get; set; } = DateTime.Now;
        public long FileSize { get; set; }
        public string FileType { get; set; } = string.Empty; // extension, e.g. .pdf
    }
}
