using System.Security.Cryptography;
using System.Text;
using InsureFlow.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace InsureFlow.Infrastructure.Security;

public sealed class TokenEncryptionService(IConfiguration configuration) : ITokenEncryptionService
{
    private readonly byte[] _key = Convert.FromBase64String(configuration["Encryption:Key"] ?? throw new InvalidOperationException("Encryption:Key is required."));

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        return $"{Convert.ToBase64String(aes.IV)}.{Convert.ToBase64String(cipherBytes)}";
    }

    public string Decrypt(string cipherText)
    {
        var parts = cipherText.Split('.', 2);
        if (parts.Length != 2)
        {
            throw new InvalidOperationException("Invalid encrypted token payload.");
        }

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = Convert.FromBase64String(parts[0]);
        using var decryptor = aes.CreateDecryptor();
        var cipherBytes = Convert.FromBase64String(parts[1]);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
