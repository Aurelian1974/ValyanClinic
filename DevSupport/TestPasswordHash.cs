using System;
using BCrypt.Net;

namespace ValyanClinic.DevSupport
{
    public class TestPasswordHash
    {
        public static void TestValeriaPassword()
  {
       Console.WriteLine("============================================");
        Console.WriteLine("TEST PASSWORD HASH - VALERIA");
        Console.WriteLine("============================================");
 Console.WriteLine();

  string username = "valeria";
string password = "Valeria1973!";
  
         // Simulează hash-ul din database (înlocuiește cu hash-ul real)
     // Run SQL query mai întâi și copy hash-ul aici:
            string hashFromDB = "PASTE_HASH_FROM_DATABASE_HERE";
 
Console.WriteLine($"Username: {username}");
    Console.WriteLine($"Password: {password}");
            Console.WriteLine();
   
   // Test 1: Generate new hash și verifică
  Console.WriteLine("Test 1: Generate fresh hash");
  Console.WriteLine("----------------------------");
      string freshHash = BCrypt.Net.BCrypt.HashPassword(password, 12);
         Console.WriteLine($"Fresh hash: {freshHash}");
  
    bool freshVerify = BCrypt.Net.BCrypt.Verify(password, freshHash);
Console.WriteLine($"Fresh hash verify: {(freshVerify ? "✓ PASS" : "✗ FAIL")}");
        Console.WriteLine();
         
     // Test 2: Verifică cu hash-ul din database
        if (hashFromDB != "PASTE_HASH_FROM_DATABASE_HERE")
{
   Console.WriteLine("Test 2: Verify against database hash");
     Console.WriteLine("-------------------------------------");
    Console.WriteLine($"DB hash: {hashFromDB}");
     
       try
     {
                  bool dbVerify = BCrypt.Net.BCrypt.Verify(password, hashFromDB);
           Console.WriteLine($"DB hash verify: {(dbVerify ? "✓ PASS" : "✗ FAIL")}");
     
     if (!dbVerify)
          {
          Console.WriteLine();
         Console.WriteLine("⚠ PROBLEM DETECTED:");
    Console.WriteLine("  Password does NOT match database hash!");
      Console.WriteLine("  Possible causes:");
       Console.WriteLine("    1. Password was typed incorrectly when creating user");
            Console.WriteLine("    2. Hash in database is corrupted");
            Console.WriteLine("    3. Different password was used");
             Console.WriteLine();
         Console.WriteLine("  Solution: Run Create-User.ps1 with -UpdateExisting");
               }
       }
     catch (Exception ex)
       {
        Console.WriteLine($"✗ ERROR: {ex.Message}");
  Console.WriteLine("  Hash format is invalid!");
     }
   }
  else
            {
        Console.WriteLine("⚠ Paste hash from database first!");
            }
   
        Console.WriteLine();
       Console.WriteLine("============================================");
        }
        
        public static void TestAnyPassword(string password, string hash)
        {
            Console.WriteLine($"Testing password: {password}");
            Console.WriteLine($"Against hash: {hash.Substring(0, Math.Min(40, hash.Length))}...");
            
  try
     {
      bool result = BCrypt.Net.BCrypt.Verify(password, hash);
Console.WriteLine($"Result: {(result ? "✓ MATCH" : "✗ NO MATCH")}");
          return;
            }
          catch (Exception ex)
  {
                Console.WriteLine($"✗ ERROR: {ex.Message}");
            }
        }
    }
}
