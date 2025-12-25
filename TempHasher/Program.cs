using BCrypt.Net;
var hash = BCrypt.Net.BCrypt.HashPassword("Admin123!", workFactor: 11);
Console.WriteLine(hash);
