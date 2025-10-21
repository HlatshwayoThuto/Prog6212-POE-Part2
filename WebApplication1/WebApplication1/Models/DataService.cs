using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApplication1.Models
{
    public class DataService
    {
        private readonly List<Claim> _claims = new();
        private int _nextClaimId = 1;
        private int _nextDocumentId = 1;
        private readonly string _dataFilePath;
        private readonly string _uploadsFolder;
        private readonly ILogger<DataService> _logger;
        private readonly object _lock = new();

        public DataService(IWebHostEnvironment env, ILogger<DataService> logger)
        {
            _logger = logger;

            var dataFolder = Path.Combine(env.ContentRootPath, "App_Data");
            if (!Directory.Exists(dataFolder)) Directory.CreateDirectory(dataFolder);

            _dataFilePath = Path.Combine(dataFolder, "claims_data.json");

            _uploadsFolder = Path.Combine(env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"), "uploads");
            if (!Directory.Exists(_uploadsFolder)) Directory.CreateDirectory(_uploadsFolder);

            LoadData();
        }

        public string GetUploadsFolder() => _uploadsFolder;

        private void SaveData()
        {
            lock (_lock)
            {
                try
                {
                    var container = new
                    {
                        Claims = _claims,
                        NextClaimId = _nextClaimId,
                        NextDocumentId = _nextDocumentId
                    };
                    var json = JsonSerializer.Serialize(container, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_dataFilePath, json);
                    _logger.LogInformation("Saved data ({Count} claims)", _claims.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving data");
                }
            }
        }

        private void LoadData()
        {
            lock (_lock)
            {
                try
                {
                    if (!File.Exists(_dataFilePath)) return;
                    var json = File.ReadAllText(_dataFilePath);
                    var doc = JsonSerializer.Deserialize<JsonElement>(json);
                    // read claims array
                    var claims = JsonSerializer.Deserialize<List<Claim>>(doc.GetProperty("Claims").GetRawText());
                    var nextClaimId = doc.GetProperty("NextClaimId").GetInt32();
                    var nextDocId = doc.GetProperty("NextDocumentId").GetInt32();

                    if (claims != null)
                    {
                        _claims.Clear();
                        _claims.AddRange(claims);
                        _nextClaimId = Math.Max(1, nextClaimId);
                        _nextDocumentId = Math.Max(1, nextDocId);
                    }
                    _logger.LogInformation("Loaded {Count} claims", _claims.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load data");
                    _claims.Clear();
                    _nextClaimId = 1;
                    _nextDocumentId = 1;
                }
            }
        }

        public List<Claim> GetAllClaims()
        {
            lock (_lock) return _claims.Select(c => Clone(c)).ToList();
        }

        public Claim? GetClaimById(int id)
        {
            lock (_lock) return _claims.FirstOrDefault(c => c.ClaimId == id);
        }

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

        public List<Claim> GetClaimsByStatus(string status)
        {
            lock (_lock)
            {
                return _claims.Where(c => string.Equals(c.Status, status, StringComparison.OrdinalIgnoreCase))
                              .Select(c => Clone(c)).ToList();
            }
        }

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
