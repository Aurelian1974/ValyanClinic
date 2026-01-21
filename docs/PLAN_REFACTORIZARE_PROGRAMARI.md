# Plan de Refactorizare - Sistemul de Programări

**Data:** 2026-01-21
**Versiune:** 1.0
**Status:** Propunere

---

## Rezumat Executiv

Sistemul de programări din aplicația ValyanClinic este bine arhitecturat conform principiilor Clean Architecture și CQRS, cu o implementare matură și funcțională. Cu toate acestea, au fost identificate **9 probleme principale** care afectează mentenabilitatea, consistența datelor, și funcționalitatea completă a sistemului.

Acest plan propune o refactorizare incrementală organizată în **3 faze** cu prioritizare bazată pe impact și risc, cu o durată estimată de **8-12 zile de dezvoltare**.

### Beneficii Așteptate:
- ✅ Eliminarea duplicării de cod și logică
- ✅ Îmbunătățirea consistenței datelor (timezone, validări)
- ✅ Funcționalitate completă (notificări email)
- ✅ Conformitate pentru audit și compliance
- ✅ Performanță îmbunătățită (indecși SQL)
- ✅ Extensibilitate (API REST pentru integrări)

---

## 1. Probleme Identificate

### 🔴 P1 - CRITICE (Impact înalt, Risc înalt)

#### **1.1. Timezone Inconsistency**
**Locație:** `Domain/Entities/Programare.cs` - Computed properties
**Problema:**
```csharp
public bool EsteInDesfasurare =>
    DateTime.Now >= DataOraInceput && DateTime.Now <= DataOraSfarsit;
```
- Folosește `DateTime.Now` (local time) în loc de UTC
- Risc major pentru clinici în timezone-uri diferite
- Inconsistență între server timezone și client timezone în aplicații web

**Impact:**
- Programări afișate incorect ca "În desfășurare" sau "Trecută"
- Conflict detection poate eșua dacă serverul schimbă timezone
- Rapoarte și statistici inexacte

**Complexitate:** Medie (3-4 ore)

---

#### **1.2. Missing Email Notifications pentru Anulări**
**Locație:** `Application/Features/ProgramareManagement/Commands/DeleteProgramare/DeleteProgramareCommandHandler.cs`

**TODOs Nerezolvate:**
```csharp
// TODO: Trimite email de notificare către pacient și doctor
// TODO: Log in audit table for compliance
```

**Impact:**
- Pacienții și doctorii nu sunt notificați la anularea programărilor
- Risc reputațional (pacientul se prezintă degeaba)
- Lipsă audit trail complet pentru investigații/dispute

**Complexitate:** Mică (2 ore - infrastructura email deja există)

---

#### **1.3. Missing Foreign Key Constraints**
**Locație:** Schema SQL - Tabelul `Programari`

**Problema:**
- Nu există constraint-uri verificate pentru `PacientID`, `DoctorID`
- Dacă un pacient/doctor este șters, programările devin "orphaned"
- Risc de integritate referențială

**Impact:**
- Date inconsistente în baza de date
- Erori runtime la afișarea programărilor (NULL references)
- Imposibil de reconstituit istoric

**Verificare Necesară:**
```sql
-- Trebuie verificat dacă există:
ALTER TABLE Programari
ADD CONSTRAINT FK_Programari_Pacienti FOREIGN KEY (PacientID)
    REFERENCES Pacienti(PacientID) ON DELETE RESTRICT;

ALTER TABLE Programari
ADD CONSTRAINT FK_Programari_Users_Doctor FOREIGN KEY (DoctorID)
    REFERENCES AspNetUsers(Id) ON DELETE RESTRICT;
```

**Complexitate:** Mică (1-2 ore + migrare date existente dacă e nevoie)

---

### 🟡 P2 - IMPORTANTE (Impact mediu, Risc mediu)

#### **2.1. DTOs cu Câmpuri Nepopulate**
**Locație:** `Application/Features/ProgramareManagement/DTOs/ProgramareListDto.cs`

**Problema:**
```csharp
public class ProgramareListDto
{
    // ... câmpuri populate
    public string? Motiv { get; set; }          // ❌ NULL - nu e populat în SP
    public string? Diagnostic { get; set; }     // ❌ NULL - nu e populat în SP
    public string? TratamentActual { get; set; } // ❌ NULL - nu e populat în SP
}
```

**Impact:**
- Confuzie pentru dezvoltatori (se așteaptă valori, dar sunt NULL)
- UI poate afișa câmpuri goale misleading
- Overhead de memorie inutil

**Soluții Posibile:**
1. **Opțiunea A (Recomandată):** Elimină câmpurile nepopulate din DTO
2. **Opțiunea B:** Populează câmpurile prin JOIN cu tabelul Consultații (dacă există relație)
3. **Opțiunea C:** Adaugă câmpurile în stored procedure

**Complexitate:** Mică (1-2 ore - depinde de opțiune)

---

#### **2.2. Duplicare Logică de Validare**
**Locații Multiple:**
- `Application/Features/ProgramareManagement/Commands/CreateProgramare/CreateProgramareCommandValidator.cs`
- `Application/Features/ProgramareManagement/Commands/UpdateProgramare/UpdateProgramareCommandValidator.cs`
- `Application/Features/ProgramareManagement/Commands/CreateProgramare/CreateProgramareCommandHandler.cs`
- `Domain/Entities/Programare.cs` (computed properties)

**Problema:**
Aceeași logică de validare duplicată în 3-4 locuri:
- Validare weekend
- Validare interval orar (07:00-20:00)
- Validare durată (5min-4ore)
- Validare pacient ≠ doctor

**Impact:**
- Dificil de menținut (modificări în mai multe locuri)
- Risc de inconsistență (uiți să actualizezi una din locații)
- Cod duplicat ~100-150 linii

**Soluție:**
Creează un service centralizat de validare:
```csharp
public interface IProgramareValidationService
{
    Task<ValidationResult> ValidateTimeSlotAsync(DateTime date, TimeSpan start, TimeSpan end);
    Task<ValidationResult> ValidateDoctorAvailabilityAsync(Guid doctorId, DateTime date);
    Task<ValidationResult> ValidateConflictAsync(Guid doctorId, DateTime date, TimeSpan start, TimeSpan end, Guid? excludeId);
}
```

**Complexitate:** Medie (4-6 ore)

---

#### **2.3. Lipsă API Controllers REST**
**Locație:** Lipsă complet - nu există controllere pentru Programari

**Problema:**
- Toate operațiunile sunt accesibile doar prin Blazor (MediatR direct)
- Imposibil de accesat din aplicații externe (mobile app, integrări)
- Nu există endpoint-uri documentate (Swagger)

**Impact:**
- Lipsă extensibilitate pentru integrări
- Nu poate fi consumat de aplicații terțe
- Lipsă documentație API

**Soluție:**
Creează `ProgramareController` cu endpoint-uri standard:
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProgramariController : ControllerBase
{
    [HttpGet] // GET /api/programari?page=1&pageSize=20
    [HttpGet("{id}")] // GET /api/programari/{id}
    [HttpPost] // POST /api/programari
    [HttpPut("{id}")] // PUT /api/programari/{id}
    [HttpDelete("{id}")] // DELETE /api/programari/{id}
    [HttpGet("doctor/{doctorId}")] // GET /api/programari/doctor/{doctorId}
    [HttpGet("pacient/{pacientId}")] // GET /api/programari/pacient/{pacientId}
}
```

**Complexitate:** Mică-Medie (3-4 ore + testing)

---

#### **2.4. Inconsistență în Validatori Create vs Update**
**Locație:**
- `CreateProgramareCommandValidator.cs`
- `UpdateProgramareCommandValidator.cs`

**Problema (Necesită Verificare):**
- `CreateProgramareCommandValidator` exclude weekend, 07:00-20:00
- Trebuie verificat dacă `UpdateProgramareCommandValidator` are aceleași reguli
- Risc: Pot actualiza o programare cu date/ore invalide

**Verificare:**
```csharp
// CreateValidator:
.Must(BeValidDayOfWeek).WithMessage("Programările nu pot fi făcute în weekend")
.Must(BeWithinWorkingHours).WithMessage("Ora trebuie să fie între 07:00 și 20:00")

// UpdateValidator: ❓ Verifică dacă are aceleași reguli
```

**Complexitate:** Mică (1 oră verificare + 1 oră fix dacă e nevoie)

---

### 🟢 P3 - ÎMBUNĂTĂȚIRI (Impact mic, Risc mic)

#### **3.1. SlotBlocat UI/UX Confusion**
**Locație:**
- `Components/Pages/Programari/Modals/ProgramareAddEditModal.razor`
- `Components/Pages/Programari/CalendarProgramari.razor`

**Problema:**
- Permite `TipProgramare = SlotBlocat` fără pacient
- UI-ul de calendar nu diferențiază vizual clar sloturile blocate
- Confuzie pentru utilizatori (se pare ca programare normală)

**Soluție:**
1. Adaugă iconiță/culoare specială pentru SlotBlocat în calendar
2. Adaugă validare explicită în modal: "SlotBlocat nu necesită pacient"
3. Separă secțiunea Pacient cu "(Opțional pentru Slot Blocat)"

**Complexitate:** Mică (2-3 ore)

---

#### **3.2. Missing Audit Logging**
**Locație:** Toate Command Handlers

**Problema:**
- Nu există audit logging centralizat
- TODO nerezolvat în `DeleteProgramareCommandHandler`
- Dificil de reconstituit istoric modificări

**Soluție:**
Implementează `IAuditLogService`:
```csharp
public interface IAuditLogService
{
    Task LogAsync(string entityType, Guid entityId, string action, string userId, object? oldValue, object? newValue);
}
```

Integrează în CommandHandlers:
```csharp
await _auditLogService.LogAsync("Programare", programareId, "Delete", command.ModificatDe, oldProgramare, null);
```

**Complexitate:** Medie (4-5 ore - include tabel SQL + service)

---

#### **3.3. Performance - Verificare Indecși SQL**
**Locație:** Tabelul `Programari` în SQL Server

**Problema:**
- Queries frecvente pe `DoctorID`, `DataProgramare`, `Status`, `PacientID`
- Nu e clar dacă există indecși optimali
- Risc de slow queries cu volume mari de date (1000+ programări)

**Verificare:**
```sql
-- Verifică indecși existenți:
EXEC sp_helpindex 'Programari';

-- Indecși recomandați:
CREATE NONCLUSTERED INDEX IX_Programari_DoctorID_DataProgramare
    ON Programari(DoctorID, DataProgramare) INCLUDE (Status, TipProgramare);

CREATE NONCLUSTERED INDEX IX_Programari_PacientID
    ON Programari(PacientID) INCLUDE (DataProgramare, Status);

CREATE NONCLUSTERED INDEX IX_Programari_Status_DataProgramare
    ON Programari(Status, DataProgramare);
```

**Complexitate:** Mică (2-3 ore - analiza + implementare)

---

#### **3.4. Missing Paging Limit Security**
**Locație:** `Application/Features/ProgramareManagement/Queries/GetProgramareList/GetProgramareListQuery.cs`

**Problema:**
- Nu există limită maximă pentru `PageSize`
- Risc de DoS attack (request cu PageSize=1000000)

**Soluție:**
```csharp
public class GetProgramareListQueryValidator : AbstractValidator<GetProgramareListQuery>
{
    public GetProgramareListQueryValidator()
    {
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100) // ✅ MAX 100 items per page
            .WithMessage("PageSize trebuie să fie între 1 și 100");
    }
}
```

**Complexitate:** Foarte mică (30 min)

---

## 2. Plan de Refactorizare - Organizare pe Faze

### **FAZA 1: Critice & Risc Înalt** (Prioritate: URGENT)
**Durată estimată:** 3-4 zile
**Obiectiv:** Remediază probleme critice care afectează integritatea datelor și funcționalitatea

| Task | Problema | Complexitate | Ore | Fișiere Afectate |
|------|----------|--------------|-----|------------------|
| 1.1 | Timezone UTC Standardization | Medie | 3-4h | `Domain/Entities/Programare.cs`, `*CommandHandlers.cs`, `*Queries.cs` |
| 1.2 | Email Notifications - Anulări | Mică | 2h | `DeleteProgramareCommandHandler.cs`, `IEmailService.cs` |
| 1.3 | Foreign Key Constraints SQL | Mică | 2h | SQL Schema + Migration script |
| 1.4 | Verificare Update Validator | Mică | 2h | `UpdateProgramareCommandValidator.cs` |
| **TOTAL FAZA 1** | | | **9-10h** | |

**Dependențe:** Niciuna - poate începe imediat
**Risc:** Scăzut (modificări izolate, backward compatible)

---

### **FAZA 2: Importante & Refactoring** (Prioritate: ÎNALT)
**Durată estimată:** 3-4 zile
**Obiectiv:** Îmbunătățește mentenabilitatea și extensibilitatea

| Task | Problema | Complexitate | Ore | Fișiere Afectate |
|------|----------|--------------|-----|------------------|
| 2.1 | Curățare DTOs (elimină câmpuri nepopulate) | Mică | 2h | `ProgramareListDto.cs`, `sp_Programari_GetAll.sql`, UI components |
| 2.2 | Centralizare Validări | Medie | 5h | `ProgramareValidationService.cs` (NOU), `*Validators.cs`, `*Handlers.cs` |
| 2.3 | API REST Controllers | Medie | 4h | `ProgramariController.cs` (NOU) + Swagger docs |
| 2.4 | Audit Logging Service | Medie | 4h | `AuditLogService.cs` (NOU), tabel SQL, integrare Handlers |
| **TOTAL FAZA 2** | | | **15h** | |

**Dependențe:**
- Task 2.2 (Centralizare Validări) depinde de Task 1.4 (Update Validator verificat)
- Task 2.4 (Audit Logging) poate integra Task 1.2 (Email Notifications)

**Risc:** Mediu (refactoring substanțial, necesită testing intens)

---

### **FAZA 3: Îmbunătățiri & Optimizări** (Prioritate: MEDIU)
**Durată estimată:** 2-3 zile
**Obiectiv:** Îmbunătățește UX și performanța

| Task | Problema | Complexitate | Ore | Fișiere Afectate |
|------|----------|--------------|-----|------------------|
| 3.1 | SlotBlocat UI/UX Îmbunătățiri | Mică | 3h | `ProgramareAddEditModal.razor`, `CalendarProgramari.razor`, CSS |
| 3.2 | Indecși SQL Performanță | Mică | 3h | SQL Schema + analiza execution plans |
| 3.3 | PageSize Limit Security | Foarte mică | 0.5h | `GetProgramareListQueryValidator.cs` |
| 3.4 | Testing & Validare Completă | Medie | 6h | Toate fișierele modificate |
| **TOTAL FAZA 3** | | | **12.5h** | |

**Dependențe:** Faza 1 și Faza 2 complete
**Risc:** Scăzut (îmbunătățiri izolate)

---

## 3. Estimare Totală

| Fază | Durată (ore) | Durată (zile @8h/zi) | Prioritate |
|------|--------------|----------------------|------------|
| **Faza 1** | 9-10h | 1.5 zile | 🔴 URGENT |
| **Faza 2** | 15h | 2 zile | 🟡 ÎNALT |
| **Faza 3** | 12.5h | 1.5 zile | 🟢 MEDIU |
| **TOTAL** | **36.5-37.5h** | **5-6 zile** | |
| **Buffer (20%)** | +7.5h | +1.5 zile | |
| **TOTAL CU BUFFER** | **44-45h** | **6-7 zile** | |

---

## 4. Fișiere Majore Afectate

### Crearea de Fișiere Noi:
1. `Application/Services/ProgramareValidationService.cs` (interfață + implementare)
2. `Application/Services/AuditLogService.cs` (interfață + implementare)
3. `WebApi/Controllers/ProgramariController.cs`
4. `DevSupport/01_Database/03_Migrations/Add_Programari_ForeignKeys.sql`
5. `DevSupport/01_Database/03_Migrations/Add_AuditLog_Table.sql`
6. `DevSupport/01_Database/04_Indexes/Optimize_Programari_Indexes.sql`

### Modificări în Fișiere Existente:
1. `Domain/Entities/Programare.cs` (UTC timezone)
2. `Application/Features/ProgramareManagement/DTOs/ProgramareListDto.cs` (curățare)
3. `Application/Features/ProgramareManagement/Commands/*/Handlers.cs` (integrare validation service, audit log, email)
4. `Application/Features/ProgramareManagement/Commands/*/Validators.cs` (centralizare)
5. `Components/Pages/Programari/Modals/ProgramareAddEditModal.razor` (SlotBlocat UX)
6. `Components/Pages/Programari/CalendarProgramari.razor` (SlotBlocat styling)
7. `Infrastructure/Repositories/ProgramareRepository.cs` (posibil, dacă e nevoie de ajustări UTC)

---

## 5. Strategia de Migrare & Testing

### Migrare Date (pentru Timezone):
```sql
-- Script de verificare date existente
SELECT COUNT(*) FROM Programari WHERE DataProgramare IS NOT NULL;

-- ❌ NU e nevoie de migrare dacă:
-- - DataProgramare e stored ca DATE (fără time component)
-- - OraInceput/OraSfarsit sunt TimeSpan (fără timezone)

-- ✅ Doar computed properties în C# trebuie modificate să folosească UTC
```

### Testing Plan:
1. **Unit Tests:**
   - Validators (toate scenariile)
   - Validation Service (centralizat)
   - Command Handlers (cu mock dependencies)

2. **Integration Tests:**
   - Repository (SQL queries)
   - Email Service (mock SMTP)
   - API Controllers (HTTP requests)

3. **UI Tests:**
   - Calendar (create, edit, delete programări)
   - Modals (validări real-time)
   - SlotBlocat (flow complet)

4. **Performance Tests:**
   - Query performance cu 10k+ programări
   - Index effectiveness (execution plans)

---

## 6. Riscuri & Mitigări

| Risc | Probabilitate | Impact | Mitigare |
|------|--------------|--------|----------|
| Breaking changes în API după centralizare validări | Medie | Înalt | Feature flags + rollback plan |
| Performance degradation după indecși noi | Scăzută | Mediu | Testare pe copie DB production + monitoring |
| Foreign key constraints blochează operațiuni | Medie | Înalt | Verificare date existente ÎNAINTE de constraint, soft delete pentru pacienti/doctori |
| Timezone migration afectează programări viitoare | Scăzută | Critic | Testing intens + comunicare cu utilizatori |
| Email notifications spam | Medie | Mediu | Rate limiting + queue system |

---

## 7. Criterii de Succes

### Faza 1:
- ✅ Toate computed properties folosesc `DateTime.UtcNow`
- ✅ Email notificări trimise la anulare (log verificabil)
- ✅ Foreign key constraints active (verificat cu `sp_helpconstraint`)
- ✅ Update validator consistent cu Create validator

### Faza 2:
- ✅ DTOs nu au câmpuri NULL nepopulate
- ✅ Centralizare validări: 0 duplicări de cod
- ✅ API Controllers funcționale cu Swagger docs
- ✅ Audit log entries pentru toate operațiunile CRUD

### Faza 3:
- ✅ SlotBlocat are UI distinct (culoare/iconiță)
- ✅ Query performance <10ms pentru 10k programări
- ✅ PageSize limitat la max 100
- ✅ 100% tests passed (unit + integration)

---

## 8. Recomandări Post-Refactoring

După finalizarea celor 3 faze, următoarele îmbunătățiri pot fi considerate:

1. **Notificări SMS** (pe lângă email) - integrare Twilio/similar
2. **Reminder Automated** (24h înainte) - job scheduler (Hangfire/Quartz)
3. **Calendar Export** (iCal/Google Calendar sync)
4. **Conflict Resolution Wizard** (dacă apar conflicte, sugerează alternative)
5. **Analytics Dashboard** (trenduri, rate no-show, doctori cel mai ocupați)
6. **Multi-tenant Support** (dacă clinica are mai multe locații)

---

## 9. Aprobări Necesare

Înainte de a începe refactorizarea, următoarele trebui confirmate:

- [ ] **Business Owner:** Aprobă prioritizarea (Faza 1 → Faza 2 → Faza 3)
- [ ] **Product Owner:** Confirmă că funcționalitățile actuale nu se modifică (backward compatibility)
- [ ] **Tech Lead:** Validează arhitectura propusă (Validation Service, Audit Log)
- [ ] **DevOps:** Confirmă strategia de deployment (migrări SQL, rollback plan)
- [ ] **QA:** Alocă resurse pentru testing (estimate 2-3 zile)

---

## 10. Următorii Pași

1. **Review Plan:** Prezentare către echipă + discuție
2. **Aprobare Formală:** Sign-off de la stakeholders
3. **Branch Setup:** `feature/refactor-programari-faza-1`
4. **Sprint Planning:** Alocă Faza 1 în următorul sprint
5. **Kickoff:** Începe implementarea cu Task 1.1 (Timezone UTC)

---

**Document creat de:** Claude Code Agent
**Baza analiză:** Explore Agent - Raport complet arhitectură programări
**Contact:** Pentru întrebări sau clarificări despre acest plan

---

## Anexa A: Fișiere Cheie de Referință

```
/Domain/Entities/Programare.cs
/Domain/Enums/TipProgramare.cs
/Domain/Enums/ProgramareStatus.cs
/Application/Features/ProgramareManagement/
  ├── Commands/
  │   ├── CreateProgramare/
  │   ├── UpdateProgramare/
  │   └── DeleteProgramare/
  ├── Queries/
  │   ├── GetProgramareList/
  │   ├── GetProgramareById/
  │   └── GetProgramariByWeek/
  └── DTOs/
/Infrastructure/Repositories/ProgramareRepository.cs
/Components/Pages/Programari/
  ├── CalendarProgramari.razor
  ├── ListaProgramari.razor
  └── Modals/
/DevSupport/01_Database/02_StoredProcedures/Programari/
```
