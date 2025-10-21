using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1.Tests
{
    public class FileProtectorTests
    {
        private readonly IFileProtector _protector;
        private readonly string _uploadsFolder;

        public FileProtectorTests()
        {
            var config = new ConfigurationBuilder().Build();
            var env = new FakeEnvironment();

            _protector = new FileProtector(config, env);

            _uploadsFolder = Path.Combine(env.WebRootPath, "uploads_test");
            if (Directory.Exists(_uploadsFolder))
                Directory.Delete(_uploadsFolder, true);

            Directory.CreateDirectory(_uploadsFolder); // ensure folder exists
        }

        [Fact]
        public async Task Encrypt_And_Decrypt_ShouldReturnSameContent()
        {
            var content = "Hello, encryption test!";
            var bytes = Encoding.UTF8.GetBytes(content);
            using var stream = new MemoryStream(bytes);
            var formFile = new FormFile(stream, 0, bytes.Length, "Data", "test.txt");

            var storedFile = await _protector.SaveEncryptedAsync(formFile, _uploadsFolder);
            var decryptedStream = await _protector.OpenDecryptedAsync(_uploadsFolder, storedFile);

            using var reader = new StreamReader(decryptedStream);
            var decryptedContent = reader.ReadToEnd();

            Assert.Equal(content, decryptedContent);
        }
    }
}