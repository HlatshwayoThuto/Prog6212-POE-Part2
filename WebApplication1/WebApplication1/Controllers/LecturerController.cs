// Import necessary namespaces
using Microsoft.AspNetCore.Mvc;       // Provides MVC controller functionality
using WebApplication1.Models;         // Includes application-specific models like Claim, Document, and services like DataService

namespace WebApplication1.Controllers
{
    // Defines a controller named LecturerController, inheriting from ASP.NET Core's Controller base class
    public class LecturerController : Controller
    {
        // Private fields for injected services
        private readonly DataService _dataService;       // Handles data operations like storing claims and documents
        private readonly IFileProtector _protector;      // Handles encryption and decryption of uploaded files

        // Allowed file types for uploads
        private readonly string[] _allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };

        // Maximum allowed file size (5MB)
        private const long MAX_FILE_BYTES = 5 * 1024 * 1024;

        // Constructor that receives dependencies via dependency injection
        public LecturerController(DataService dataService, IFileProtector protector)
        {
            _dataService = dataService;
            _protector = protector;
        }

        // GET action to display the claim submission form
        [HttpGet]
        public IActionResult SubmitClaim()
        {
            // Returns the view with a new Claim model instance
            return View(new Claim());
        }

        // POST action to handle claim submission with optional file upload
        [HttpPost]
        [ValidateAntiForgeryToken] // Prevents CSRF attacks
        public async Task<IActionResult> SubmitClaim(Claim claim, IFormFile? supportingDocument)
        {
            // If the model is invalid, redisplay the form with validation messages
            if (!ModelState.IsValid)
                return View(claim);

            try
            {
                // Save the claim metadata to the database (without file)
                _dataService.AddClaim(claim);

                // If a file is uploaded, validate and encrypt it
                if (supportingDocument != null && supportingDocument.Length > 0)
                {
                    // Get the file extension in lowercase
                    var ext = Path.GetExtension(supportingDocument.FileName).ToLowerInvariant();

                    // Validate file type
                    if (!_allowedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError("", "Invalid file type. Only PDF, DOCX and XLSX allowed.");
                        return View(claim);
                    }

                    // Validate file size
                    if (supportingDocument.Length > MAX_FILE_BYTES)
                    {
                        ModelState.AddModelError("", "File size exceeds 5MB limit.");
                        return View(claim);
                    }

                    // Get the folder path where files are stored
                    var uploadsFolder = _dataService.GetUploadsFolder();

                    // Encrypt and save the file, returning the stored filename
                    var storedName = await _protector.SaveEncryptedAsync(supportingDocument, uploadsFolder);

                    // Create a Document object to associate with the claim
                    var doc = new Document
                    {
                        FileName = supportingDocument.FileName,     // Original filename
                        StoredFileName = storedName,               // Encrypted filename
                        FileSize = supportingDocument.Length,      // File size in bytes
                        FileType = ext                             // File extension
                    };

                    // Link the document to the claim in the database
                    _dataService.AddDocumentToClaim(claim.ClaimId, doc);
                }

                // Store a success message to display after redirection
                TempData["SuccessMessage"] = "Claim submitted successfully.";

                // Redirect to the claim tracking page
                return RedirectToAction("TrackClaims");
            }
            catch (Exception ex)
            {
                // If an error occurs, display it on the form
                ModelState.AddModelError("", $"Error submitting claim: {ex.Message}");
                return View(claim);
            }
        }

        // Action to display all submitted claims for tracking
        public IActionResult TrackClaims()
        {
            // Retrieve all claims from the data service
            var claims = _dataService.GetAllClaims();

            // Pass the claims to the view for display
            return View(claims);
        }

        // Action to download a previously uploaded and encrypted document
        public async Task<IActionResult> DownloadDocument(int documentId)
        {
            // Find the document by ID from all claims
            var doc = _dataService.GetAllClaims()
                .SelectMany(c => c.Documents) // Flatten all documents from all claims
                .FirstOrDefault(d => d.DocumentId == documentId); // Find the matching document

            // If not found, return 404
            if (doc == null) return NotFound();

            // Get the folder path where encrypted files are stored
            var uploads = _dataService.GetUploadsFolder();

            try
            {
                // Decrypt and open the file stream
                var stream = await _protector.OpenDecryptedAsync(uploads, doc.StoredFileName);

                // Determine the correct MIME type based on file extension
                var contentType = doc.FileType.ToLowerInvariant() switch
                {
                    ".pdf" => "application/pdf",
                    ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    _ => "application/octet-stream" // Default for unknown types
                };

                // Return the file stream to the browser for download
                return File(stream, contentType, doc.FileName);
            }
            catch (FileNotFoundException)
            {
                // If the file is missing on disk, return 404 with a message
                return NotFound("File not found on server.");
            }
        }
    }
}