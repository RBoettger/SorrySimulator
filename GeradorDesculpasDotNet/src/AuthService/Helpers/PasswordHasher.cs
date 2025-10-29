using System.Linq;
using System.Security.Cryptography;

namespace AuthService.Helpers;

public static class PasswordHasher
{
    public static (byte[] Hash, byte[] Salt, string Algo, int Iterations) HashPassword(string password)
    {
        const int saltSize = 16;    
        const int hashSize = 32;     
        const int iterations = 100_000;
        const string algo = "PBKDF2-SHA256";

        var salt = RandomNumberGenerator.GetBytes(saltSize);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(hashSize);

        return (hash, salt, algo, iterations);
    }

    // 🔹 Validação da senha
    public static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt, int iterations)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, storedSalt, iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(storedHash.Length);
        return hash.SequenceEqual(storedHash);
    }
}
