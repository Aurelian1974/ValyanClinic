using System;
using Microsoft.Data.SqlClient;

namespace ValyanClinic.DevSupport
{
    public class FixValeriaPassword
    {
        private const string ConnectionString = "Server=DESKTOP-3Q8HI82\\ERP;Database=ValyanMed;Integrated Security=True;TrustServerCertificate=True;";

        public static void Execute()
        {
 Console.WriteLine("============================================");
            Console.WriteLine("FIX VALERIA PASSWORD");
            Console.WriteLine("============================================");
       Console.WriteLine();

  string correctPassword = "Valeria1973!";
            
     Console.WriteLine($"Generating BCrypt hash for password: {correctPassword}");
  
    // Generate hash
     string passwordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword, workFactor: 12);
  
 Console.WriteLine($"Hash generated: {passwordHash}");
            Console.WriteLine();
      
          // Verify it works
 bool isValid = BCrypt.Net.BCrypt.Verify(correctPassword, passwordHash);
Console.WriteLine($"Verification test: {(isValid ? "✓ VALID" : "✗ INVALID")}");
            Console.WriteLine();
            
            if (!isValid)
  {
  Console.WriteLine("ERROR: Hash generation failed!");
   return;
          }
  
            Console.WriteLine("Connecting to database...");
 
            try
   {
                using (var connection = new SqlConnection(ConnectionString))
             {
     connection.Open();
           Console.WriteLine("✓ Connected");
        Console.WriteLine();
     
      // Check if user exists
  string checkSql = "SELECT Username, EsteActiv, NumarIncercariEsuate FROM Utilizatori WHERE Username = @Username";
      using (var checkCmd = new SqlCommand(checkSql, connection))
 {
          checkCmd.Parameters.AddWithValue("@Username", "valeria");
         using (var reader = checkCmd.ExecuteReader())
             {
            if (!reader.Read())
       {
                  Console.WriteLine("✗ User 'valeria' not found!");
           return;
          }
           Console.WriteLine($"User found: {reader["Username"]}");
          Console.WriteLine($"  Active: {reader["EsteActiv"]}");
         Console.WriteLine($"  Failed attempts: {reader["NumarIncercariEsuate"]}");
            }
      }
         
        Console.WriteLine();
  Console.WriteLine("Updating password...");
   
          string updateSql = @"
         UPDATE Utilizatori
      SET 
     PasswordHash = @PasswordHash,
 Salt = '',
   NumarIncercariEsuate = 0,
         DataBlocare = NULL,
          EsteActiv = 1,
              DataUltimeiModificari = GETDATE(),
 ModificatDe = 'DevSupport_PasswordFix'
            WHERE Username = @Username";
          
  using (var updateCmd = new SqlCommand(updateSql, connection))
      {
  updateCmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
      updateCmd.Parameters.AddWithValue("@Username", "valeria");
   
                  int rowsAffected = updateCmd.ExecuteNonQuery();
          
    if (rowsAffected > 0)
            {
      Console.WriteLine($"✓ Password updated successfully! ({rowsAffected} row(s))");
    Console.WriteLine();
      Console.WriteLine("============================================");
      Console.WriteLine("CREDENTIALS");
                  Console.WriteLine("============================================");
       Console.WriteLine($"  Username: valeria");
  Console.WriteLine($"  Password: {correctPassword}");
           Console.WriteLine();
   Console.WriteLine("✓ You can now login with these credentials!");
     }
        else
       {
            Console.WriteLine("✗ Update failed - no rows affected!");
      }
  }
      }
    }
    catch (Exception ex)
     {
    Console.WriteLine($"✗ ERROR: {ex.Message}");
            }
  
       Console.WriteLine();
   Console.WriteLine("============================================");
        }
    }
}
