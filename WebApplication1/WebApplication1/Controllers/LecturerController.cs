using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class LecturerController : Controller
    {
        private readonly DataService _dataService;
        private readonly IFileProtector _protector;

        private readonly string[] _allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
        private const long MAX_FILE_BYTES = 5 * 1024 * 1024; // 5MB

        public LecturerController(DataService dataService, IFileProtector protector)
        {
            _dataService = dataService;
            _protector = protector;
        }

        [HttpGet]
        public IActionResult SubmitClaim()
        {
            return View(new Claim());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(Claim claim, IFormFile? supportingDocument)
        {
            if (!ModelState.IsValid)
                return View(claim);

            try
            {
                // Add claim metadata (no file yet)
                _dataService.AddClaim(claim);

                // If there is a file - validate and save encrypted
                if (supportingDocument != null && supportingDocument.Length > 0)
                {
                    var ext = Path.GetExtension(supportingDocument.FileName).ToLowerInvariant();
                    if (!_allowedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError("", "Invalid file type. Only PDF, DOCX and XLSX allowed.");
                        return View(claim);
                    }

                    if (supportingDocument.Length > MAX_FILE_BYTES)
                    {
                        ModelState.AddModelError("", "File size exceeds 5MB limit.");
                        return View(claim);
                    }

                    // Save encrypted
                    var uploadsFolder = _dataService.GetUploadsFolder();
                    var storedName = await _protector.SaveEncryptedAsync(supportingDocument, uploadsFolder);

                    var doc = new Document
                    {
                        FileName = supportingDocument.FileName,
                        StoredFileName = storedName,
                        FileSize = supportingDocument.Length,
                        FileType = ext
                    };

                    _dataService.AddDocumentToClaim(claim.ClaimId, doc);
                }

                TempData["SuccessMessage"] = "Claim submitted successfully.";
                return RedirectToAction("TrackClaims");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error submitting claim: {ex.Message}");
                return View(claim);
            }
        }

        public IActionResult TrackClaims()
        {
            var claims = _dataService.GetAllClaims();
            return View(claims);
        }

        // Download decrypted file stream
        public async Task<IActionResult> DownloadDocument(int documentId)
        {
            var doc = _dataService.GetAllClaims()
                .SelectMany(c => c.Documents)
                .FirstOrDefault(d => d.DocumentId == documentId);

            if (doc == null) return NotFound();

            var uploads = _dataService.GetUploadsFolder();
            try
            {
                var stream = await _protector.OpenDecryptedAsync(uploads, doc.StoredFileName);
                var contentType = doc.FileType.ToLowerInvariant() switch
                {
                    ".pdf" => "application/pdf",
                    ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    _ => "application/octet-stream"
                };
                return File(stream, contentType, doc.FileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound("File not found on server.");
            }
        }
    }
}
