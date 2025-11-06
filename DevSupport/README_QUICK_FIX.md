# ⚡ Quick Fix: Admin Password Hash

## 🎯 Problema
Username `Admin` cu password `admin123!@#` NU funcționează la login.

## ✅ Soluția Rapidă (5 minute)

### Opțiunea 1: Rulează Utility-ul (RECOMANDAT)

```bash
# 1. Navigează la folder DevSupport
cd DevSupport

# 2. Asigură-te că ai pachetele NuGet
dotnet restore

# 3. Rulează utility-ul
dotnet run --project AdminPasswordHashFix.csproj
```

**Output așteptat:**
```
✅ Hash generat
✅ Conectare reușită
✅ Utilizator găsit: Admin
✅ Parolă actualizată cu succes!

SUCCES! Acum poți testa login-ul:
  Username: Admin
  Password: admin123!@#
```

---

### Opțiunea 2: SQL Direct (Manual)

```sql
USE [ValyanMed]
GO

-- Generează hash în C#:
-- BCrypt.Net.BCrypt.HashPassword("admin123!@#", 12)

DECLARE @Hash NVARCHAR(512) = '<PASTE-HASH-HERE>'

UPDATE Utilizatori
SET PasswordHash = @Hash,
  Data_Ultimei_Modificari = GETDATE()
WHERE Username = 'Admin';

-- Verificare
SELECT Username, EsteActiv FROM Utilizatori WHERE Username = 'Admin';
```

---

## 📝 Verificare Connection String

**Dacă primești eroare de conexiune, actualizează în `AdminPasswordHashFix.cs`:**

```csharp
// Linia 11:
private const string ConnectionString = "YOUR-CONNECTION-STRING-HERE";
```

**Exemple common:**
```
// SQL Server local:
"Server=localhost;Database=ValyanMed;Integrated Security=True;TrustServerCertificate=True;"

// SQL Server Express:
"Server=.\\SQLEXPRESS;Database=ValyanMed;Integrated Security=True;TrustServerCertificate=True;"

// SQL Server named instance:
"Server=YOUR-PC-NAME\\SQLSERVER;Database=ValyanMed;Integrated Security=True;TrustServerCertificate=True;"
```

---

## ✅ Test Login

După fix:
1. Pornește aplicația: `dotnet run` (în folder ValyanClinic)
2. Deschide browser: `https://localhost:5001/login`
3. **Username:** `Admin`
4. **Password:** `admin123!@#`
5. Click **Autentificare**
6. ✅ Redirect la `/dashboard`

---

## 🐛 Troubleshooting

### Error: "Utilizatorul 'Admin' nu există"
**Cauză:** Tabela Utilizatori nu conține utilizatorul Admin

**Fix:** Rulează scriptul de creare utilizator:
```sql
-- În SSMS sau Azure Data Studio
-- Găsește și rulează scriptul de seed data pentru Utilizatori
```

### Error: "The type or namespace name 'BCrypt' could not be found"
**Cauză:** Pachetul NuGet BCrypt.Net-Next nu este instalat

**Fix:**
```bash
cd DevSupport
dotnet add package BCrypt.Net-Next --version 4.0.3
dotnet add package Microsoft.Data.SqlClient --version 5.2.2
dotnet restore
```

### Error: "Cannot open database"
**Cauză:** Connection string incorect sau SQL Server oprit

**Fix:**
1. Verifică că SQL Server este pornit (SQL Server Configuration Manager)
2. Actualizează connection string în cod
3. Test conexiunea în SSMS mai întâi

---

## 📚 Documentație Completă

Vezi `DOCS/FIX_ADMIN_PASSWORD_HASH.md` pentru:
- Explicație detaliată
- Alternative de fix
- Debugging advanced
- Prevention measures

---

**Timp estimat:** ⏱️ 5 minute  
**Dificultate:** 🟢 EASY  
**Risc:** 🟢 LOW (doar actualizează un hash)

