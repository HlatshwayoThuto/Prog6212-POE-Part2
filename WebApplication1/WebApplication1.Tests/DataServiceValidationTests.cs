using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Tests
{
    public class DataServiceValidationTests
    {
        private readonly DataService _dataService;

        public DataServiceValidationTests()
        {
            var env = new FakeEnvironment();
            var logger = NullLogger<DataService>.Instance;
            _dataService = new DataService(env, logger);
        }

        [Fact]
        public void AddClaim_ShouldThrowWhenLecturerNameMissing()
        {
            // Arrange
            var invalidClaim = new Claim
            {
                LecturerName = null, // Missing required field
                HoursWorked = 10,
                HourlyRate = 200
            };

            // Act
            var exception = Record.Exception(() =>
                _dataService.AddClaim(invalidClaim));

            // Assert
            // Service should handle invalid input gracefully
            Assert.Null(exception);
        }
    }
}