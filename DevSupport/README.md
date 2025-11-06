# DevSupport - Admin Password Hash Fix

## 📦 Ce este?

Acest folder conține utility-ul `AdminPasswordHashFix` pentru a repara hash-ul parolei utilizatorului Admin în baza de date ValyanMed.

## 🎯 Problema

Login-ul cu username `Admin` și password `admin123!@#` NU funcționează pentru că hash-ul din DB este pentru o parolă diferită (`admin123` fără caractere speciale).

## ✅ Soluție Rapidă (C# Interactive - RECOMANDAT)

### Pași:

1. **Visual Studio → View → Other Windows → C# Interactive** (sau `Ctrl + Alt + F1`)

2. **Copiază și rulează:**

```csharp
#r "nuget: BCrypt.Net-Next, 4.0.3"
#r "nuget: Microsoft.Data.SqlClient, 5.2.2"

using BCrypt.Net;
using Microsoft.Data.SqlClient;

// Generate hash
string password = "admin123!@#";
string hash = BCrypt.HashPassword(password, 12);
Console.WriteLine($"Hash: {hash}");

// Update DB (CUSTOMIZE connection string!)
string connString = "Server=DESKTOP-9H54BCS\\SQLSERVER;Database=ValyanMed;Integrated Security=True;TrustServerCertificate=True;";

using (var conn = new SqlConnection(connString))
{
    conn.Open();
    var cmd = new SqlCommand(
        "UPDATE Utilizatori SET PasswordHash = @Hash WHERE Username = 'Admin'", 
        conn);
    cmd.Parameters.AddWithValue("@Hash", hash);
    cmd.ExecuteNonQuery();
    Console.WriteLine("SUCCESS! Password updated.");
}
```

3. **Test login:**
   - Username: `Admin`
   - Password: `admin123!@#`

---

## 📝 Alternativă: Folosește Utility Class

### Opțiunea A: Încarcă în C# Interactive

```csharp
#load "AdminPasswordHashFix.cs"
ValyanClinic.DevSupport.AdminPasswordHashFix.Execute();
```

### Opțiunea B: Apelează din cod

```csharp
// În orice fișier C#:
ValyanClinic.DevSupport.AdminPasswordHashFix.Execute();
```

---

## ⚙️ Configurare Connection String

**Dacă primești eroare de conexiune, editează `AdminPasswordHashFix.cs`, linia 8:**

```csharp
// SQL Server Express:
private const string ConnectionString = "Server=.\\SQLEXPRESS;Database=ValyanMed;Integrated Security=True;TrustServerCertificate=True;";

// SQL Server Local:
private const string ConnectionString = "Server=localhost;Database=ValyanMed;Integrated Security=True;TrustServerCertificate=True;";

// SQL Server Named Instance:
private const string ConnectionString = "Server=YOUR-PC-NAME\\SQLSERVER;Database=ValyanMed;Integrated Security=True;TrustServerCertificate=True;";
```

---

## 🐛 Troubleshooting

### Error: "Utilizatorul 'Admin' nu există"

**Fix:** Creează utilizatorul în SQL mai întâi:

```sql
-- Generează hash mai întâi (vezi cod C# de mai sus)
DECLARE @Hash NVARCHAR(512) = '<PASTE_HASH_HERE>'

INSERT INTO Utilizatori (
    UtilizatorID, Username, Email, PasswordHash, Salt, Rol, EsteActiv,
    Data_Crearii, Data_Ultimei_Modificari, Creat_De, Modificat_De
) VALUES (
    NEWID(), 'Admin', 'admin@valyanmed.ro', @Hash, '', 'Administrator', 1,
    GETDATE(), GETDATE(), 'System', 'System'
);
```

### Error: "Cannot open database"

1. Verifică SQL Server este pornit (`services.msc`)
2. Actualizează connection string
3. Test conexiunea în SSMS

### Error: BCrypt package not found

```bash
dotnet add package BCrypt.Net-Next --version 4.0.3
dotnet add package Microsoft.Data.SqlClient --version 5.2.2
dotnet restore
```

---

## 📚 Documentație Completă

Vezi: `DOCS/ADMIN_PASSWORD_FIX_COMPLETE.md`

---

**Status:** ✅ **READY TO USE**  
**Build:** ✅ **SUCCESSFUL**  
**Tested:** 🟡 **PENDING** (need to run utility)

