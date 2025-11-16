# 🔐 AUTHORIZATION & SECURITY ROADMAP - ValyanClinic

**Data:** 2025-01-16  
**Status:** 📋 **PLANIFICAT PENTRU VIITOR**  
**Current State:** ✅ Role-based authorization activ (Roles: "Doctor,Medic")

---

## 🎯 **STADIU ACTUAL**

### ✅ **Implementat:**
1. ✅ **Role-based Authorization** - `@attribute [Authorize(Roles = "Doctor,Medic")]`
2. ✅ **Claims-based Authentication** - PersonalMedicalID în claims
3. ✅ **Session Management** - CustomAuthenticationStateProvider
4. ✅ **Password Hashing** - BCrypt
5. ✅ **Session Expiration** - 8 ore
6. ✅ **Account Lockout** - 5 încercări eșuate

---

## 🚀 **PLANIFICARE VIITOARE**

### **FAZA 1: Granular Permissions (P1)** 🔴 **HIGH PRIORITY**

#### **1.1 Permissions Matrix**
```csharp
// Permisiuni granulare per resursă
public enum Resource
{
    Pacienti,
    Programari,
    Consultatii,
    Retete,
    Investigatii,
    Documente,
    Rapoarte,
    Setari,
    Utilizatori
}

public enum Action
{
    View,       // Vizualizare
    Create,     // Creare
    Edit,       // Editare
    Delete,     // Ștergere
    Export,     // Export date
    Approve     // Aprobare (workflow)
}

public class Permission
{
    public Guid PermissionID { get; set; }
    public Resource Resource { get; set; }
    public Action Action { get; set; }
    public string Scope { get; set; } // "Own", "Team", "Department", "All"
}
```

#### **1.2 Role-Permission Mapping**
```sql
CREATE TABLE Roluri (
    RolID UNIQUEIDENTIFIER PRIMARY KEY,
    NumeRol NVARCHAR(50) NOT NULL,
    Descriere NVARCHAR(200),
    EsteSystemRole BIT DEFAULT 0,  -- Nu poate fi șters
    EsteActiv BIT DEFAULT 1
);

CREATE TABLE Permisiuni (
    PermisiuneID UNIQUEIDENTIFIER PRIMARY KEY,
    Resursa NVARCHAR(50) NOT NULL,  -- Pacienti, Programari, etc.
    Actiune NVARCHAR(50) NOT NULL,  -- View, Create, Edit, Delete
    Scop NVARCHAR(50) NOT NULL,     -- Own, Team, Department, All
    Descriere NVARCHAR(200)
);

CREATE TABLE Roluri_Permisiuni (
    RolID UNIQUEIDENTIFIER,
    PermisiuneID UNIQUEIDENTIFIER,
    PRIMARY KEY (RolID, PermisiuneID),
    FOREIGN KEY (RolID) REFERENCES Roluri(RolID),
    FOREIGN KEY (PermisiuneID) REFERENCES Permisiuni(PermisiuneID)
);
```

#### **1.3 Permission Check Service**
```csharp
public interface IPermissionService
{
    Task<bool> HasPermissionAsync(
        Guid userId, 
        Resource resource, 
        Action action, 
        string scope = "Own"
    );
    
    Task<List<Permission>> GetUserPermissionsAsync(Guid userId);
    Task<bool> CanAccessPacientAsync(Guid userId, Guid pacientId);
    Task<bool> CanModifyProgramareAsync(Guid userId, Guid programareId);
}

// Usage:
@inject IPermissionService PermissionService

@if (await PermissionService.HasPermissionAsync(currentUserId, Resource.Pacienti, Action.Delete))
{
    <button @onclick="DeletePacient">Șterge Pacient</button>
}
```

---

### **FAZA 2: Advanced Authorization Rules (P1)** 🟠 **MEDIUM PRIORITY**

#### **2.1 Field-Level Security**
```csharp
// Mascare date sensibile bazat pe rol
public class PacientDto
{
    public string NumeComplet { get; set; }
    
    [Authorize(Roles = "Doctor,Asistent")]
    public string CNP { get; set; }  // Doar Doctor/Asistent
    
    [Authorize(Roles = "Doctor")]
    public string? Diagnostic { get; set; }  // Doar Doctor
    
    [Authorize(Roles = "Doctor,Administrator")]
    public decimal? Tarif { get; set; }  // Doar Doctor/Admin
}

// Auto-masking service
public class DataMaskingService
{
    public T ApplyMasking<T>(T dto, ClaimsPrincipal user)
    {
        // Reflection-based masking bazat pe Authorize attributes
    }
}
```

#### **2.2 Temporal Permissions**
```csharp
// Acces restricționat la ore specifice
[TimeBasedAuthorize(StartTime = "08:00", EndTime = "18:00")]
public class Programari : ComponentBase { }

// Sau per utilizator
public class UtilizatorSettings
{
    public Guid UtilizatorID { get; set; }
    public TimeSpan? AccessStartTime { get; set; }  // 08:00
    public TimeSpan? AccessEndTime { get; set; }    // 18:00
    public List<DayOfWeek> AllowedDays { get; set; } // Luni-Vineri
}
```

#### **2.3 Geographic Restrictions**
```csharp
// Acces doar la pacienți din regiunea specifică
[GeographicAuthorize(AllowedRegions = "Bucuresti,Ilfov")]
public class PacientiLocalitate : ComponentBase { }

// Sau per doctor
public class PersonalMedical
{
    public List<string> AllowedJudete { get; set; }  // Doctor poate vedea doar pacienți din aceste județe
    public string? PrimaryCabinet { get; set; }      // Locație principală
}
```

---

### **FAZA 3: Advanced Security Features (P2)** 🟡 **LOW PRIORITY**

#### **3.1 Two-Factor Authentication (2FA)**
```csharp
public class Utilizator
{
    public bool IsTwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }  // TOTP secret
    public List<string> BackupCodes { get; set; }  // 10 backup codes
}

// Login flow:
1. Username + Password ✅
2. → Generate TOTP code
3. User input TOTP code
4. Verify TOTP → Grant access
```

#### **3.2 Session Management Advanced**
```csharp
public class UserSession
{
    public Guid SessionID { get; set; }
    public Guid UtilizatorID { get; set; }
    public string DeviceInfo { get; set; }      // Browser, OS, IP
    public string IPAddress { get; set; }
    public DateTime LoginTime { get; set; }
    public DateTime LastActivityTime { get; set; }
    public bool IsActive { get; set; }
}

// Features:
- Limită sesiuni concurente (max 3 per user)
- View active sessions
- Force logout all sessions
- Session hijacking detection (IP change)
```

#### **3.3 Audit Trail Complete**
```csharp
public class AuditLog
{
    public Guid AuditID { get; set; }
    public Guid UtilizatorID { get; set; }
    public string Action { get; set; }           // "VIEW", "CREATE", "UPDATE", "DELETE"
    public string Resource { get; set; }         // "Pacient", "Programare"
    public Guid? ResourceID { get; set; }
    public string OldValue { get; set; }         // JSON
    public string NewValue { get; set; }         // JSON
    public string IPAddress { get; set; }
    public string DeviceInfo { get; set; }
    public DateTime Timestamp { get; set; }
}

// Log automat la:
- Toate operațiile CRUD
- Login/Logout
- Failed login attempts
- Permission changes
- Sensitive data access (CNP, diagnostice)
```

#### **3.4 Data Encryption at Rest**
```csharp
// Criptare câmpuri sensibile în database
public class Pacient
{
    public Guid Id { get; set; }
    
    [Encrypted]
    public string CNP { get; set; }  // Criptat în DB
    
    [Encrypted]
    public string? Alergii { get; set; }  // Criptat în DB
    
    [Encrypted]
    public string? BoliCronice { get; set; }  // Criptat în DB
}

// Service:
public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
```

---

### **FAZA 4: Compliance & GDPR (P1)** 🔴 **HIGH PRIORITY**

#### **4.1 Consent Management**
```csharp
public class PacientConsent
{
    public Guid ConsentID { get; set; }
    public Guid PacientID { get; set; }
    public string ConsentType { get; set; }  // "DataProcessing", "Marketing", "Research"
    public bool IsGranted { get; set; }
    public DateTime GrantedDate { get; set; }
    public DateTime? RevokedDate { get; set; }
    public int Version { get; set; }  // Tracking version consent form
}

// Verificare înainte de processing:
if (!await ConsentService.HasConsentAsync(pacientId, "DataProcessing"))
{
    throw new UnauthorizedAccessException("Pacientul nu a acordat consimțământ pentru prelucrare date");
}
```

#### **4.2 Right to Access (GDPR Art. 15)**
```csharp
// Pacient poate solicita toate datele sale
public interface IGDPRService
{
    Task<PacientDataExport> ExportAllDataAsync(Guid pacientId);
    Task<bool> AnonymizeDataAsync(Guid pacientId);  // Right to be forgotten
    Task<bool> RectifyDataAsync(Guid pacientId, PacientUpdateDto updates);
}

// Export format: JSON structurat cu toate datele
{
    "pacient": { "nume": "...", "cnp": "..." },
    "consultatii": [...],
    "programari": [...],
    "diagnostic": [...],
    "retete": [...],
    "facturi": [...]
}
```

#### **4.3 Data Retention Policies**
```csharp
public class DataRetentionPolicy
{
    public string ResourceType { get; set; }  // "Consultatie", "Programare"
    public int RetentionYears { get; set; }   // Câți ani se păstrează
    public bool AutoArchive { get; set; }     // Archive automat după expirare
    public bool AutoDelete { get; set; }      // Ștergere automată
}

// Job automat:
// - Archive consultation data > 7 ani
// - Delete programări cancelled > 5 ani
// - Anonymize pacienți inactivi > 10 ani
```

---

### **FAZA 5: Advanced Workflows (P2)** 🟡 **LOW PRIORITY**

#### **5.1 Approval Workflows**
```csharp
// Workflow pentru acțiuni critice
public class ApprovalWorkflow
{
    public Guid WorkflowID { get; set; }
    public string ActionType { get; set; }     // "DeletePacient", "ExportData"
    public Guid RequestedBy { get; set; }      // Cine solicită
    public Guid? ApprovedBy { get; set; }      // Cine aprobă
    public string Status { get; set; }         // "Pending", "Approved", "Rejected"
    public DateTime RequestDate { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? Reason { get; set; }
}

// Exemplu:
// 1. Asistent vrea să șteargă pacient → Request Approval
// 2. Manager primește notificare
// 3. Manager aprobă → Ștergere efectivă
```

#### **5.2 Segregation of Duties**
```csharp
// Un user NU poate aproba propria acțiune
public class ActionApprovalRule
{
    public string ActionType { get; set; }
    public bool RequiresApproval { get; set; }
    public List<string> ApproverRoles { get; set; }  // Cine poate aproba
    public bool AllowSelfApproval { get; set; }      // Default: FALSE
}

// Check:
if (action.RequestedBy == action.ApprovedBy && !rule.AllowSelfApproval)
{
    throw new BusinessRuleException("Nu poți aproba propria acțiune");
}
```

---

## 📊 **PRIORITY MATRIX**

| Feature | Priority | Complexity | Effort | Status |
|---------|----------|------------|--------|--------|
| **Granular Permissions** | 🔴 P1 | HIGH | 3-4 săptămâni | 📋 Planned |
| **Field-Level Security** | 🔴 P1 | MEDIUM | 1-2 săptămâni | 📋 Planned |
| **GDPR Compliance** | 🔴 P1 | HIGH | 2-3 săptămâni | 📋 Planned |
| **Audit Trail** | 🟠 P1 | MEDIUM | 1-2 săptămâni | 📋 Planned |
| **2FA** | 🟡 P2 | MEDIUM | 1 săptămână | 📋 Planned |
| **Session Management** | 🟡 P2 | LOW | 3-5 zile | 📋 Planned |
| **Data Encryption** | 🟡 P2 | HIGH | 2 săptămâni | 📋 Planned |
| **Temporal Permissions** | 🟢 P3 | LOW | 2-3 zile | 📋 Planned |
| **Geographic Restrictions** | 🟢 P3 | LOW | 2-3 zile | 📋 Planned |
| **Approval Workflows** | 🟢 P3 | HIGH | 2-3 săptămâni | 📋 Planned |

---

## 🛠️ **IMPLEMENTATION PLAN**

### **Sprint 1-2: Granular Permissions (P1)**
- [ ] Create Roluri, Permisiuni, Roluri_Permisiuni tables
- [ ] Implement PermissionService
- [ ] Create Permission Matrix UI (Blazor component)
- [ ] Update all pages with permission checks
- [ ] Testing & validation

### **Sprint 3: Field-Level Security (P1)**
- [ ] Implement DataMaskingService
- [ ] Add [Authorize] attributes per field
- [ ] Update DTOs with security attributes
- [ ] Testing masking logic

### **Sprint 4-5: GDPR Compliance (P1)**
- [ ] Implement ConsentManagement
- [ ] Create GDPR export functionality
- [ ] Implement Right to be Forgotten
- [ ] Create Data Retention Policies
- [ ] Testing GDPR workflows

### **Sprint 6: Audit Trail (P1)**
- [ ] Create AuditLog table
- [ ] Implement AuditService
- [ ] Add automatic logging interceptor
- [ ] Create Audit Log Viewer UI
- [ ] Testing audit logging

### **Sprint 7: 2FA (P2)**
- [ ] TOTP integration
- [ ] Backup codes generation
- [ ] 2FA setup UI
- [ ] Testing 2FA flow

### **Sprint 8: Advanced Session Management (P2)**
- [ ] Multi-session tracking
- [ ] Active sessions viewer
- [ ] Force logout functionality
- [ ] Session hijacking detection

---

## 📚 **DOCUMENTAȚIE NECESARĂ**

### **Pentru Dezvoltatori:**
1. **Authorization Architecture Guide**
2. **Permission System Design Document**
3. **Security Best Practices**
4. **GDPR Compliance Checklist**
5. **Audit Trail Guidelines**

### **Pentru Administratori:**
1. **Role Management User Manual**
2. **Permission Matrix Configuration**
3. **GDPR Compliance Operations**
4. **Security Incident Response Plan**

### **Pentru Utilizatori:**
1. **Password Best Practices**
2. **2FA Setup Guide**
3. **Data Privacy Rights (GDPR)**
4. **Session Management User Guide**

---

## ✅ **CHECKLIST PRE-IMPLEMENTATION**

Înainte de a începe implementarea, asigură-te că:

- [ ] **Database Design** complet și aprobat
- [ ] **Security Audit** initial efectuat
- [ ] **GDPR Legal Review** complet
- [ ] **Performance Impact Assessment** (permissions checks la fiecare request)
- [ ] **Caching Strategy** pentru permissions (evită query la fiecare click)
- [ ] **Migration Plan** pentru date existente
- [ ] **Rollback Strategy** documentată
- [ ] **Testing Strategy** detaliată (unit + integration + security tests)
- [ ] **Training Plan** pentru utilizatori și administratori
- [ ] **Documentation** ready (technical + user manuals)

---

## 🎯 **SUCCESS METRICS**

### **Security:**
- ✅ **Zero security incidents** în primele 3 luni
- ✅ **100% permission checks** pe operații critice
- ✅ **Audit log coverage** >99% pentru acțiuni critice
- ✅ **GDPR compliance** 100% (toate drepturile implementate)

### **Performance:**
- ✅ **Permission check latency** <50ms (cu caching)
- ✅ **Audit log write** <100ms (async)
- ✅ **No performance degradation** pentru utilizatori (<5% overhead)

### **Usability:**
- ✅ **User satisfaction** >4.5/5 pentru UI permissions
- ✅ **Admin feedback** pozitiv pentru role management
- ✅ **Zero confusion** despre permissions (clear messaging)

---

## 📝 **NOTES**

### **Considerații Importante:**

1. **Performance:** Permission checks la fiecare request → **CACHE aggressive**
   - In-memory cache pentru permissions (refresh la 5 min)
   - Redis pentru multi-server scenarios

2. **User Experience:** Nu bloca UX-ul cu prea multe pop-up-uri de acces denied
   - Hide buttons user-ul nu are acces (nu doar disable)
   - Mesaje clare și friendly pentru denied access

3. **Backward Compatibility:** Migrare graduală
   - Phase 1: Roles (current) + Granular permissions (side by side)
   - Phase 2: Deprecate simple roles, move to permissions only

4. **Testing:** Security testing critical
   - Penetration testing după implementare
   - Regular security audits (quarterly)

---

**Status:** 📋 **PLANIFICAT PENTRU VIITOR**  
**Estimare timp total:** 8-12 săptămâni (pentru toate features P1 + P2)  
**Recomandare:** Implementare iterativă, un sprint la 1-2 săptămâni

---

**Document creat:** 2025-01-16  
**Versiune:** 1.0  
**Aplicație:** ValyanClinic - .NET 9 Blazor Server  
**Review Status:** 📋 **READY FOR PLANNING**
