using System;
using Microsoft.Data.SqlClient;

namespace ValyanClinic.DevSupport
{
    public class AdminPasswordHashFix
    {
      // Updated connection string to use ERP instance where ValyanMed exists
        private const string ConnectionString = "Server=DESKTOP-3Q8HI82\\ERP;Database=ValyanMed;Integrated Security=True;TrustServerCertificate=True;";

        // Entry point for console application
        public static void Main(string[] args)
        {
            Execute();
        }

        // Removed Main method to avoid multiple entry point conflict
        // Use Execute() instead
 public static void Execute()
        {
            Console.WriteLine("============================================");
            Console.WriteLine("VALYANMED - FIX ADMIN PASSWORD HASH");
            Console.WriteLine("============================================");
            Console.WriteLine();
          
            string correctPassword = "admin123!@#";
            
     Console.WriteLine($"Generare BCrypt hash pentru parola: {correctPassword}");
            
    // Use BCrypt.Net.BCrypt with full namespace
     string passwordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword, workFactor: 12);
            
 Console.WriteLine($"Hash generat: {passwordHash.Substring(0, 30)}...");
    Console.WriteLine();
     
      bool isValid = BCrypt.Net.BCrypt.Verify(correctPassword, passwordHash);
    string validStatus = isValid ? "VALID" : "INVALID";
          Console.WriteLine($"Verificare hash: {validStatus}");
            Console.WriteLine();
       
    if (!isValid)
       {
         Console.WriteLine("ERROR: Hash-ul generat nu este valid!");
   return;
     }
   
            Console.WriteLine("Conectare la database...");
       
     try
        {
            using (var connection = new SqlConnection(ConnectionString))
          {
   connection.Open();
               Console.WriteLine("Conectare reusita");
     Console.WriteLine();
       
                    string checkUserSql = "SELECT COUNT(*) FROM Utilizatori WHERE Username = @Username";
    using (var checkCmd = new SqlCommand(checkUserSql, connection))
           {
           checkCmd.Parameters.AddWithValue("@Username", "Admin");
           int userCount = (int)checkCmd.ExecuteScalar();
  
 if (userCount == 0)
      {
              Console.WriteLine("ERROR: Utilizatorul 'Admin' nu exista in database!");
         return;
        }
     
          Console.WriteLine("Utilizator gasit: Admin");
           }
     
          Console.WriteLine();
     Console.WriteLine("Actualizare parola in database...");
             
        string updateSql = @"
    UPDATE Utilizatori 
       SET 
   PasswordHash = @PasswordHash,
              Salt = '',
             DataUltimeiModificari = GETDATE(),
              ModificatDe = 'System_PasswordFix'
        WHERE Username = @Username";
       
     using (var updateCmd = new SqlCommand(updateSql, connection))
                    {
               updateCmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
 updateCmd.Parameters.AddWithValue("@Username", "Admin");
       
         int rowsAffected = updateCmd.ExecuteNonQuery();

            if (rowsAffected > 0)
       {
    Console.WriteLine($"SUCCESS: Parola actualizata! ({rowsAffected} rand(uri))");
      Console.WriteLine();
          
        string selectSql = @"
       SELECT 
 UtilizatorID,
         Username,
       Email,
 Rol,
       EsteActiv,
   LEFT(PasswordHash, 30) + '...' AS PasswordHashPreview,
              DataUltimeiModificari,
       ModificatDe
  FROM Utilizatori
 WHERE Username = @Username";
         
      using (var selectCmd = new SqlCommand(selectSql, connection))
     {
    selectCmd.Parameters.AddWithValue("@Username", "Admin");
   
       using (var reader = selectCmd.ExecuteReader())
   {
         if (reader.Read())
   {
          Console.WriteLine("============================================");
      Console.WriteLine("UTILIZATOR ACTUALIZAT:");
                  Console.WriteLine("============================================");
   Console.WriteLine($"  ID: {reader["UtilizatorID"]}");
      Console.WriteLine($"  Username: {reader["Username"]}");
      Console.WriteLine($"  Email: {reader["Email"]}");
       Console.WriteLine($"  Rol: {reader["Rol"]}");
         Console.WriteLine($"  Activ: {reader["EsteActiv"]}");
          Console.WriteLine($"  Hash (preview): {reader["PasswordHashPreview"]}");
           Console.WriteLine($"  Ultima modificare: {reader["DataUltimeiModificari"]}");
   Console.WriteLine($"  Modificat de: {reader["ModificatDe"]}");
      }
      }
       }
            
           Console.WriteLine();
       Console.WriteLine("============================================");
        Console.WriteLine("SUCCES! Acum poti testa login-ul:");
        Console.WriteLine("============================================");
           Console.WriteLine($"  Username: Admin");
   Console.WriteLine($"  Password: {correctPassword}");
              Console.WriteLine();
       }
  else
           {
     Console.WriteLine("ERROR: Niciun rand nu a fost actualizat!");
          }
           }
    }
            }
 catch (SqlException sqlEx)
            {
         Console.WriteLine($"ERROR SQL: {sqlEx.Message}");
         Console.WriteLine($"   Number: {sqlEx.Number}");
     Console.WriteLine($"   Server: {sqlEx.Server}");
      Console.WriteLine();
  Console.WriteLine("Verifica connection string-ul si accesul la database.");
      }
         catch (Exception ex)
{
       Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine($"   Type: {ex.GetType().FullName}");
  }
        
          Console.WriteLine();
  Console.WriteLine("============================================");
     Console.WriteLine("FINALIZAT");
        Console.WriteLine("============================================");
        }
        
        public static bool TestPassword(string password, string hash)
    {
  return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        
    public static string GenerateHash(string password, int workFactor = 12)
    {
 return BCrypt.Net.BCrypt.HashPassword(password, workFactor);
        }
    }
}
