# 📧 **SendDailyEmailModal - Email Composer Modal**

**Data creării:** 2025-01-20  
**Status:** ✅ **IMPLEMENTED** (UI Complete, Business Logic TODO)  
**Design:** Gmail/Yahoo/Outlook style email composer

---

## 🎯 **OBIECTIV**

Modal **email composer** profesional pentru previzualizarea și trimiterea email-urilor cu programările de mâine către doctori.

---

## 📍 **LOCAȚIE**

### **Component:**
```
ValyanClinic/Components/Pages/Programari/Modals/
├── SendDailyEmailModal.razor
└── SendDailyEmailModal.razor.cs
```

### **Invocat din:**
- **CalendarProgramari** - buton "📧 Email Programări Mâine"

---

## 🎨 **DESIGN - GMAIL STYLE**

### **Layout Structure:**

```
┌──────────────────────────────────────────────────────┐
│  📧 Email nou            [X]    │ ← Header
├──────────────────────────────────────────────────────┤
│  Către:  👥 5 doctori (Dr. Popescu, Dr. Ionescu...) │ ← TO Field
│  De la:  🏥 ValyanClinic <clinica.valyan@gmail.com> │ ← FROM Field
│  Subiect: 📅 Programările tale pentru 16.11.2025   │ ← SUBJECT Field
├──────────────────────────────────────────────────────┤
│          │
│  ┌────────────────────────────────────────────────┐ │
│  │  📅 Programările tale pentru 16.11.2025       │ │
│  │  Dr. [Nume Doctor]            │ │
│  ├────────────────────────────────────────────────┤ │
│  │         │ │ ← Email Preview
│  │  Bună ziua, Dr. [Nume]!      │ │   (HTML formatted)
│  │  Aici sunt programările tale pentru mâine:   │ │
│  │      │ │
│  │  [Tabel programări]  │ │
│  │               │ │
│  └────────────────────────────────────────────────┘ │
│          │
│  📊 Stats: 5 Doctori | 15 Programări               │
├──────────────────────────────────────────────────────┤
│  Se vor trimite 5 email-uri    [Anulează] [Trimite]│ ← Footer
└──────────────────────────────────────────────────────┘
```

---

## ⚙️ **COMPONENTELE MODALULUI**

### **1. HEADER - Email Composer Style**

**Design:**
- Background: `#f9fafb` (light gray)
- Icon: 📧 (fa-envelope)
- Title: "Email nou"
- Close button: Hover effect

**Code:**
```razor
<div class="email-header">
    <div>
    <i class="fas fa-envelope"></i>
        <h3>Email nou</h3>
 </div>
    <button @onclick="Close">
  <i class="fas fa-times"></i>
    </button>
</div>
```

---

### **2. EMAIL FIELDS - TO / FROM / SUBJECT**

#### **A. TO Field (Către:)**

**Display:**
```
Către: 👥 5 doctori (Dr. Popescu Ion, Dr. Ionescu Maria, +3)
```

**States:**
- **Loading:** "Se încarcă destinatari..."
- **Success:** Shows count + first 2 names
- **Empty:** "Nu s-au găsit destinatari" (red text)

#### **B. FROM Field (De la:)**

**Display:**
```
De la: 🏥 ValyanClinic <clinica.valyan@gmail.com>
```

**Fixed:** Non-editable, pre-filled from `appsettings.json`

#### **C. SUBJECT Field (Subiect:)**

**Display:**
```
Subiect: 📅 Programările tale pentru 16.11.2025 - ValyanClinic
```

**Dynamic:** Date changed based on `TargetDate` parameter

---

### **3. BODY PREVIEW - HTML Email Template**

**Content:**
- **Header:** Gradient blue banner with title + doctor name
- **Greeting:** "Bună ziua, Dr. [Nume]!"
- **Date:** Formatted date (ex: "luni, 16 noiembrie 2025")
- **Appointments Table:**
  - Columns: Oră | Pacient | Tip | Status
  - Rows: Sample appointments (2-3 examples)
- **Stats Box:** Total programări count
- **Footer:** Auto-generated message + copyright

**States:**
- **Loading:** Spinner + "Se încarcă previzualizare..."
- **Empty:** Inbox icon + "Nu există programări"
- **Success:** Full HTML preview

---

### **4. STATS BAR**

**Display:**
```
[  5  ][ 15  ]
Doctori    Programări
```

**Styling:**
- White cards with shadow
- Blue/green accent colors
- Centered below preview

---

### **5. FOOTER - ACTION BUTTONS**

#### **Left Side:**
```
Se vor trimite 5 email-uri
```

#### **Right Side:**
```
[Anulează]  [Trimite Email-uri]
```

**Button States:**
- **Anulează:** Always enabled (unless sending)
- **Trimite:**
  - **Disabled:** If no recipients or already sending
  - **Loading:** Spinner + "Se trimit..."
  - **Enabled:** Blue gradient with icon

---

## 🔧 **PARAMETERS**

### **Input Parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `IsVisible` | `bool` | `false` | Show/hide modal |
| `TargetDate` | `DateTime` | Tomorrow | Data pentru email-uri |

### **Output Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `IsVisibleChanged` | `EventCallback<bool>` | Notifică parent când se închide |
| `OnEmailsSent` | `EventCallback<int>` | Returnează număr email-uri trimise |

---

## 📊 **DATA FLOW**

### **1. Modal Open:**

```csharp
// User click pe buton în CalendarProgramari
private void OpenSendEmailModal()
{
    ShowSendEmailModal = true;
}
```

### **2. Load Preview Data:**

```csharp
protected override async Task OnParametersSetAsync()
{
    if (IsVisible && DoctorRecipients == null)
    {
 await LoadPreviewData();
    }
}
```

**What LoadPreviewData does:**
1. Query DB pentru programări în `TargetDate`
2. Group by `DoctorID`
3. Extract doctor emails
4. Count total appointments
5. Populate `DoctorRecipients` list

### **3. User Actions:**

#### **A. Click "Anulează":**
```csharp
private async Task Close()
{
  DoctorRecipients = null; // Reset data
    await IsVisibleChanged.InvokeAsync(false);
}
```

#### **B. Click "Trimite Email-uri":**
```csharp
private async Task HandleSend()
{
  IsSending = true;
    
    var emailsSent = await EmailService.SendDailyAppointmentsEmailAsync(TargetDate);
  
    await OnEmailsSent.InvokeAsync(emailsSent);
    await Close();
}
```

#### **C. Click Overlay (outside modal):**
```csharp
private void HandleOverlayClick()
{
    if (!IsSending) // Prevent close while sending
    {
_ = Close();
    }
}
```

---

## 🎨 **STYLING FEATURES**

### **Animations:**

1. **Modal Entry:**
```css
@keyframes slideUp {
    from { opacity: 0; transform: translateY(20px); }
    to { opacity: 1; transform: translateY(0); }
}
```

2. **Overlay Fade:**
```css
@keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
}
```

### **Interactive Elements:**

- **Close Button:** Hover → gray background
- **Send Button:** Gradient + shadow + hover lift effect
- **Overlay Click:** Close modal (if not sending)

---

## 🧪 **CURRENT STATUS - DEMO MODE**

### **✅ Implemented:**
- Modal UI complete (Gmail style)
- Email fields (TO/FROM/SUBJECT)
- HTML preview template
- Loading states
- Button interactions
- Animations

### **⚠️ TODO - Mock Data:**

**LoadPreviewData trenutrente returnează date MOCK:**

```csharp
DoctorRecipients = new List<DoctorRecipientDto>
{
    new() { NumeComplet = "Dr. Popescu Ion", Email = "popescu@example.com", NumarProgramari = 5 },
    new() { NumeComplet = "Dr. Ionescu Maria", Email = "ionescu@example.com", NumarProgramari = 3 }
};
```

**Pentru REAL data, trebuie:**
1. Inject `IMediator` în `SendDailyEmailModal`
2. Query `GetProgramariByDateQuery(TargetDate)`
3. Group by `DoctorID`
4. Extract `DoctorEmail` (necesită adăugare în DTO!)
5. Populate `DoctorRecipients` cu date reale

---

## 🔨 **IMPLEMENTARE COMPLETĂ - NEXT STEPS**

### **Pas 1: Inject Dependencies**

```csharp
// În SendDailyEmailModal.razor.cs
[Inject] private IMediator Mediator { get; set; } = default!;
```

### **Pas 2: Implementează LoadPreviewData**

```csharp
private async Task LoadPreviewData()
{
    IsLoading = true;
    
    try
    {
        // Query programări pentru TargetDate
        var query = new GetProgramariByDateQuery 
    { 
      DataProgramare = TargetDate,
          DoctorID = null // All doctors
    };
 
   var result = await Mediator.Send(query);
        
    if (!result.IsSuccess || result.Value == null)
    {
            DoctorRecipients = new List<DoctorRecipientDto>();
            return;
    }
 
      // Group by Doctor
  var groupedByDoctor = result.Value
            .Where(p => !string.IsNullOrEmpty(p.DoctorEmail))
     .GroupBy(p => p.DoctorID)
     .ToList();

        DoctorRecipients = groupedByDoctor.Select(g => new DoctorRecipientDto
     {
     NumeComplet = g.First().DoctorNumeComplet,
      Email = g.First().DoctorEmail!,
     NumarProgramari = g.Count()
  }).ToList();
 
   TotalAppointments = DoctorRecipients.Sum(d => d.NumarProgramari);
        
        Logger.LogInformation("✅ Loaded {DoctorCount} doctors with {TotalAppts} appointments",
            DoctorRecipients.Count, TotalAppointments);
    }
    catch (Exception ex)
{
  Logger.LogError(ex, "❌ Error loading preview data");
  DoctorRecipients = new List<DoctorRecipientDto>();
    }
    finally
    {
  IsLoading = false;
    }
}
```

### **Pas 3: Adaugă DoctorEmail în DTO**

**Problem:** `ProgramareListDto` NU conține `DoctorEmail`!

**Soluție:**

```csharp
// În ProgramareListDto.cs
public string? DoctorEmail { get; set; }
```

**Și în Stored Procedure:**

```sql
-- sp_Programari_GetByDate.sql
SELECT 
    -- ... alte câmpuri ...
    doc.Email AS DoctorEmail
FROM Programari p
INNER JOIN PersonalMedical doc ON p.DoctorID = doc.PersonalID
```

---

## 📝 **USAGE EXAMPLE**

### **În CalendarProgramari.razor:**

```razor
<!-- Buton -->
<button class="btn btn-info" @onclick="OpenSendEmailModal">
    <i class="fas fa-envelope"></i>
    Email Programări Mâine
</button>

<!-- Modal -->
<SendDailyEmailModal 
    IsVisible="@ShowSendEmailModal"
    IsVisibleChanged="@(EventCallback.Factory.Create<bool>(this, v => ShowSendEmailModal = v))"
    TargetDate="@DateTime.Today.AddDays(1)"
    OnEmailsSent="HandleEmailsSent" />
```

### **În CalendarProgramari.razor.cs:**

```csharp
private bool ShowSendEmailModal { get; set; }

private void OpenSendEmailModal()
{
    ShowSendEmailModal = true;
}

private async Task HandleEmailsSent(int emailsSent)
{
    ShowSendEmailModal = false;
    
    if (emailsSent > 0)
    {
  await NotificationService.ShowSuccessAsync(
      $"✅ Trimise {emailsSent} email-uri cu succes!");
    }
    else
    {
    await NotificationService.ShowWarningAsync(
            "⚠️ Nu s-au putut trimite email-urile.");
    }
}
```

---

## 🎉 **FEATURES HIGHLIGHTS**

### **✅ Professional Design:**
- Gmail/Yahoo/Outlook style composer
- Clean, modern UI
- Responsive layout

### **✅ User Experience:**
- Preview before send
- Loading states
- Disable during send
- Error handling
- Toast notifications

### **✅ Flexibility:**
- TargetDate parameter (not just tomorrow)
- Extensible for other email types
- Reusable component

### **✅ Safety:**
- Confirm before send (modal itself is confirmation)
- Show recipient count
- Preview email content
- Can't close while sending

---

## 🚀 **NEXT RELEASE (v2.0)**

### **Planned Features:**

1. **Edit Fields:**
   - Editable subject line
   - Custom message addition
   - CC/BCC support

2. **Advanced Preview:**
   - Per-doctor preview (dropdown selector)
   - Show actual appointment details
   - Attachment support (PDF reports)

3. **Scheduling:**
   - "Send later" option
   - Pick specific time
   - Recurring sends

4. **Analytics:**
   - Track open rates
   - Track click rates
   - Delivery status

---

## 📊 **STATUS SUMMARY**

**UI:** ✅ **COMPLETE** (Gmail style, responsive, animated)  
**Data Loading:** ⚠️ **MOCK** (needs real DB query implementation)  
**Email Sending:** ✅ **READY** (calls EmailService.SendDailyAppointmentsEmailAsync)  
**Error Handling:** ✅ **IMPLEMENTED**  
**Build Status:** ✅ **SUCCESS**

---

*Document creat: 2025-01-20*  
*Framework: .NET 9 + Blazor Server*  
*Design inspiration: Gmail Composer*  
*Status: 🎨 **UI Complete - Business Logic TODO***

---

## 🎯 **DEMO READY!**

**Poți testa UI-ul ACUM:**
1. Rulează aplicația (F5)
2. Navighează la `/programari`
3. Click pe "📧 Email Programări Mâine"
4. **Modalul se deschide** cu date MOCK
5. Testează butoanele, animațiile, loading states

**Pentru date reale:** Implementează `LoadPreviewData` conform pașilor de mai sus! 🚀
