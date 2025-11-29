# 🔐 Faza 1: Implementare Autentificare și Securitate - OVERVIEW

**Aplicație:** ValyanClinic - Sistem Medical Management  
**Framework:** .NET 9 Blazor Server  
**Data:** 2025-01-15  
**Status:** 🚀 **IN IMPLEMENTARE - FAZA 1**  
**Prioritate:** 🔴 **CRITICAL (P0)**

---

## 📋 Setări de Implementat în Faza 1

### ✅ Setări Selectate (Marcate cu [x])

1. **[x] Politici parole** (lungime minimă, complexitate, expirare)
2. **[x] Timeout sesiune utilizator** (inactivitate)
3. **[x] Lockout cont** după N încercări eșuate
4. **[x] Audit log** pentru accesări sistem
5. **[x] Securitate parolă implicită** pentru conturi noi
6. **[x] Istoric parole** (nu permite refolosirea ultimelor N parole)

---

## 📁 Structură Fișiere SQL

```
DevSupport/Documentation/Settings/Phase1/
├── PHASE1_OVERVIEW.md (acest fișier)
├── SQLScripts/
│   ├── 01_Tables/
│   │   ├── 01_CREATE_TABLE_Setari_Sistem.sql
│   │   ├── 02_INSERT_Setari_Sistem_InitialData.sql
│   │   ├── 03_ALTER_TABLE_Utilizatori.sql
│   │   ├── 04_CREATE_TABLE_PasswordHistory.sql
│   │   ├── 06_CREATE_TABLE_Audit_Log.sql
│   │   └── 07_CREATE_TABLE_UserSessions.sql
│   ├── 02_Triggers/
│   │   └── 05_CREATE_TRIGGER_Utilizatori_PasswordChange.sql
│   ├── 03_StoredProcedures/
│   │├── 08_SP_GetSystemSetting.sql
│   │   ├── 09_SP_UpdateSystemSetting.sql
│   │   ├── 10_SP_RecordLoginAttempt.sql
│   │   ├── 13_SP_CreateUserSession.sql
│   │   ├── 14_SP_UpdateSessionActivity.sql
││   ├── 15_SP_ChangePassword.sql
│   │   ├── 16_SP_CleanupExpiredSessions.sql
│   │   ├── 17_SP_NotifyPasswordExpirations.sql
│   │   └── 18_SP_UnlockExpiredLockouts.sql
│   ├── 04_Functions/
│   │   ├── 11_FN_IsPasswordInHistory.sql
│   │ └── 12_FN_IsAccountLocked.sql
│   └── 05_Views/
│       ├── 19_VW_ActiveSessions.sql
│       ├── 20_VW_LockedAccounts.sql
│       ├── 21_VW_LoginAttempts_Last24h.sql
│       └── 22_VW_PasswordExpirations_Next7Days.sql
```

---

## 🚀 Ordinea de Execuție Scripturi

**IMPORTANT:** Scripturile trebuie executate **EXACT** în această ordine pentru a evita erori de dependențe!

### **Pasul 1: Tabele (Folder 01_Tables/)**
1. ✅ `01_CREATE_TABLE_Setari_Sistem.sql` - Tabel setări globale (Key-Value)
2. ✅ `02_INSERT_Setari_Sistem_InitialData.sql` - 17 setări inițiale autentificare
3. ✅ `03_ALTER_TABLE_Utilizatori.sql` - Adaugă 8 coloane noi + index
4. ✅ `04_CREATE_TABLE_PasswordHistory.sql` - Istoric parole (cu FK)
5. ✅ `06_CREATE_TABLE_Audit_Log.sql` - Audit trail (4 indexuri)
6. ✅ `07_CREATE_TABLE_UserSessions.sql` - Sesiuni active (cu FK)

### **Pasul 2: Triggers (Folder 02_Triggers/)**
7. ✅ `05_CREATE_TRIGGER_Utilizatori_PasswordChange.sql` - Auto-populate PasswordHistory

### **Pasul 3: Functions (Folder 04_Functions/)**
8. ✅ `11_FN_IsPasswordInHistory.sql` - Verifică parolă în istoric
9. ✅ `12_FN_IsAccountLocked.sql` - Verifică cont lockuit

### **Pasul 4: Stored Procedures (Folder 03_StoredProcedures/)**
10. ✅ `08_SP_GetSystemSetting.sql` - Get setare sistem
11. ✅ `09_SP_UpdateSystemSetting.sql` - Update setare (cu audit)
12. ✅ `10_SP_RecordLoginAttempt.sql` - Record login (cu lockout logic)
13. ✅ `13_SP_CreateUserSession.sql` - Creare sesiune
14. ✅ `14_SP_UpdateSessionActivity.sql` - Update heartbeat sesiune
15. ✅ `15_SP_ChangePassword.sql` - Schimbare parolă (cu validări)
16. ✅ `16_SP_CleanupExpiredSessions.sql` - Cleanup automat sesiuni
17. ✅ `17_SP_NotifyPasswordExpirations.sql` - Notificări expirare parole
18. ✅ `18_SP_UnlockExpiredLockouts.sql` - Unlock automat conturi

### **Pasul 5: Views (Folder 05_Views/)**
19. ✅ `19_VW_ActiveSessions.sql` - View sesiuni active
20. ✅ `20_VW_LockedAccounts.sql` - View conturi lockuite
21. ✅ `21_VW_LoginAttempts_Last24h.sql` - View încercări login 24h
22. ✅ `22_VW_PasswordExpirations_Next7Days.sql` - View parole care expiră

---

## 📊 Rezumat Tehnic

### **Tabele Create: 5**
- `Setari_Sistem` - 17 setări inițiale (Key-Value Pattern)
- `PasswordHistory` - Istoric parole (cu trigger automat)
- `Audit_Log` - Audit trail (4 indexuri pentru performanță)
- `UserSessions` - Sesiuni active (tracking timeout)
- `Utilizatori` - Extins cu 8 coloane noi

### **Stored Procedures: 11**
- 2 Settings Management (Get, Update)
- 3 Authentication (RecordLogin, CreateSession, UpdateSession)
- 1 Password Management (ChangePassword)
- 3 Automated Jobs (Cleanup, Notify, Unlock)

### **Functions: 2**
- `FN_IsPasswordInHistory` - Validare istoric parole
- `FN_IsAccountLocked` - Check lockout status

### **Views: 4**
- Raportare sesiuni active
- Raportare conturi lockuite
- Raportare încercări login
- Raportare expirări parole

### **Triggers: 1**
- `TR_Utilizatori_PasswordChange` - Auto-populate PasswordHistory + cleanup

---

## ⚙️ SQL Server Jobs Recomandate

Configurează următoarele SQL Server Agent Jobs:

### **1. Session Cleanup (La fiecare 15 minute)**
```sql
EXEC SP_CleanupExpiredSessions;
```

### **2. Password Expiration Notifications (Zilnic la 08:00)**
```sql
EXEC SP_NotifyPasswordExpirations;
```

### **3. Auto-Unlock Expired Lockouts (La fiecare 5 minute)**
```sql
EXEC SP_UnlockExpiredLockouts;
```

---

## 🎯 KPIs pentru Monitoring

După implementare, monitorizează următoarele metrici:

```sql
-- 1. Total sesiuni active
SELECT COUNT(*) AS TotalActiveSessions FROM VW_ActiveSessions;

-- 2. Conturi lockuite
SELECT COUNT(*) AS TotalLockedAccounts FROM VW_LockedAccounts;

-- 3. Tentative login eșuate (ultimele 24h)
SELECT COUNT(*) AS TotalFailedLogins 
FROM Audit_Log
WHERE Actiune = 'LoginFailed' 
  AND DataActiune >= DATEADD(HOUR, -24, GETUTCDATE());

-- 4. Parole care expiră în 7 zile
SELECT COUNT(*) AS PasswordsExpiringSoon FROM VW_PasswordExpirations_Next7Days;

-- 5. Dashboard complet
SELECT 
    (SELECT COUNT(*) FROM VW_ActiveSessions) AS ActiveSessions,
    (SELECT COUNT(*) FROM VW_LockedAccounts) AS LockedAccounts,
    (SELECT COUNT(*) FROM VW_LoginAttempts_Last24h WHERE StatusActiune = 'Failed') AS FailedLoginsLast24h,
    (SELECT COUNT(*) FROM VW_PasswordExpirations_Next7Days) AS PasswordsExpiringSoon;
```

---

## ✅ Checklist Implementare Database

- [ ] **Backup Database** înainte de orice modificare
- [ ] Executare `01_Tables/01_CREATE_TABLE_Setari_Sistem.sql`
- [ ] Executare `01_Tables/02_INSERT_Setari_Sistem_InitialData.sql`
- [ ] Executare `01_Tables/03_ALTER_TABLE_Utilizatori.sql`
- [ ] Executare `01_Tables/04_CREATE_TABLE_PasswordHistory.sql`
- [ ] Executare `01_Tables/06_CREATE_TABLE_Audit_Log.sql`
- [ ] Executare `01_Tables/07_CREATE_TABLE_UserSessions.sql`
- [ ] Executare `02_Triggers/05_CREATE_TRIGGER_Utilizatori_PasswordChange.sql`
- [ ] Executare `04_Functions/11_FN_IsPasswordInHistory.sql`
- [ ] Executare `04_Functions/12_FN_IsAccountLocked.sql`
- [ ] Executare toate scripturile din `03_StoredProcedures/` (9 fișiere)
- [ ] Executare toate scripturile din `05_Views/` (4 fișiere)
- [ ] **Test Manual:** Insert/Update în `Setari_Sistem`
- [ ] **Test Manual:** Schimbare parolă utilizator (verifică trigger)
- [ ] **Test Manual:** Tentativă login eșuată (verifică lockout)
- [ ] **Test Manual:** Creare sesiune (verifică timeout)
- [ ] Configurare SQL Server Agent Jobs (3 jobs)
- [ ] **Verificare Audit_Log** - minim 1 înregistrare
- [ ] **Verificare Views** - toate returnează date corecte
- [ ] **Performance Test** - query-uri sub 100ms

---

## 🔧 Testare Funcționalități

### **1. Test Politici Parole**
```sql
-- Update parola requirements
EXEC SP_UpdateSystemSetting 
    @Categorie = 'Autentificare', 
    @Cheie = 'PasswordMinLength', 
    @Valoare = '10', 
    @ModificatDe = 'admin';

-- Verifică audit log
SELECT TOP 5 * FROM Audit_Log ORDER BY DataActiune DESC;
```

### **2. Test Lockout Cont**
```sql
-- Simulează 5 încercări eșuate
DECLARE @UserID UNIQUEIDENTIFIER = 'user-guid-here';
DECLARE @i INT = 1;
WHILE @i <= 5
BEGIN
    EXEC SP_RecordLoginAttempt 
        @UserName = 'test@valyan.ro', 
        @AdresaIP = '192.168.1.100', 
        @UserAgent = 'Test Browser', 
        @Success = 0, 
        @UtilizatorID = @UserID;
    SET @i = @i + 1;
END

-- Verifică lockout
SELECT dbo.FN_IsAccountLocked(@UserID); -- Trebuie să returneze 1
SELECT * FROM VW_LockedAccounts;
```

### **3. Test Istoric Parole**
```sql
DECLARE @UserID UNIQUEIDENTIFIER = 'user-guid-here';
DECLARE @NewPasswordHash NVARCHAR(MAX) = 'hash-parola-noua';

-- Verifică dacă parola este în istoric
SELECT dbo.FN_IsPasswordInHistory(@UserID, @NewPasswordHash);

-- Schimbă parola
EXEC SP_ChangePassword 
    @UtilizatorID = @UserID, 
    @NewPasswordHash = @NewPasswordHash, 
    @ModificatDe = 'admin';

-- Verifică istoric
SELECT * FROM PasswordHistory WHERE UtilizatorID = @UserID ORDER BY DataCrearii DESC;
```

### **4. Test Sesiuni**
```sql
DECLARE @UserID UNIQUEIDENTIFIER = 'user-guid-here';
DECLARE @SessionToken NVARCHAR(500);
DECLARE @SessionID UNIQUEIDENTIFIER;

-- Creare sesiune
EXEC SP_CreateUserSession 
    @UtilizatorID = @UserID, 
    @AdresaIP = '192.168.1.100', 
    @UserAgent = 'Chrome/120.0', 
    @Dispozitiv = 'Desktop Windows', 
    @SessionToken = @SessionToken OUTPUT, 
    @SessionID = @SessionID OUTPUT;

PRINT 'SessionToken: ' + @SessionToken;

-- Verifică sesiuni active
SELECT * FROM VW_ActiveSessions WHERE UtilizatorID = @UserID;

-- Update activitate
EXEC SP_UpdateSessionActivity @SessionToken = @SessionToken;
```

---

## 📚 Next Steps după Database

### **1. .NET Implementation (Domain Layer)**
Creează entitățile C# pentru noile tabele:
- `SystemSetting.cs`
- `PasswordHistory.cs`
- `AuditLog.cs`
- `UserSession.cs`

### **2. .NET Implementation (Application Layer)**
Implementează servicii:
- `ISettingsService` / `SettingsService`
- `IAuditService` / `AuditService`
- `ISessionService` / `SessionService`

### **3. Blazor UI**
Creează paginile:
- `SetariAutentificare.razor` - Configurare politici parole, lockout, timeout
- `AuditLog.razor` - Vizualizare istoric acțiuni
- `ActiveSessions.razor` - Monitorizare sesiuni active

### **4. Integration Testing**
- Unit tests pentru fiecare SP
- Integration tests pentru flow-uri complete (login → session → logout)
- Security testing (SQL injection, XSS)

---

## 📖 Documentație Suplimentară

- **Database Design:** Diagrame ER pentru tabele noi
- **API Documentation:** Swagger pentru servicii .NET
- **User Manual:** Ghid utilizare pentru administratori
- **Security Audit:** Raport compliance GDPR

---

**Status:** 📝 **READY FOR IMPLEMENTATION**  
**Estimare Timp:** 3-5 zile pentru database + 5-7 zile pentru .NET + Blazor UI  
**Total:** ~2 săptămâni (10 zile lucru)

---

*Document creat: 2025-01-15*  
*Aplicație: ValyanClinic - .NET 9 Blazor Server*  
*Fază: 1 (Autentificare și Securitate - Database Foundation)*
