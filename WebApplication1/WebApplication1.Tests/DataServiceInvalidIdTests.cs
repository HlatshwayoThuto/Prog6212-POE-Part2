using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Tests
{
    public class DataServiceInvalidIdTests
    {
        private readonly DataService _dataService;

        public DataServiceInvalidIdTests()
        {
            var env = new FakeEnvironment();
            var logger = NullLogger<DataService>.Instance;
            _dataService = new DataService(env, logger);
        }

        [Fact]
        public void UpdateClaimStatus_ShouldHandleInvalidIdGracefully()
        {
            // Arrange
            var invalidId = 9999; // Non-existent ID

            // Act
            var exception = Record.Exception(() =>
                _dataService.UpdateClaimStatus(invalidId, "Approved", "Manager"));

            // Assert
            // Should not throw any unhandled exceptions
            Assert.Null(exception);
        }
    }
}


