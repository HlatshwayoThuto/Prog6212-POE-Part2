using System.IO;
using WebApplication1.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace WebApplication1.Tests
{
    public class DataServiceTests
    {
        private readonly DataService _dataService;

        public DataServiceTests()
        {
            var env = new FakeEnvironment(); // mock hosting environment with temp folders
            var logger = NullLogger<DataService>.Instance; // no-op logger
            _dataService = new DataService(env, logger);
        }

        [Fact]
        public void AddClaim_ShouldIncreaseClaimCount()
        {
            var before = _dataService.GetAllClaims().Count;
            _dataService.AddClaim(new Claim { LecturerName = "Test", HoursWorked = 5, HourlyRate = 100 });
            var after = _dataService.GetAllClaims().Count;

            Assert.True(after > before);
        }

        [Fact]
        public void UpdateClaimStatus_ShouldChangeStatusAndApprovedBy()
        {
            var claim = new Claim { LecturerName = "Test2", HoursWorked = 8, HourlyRate = 120 };
            _dataService.AddClaim(claim);

            _dataService.UpdateClaimStatus(claim.ClaimId, "Approved", "Manager");
            var updated = _dataService.GetClaimById(claim.ClaimId);

            Assert.Equal("Approved", updated.Status);
            Assert.Equal("Manager", updated.ApprovedBy);
        }

        [Fact]
        public void GetClaimsByStatus_ShouldReturnOnlyMatchingClaims()
        {
            _dataService.AddClaim(new Claim { LecturerName = "Alice", HoursWorked = 6, HourlyRate = 90, Status = "Pending" });
            _dataService.AddClaim(new Claim { LecturerName = "Bob", HoursWorked = 7, HourlyRate = 100, Status = "Verified" });

            var pending = _dataService.GetClaimsByStatus("Pending");

            Assert.All(pending, c => Assert.Equal("Pending", c.Status));
        }

        [Fact]
        public void AddDocumentToClaim_ShouldAttachDocumentSuccessfully()
        {
            var claim = new Claim { LecturerName = "DocTest", HoursWorked = 10, HourlyRate = 100 };
            _dataService.AddClaim(claim);

            var doc = new Document
            {
                FileName = "test.pdf",
                StoredFileName = "fakepath",
                FileType = ".pdf",
                FileSize = 1234
            };

            _dataService.AddDocumentToClaim(claim.ClaimId, doc);

            var updated = _dataService.GetClaimById(claim.ClaimId);
            Assert.Single(updated.Documents);
            Assert.Equal(".pdf", updated.Documents[0].FileType);
        }

        [Fact]
        public void TotalAmount_ShouldBeCalculatedCorrectly()
        {
            var claim = new Claim { HoursWorked = 10, HourlyRate = 150 };
            Assert.Equal(1500, claim.TotalAmount);
        }
    }

    // ✅ Mock environment compatible with .NET 9 and isolated for tests
    public class FakeEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "TestApp";

        public IFileProvider WebRootFileProvider { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }

        public FakeEnvironment()
        {
            // Use temp folders so tests are isolated and do not depend on disk
            ContentRootPath = Path.Combine(Path.GetTempPath(), "TestContent_" + Guid.NewGuid());
            WebRootPath = Path.Combine(Path.GetTempPath(), "TestWebRoot_" + Guid.NewGuid());
            Directory.CreateDirectory(ContentRootPath);
            Directory.CreateDirectory(WebRootPath);

            WebRootFileProvider = new PhysicalFileProvider(WebRootPath);
            ContentRootFileProvider = new PhysicalFileProvider(ContentRootPath);
        }
    }
}
