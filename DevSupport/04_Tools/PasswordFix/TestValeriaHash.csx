using System;

// Reference the BCrypt DLL
#r "D:\Lucru\CMS\ValyanClinic\bin\Debug\net9.0\BCrypt.Net-Next.dll"

using BCrypt.Net;

Console.WriteLine("===========================================");
Console.WriteLine("TEST VALERIA PASSWORD HASH");
Console.WriteLine("===========================================");
Console.WriteLine();

string password = "Valeria1973!";
string hashFromDB = "$2a$12$1u7PllF3FjyAg6jqFjUU0uJi6uluP7H99a1tBnG0leXlfr4Kbd91y";

Console.WriteLine($"Password: {password}");
Console.WriteLine($"Hash from DB: {hashFromDB}");
Console.WriteLine();

try
{
    bool result = BCrypt.Verify(password, hashFromDB);
    Console.WriteLine($"Verification result: {(result ? "? SUCCESS" : "? FAILED")}");
    
    if (!result)
    {
        Console.WriteLine();
        Console.WriteLine("? PASSWORD DOES NOT MATCH!");
        Console.WriteLine();
 Console.WriteLine("Generating NEW hash...");
        string newHash = BCrypt.HashPassword(password, 12);
  Console.WriteLine($"New hash: {newHash}");
        Console.WriteLine();
        Console.WriteLine("Run this SQL to fix:");
        Console.WriteLine("--------------------");
     Console.WriteLine($"UPDATE Utilizatori SET PasswordHash = '{newHash}', Salt = '' WHERE Username = 'valeria'");
 }
    else
    {
        Console.WriteLine();
   Console.WriteLine("? Hash is CORRECT! Problem is elsewhere.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"? ERROR: {ex.Message}");
}

Console.WriteLine();
Console.WriteLine("===========================================");
