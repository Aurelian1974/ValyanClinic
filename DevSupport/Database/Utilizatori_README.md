# Utilizatori - User Management Table

## 🎯 Ce face?

Implementează gestionarea utilizatorilor aplicației **ValyanClinic**. Fiecare utilizator este asociat cu un membru din **PersonalMedical** (doctori, asistenți medicali, etc.).

- ✅ Un **PersonalMedical** = Un **Utilizator** (relație **1:1**)
- ✅ **NU** se asociază cu tabela **Personal** (care este pentru HR)
- ✅ Securitate completă: hash parole, salt, blocare automată, reset parola
- ✅ Roluri multiple: Administrator, Doctor, Asistent, Receptioner, Manager, Utilizator
- ✅ Audit trail complet pentru toate modificările

---

## 🚀 Quick Start (1 minut)

```powershell
cd DevSupport\Scripts\PowerShellScripts
.\Deploy-Utilizatori.ps1
```

**Asta e tot!** Scriptul face automat:
- ✅ Creează tabela `Utilizatori`
- ✅ Creează 12 stored procedures
- ✅ Adaugă indecși pentru performanță
- ✅ Adaugă constraints și Foreign Keys
- ✅ Rulează teste automate (optional)
- ✅ Adaugă date de test (optional)

---

## 📁 Fișiere create

```
DevSupport/
├── Database/
│   ├── TableStructure/
│   │   └── Utilizatori_Complete.sql ................. Tabela
│   └── StoredProcedures/
│       └── sp_Utilizatori.sql ....................... 12 SP-uri
├── Scripts/
│   └── PowerShellScripts/
│       ├── Deploy-Utilizatori.ps1 ................... Deployment
│       └── Test-Utilizatori.ps1 ..................... Tests
└── Documentation/
    └── Database/
        └── Utilizatori_README.md .................... Acest fisier
```

---

## 🗂️ Structura tabelei

| Coloană | Tip | Descriere |
|---------|-----|-----------|
| **UtilizatorID** | UNIQUEIDENTIFIER | PK - ID utilizator |
| **PersonalMedicalID** | UNIQUEIDENTIFIER | FK către PersonalMedical (1:1) |
| **Username** | NVARCHAR(100) | Nume utilizator (UNIQUE) |
| **Email** | NVARCHAR(100) | Email (UNIQUE) |
| **PasswordHash** | NVARCHAR(256) | Hash parola (SHA256/bcrypt) |
| **Salt** | NVARCHAR(100) | Salt pentru hash |
| **Rol** | NVARCHAR(50) | Administrator, Doctor, Asistent, etc. |
| **EsteActiv** | BIT | Utilizator activ? |
| **DataCreare** | DATETIME2 | Când a fost creat contul |
| **DataUltimaAutentificare** | DATETIME2 | Ultima autentificare reușită |
| **NumarIncercariEsuate** | INT | Încercări eșuate de login |
| **DataBlocare** | DATETIME2 | Când a fost blocat (NULL = activ) |
| **TokenResetareParola** | NVARCHAR(256) | Token pentru reset parola |
| **DataExpirareToken** | DATETIME2 | Când expiră token-ul |
| + audit fields | ... | CreatDe, ModificatDe, etc. |

---

## 🔐 Caracteristici de Securitate

### ✅ Hash Parole
- ❌ **NU** se stochează parolele în clar
- ✅ Se folosește **PasswordHash** + **Salt**
- ✅ Recomandat: **bcrypt**, **Argon2** sau **PBKDF2**

### ✅ Blocare Automată
- ✅ După **5 încercări eșuate** → cont blocat automat
- ✅ `DataBlocare` setată → user nu se poate autentifica
- ✅ Deblocare: schimbare parolă sau admin

### ✅ Reset Parola
- ✅ Token unic generat pentru reset
- ✅ Token cu expirare (ex: 1 oră)
- ✅ Token șters după utilizare

---

## 📝 Stored Procedures (12)

### 1. **sp_Utilizatori_GetAll** - Lista utilizatori (cu paginare)
```sql
EXEC sp_Utilizatori_GetAll 
    @EsteActiv = 1,
    @Rol = 'Doctor',
    @SearchText = 'ion',
    @PageNumber = 1,
    @PageSize = 20,
    @SortColumn = 'Username',
    @SortDirection = 'ASC'
```

### 2. **sp_Utilizatori_GetCount** - Numar total (pentru paginare)
```sql
EXEC sp_Utilizatori_GetCount 
    @EsteActiv = 1,
    @Rol = 'Doctor',
    @SearchText = 'ion'
```

### 3. **sp_Utilizatori_GetById** - Detalii utilizator
```sql
EXEC sp_Utilizatori_GetById 
    @UtilizatorID = 'GUID-UTILIZATOR'
```

### 4. **sp_Utilizatori_GetByUsername** - Găsește după username (autentificare)
```sql
EXEC sp_Utilizatori_GetByUsername 
    @Username = 'dr.popescu'
```

### 5. **sp_Utilizatori_GetByEmail** - Găsește după email
```sql
EXEC sp_Utilizatori_GetByEmail 
    @Email = 'doctor@clinic.ro'
```

### 6. **sp_Utilizatori_Create** - Creează utilizator nou
```sql
EXEC sp_Utilizatori_Create 
    @PersonalMedicalID = 'GUID-PERSONAL-MEDICAL',
    @Username = 'dr.ionescu',
    @Email = 'ionescu@clinic.ro',
    @PasswordHash = 'HASH_FROM_BCRYPT',
    @Salt = 'RANDOM_SALT',
    @Rol = 'Doctor',
    @EsteActiv = 1,
    @CreatDe = 'Admin'
```

**Validări automate:**
- ✅ PersonalMedicalID există și este activ
- ✅ Username nu există deja (UNIQUE)
- ✅ Email nu există deja (UNIQUE)
- ✅ PersonalMedicalID nu are deja un utilizator (1:1)

### 7. **sp_Utilizatori_Update** - Actualizează utilizator
```sql
EXEC sp_Utilizatori_Update 
    @UtilizatorID = 'GUID-UTILIZATOR',
    @Username = 'dr.ionescu.maria',
    @Email = 'maria.ionescu@clinic.ro',
  @Rol = 'Administrator',
    @EsteActiv = 1,
    @ModificatDe = 'Admin'
```

### 8. **sp_Utilizatori_ChangePassword** - Schimbă parola
```sql
EXEC sp_Utilizatori_ChangePassword 
    @UtilizatorID = 'GUID-UTILIZATOR',
    @NewPasswordHash = 'NEW_HASH',
    @NewSalt = 'NEW_SALT',
    @ModificatDe = 'User'
```

**Efecte:**
- ✅ Resetează `NumarIncercariEsuate` la 0
- ✅ Șterge `DataBlocare` (deblochează contul)
- ✅ Șterge `TokenResetareParola` și `DataExpirareToken`

### 9. **sp_Utilizatori_UpdateUltimaAutentificare** - Login reușit
```sql
EXEC sp_Utilizatori_UpdateUltimaAutentificare 
    @UtilizatorID = 'GUID-UTILIZATOR'
```

**Efecte:**
- ✅ Setează `DataUltimaAutentificare` = NOW
- ✅ Resetează `NumarIncercariEsuate` la 0
- ✅ Șterge `DataBlocare`

### 10. **sp_Utilizatori_IncrementIncercariEsuate** - Login eșuat
```sql
EXEC sp_Utilizatori_IncrementIncercariEsuate 
    @UtilizatorID = 'GUID-UTILIZATOR'
```

**Logică:**
- ✅ Incrementează `NumarIncercariEsuate`
- ✅ Dacă >= 5 → setează `DataBlocare` = NOW
- ✅ Returnează mesaj: "X încercări rămase" sau "Cont blocat"

### 11. **sp_Utilizatori_SetTokenResetareParola** - Reset parola (email)
```sql
EXEC sp_Utilizatori_SetTokenResetareParola 
    @Email = 'doctor@clinic.ro',
    @Token = 'RANDOM_GUID_TOKEN',
@DataExpirare = '2025-01-24 15:30:00'
```

### 12. **sp_Utilizatori_GetStatistics** - Statistici
```sql
EXEC sp_Utilizatori_GetStatistics
```

**Returnează:**
- Total utilizatori (și câți activi)
- Administratori (și câți activi)
- Doctori (și câți activi)
- Asistenți (și câți activi)
- Recepționeri (și câți activi)
- Blocați

---

## 💡 Exemple de utilizare

### Exemplu 1: Creează utilizator pentru un doctor

```sql
-- 1. Găsește PersonalMedical pentru Dr. Popescu
DECLARE @PersonalMedicalID UNIQUEIDENTIFIER = 
    (SELECT TOP 1 PersonalID 
     FROM PersonalMedical 
     WHERE Nume = 'Popescu' AND Prenume = 'Ion' AND EsteActiv = 1)

-- 2. Verifică că nu are deja cont
IF NOT EXISTS (SELECT 1 FROM Utilizatori WHERE PersonalMedicalID = @PersonalMedicalID)
BEGIN
    -- 3. Creează utilizator
    -- IMPORTANT: În aplicație, folosește bcrypt pentru hash!
    DECLARE @PasswordHash NVARCHAR(256) = 'HASH_FROM_BCRYPT'  -- NU parola în clar!
  DECLARE @Salt NVARCHAR(100) = 'RANDOM_SALT_FROM_APP'
    
    EXEC sp_Utilizatori_Create 
        @PersonalMedicalID = @PersonalMedicalID,
        @Username = 'dr.popescu',
  @Email = 'ion.popescu@clinic.ro',
        @PasswordHash = @PasswordHash,
    @Salt = @Salt,
        @Rol = 'Doctor',
     @EsteActiv = 1,
        @CreatDe = 'Admin'
END
ELSE
BEGIN
    PRINT 'Dr. Popescu are deja un cont de utilizator'
END
```

### Exemplu 2: Proces de autentificare (pseudo-code)

```sql
-- 1. Găsește utilizator după username
DECLARE @Username NVARCHAR(100) = 'dr.popescu'
DECLARE @InputPassword NVARCHAR(100) = 'parola_introdusa_de_user'

-- Get user
EXEC sp_Utilizatori_GetByUsername @Username = @Username
-- Returnează: UtilizatorID, PasswordHash, Salt, EsteActiv, DataBlocare, NumarIncercariEsuate

-- 2. În aplicația C# (pseudo-code):
/*
if (user == null) 
    return "Username invalid"

if (!user.EsteActiv) 
    return "Cont inactiv"

if (user.DataBlocare != null) 
    return "Cont blocat. Contactați administratorul."

// Hash input password with stored salt
var hashedInput = BCrypt.HashPassword(inputPassword, user.Salt)

if (hashedInput == user.PasswordHash) {
    // SUCCESS
    EXEC sp_Utilizatori_UpdateUltimaAutentificare @UtilizatorID
    return "Autentificare reușită"
} else {
    // FAILED
    EXEC sp_Utilizatori_IncrementIncercariEsuate @UtilizatorID
    // SP returnează "X încercări rămase" sau "Cont blocat"
}
*/
```

### Exemplu 3: Reset parola (flow complet)

```sql
-- STEP 1: User cere reset (pe pagina de login)
DECLARE @Email NVARCHAR(100) = 'doctor@clinic.ro'
DECLARE @Token NVARCHAR(256) = NEWID()  -- Generate random token
DECLARE @Expirare DATETIME2 = DATEADD(HOUR, 1, GETDATE())  -- Expires in 1 hour

EXEC sp_Utilizatori_SetTokenResetareParola 
    @Email = @Email,
    @Token = @Token,
    @DataExpirare = @Expirare

-- STEP 2: Trimite email cu link (în aplicație)
-- Link: https://clinic.ro/reset-password?token=GUID

-- STEP 3: User accesează link-ul și introduce parola nouă
-- Verifică token în C#:
/*
var user = GetByEmail(email)
if (user.TokenResetareParola != inputToken)
    return "Token invalid"
if (user.DataExpirareToken < DateTime.Now)
    return "Token expirat"
*/

-- STEP 4: Schimbă parola
DECLARE @NewPasswordHash NVARCHAR(256) = 'NEW_HASH_FROM_BCRYPT'
DECLARE @NewSalt NVARCHAR(100) = 'NEW_SALT'

EXEC sp_Utilizatori_ChangePassword 
    @UtilizatorID = 'GUID-UTILIZATOR',
    @NewPasswordHash = @NewPasswordHash,
    @NewSalt = @NewSalt,
    @ModificatDe = 'PasswordReset'

-- SP șterge automat TokenResetareParola și deblocă contul
```

---

## 🧪 Testare

```powershell
cd DevSupport\Scripts\PowerShellScripts
.\Test-Utilizatori.ps1
```

**15+ teste automate:**
- ✅ Verificare existență tabelă
- ✅ Verificare Foreign Keys (PersonalMedical)
- ✅ Verificare Unique Constraints (Username, Email, PersonalMedicalID)
- ✅ Verificare Stored Procedures (toate 12)
- ✅ Test Create Utilizator (cu validări)
- ✅ Test GetByUsername
- ✅ Test UpdateUltimaAutentificare
- ✅ Test IncrementIncercariEsuate (blocare automată dupa 5)
- ✅ Test ChangePassword (deblocheaza contul)
- ✅ Test GetStatistics
- ✅ **Cleanup automat după teste** - cu opțiune de golire tabelă

---

## 🗑️ Golire Tabelă

### Opțiunea 1: Script dedicat (RECOMANDAT)
```powershell
cd DevSupport\Scripts\PowerShellScripts
.\Clean-Utilizatori.ps1
```

**Caracteristici:**
- ⚠️ Cere confirmare explicit ("DELETE ALL")
- ✅ Verifică numărul de utilizatori înainte
- ✅ Validează că tabela e goală după ștergere
- ✅ Operațiune sigură cu multiple verificări

### Opțiunea 2: Cu parametru -Force (fără confirmare)
```powershell
.\Clean-Utilizatori.ps1 -Force
```

### Opțiunea 3: După teste automate
După rularea `Test-Utilizatori.ps1`, scriptul întreabă dacă dorești să golești tabela.

### Opțiunea 4: SQL direct (pentru avansați)
```sql
-- ATENTIE: Operatiune ireversibila!
DELETE FROM Utilizatori;
```

---

## 📊 Integrare C#

### Entity (Domain)
```csharp
namespace ValyanClinic.Domain.Entities;

public class Utilizator
{
    public Guid UtilizatorID { get; set; }
    public Guid PersonalMedicalID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool EsteActiv { get; set; } = true;
    public DateTime DataCreare { get; set; }
    public DateTime? DataUltimaAutentificare { get; set; }
    public int NumarIncercariEsuate { get; set; }
    public DateTime? DataBlocare { get; set; }
    public string? TokenResetareParola { get; set; }
    public DateTime? DataExpirareToken { get; set; }
    
    // Navigation property
 public PersonalMedical? PersonalMedical { get; set; }
    
    // Computed properties
    public bool EsteBlocat => DataBlocare.HasValue;
    public bool TokenEsteValid => 
        !string.IsNullOrEmpty(TokenResetareParola) && 
        DataExpirareToken.HasValue && 
DataExpirareToken.Value > DateTime.Now;
}
```

### DTOs (Application)
```csharp
public class UtilizatorListDto
{
    public Guid UtilizatorID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool EsteActiv { get; set; }
    public DateTime? DataUltimaAutentificare { get; set; }
    public string NumeCompletPersonalMedical { get; set; } = string.Empty;
    public string? Specializare { get; set; }
    public bool EsteBlocat { get; set; }
}

public class LoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    public Guid UtilizatorID { get; set; }
    public string OldPassword { get; set; } = string.Empty;
  public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
```

### Repository (Infrastructure)
```csharp
public interface IUtilizatorRepository
{
    Task<(IEnumerable<Utilizator> Items, int TotalCount)> GetAllAsync(
        bool? esteActiv = null,
        string? rol = null,
        string? searchText = null,
        int pageNumber = 1,
      int pageSize = 50,
        string sortColumn = "Username",
 string sortDirection = "ASC",
        CancellationToken cancellationToken = default);
    
    Task<Utilizator?> GetByIdAsync(Guid utilizatorID, CancellationToken cancellationToken = default);
    Task<Utilizator?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<Utilizator?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    
  Task<Utilizator> CreateAsync(Utilizator utilizator, CancellationToken cancellationToken = default);
    Task<Utilizator> UpdateAsync(Utilizator utilizator, CancellationToken cancellationToken = default);
    
    Task<bool> ChangePasswordAsync(
    Guid utilizatorID, 
 string newPasswordHash, 
   string newSalt, 
        string modificatDe, 
        CancellationToken cancellationToken = default);
    
    Task<bool> UpdateUltimaAutentificareAsync(Guid utilizatorID, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> IncrementIncercariEsuateAsync(Guid utilizatorID, CancellationToken cancellationToken = default);
    
    Task<bool> SetTokenResetareParolaAsync(
  string email, 
     string token, 
        DateTime dataExpirare, 
        CancellationToken cancellationToken = default);
    
    Task<Dictionary<string, (int Total, int Activi)>> GetStatisticsAsync(CancellationToken cancellationToken = default);
}
```

### Queries (MediatR)
```csharp
// Login Query
public record LoginQuery(string Username, string Password) 
  : IRequest<Result<AuthenticationResult>>;

// Get All Utilizatori Query
public record GetUtilizatoriListQuery(
    bool? EsteActiv = null,
    string? Rol = null,
    string? SearchText = null,
    int PageNumber = 1,
    int PageSize = 50,
    string SortColumn = "Username",
    string SortDirection = "ASC"
) : IRequest<Result<PagedResult<UtilizatorListDto>>>;

// Change Password Command
public record ChangePasswordCommand(
    Guid UtilizatorID,
    string OldPassword,
    string NewPassword
) : IRequest<Result<bool>>;
```

---

## 🔑 Implementare Autentificare (exemplu simplificat)

### Service de Autentificare
```csharp
public class AuthenticationService : IAuthenticationService
{
    private readonly IUtilizatorRepository _utilizatorRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthenticationService> _logger;
    
    public async Task<Result<AuthenticationResult>> LoginAsync(
      string username, 
        string password, 
      CancellationToken cancellationToken = default)
 {
        // 1. Get user
        var utilizator = await _utilizatorRepository.GetByUsernameAsync(username, cancellationToken);
        
   if (utilizator == null)
   {
       _logger.LogWarning("Login failed: Username not found - {Username}", username);
            return Result<AuthenticationResult>.Failure("Username sau parola incorecte");
   }
     
        // 2. Check if active
        if (!utilizator.EsteActiv)
        {
  _logger.LogWarning("Login failed: User inactive - {Username}", username);
      return Result<AuthenticationResult>.Failure("Contul este inactiv");
        }
        
        // 3. Check if blocked
        if (utilizator.DataBlocare.HasValue)
    {
        _logger.LogWarning("Login failed: User blocked - {Username}", username);
          return Result<AuthenticationResult>.Failure(
          "Contul este blocat dupa 5 incercari esuate. Contactati administratorul sau resetati parola.");
      }
        
        // 4. Verify password
  var passwordValid = _passwordHasher.VerifyPassword(password, utilizator.PasswordHash, utilizator.Salt);
        
        if (!passwordValid)
        {
 _logger.LogWarning("Login failed: Invalid password - {Username}", username);
     
            // Increment failed attempts
            var (success, message) = await _utilizatorRepository.IncrementIncercariEsuateAsync(
      utilizator.UtilizatorID, 
                cancellationToken);
      
return Result<AuthenticationResult>.Failure(message);
        }
        
   // 5. SUCCESS - Update last login
    await _utilizatorRepository.UpdateUltimaAutentificareAsync(
  utilizator.UtilizatorID, 
            cancellationToken);
        
        _logger.LogInformation("Login successful - {Username}", username);
        
    // 6. Generate JWT token (sau session, cookie, etc.)
        var token = GenerateJwtToken(utilizator);
        
      return Result<AuthenticationResult>.Success(new AuthenticationResult
 {
            Token = token,
  UtilizatorID = utilizator.UtilizatorID,
          Username = utilizator.Username,
            Rol = utilizator.Rol,
            NumeComplet = utilizator.PersonalMedical?.NumeComplet ?? ""
        });
    }
}
```

---

## ⚠️ IMPORTANTE - BEST PRACTICES

### ✅ Securitate Parole
1. **NU stoca parolele în clar** - întotdeauna hash + salt
2. **Folosește algoritmi puternici**: bcrypt (recomandat), Argon2, sau PBKDF2
3. **NU folosi MD5 sau SHA1** - sunt nesigure
4. **Generează salt random** pentru fiecare parolă
5. **Minimul 8 caractere** pentru parolă (mai bine 12+)

### ✅ Validări
1. **Username**: minim 3 caractere, fără spații, alfanumeric + underscore/dot
2. **Email**: format valid (regex)
3. **Parola**: minim 8 caractere, litere mari + mici + cifre + simboluri
4. **PersonalMedicalID**: trebuie să existe și să fie activ

### ✅ Blocare Cont
1. **5 încercări eșuate** → blocare automată
2. **Deblocare**: doar prin schimbare parolă sau admin
3. **Log toate încercările** pentru audit

### ✅ Reset Parola
1. **Token unic** (GUID) cu expirare (1 oră recomandat)
2. **Trimite email** cu link securizat
3. **Șterge token** după utilizare (one-time use)
4. **NU trimite parola în email** - doar link de reset

---

## ✅ Checklist Implementare

### Database Layer
- [x] Script SQL pentru creare tabelă
- [x] Script SQL pentru stored procedures (12)
- [x] Script PowerShell deployment
- [x] Script PowerShell testare
- [x] Documentație completă

### Application Layer (TODO)
- [ ] **Entity Utilizator** în ValyanClinic.Domain
- [ ] **Repository Interface** în ValyanClinic.Domain
- [ ] **Repository Implementation** în ValyanClinic.Infrastructure (Dapper)
- [ ] **DTOs** în ValyanClinic.Application
- [ ] **Queries/Commands** (MediatR)
- [ ] **Authentication Service** pentru login/logout
- [ ] **Password Hasher Service** (bcrypt)

### Presentation Layer (TODO)
- [ ] **Login Page** (Blazor component)
- [ ] **User Management Page** (CRUD utilizatori)
- [ ] **Change Password Page**
- [ ] **Forgot Password Page**
- [ ] **Authentication State Provider**
- [ ] **Authorization Policies** (roluri)

---

## 📞 Support

Pentru probleme:
1. Verifică documentația completă
2. Rulează test suite pentru diagnostic: `.\Test-Utilizatori.ps1`
3. Verifică logs în SQL Server
4. Verifică că PersonalMedical există și este activ

---

## 🎯 Ce urmează?

1. ✅ **Deploy database** (DONE - rulează `Deploy-Utilizatori.ps1`)
2. ⏳ **Creează entități C#** (TODO - vezi secțiunea "Integrare C#")
3. ⏳ **Implementează autentificare** (TODO - vezi exemplu Authentication Service)
4. ⏳ **Creează UI Blazor** (TODO - Login, User Management)

---

**Status:** ✅ **READY FOR USE**  
**Created:** 2025-01-24  
**Version:** 1.0  
**Database:** ValyanMed  
**Association:** PersonalMedical (1:1) - NU Personal!  

---

## 🔗 Relații cu alte tabele

```
Utilizatori (1) ←→ (1) PersonalMedical
     │
     ├─→ Username (UNIQUE)
   ├─→ Email (UNIQUE)
     └─→ PersonalMedicalID (UNIQUE, FK)
```

**IMPORTANT:** 
- ❌ **NU** există relație cu tabela `Personal` (care este pentru HR)
- ✅ **DA** există relație **1:1** cu `PersonalMedical` (pentru aplicație)
- ✅ Un membru `PersonalMedical` poate avea **maxim un** cont `Utilizator`
- ✅ Un `Utilizator` este asociat cu **exact un** membru `PersonalMedical`

---

**🎉 Deployment-ul este complet! Tabela Utilizatori este gata de utilizare! 🚀**
