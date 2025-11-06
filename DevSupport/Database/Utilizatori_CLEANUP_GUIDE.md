# 🗑️ Golire Tabelă Utilizatori - Ghid Complet

## 📋 Opțiuni Disponibile

### ✅ Opțiunea 1: Script Dedicat (RECOMANDAT)

```powershell
cd DevSupport\Scripts\PowerShellScripts
.\Clean-Utilizatori.ps1
```

**Când să folosești:**
- După finalizarea testelor
- Când vrei să resetezi complet baza de date
- Înainte de a importa utilizatori din altă sursă

**Caracteristici de securitate:**
- ⚠️ Cere confirmare explicită (trebuie să scrii "DELETE ALL")
- ✅ Afișează numărul de utilizatori care vor fi șterși
- ✅ Verifică că tabela e goală după ștergere
- ✅ Nu permite ștergere accidentală

**Output exemplu:**
```
════════════════════════════════════════
  ⚠️  ATENTIE! OPERATIUNE PERICULOASA! ⚠️
════════════════════════════════════════

Urmeaza sa STERGI TOTI UTILIZATORII din tabela Utilizatori!
Operatiunea este IREVERSIBILA!

Numar utilizatori care vor fi stersi: 15

Scrie 'DELETE ALL' pentru a confirma stergerea: DELETE ALL

Stergere utilizatori...

✅ Succes!
  15 utilizatori stersi

✅ Verificare: Tabela este goala
```

---

### ⚡ Opțiunea 2: Cu Parametru -Force (Fără Confirmare)

```powershell
.\Clean-Utilizatori.ps1 -Force
```

**Când să folosești:**
- În scripturi automate
- Când ești 100% sigur de operațiune
- În mediul de development/testing

**⚠️ ATENȚIE:** Această opțiune șterge imediat fără confirmare!

---

### 🧪 Opțiunea 3: După Teste Automate

```powershell
.\Test-Utilizatori.ps1
```

**Ce face:**
1. Rulează toate testele (15+)
2. La final, întreabă: "Doresti sa golesti tabela Utilizatori? (DA/NU)"
3. Dacă răspunzi "DA" → șterge toți utilizatorii
4. Dacă răspunzi "NU" → păstrează utilizatorii

**Când să folosești:**
- După rularea testelor de integrare
- Când vrei să verifici funcționalitatea ȘI să cureți după

---

### 🔧 Opțiunea 4: Cu Parametru în Deploy

```powershell
.\Deploy-Utilizatori.ps1 -CleanTable
```

**Ce face:**
1. Creează tabela și stored procedures
2. La final, întreabă dacă dorești să golești tabela
3. Util pentru re-deployment curat

---

### 💻 Opțiunea 5: SQL Direct (Pentru Avansați)

**În SQL Server Management Studio:**

```sql
-- ATENTIE: Operatiune ireversibila!
USE ValyanMed;
GO

-- Verifică numărul de utilizatori
SELECT COUNT(*) AS NumarUtilizatori FROM Utilizatori;
GO

-- Șterge toți utilizatorii
DELETE FROM Utilizatori;
GO

-- Verifică că tabela e goală
SELECT COUNT(*) AS NumarUtilizatori FROM Utilizatori;
GO
```

**Când să folosești:**
- Când lucrezi direct în SSMS
- Pentru debugging
- Pentru operațiuni rapide în development

---

## 📊 Comparație Opțiuni

| Opțiune | Confirmare | Verificări | Siguranță | Viteză | Use Case |
|---------|-----------|-----------|-----------|--------|----------|
| Clean-Utilizatori.ps1 | ✅ Da ("DELETE ALL") | ✅✅✅ Triple | 🟢 Foarte sigur | 🟡 Mediu | Production-safe |
| Clean-Utilizatori.ps1 -Force | ❌ Nu | ✅✅ Double | 🟡 Moderat | 🟢 Rapid | Automation |
| Test-Utilizatori.ps1 (cleanup) | ✅ Da (DA/NU) | ✅✅ Double | 🟢 Sigur | 🟡 Mediu | După teste |
| Deploy-Utilizatori.ps1 -CleanTable | ✅ Da (DA/NU) | ✅✅ Double | 🟢 Sigur | 🟡 Mediu | Re-deployment |
| SQL Direct | ❌ Nu | ❌ Niciuna | 🔴 Periculos | 🟢 Instant | Development only |

---

## ⚠️ Avertismente Importante

### 🔴 ÎNAINTE de a goli tabela:

1. **Backup Database**
   ```powershell
   # În SSMS:
   # Right-click pe ValyanMed → Tasks → Back Up...
   ```

2. **Verifică dacă există utilizatori importanți**
   ```sql
   SELECT Username, Email, Rol, DataCreare 
   FROM Utilizatori 
   WHERE Rol IN ('Administrator', 'Manager')
   ORDER BY DataCreare;
   ```

3. **Exportă datele dacă e necesar**
   ```sql
   SELECT * FROM Utilizatori
   -- Right-click → Save Results As... → CSV
   ```

### 🟡 După golire:

- ✅ Contul de administrator va trebui recreat
- ✅ Toți utilizatorii vor trebui să-și facă conturi noi
- ✅ Istoricul de autentificare se pierde
- ✅ Token-urile de reset parolă se pierd

---

## 🔄 Workflow Recomandat

### Pentru Development:
```powershell
# 1. Rulează teste
.\Test-Utilizatori.ps1

# 2. La final, când întreabă "Doresti sa golesti tabela?" → răspunde DA
# Tabela este goală, gata pentru dezvoltare curată
```

### Pentru Testing:
```powershell
# 1. Golește tabela
.\Clean-Utilizatori.ps1

# 2. Adaugă date de test
.\Deploy-Utilizatori.ps1
# (fără -SkipTestData pentru a adăuga utilizatori de test)

# 3. Rulează testele
.\Test-Utilizatori.ps1

# 4. Golește din nou
# Răspunde DA când întreabă
```

### Pentru Production (ATENȚIE!):
```powershell
# ❌ NU folosi Clean-Utilizatori.ps1 în production!
# ❌ NU șterge utilizatorii din production!

# În production, folosește:
# - UPDATE pentru dezactivare: UPDATE Utilizatori SET EsteActiv = 0 WHERE ...
# - Nu DELETE, doar dacă ești ABSOLUT sigur
```

---

## 📝 Logs și Audit

### Verifică cine a șters date:
```sql
-- Dacă ai audit trail activat
SELECT TOP 100 *
FROM Audit_Utilizator
WHERE [Action] = 'DELETE'
ORDER BY ActionDate DESC;
```

### Backup înainte de ștergere:
```powershell
# Script PowerShell pentru backup automat
$backupPath = "D:\Backups\ValyanMed_Utilizatori_$(Get-Date -Format 'yyyyMMdd_HHmmss').bak"
$query = "BACKUP DATABASE ValyanMed TO DISK = '$backupPath'"
# ... execute query
```

---

## 🆘 Recovery (Dacă ai șters din greșeală)

### Opțiunea 1: Restore din Backup
```sql
USE master;
GO

-- Restore database din backup
RESTORE DATABASE ValyanMed 
FROM DISK = 'D:\Backups\ValyanMed_20250124.bak'
WITH REPLACE;
GO
```

### Opțiunea 2: Recreează Administrator
```sql
-- Găsește un PersonalMedical activ
SELECT TOP 1 PersonalID, Nume + ' ' + Prenume AS NumeComplet
FROM PersonalMedical
WHERE EsteActiv = 1
ORDER BY Nume;

-- Creează administrator
EXEC sp_Utilizatori_Create 
    @PersonalMedicalID = 'GUID-FROM-ABOVE',
    @Username = 'admin',
    @Email = 'admin@clinic.ro',
    @PasswordHash = 'HASH_FROM_BCRYPT',
    @Salt = 'SALT',
  @Rol = 'Administrator',
    @EsteActiv = 1,
    @CreatDe = 'System';
```

---

## 📞 Support

**Dacă ai probleme:**

1. **Script nu funcționează:**
   - Verifică conexiunea la SQL Server
   - Verifică permisiunile (DELETE permission)
   - Verifică că tabela există

2. **Eroare "Foreign Key constraint":**
 - Normal dacă există relații cu alte tabele în viitor
   - În current design nu ar trebui să apară

3. **Tabela nu se golește complet:**
   - Verifică dacă există triggers care blochează DELETE
- Rulează `SELECT COUNT(*) FROM Utilizatori` pentru verificare

---

## ✅ Checklist Înainte de Ștergere

- [ ] Am backup recent al bazei de date
- [ ] Am exportat utilizatorii importanți (dacă e necesar)
- [ ] Am verificat că sunt pe database-ul corect (Development, nu Production!)
- [ ] Am înțeles că operațiunea este ireversibilă
- [ ] Am citit și înțeles consecințele
- [ ] Sunt pregătit să recreez contul de administrator

---

**Creat:** 2025-01-24  
**Versiune:** 1.0  
**Pentru:** ValyanClinic - Tabela Utilizatori  
**Status:** ✅ Production Ready (cu precauții)

---

**⚠️ REMINDER: Întotdeauna fă backup înainte de operațiuni destructive! ⚠️**
