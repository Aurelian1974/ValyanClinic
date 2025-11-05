# 🔍 TROUBLESHOOTING - Modal "Gestionează Doctori"

## ✅ CE AM CORECTAT

### 1. **Two-Way Binding**
Am schimbat din:
```razor
<PacientDoctoriModal IsVisible="@ShowDoctoriModal"
     IsVisibleChanged="@(EventCallback.Factory.Create<bool>(this, value => ShowDoctoriModal = value))"
PacientID="@SelectedPacientId"
  PacientNume="@SelectedPacientNume" />
```

În:
```razor
<PacientDoctoriModal @bind-IsVisible="ShowDoctoriModal"
   PacientID="@SelectedPacientId"
      PacientNume="@SelectedPacientNume" />
```

**Explicație:** `@bind-IsVisible` creează automat two-way binding și este mai simplu.

### 2. **CSS Import**
Am adăugat în `app.css`:
```css
@import url('modals/pacient-doctori-modal.css');
```

**Explicație:** CSS-ul trebuie importat pentru ca modalul să arate corect.

### 3. **Clase Modal**
Am actualizat clasele pentru compatibilitate:
```razor
<div class="modal-overlay visible">
    <div class="modal-content modal-lg show">
```

**Explicație:** Clasele `visible` și `show` activează animațiile din CSS.

---

## 🧪 PAȘI DE TESTARE

### Test 1: Verificare Basic
1. ✅ Rulează aplicația (`dotnet run` sau F5 în Visual Studio)
2. ✅ Navighează la pagina "Vizualizare Pacienți"
3. ✅ Selectează un pacient din grid (click pe linie)
4. ✅ Verifică că toolbar-ul devine activ (border albastru)
5. ✅ Click pe butonul "GESTIONEAZĂ DOCTORI" (violet)

**Rezultat așteptat:** Modalul se deschide cu:
- Header violet cu titlu "Doctori asociați - [Nume Pacient]"
- Loading spinner dacă se încarcă date
- Lista de doctori sau mesaj "Nu există doctori activi"

### Test 2: Verificare în Console (F12)
1. ✅ Deschide Developer Tools (F12)
2. ✅ Click pe tab "Console"
3. ✅ Click "Gestionează Doctori"
4. ✅ Vezi dacă apar erori

**Erori posibile:**
- ❌ `Cannot read property 'Value' of null` → PacientID este null
- ❌ `sp_PacientiPersonalMedical_GetDoctoriByPacient not found` → Stored procedure lipsește
- ❌ `500 Internal Server Error` → Eroare la server

### Test 3: Verificare în Network Tab
1. ✅ Deschide F12 → Tab "Network"
2. ✅ Click "Gestionează Doctori"
3. ✅ Vezi request-uri către server

**Request-uri așteptate:**
- ✅ `/_blazor?...` (SignalR connection)
- ✅ Никакви 404 sau 500 errors

---

## 🐛 PROBLEME COMUNE ȘI SOLUȚII

### Problema 1: Modalul nu se deschide deloc

**Cauze posibile:**
1. ❌ `ShowDoctoriModal` nu se setează pe `true`
2. ❌ `IsVisible` nu se propagă corect la modal
3. ❌ CSS lipsește sau nu e loaded

**Soluții:**
1. ✅ Adaugă `Console.WriteLine` în `HandleManageDoctors()`:
```csharp
private async Task HandleManageDoctors()
{
    Console.WriteLine($"[DEBUG] Opening doctori modal for pacient: {SelectedPacient?.Id}");
    
    if (_disposed || SelectedPacient == null) 
    {
        Console.WriteLine("[DEBUG] Modal not opened - disposed or no pacient selected");
        return;
    }
    
    Console.WriteLine($"[DEBUG] Setting ShowDoctoriModal = true");
    SelectedPacientId = SelectedPacient.Id;
    SelectedPacientNume = SelectedPacient.NumeComplet;
    ShowDoctoriModal = true;

    Console.WriteLine($"[DEBUG] ShowDoctoriModal is now: {ShowDoctoriModal}");
    StateHasChanged();
}
```

2. ✅ Verifică în console dacă debug messages apar

### Problema 2: Modalul se deschide dar e gol/alb

**Cauze posibile:**
1. ❌ `PacientID` este null
2. ❌ Query-ul returnează eroare
3. ❌ CSS nu e loaded corect

**Soluții:**
1. ✅ Adaugă debug în `OnParametersSetAsync`:
```csharp
protected override async Task OnParametersSetAsync()
{
    Console.WriteLine($"[PacientDoctoriModal] OnParametersSetAsync - IsVisible: {IsVisible}, PacientID: {PacientID}");
    
    if (IsVisible && PacientID.HasValue)
    {
     Console.WriteLine($"[PacientDoctoriModal] Loading doctori for PacientID: {PacientID}");
        await LoadDoctori();
    }
    else
    {
        Console.WriteLine($"[PacientDoctoriModal] NOT loading - IsVisible: {IsVisible}, HasValue: {PacientID.HasValue}");
    }
}
```

2. ✅ Verifică în console output

### Problema 3: Eroare "Stored procedure not found"

**Cauze:**
❌ Stored procedures nu au fost create în database

**Soluții:**
1. ✅ Rulează din nou deployment script:
```powershell
cd D:\Lucru\CMS\DevSupport\Scripts\PowerShellScripts
.\Deploy-PacientiPersonalMedical.ps1
```

2. ✅ Verifică în SQL Server Management Studio:
```sql
USE ValyanMed
GO

SELECT name FROM sys.procedures 
WHERE name LIKE 'sp_PacientiPersonalMedical%'
ORDER BY name
```

**Rezultat așteptat:** 8 stored procedures

### Problema 4: Modalul se deschide dar nu arată doctori

**Cauze posibile:**
1. ❌ Nu există relații în database
2. ❌ Query-ul returnează vid
3. ❌ DTO mapping e greșit

**Soluții:**
1. ✅ Testează direct în SQL:
```sql
-- Caută un pacient
SELECT TOP 1 Id, Nume, Prenume FROM Pacienti WHERE Activ = 1

-- Execută stored procedure (folosește ID-ul de mai sus)
EXEC sp_PacientiPersonalMedical_GetDoctoriByPacient 
    @PacientID = 'GUID-UL-AICI',
    @ApenumereActivi = 0
```

2. ✅ Dacă nu returnează nimic, adaugă relații de test:
```sql
DECLARE @PacientID UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Pacienti WHERE Activ = 1)
DECLARE @DoctorID UNIQUEIDENTIFIER = (SELECT TOP 1 PersonalID FROM PersonalMedical WHERE EsteActiv = 1)

EXEC sp_PacientiPersonalMedical_AddRelatie 
 @PacientID = @PacientID,
    @PersonalMedicalID = @DoctorID,
    @TipRelatie = 'MedicPrimar',
    @Motiv = 'Test relatie',
    @CreatDe = 'Admin'
```

---

## ✅ CHECKLIST VERIFICARE

Bifează pe măsură ce verifici:

### Frontend
- [ ] Build-ul reușește fără erori
- [ ] Pagina "Vizualizare Pacienți" se încarcă
- [ ] Grid-ul afișează pacienți
- [ ] Selectând un pacient activează toolbar-ul
- [ ] Butonul "Gestionează Doctori" e vizibil și violet
- [ ] Click pe buton deschide modalul
- [ ] Modalul are header violet cu titlu
- [ ] Modalul are buton X pentru închidere

### Backend
- [ ] SQL Server e pornit
- [ ] Database ValyanMed există
- [ ] Tabela Pacienti_PersonalMedical există
- [ ] Cele 8 stored procedures există
- [ ] Connection string e corect în appsettings.json

### Data
- [ ] Există pacienți în tabela Pacienti
- [ ] Există doctori în tabela PersonalMedical
- [ ] (Opțional) Există relații în Pacienti_PersonalMedical

---

## 🔧 COMENZI UTILE

### Verificare SQL Server
```sql
-- Check connection
SELECT @@VERSION

-- Check tables
SELECT name FROM sys.tables 
WHERE name IN ('Pacienti', 'PersonalMedical', 'Pacienti_PersonalMedical')

-- Check stored procedures
SELECT name FROM sys.procedures 
WHERE name LIKE 'sp_PacientiPersonalMedical%'

-- Count data
SELECT 
    (SELECT COUNT(*) FROM Pacienti WHERE Activ = 1) AS PacientiActivi,
    (SELECT COUNT(*) FROM PersonalMedical WHERE EsteActiv = 1) AS DoctoriActivi,
    (SELECT COUNT(*) FROM Pacienti_PersonalMedical WHERE EsteActiv = 1) AS RelatiiActive
```

### Verificare Aplicație
```bash
# Clean build
dotnet clean
dotnet build

# Run with logging
dotnet run --project ValyanClinic --verbosity detailed

# Watch for changes
dotnet watch run --project ValyanClinic
```

---

## 📞 DACĂ PROBLEMA PERSISTĂ

### Pași de debugging avansat:

1. **Adaugă breakpoint** în `VizualizarePacienti.razor.cs`:
   - Linia `ShowDoctoriModal = true;`
   - Vezi dacă se execută

2. **Adaugă breakpoint** în `PacientDoctoriModal.razor`:
   - Linia `await LoadDoctori();`
   - Vezi dacă se execută

3. **Verifică în browser DevTools**:
   - Tab "Elements" → Caută `modal-overlay`
   - Vezi dacă modalul e în DOM dar ascuns

4. **Check CSS**:
   - În DevTools, caută clasa `.modal-overlay`
   - Vezi dacă are `display: none` sau `visibility: hidden`

5. **Salvează screenshot** sau **copiază error message** și:
   - Verifică în console log
   - Verifică în Output window (Visual Studio)
   - Verifică în SQL Server logs

---

## ✅ SUCCESS INDICATORS

Modalul funcționează corect dacă:
- ✅ Se deschide smooth cu animație fade-in
- ✅ Afișează loading spinner inițial
- ✅ După loading, afișează lista de doctori SAU "Nu există doctori"
- ✅ Butonul X închide modalul
- ✅ Click pe overlay (fundal întunecat) închide modalul
- ✅ Butonul "+ Adaugă doctor" deschide al doilea modal
- ✅ CSS-ul arată bine (violet header, cards cu shadow, etc.)

---

**Versiune:** 1.0  
**Data:** 2025-01-23  
**Status:** Corecțiile au fost aplicate  
**Next:** Testare în browser

🎯 **Rulează aplicația și testează!**
