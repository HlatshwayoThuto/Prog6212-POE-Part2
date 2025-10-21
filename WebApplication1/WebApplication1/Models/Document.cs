namespace WebApplication1.Models
{
    // Represents a file uploaded in support of a claim
    public class Document
    {
        // Unique identifier for the document (auto-incremented)
        public int DocumentId { get; set; }

        // Foreign key linking this document to a specific claim
        public int ClaimId { get; set; }

        // The original name of the uploaded file (used for display purposes)
        public string FileName { get; set; } = string.Empty;

        // The name of the file as stored on disk (typically encrypted or renamed for security)
        public string StoredFileName { get; set; } = string.Empty;

        // Timestamp indicating when the file was uploaded (defaults to current time)
        public DateTime UploadDate { get; set; } = DateTime.Now;

        // Size of the file in bytes
        public long FileSize { get; set; }

        // File extension/type (e.g., .pdf, .docx, .xlsx)
        public string FileType { get; set; } = string.Empty;
    }
}