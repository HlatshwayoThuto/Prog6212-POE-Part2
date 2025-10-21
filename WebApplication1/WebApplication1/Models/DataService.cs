// Required namespaces for JSON serialization, hosting environment access, and logging
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApplication1.Models
{
    // Service class responsible for managing claim and document data
    public class DataService
    {
        // In-memory list to store claims
        private readonly List<Claim> _claims = new();

        // Auto-incrementing IDs for claims and documents
        private int _nextClaimId = 1;
        private int _nextDocumentId = 1;

        // File path for storing serialized claim data
        private readonly string _dataFilePath;

        // Directory path for storing uploaded files
        private readonly string _uploadsFolder;

        // Logger for recording application events and errors
        private readonly ILogger<DataService> _logger;

        // Lock object to ensure thread-safe access to shared resources
        private readonly object _lock = new();

        // Constructor initializes paths and loads existing data
        public DataService(IWebHostEnvironment env, ILogger<DataService> logger)
        {
            _logger = logger;

            // Create App_Data folder if it doesn't exist
            var dataFolder = Path.Combine(env.ContentRootPath, "App_Data");
            if (!Directory.Exists(dataFolder)) Directory.CreateDirectory(dataFolder);

            // Set path for JSON data file
            _dataFilePath = Path.Combine(dataFolder, "claims_data.json");

            // Set path for uploads folder (uses WebRootPath or falls back to wwwroot)
            _uploadsFolder = Path.Combine(env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"), "uploads");
            if (!Directory.Exists(_uploadsFolder)) Directory.CreateDirectory(_uploadsFolder);

            // Load existing claims from disk
            LoadData();
        }

        // Returns the path to the uploads folder
        public string GetUploadsFolder() => _uploadsFolder;

        // Saves current claim data to disk as JSON
        private void SaveData()
        {
            lock (_lock)
            {
                try
                {
                    // Create a container object for serialization
                    var container = new
                    {
                        Claims = _claims,
                        NextClaimId = _nextClaimId,
                        NextDocumentId = _nextDocumentId
                    };

                    // Serialize to JSON with indentation
                    var json = JsonSerializer.Serialize(container, new JsonSerializerOptions { WriteIndented = true });

                    // Write JSON to file
                    File.WriteAllText(_dataFilePath, json);

                    // Log success
                    _logger.LogInformation("Saved data ({Count} claims)", _claims.Count);
                }
                catch (Exception ex)
                {
                    // Log any errors during save
                    _logger.LogError(ex, "Error saving data");
                }
            }
        }

        // Loads claim data from disk into memory
        private void LoadData()
        {
            lock (_lock)
            {
                try
                {
                    // If file doesn't exist, skip loading
                    if (!File.Exists(_dataFilePath)) return;

                    // Read JSON content from file
                    var json = File.ReadAllText(_dataFilePath);

                    // Deserialize into a JsonElement for manual parsing
                    var doc = JsonSerializer.Deserialize<JsonElement>(json);

                    // Extract claims and ID counters
                    var claims = JsonSerializer.Deserialize<List<Claim>>(doc.GetProperty("Claims").GetRawText());
                    var nextClaimId = doc.GetProperty("NextClaimId").GetInt32();
                    var nextDocId = doc.GetProperty("NextDocumentId").GetInt32();

                    // If claims exist, populate memory and update counters
                    if (claims != null)
                    {
                        _claims.Clear();
                        _claims.AddRange(claims);
                        _nextClaimId = Math.Max(1, nextClaimId);
                        _nextDocumentId = Math.Max(1, nextDocId);
                    }

                    // Log success
                    _logger.LogInformation("Loaded {Count} claims", _claims.Count);
                }
                catch (Exception ex)
                {
                    // Log failure and reset state
                    _logger.LogError(ex, "Failed to load data");
                    _claims.Clear();
                    _nextClaimId = 1;
                    _nextDocumentId = 1;
                }
            }
        }

        // Returns a deep copy of all claims
        public List<Claim> GetAllClaims()
        {
            lock (_lock) return _claims.Select(c => Clone(c)).ToList();
        }

        // Retrieves a specific claim by ID
        public Claim? GetClaimById(int id)
        {
            lock (_lock) return _claims.FirstOrDefault(c => c.ClaimId == id);
        }

        // Adds a new claim and assigns a unique ID
        public void AddClaim(Claim claim)
        {
            lock (_lock)
            {
                claim.ClaimId = _nextClaimId++;
                claim.SubmissionDate = DateTime.Now;
                _claims.Add(claim);
                SaveData();
            }
        }

        // Updates the status and approval metadata of a claim
        public void UpdateClaimStatus(int claimId, string status, string approvedBy = "")
        {
            lock (_lock)
            {
                var claim = _claims.FirstOrDefault(c => c.ClaimId == claimId);
                if (claim == null) return;

                claim.Status = status;
                claim.ApprovalDate = DateTime.Now;
                claim.ApprovedBy = approvedBy;

                SaveData();
            }
        }

        // Adds a document to a specific claim
        public void AddDocumentToClaim(int claimId, Document doc)
        {
            lock (_lock)
            {
                var claim = _claims.FirstOrDefault(c => c.ClaimId == claimId);
                if (claim == null) return;

                doc.DocumentId = _nextDocumentId++;
                doc.ClaimId = claimId;
                doc.UploadDate = DateTime.Now;

                claim.Documents.Add(doc);
                SaveData();
            }
        }

        // Retrieves claims filtered by status (e.g., "Pending", "Approved")
        public List<Claim> GetClaimsByStatus(string status)
        {
            lock (_lock)
            {
                return _claims
                    .Where(c => string.Equals(c.Status, status, StringComparison.OrdinalIgnoreCase))
                    .Select(c => Clone(c))
                    .ToList();
            }
        }

        // Creates a deep copy of a claim and its documents to avoid exposing internal references
        private Claim Clone(Claim c)
        {
            return new Claim
            {
                ClaimId = c.ClaimId,
                LecturerName = c.LecturerName,
                HoursWorked = c.HoursWorked,
                HourlyRate = c.HourlyRate,
                Notes = c.Notes,
                Status = c.Status,
                SubmissionDate = c.SubmissionDate,
                ApprovalDate = c.ApprovalDate,
                ApprovedBy = c.ApprovedBy,
                Documents = c.Documents.Select(d => new Document
                {
                    DocumentId = d.DocumentId,
                    ClaimId = d.ClaimId,
                    FileName = d.FileName,
                    StoredFileName = d.StoredFileName,
                    UploadDate = d.UploadDate,
                    FileSize = d.FileSize,
                    FileType = d.FileType
                }).ToList()
            };
        }
    }
}