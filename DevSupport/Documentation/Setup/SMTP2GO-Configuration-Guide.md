# 📧 **Ghid Configurare SMTP2GO pentru ValyanClinic**

**Data:** {DateTime.Now:yyyy-MM-dd}  
**Status:** ⚙️ **CONFIGURARE NECESARĂ**

---

## ✅ **CE AI IMPLEMENTAT DEJA**

1. **✅ Pachete NuGet instalate:**
   - `MailKit 4.14.1` - Client SMTP modern
   - `MimeKit 4.14.0` - Email message builder

2. **✅ Service creat:**
   - `IEmailService` / `EmailService` - Implementare completă
   - Înregistrat în DI container (`Program.cs`)

3. **✅ Configurare `appsettings.json`:**
   ```json
   "EmailSettings": {
     "SmtpHost": "mail.smtp2go.com",
     "SmtpPort": "2525",
     "SmtpUser": "",        // ⚠️ NECESITĂ COMPLETARE
     "SmtpPassword": "", // ⚠️ NECESITĂ COMPLETARE
     "EnableSsl": "true",
     "FromEmail": "noreply@valyanclinic.ro",
     "FromName": "ValyanClinic"
   }
   ```

4. **✅ User Secrets inițializat:**
- `dotnet user-secrets init` - executat
   - Placeholder-uri setate (trebuie înlocuite cu valorile reale)

---

## 🚀 **PAȘI FINALI PENTRU ACTIVARE**

### **Pasul 1: Creează cont SMTP2GO**

1. **Accesează:** https://www.smtp2go.com/
2. **Click pe "Sign Up Free"**
3. **Completează formularul:**
 - **Email:** aurelian@valyanclinic.ro (sau email-ul tău)
   - **Password:** (alege o parolă sigură)
   - **Company Name:** ValyanClinic
4. **Verifică email-ul** (click pe linkul de confirmare)

**Screenshot recomandat:** Salvează captură ecran cu detaliile contului

---

### **Pasul 2: Creează SMTP User**

După ce te-ai logat în SMTP2GO:

1. **Navighează la:** **Settings** → **Users** → **Add SMTP User**
2. **Username:** Alege un username (ex: `valyanclinic-app`)
3. **Click "Generate"** - SMTP2GO va genera automat:
   - **Username:** (ex: `valyanclinic-app`)
   - **Password:** (ex: `abc123xyz456def789`)

**⚠️ IMPORTANT:** **COPIAZĂ PAROLA ACUM!** Nu o vei mai vedea din nou!

**Notează undeva sigur:**
```
SMTP Host: mail.smtp2go.com
SMTP Port: 2525 (sau 587, 8025)
SMTP Username: valyanclinic-app
SMTP Password: abc123xyz456def789
```

---

### **Pasul 3: Setează credențialele în User Secrets**

Deschide **PowerShell** în Visual Studio (sau Command Prompt) și rulează:

```powershell
# Navighează în folderul proiectului
cd D:\Lucru\CMS\ValyanClinic

# Setează username-ul SMTP2GO
dotnet user-secrets set "EmailSettings:SmtpUser" "valyanclinic-app"

# Setează parola SMTP2GO (înlocuiește cu parola ta reală)
dotnet user-secrets set "EmailSettings:SmtpPassword" "abc123xyz456def789"
```

**Output așteptat:**
```
Successfully saved EmailSettings:SmtpUser to the secret store.
Successfully saved EmailSettings:SmtpPassword to the secret store.
```

---

### **Pasul 4: Verifică configurarea**

Rulează acest script pentru a verifica că totul e configurat corect:

```powershell
# Verifică user secrets
dotnet user-secrets list
```

**Output așteptat:**
```
EmailSettings:SmtpUser = valyanclinic-app
EmailSettings:SmtpPassword = abc123xyz456def789
```

---

### **Pasul 5: Testează trimiterea de email**

Creează un fișier de test (sau folosește un endpoint existent):

```csharp
// Într-o componentă Blazor sau într-un controller:
[Inject] private IEmailService EmailService { get; set; } = default!;

private async Task TestEmail()
{
    var message = new EmailMessageDto
    {
        To = "aurelian@valyanclinic.ro", // Email-ul tău
   Subject = "🎉 Test Email - SMTP2GO",
        Body = @"
   <h2>Felicitări!</h2>
            <p>Email-ul funcționează perfect prin SMTP2GO!</p>
            <p><strong>Data:</strong> " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"</p>
            <p><em>Echipa ValyanClinic</em></p>
        ",
        IsHtml = true
    };

    var success = await EmailService.SendEmailAsync(message);
    
    if (success)
    {
        Logger.LogInformation("✅ Email trimis cu succes!");
    }
    else
    {
        Logger.LogError("❌ Eroare la trimiterea email-ului!");
    }
}
```

---

## 📊 **VERIFICARE PORT-URI SMTP2GO**

SMTP2GO suportă **3 porturi** diferite (în caz că unul e blocat de firewall):

| Port | Encryption | Când să folosești |
|------|------------|-------------------|
| **2525** | TLS/STARTTLS | **Recomandat** - aproape niciodată blocat |
| **587** | TLS/STARTTLS | Standard SMTP submission port |
| **8025** | TLS/STARTTLS | Alternative port (dacă 2525 e blocat) |

**Dacă 2525 nu funcționează**, încearcă să schimbi în `appsettings.json`:
```json
"SmtpPort": "587"  // SAU "8025"
```

---

## 🔒 **SECURITATE - BEST PRACTICES**

### **✅ User Secrets (Development):**
```powershell
# CORECT - Parola în User Secrets (nu în Git)
dotnet user-secrets set "EmailSettings:SmtpPassword" "parola-ta"
```

### **❌ NU pune parola în appsettings.json:**
```json
// ❌ GRESIT - Parola în appsettings.json (ajunge în Git!)
"SmtpPassword": "abc123xyz456"  // NU FACE ASA!
```

### **🔐 Production (Azure Key Vault):**
Când deploy-ezi în production, folosește **Azure Key Vault**:
```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri("https://valyanclinic-keyvault.vault.azure.net/"),
  new DefaultAzureCredential());
```

---

## 📧 **VERIFICĂ DOMENIUL (OPȚIONAL dar recomandat)**

Pentru a **evita spam folder** și a avea **deliverability rate mai bun**:

1. **Mergi în SMTP2GO:** **Sending** → **Sender Domains**
2. **Click "Add Domain"**
3. **Introdu:** `valyanclinic.ro` (dacă ai domeniu propriu)
4. **Urmează instrucțiunile** pentru:
   - **SPF Record** (TXT record în DNS)
   - **DKIM Record** (TXT record în DNS)
   - **Domain Verification** (TXT record în DNS)

**Dacă NU ai domeniu propriu:**
- Sari peste acest pas
- Vei trimite cu email-ul generic SMTP2GO
- Funcționează perfect, dar poate fi marcat ca spam mai ușor

---

## 🧪 **TROUBLESHOOTING**

### **Problem: Build error - "MailKit not found"**
**Soluție:**
```powershell
cd D:\Lucru\CMS\ValyanClinic
dotnet restore
dotnet build
```

### **Problem: "Authentication failed"**
**Cauze posibile:**
1. Username/Password greșit → Verifică `dotnet user-secrets list`
2. Account SMTP2GO neactivat → Check email pentru confirmare
3. Credențialele în `appsettings.json` (gol) override User Secrets → Lasă goale în appsettings!

### **Problem: "Connection timeout"**
**Cauze posibile:**
1. Port blocat de firewall → Încearcă 587 sau 8025
2. Server SMTP2GO down (rar) → Check https://status.smtp2go.com/

### **Problem: Email-urile ajung în spam**
**Soluție:**
1. Verifică domeniul în SMTP2GO (SPF + DKIM)
2. Folosește FromEmail cu domeniu verificat
3. Evită cuvinte spam ("FREE", "WIN", etc.)

---

## ✅ **CHECKLIST FINAL**

- [ ] Cont SMTP2GO creat și verificat
- [ ] SMTP User creat în SMTP2GO dashboard
- [ ] Username și Password notate undeva sigur
- [ ] `dotnet user-secrets set` executat pentru ambele credențiale
- [ ] `dotnet user-secrets list` arată credențialele corect
- [ ] Build successful (`dotnet build`)
- [ ] Test email trimis și primit cu succes
- [ ] (Opțional) Domeniu verificat în SMTP2GO

---

## 📞 **NEXT STEPS**

După ce ai configurat SMTP2GO cu succes:

### **1. Implementare Email Confirmare Programare:**
```csharp
public async Task<bool> SendAppointmentConfirmationAsync(Guid programareId)
{
    // 1. Load programare din DB (MediatR)
    var programare = await Mediator.Send(new GetProgramareByIdQuery(programareId));
    
    // 2. Generează HTML email body cu detalii
    var emailBody = $@"
    <h2>Confirmare Programare - ValyanClinic</h2>
        <p><strong>Data:</strong> {programare.DataProgramare:dd.MM.yyyy}</p>
      <p><strong>Ora:</strong> {programare.OraInceput:hh\:mm}</p>
      <p><strong>Doctor:</strong> Dr. {programare.DoctorNumeComplet}</p>
    ";
    
    // 3. Trimite email
    return await SendEmailAsync(new EmailMessageDto
    {
    To = programare.PacientEmail,
        Subject = "✅ Confirmare Programare - ValyanClinic",
        Body = emailBody,
        IsHtml = true
    });
}
```

### **2. Implementare Reminder Programări (24h înainte):**
- Crează **Background Service** (Hosted Service)
- Verifică programări în următoarele 24h
- Trimite email reminder automat

### **3. Implementare Reset Parolă:**
- Generează token unic (GUID)
- Trimite email cu link reset
- Implementare pagină reset password

---

## 🎉 **STATUS CURENT**

**✅ Implementare completă:**
- EmailService functional
- MailKit/MimeKit instalate
- User Secrets configurate (placeholder-uri)
- Build successful

**⚠️ NECESITĂ ACȚIUNE:**
- **Creare cont SMTP2GO**
- **Obținere credențiale SMTP**
- **Update User Secrets cu credențiale reale**
- **Test trimiter email**

---

**Timp estimat:** **15-20 minute** pentru configurare completă  
**Dificultate:** ⭐⭐☆☆☆ (Ușor - doar urmează pașii)  
**Beneficiu:** 📧 **Email-uri profesionale HIPAA-compliant** pentru clinică!

---

*Document creat: {DateTime.Now:yyyy-MM-dd}*  
*Framework: .NET 9 + Blazor Server*  
*Email Library: MailKit 4.14.1 + MimeKit 4.14.0*  
*Provider: SMTP2GO (Medical/Clinical focused)*  
*Status: ⚙️ **Configurare finală necesară**

🎯 **După configurare:** Aplicația ta va putea trimite email-uri profesionale pentru programări, remindere, și notificări! 📧✨
