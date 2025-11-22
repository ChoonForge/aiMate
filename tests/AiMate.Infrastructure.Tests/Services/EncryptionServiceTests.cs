using AiMate.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Xunit;

namespace AiMate.Infrastructure.Tests.Services;

/// <summary>
/// Tests for EncryptionService using Data Protection API
/// </summary>
public class EncryptionServiceTests
{
    private readonly EncryptionService _encryptionService;

    public EncryptionServiceTests()
    {
        // Create a test data protection provider (ephemeral - keys only in memory)
        var dataProtectionProvider = DataProtectionProvider.Create("AiMate.Tests");
        _encryptionService = new EncryptionService(dataProtectionProvider);
    }

    [Fact]
    public void Encrypt_WithValidPlaintext_ReturnsEncryptedString()
    {
        // Arrange
        var plaintext = "my-secret-api-key-12345";

        // Act
        var encrypted = _encryptionService.Encrypt(plaintext);

        // Assert
        encrypted.Should().NotBeNullOrEmpty();
        encrypted.Should().NotBe(plaintext, "encrypted value should differ from plaintext");
    }

    [Fact]
    public void Decrypt_WithValidCiphertext_ReturnsOriginalPlaintext()
    {
        // Arrange
        var plaintext = "my-secret-api-key-12345";
        var encrypted = _encryptionService.Encrypt(plaintext);

        // Act
        var decrypted = _encryptionService.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(plaintext);
    }

    [Fact]
    public void Encrypt_SamePlaintext_ReturnsDifferentCiphertexts()
    {
        // Arrange
        var plaintext = "my-secret-api-key";

        // Act
        var encrypted1 = _encryptionService.Encrypt(plaintext);
        var encrypted2 = _encryptionService.Encrypt(plaintext);

        // Assert
        // Data Protection API includes random IV, so same plaintext produces different ciphertexts
        encrypted1.Should().NotBe(encrypted2, "each encryption should use different IV");

        // But both should decrypt to same value
        _encryptionService.Decrypt(encrypted1).Should().Be(plaintext);
        _encryptionService.Decrypt(encrypted2).Should().Be(plaintext);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Encrypt_WithNullOrEmptyPlaintext_ThrowsArgumentException(string? plaintext)
    {
        // Act & Assert
        var act = () => _encryptionService.Encrypt(plaintext!);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be null or empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Decrypt_WithNullOrEmptyCiphertext_ThrowsArgumentException(string? ciphertext)
    {
        // Act & Assert
        var act = () => _encryptionService.Decrypt(ciphertext!);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be null or empty*");
    }

    [Fact]
    public void Decrypt_WithInvalidCiphertext_ThrowsInvalidOperationException()
    {
        // Arrange
        var invalidCiphertext = "this-is-not-valid-encrypted-data";

        // Act & Assert
        var act = () => _encryptionService.Decrypt(invalidCiphertext);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Failed to decrypt*");
    }

    [Fact]
    public void Encrypt_WithLongApiKey_WorksCorrectly()
    {
        // Arrange - typical OpenAI API key format
        var longApiKey = "sk-proj-" + new string('a', 100);

        // Act
        var encrypted = _encryptionService.Encrypt(longApiKey);
        var decrypted = _encryptionService.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(longApiKey);
    }

    [Fact]
    public void Encrypt_WithSpecialCharacters_WorksCorrectly()
    {
        // Arrange
        var specialCharsKey = "key!@#$%^&*()_+{}[]|:;<>?,./~`";

        // Act
        var encrypted = _encryptionService.Encrypt(specialCharsKey);
        var decrypted = _encryptionService.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(specialCharsKey);
    }

    [Fact]
    public void Encrypt_WithUnicodeCharacters_WorksCorrectly()
    {
        // Arrange
        var unicodeKey = "密钥-🔑-clé-Schlüssel";

        // Act
        var encrypted = _encryptionService.Encrypt(unicodeKey);
        var decrypted = _encryptionService.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(unicodeKey);
    }

    [Fact]
    public void Decrypt_WithCiphertextFromDifferentPurpose_ThrowsException()
    {
        // Arrange - create another protector with different purpose
        var dataProtectionProvider = DataProtectionProvider.Create("AiMate.Tests");
        var differentProtector = dataProtectionProvider.CreateProtector("DifferentPurpose");
        var plaintext = "my-secret";
        var encryptedWithDifferentPurpose = differentProtector.Protect(plaintext);

        // Act & Assert - should fail because purpose doesn't match
        var act = () => _encryptionService.Decrypt(encryptedWithDifferentPurpose);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Failed to decrypt*");
    }
}
