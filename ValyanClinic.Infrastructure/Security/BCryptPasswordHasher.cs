using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using ValyanClinic.Domain.Interfaces.Security;

namespace ValyanClinic.Infrastructure.Security;

/// <summary>
/// Implementare BCrypt pentru hashing parole
/// Folosește BCrypt.Net-Next cu Work Factor 12 (recomandat pentru 2025)
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12; // Securitate balansată (2025 standard)
    
    /// <summary>
    /// Hash-uiește parola folosind BCrypt Work Factor 12
    /// BCrypt generează automat salt-ul și îl include în hash
    /// </summary>
    public string HashPassword(string password)
 {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password), "Password cannot be null or empty");
  
   // BCrypt.HashPassword generează automat salt și returnează hash complet
        // Format: $2a$12$[22 chars salt][31 chars hash]
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }
    
    /// <summary>
    /// Verifică parola față de hash-ul BCrypt
    /// BCrypt extrage automat salt-ul din hash pentru verificare
    /// </summary>
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password))
  return false;
   
    if (string.IsNullOrEmpty(hash))
       return false;
      
        try
    {
      // BCrypt.Verify face verificarea automată (extrage salt din hash)
     return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            // În caz de hash invalid sau corrupt
    return false;
        }
    }
    
    /// <summary>
    /// Generează o parolă aleatoare sigură
/// Folosită pentru "Generează parolă automată" din UI
    /// </summary>
    public string GenerateRandomPassword(int length = 12, bool includeSpecialChars = true)
    {
        if (length < 8)
       throw new ArgumentException("Password length must be at least 8 characters", nameof(length));
        
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
     const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
 const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        
 var allChars = lowercase + uppercase + digits;
   if (includeSpecialChars)
      allChars += specialChars;
        
   var password = new StringBuilder(length);
      using var rng = RandomNumberGenerator.Create();
    
        // Asigură că parola conține cel puțin:
        // - 1 literă mică
        // - 1 literă mare
        // - 1 cifră
        // - 1 caracter special (dacă includeSpecialChars = true)
     password.Append(GetRandomChar(lowercase, rng));
        password.Append(GetRandomChar(uppercase, rng));
        password.Append(GetRandomChar(digits, rng));
        
    if (includeSpecialChars)
password.Append(GetRandomChar(specialChars, rng));
        
        // Completează restul cu caractere aleatorii
        for (int i = password.Length; i < length; i++)
        {
            password.Append(GetRandomChar(allChars, rng));
        }
  
        // Amestecă caracterele pentru randomizare
        return ShuffleString(password.ToString(), rng);
    }
    
    private static char GetRandomChar(string chars, RandomNumberGenerator rng)
    {
        var randomBytes = new byte[4];
        rng.GetBytes(randomBytes);
        var randomValue = BitConverter.ToUInt32(randomBytes, 0);
        var index = randomValue % (uint)chars.Length;
return chars[(int)index];
    }
    
    private static string ShuffleString(string input, RandomNumberGenerator rng)
    {
        var array = input.ToCharArray();
        var n = array.Length;
        
      while (n > 1)
        {
         var randomBytes = new byte[4];
            rng.GetBytes(randomBytes);
            var k = (int)(BitConverter.ToUInt32(randomBytes, 0) % (uint)n);
          n--;
            (array[n], array[k]) = (array[k], array[n]);
        }
 
        return new string(array);
  }
}
