namespace AiMate.Core.Services;

/// <summary>
/// Service for encrypting and decrypting sensitive data
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypt plaintext string
    /// </summary>
    string Encrypt(string plaintext);

    /// <summary>
    /// Decrypt ciphertext string
    /// </summary>
    string Decrypt(string ciphertext);
}
