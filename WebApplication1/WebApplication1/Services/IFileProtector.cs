using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;

public interface IFileProtector
{
    /// <summary>Save the uploaded file encrypted on disk. Returns the stored filename.</summary>
    Task<string> SaveEncryptedAsync(IFormFile file, string uploadsFolder);

    /// <summary>Open a decrypted stream for the stored encrypted file.</summary>
    Task<Stream> OpenDecryptedAsync(string uploadsFolder, string storedFileName);

    /// <summary>Ensure encryption key exists and returns a base64 key:iv string.</summary>
    string GetKeyIdentifier();
}

public class FileProtector : IFileProtector
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public FileProtector(IConfiguration config, IWebHostEnvironment env)
    {
        // Create or load key in App_Data folder
        var keyFile = Path.Combine(env.ContentRootPath, "App_Data", "file_key.txt");
        if (!Directory.Exists(Path.GetDirectoryName(keyFile)))
            Directory.CreateDirectory(Path.GetDirectoryName(keyFile)!);

        if (File.Exists(keyFile))
        {
            var combined = File.ReadAllText(keyFile);
            var parts = combined.Split(':');
            _key = Convert.FromBase64String(parts[0]);
            _iv = Convert.FromBase64String(parts[1]);
        }
        else
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();
            aes.GenerateIV();
            _key = aes.Key;
            _iv = aes.IV;
            var combined = Convert.ToBase64String(_key) + ":" + Convert.ToBase64String(_iv);
            File.WriteAllText(keyFile, combined);
        }
    }

    public string GetKeyIdentifier() => Convert.ToBase64String(_key) + ":" + Convert.ToBase64String(_iv);

    public async Task<string> SaveEncryptedAsync(IFormFile file, string uploadsFolder)
    {
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        var storedFileName = $"{Guid.NewGuid():N}.bin";
        var storedPath = Path.Combine(uploadsFolder, storedFileName);

        using var outFs = new FileStream(storedPath, FileMode.CreateNew);
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        using var crypto = new CryptoStream(outFs, aes.CreateEncryptor(), CryptoStreamMode.Write);
        await file.CopyToAsync(crypto);
        await crypto.FlushAsync();
        return storedFileName;
    }

    public Task<Stream> OpenDecryptedAsync(string uploadsFolder, string storedFileName)
    {
        var storedPath = Path.Combine(uploadsFolder, storedFileName);
        if (!File.Exists(storedPath)) throw new FileNotFoundException("Encrypted file missing.", storedPath);

        var mem = new MemoryStream();
        using (var inFs = File.OpenRead(storedPath))
        using (var aes = Aes.Create())
        using (var crypto = new CryptoStream(inFs, aes.CreateDecryptor(_key, _iv), CryptoStreamMode.Read))
        {
            crypto.CopyTo(mem);
        }
        mem.Position = 0;
        return Task.FromResult<Stream>(mem);
    }
}
