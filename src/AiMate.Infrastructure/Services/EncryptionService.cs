using AiMate.Core.Services;
using Microsoft.AspNetCore.DataProtection;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Implementation of encryption service using ASP.NET Core Data Protection API
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly IDataProtector _protector;

    public EncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        // Create a protector with a specific purpose string
        // This ensures keys encrypted with this purpose can only be decrypted by the same purpose
        _protector = dataProtectionProvider.CreateProtector("AiMate.ApiKeys.v1");
    }

    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            throw new ArgumentException("Plaintext cannot be null or empty", nameof(plaintext));

        return _protector.Protect(plaintext);
    }

    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
            throw new ArgumentException("Ciphertext cannot be null or empty", nameof(ciphertext));

        try
        {
            return _protector.Unprotect(ciphertext);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to decrypt ciphertext. The data may be corrupted or encrypted with a different key.", ex);
        }
    }
}
