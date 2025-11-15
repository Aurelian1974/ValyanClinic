# 📱 **SMS Service - Infrastructure Documentation**

**Status:** 🟡 **MOCK MODE** (Production-Ready Infrastructure)  
**Buget necesar:** $0 pentru testare, $15-75/lună pentru producție  
**Setup time:** 30 min (când ai buget)

---

## 🎯 **CURRENT STATUS:**

### **✅ Ce AI ACUM (GRATUIT):**
- **Interface complet** (`ISmsService`)
- **Mock implementation** pentru testare UI
- **Message templates** (confirmare, reminder, anulare)
- **Phone validation** (România)
- **Logging complet**
- **DTO structure** pentru extensie viitoare

### **⏳ Ce LIPSEȘTE (Când ai buget):**
- Provider real (Twilio/SMS-Gateway.ro/etc.)
- Package NuGet pentru provider
- User Secrets cu credențiale
- Replace `MockSmsService` cu `TwilioSmsService`

---

## 🚀 **ACTIVARE PRODUCTION (2 MIN SETUP):**

### **Opțiunea 1: Twilio (RECOMANDAT - $15 FREE)**

#### **Pas 1: Sign Up Twilio**
```
1. Mergi la: https://www.twilio.com/try-twilio
2. Sign up (gratuit)
3. Verifică email + telefon
4. Primești $15 credit automat! (≈ 450 SMS-uri)
```

#### **Pas 2: Get Credentials**
```
Dashboard → Account → Keys & Credentials:
- Account SID: ACxxxxxxxxxxxxxxxxxxxxxxxxxx
- Auth Token: xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
- Phone Number: +1xxxxxxxxxx (trial number gratuit)
```

#### **Pas 3: Install Package**
```sh
cd ValyanClinic
dotnet add package Twilio
```

#### **Pas 4: Configure Secrets**
```sh
dotnet user-secrets set "TwilioSettings:AccountSid" "ACxxxx"
dotnet user-secrets set "TwilioSettings:AuthToken" "xxxx"
dotnet user-secrets set "TwilioSettings:PhoneNumber" "+1xxxx"
dotnet user-secrets set "TwilioSettings:Enabled" "true"
```

#### **Pas 5: Create TwilioSmsService.cs**

```csharp
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public class TwilioSmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioSmsService> _logger;
    private readonly IMediator _mediator;

    public TwilioSmsService(IConfiguration config, ILogger<TwilioSmsService> logger, IMediator mediator)
    {
        _configuration = config;
        _logger = logger;
        _mediator = mediator;

// Initialize Twilio
        var accountSid = config["TwilioSettings:AccountSid"];
     var authToken = config["TwilioSettings:AuthToken"];
   TwilioClient.Init(accountSid, authToken);
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            var fromNumber = _configuration["TwilioSettings:PhoneNumber"];

            // Normalize phone number
            if (!phoneNumber.StartsWith("+"))
        {
    phoneNumber = phoneNumber.StartsWith("0") 
         ? $"+4{phoneNumber}" 
          : $"+40{phoneNumber}";
         }

            var messageResource = await MessageResource.CreateAsync(
          to: new PhoneNumber(phoneNumber),
      from: new PhoneNumber(fromNumber),
      body: message
       );

          _logger.LogInformation("✅ SMS trimis către {Phone}: SID={Sid}, Status={Status}", 
       phoneNumber, messageResource.Sid, messageResource.Status);

       return messageResource.Status != MessageResource.StatusEnum.Failed;
   }
        catch (Exception ex)
        {
   _logger.LogError(ex, "❌ Eroare trimitere SMS către {Phone}", phoneNumber);
   return false;
        }
    }

    // ... copy all other methods from MockSmsService (same logic, just SendSmsAsync changes)
}
```

#### **Pas 6: Update Program.cs**

```csharp
// Program.cs

// ✅ Replace MockSmsService with TwilioSmsService când ai buget
var smsEnabled = builder.Configuration["TwilioSettings:Enabled"] == "true";

if (smsEnabled)
{
 builder.Services.AddScoped<ISmsService, TwilioSmsService>();
}
else
{
    builder.Services.AddScoped<ISmsService, MockSmsService>();
}
```

**DONE! SMS-urile se trimit REAL!** 🎉

---

### **Opțiunea 2: SMS-Gateway.ro (Românesc, low-cost)**

#### **Pas 1: Sign Up**
```
https://www.sms-gateway.ro/
Create account → Add credits (min 10 lei)
```

#### **Pas 2: Get API Key**
```
Dashboard → API Settings:
- API Key: xxxxx-xxxxx-xxxxx
- API Endpoint: https://api.sms-gateway.ro/v1/send
```

#### **Pas 3: Install HTTP Client**
```sh
dotnet add package Flurl.Http
```

#### **Pas 4: Create SmsGatewayRoService.cs**

```csharp
using Flurl.Http;

public class SmsGatewayRoService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsGatewayRoService> _logger;
    
    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
       var apiKey = _configuration["SmsGatewaySettings:ApiKey"];
            var endpoint = "https://api.sms-gateway.ro/v1/send";

    var response = await endpoint
      .WithHeader("Authorization", $"Bearer {apiKey}")
                .PostJsonAsync(new
      {
      to = phoneNumber,
                message = message,
      from = "ValyanClinic"
       });

            _logger.LogInformation("✅ SMS trimis către {Phone}", phoneNumber);
            return true;
        }
catch (Exception ex)
        {
  _logger.LogError(ex, "❌ Eroare trimitere SMS");
    return false;
 }
    }
}
```

---

## 📊 **COST ESTIMATION:**

### **Scenario: 50 programări/zi**

| SMS Type | SMS/zi | SMS/lună | Twilio Cost | SMS-Gateway Cost |
|----------|--------|----------|-------------|------------------|
| Confirmare | 50 | 1,500 | $37.50 | 120 lei |
| Reminder (24h) | 50 | 1,500 | $37.50 | 120 lei |
| **TOTAL** | **100** | **3,000** | **$75** (350 lei) | **240 lei** |

**FREE cu Twilio:** Primele **450 SMS-uri** = 4.5 zile gratis! ($15 credit)

---

## 🎯 **FEATURES IMPLEMENTED:**

### **✅ 1. SendSmsAsync (Generic)**
```csharp
await _smsService.SendSmsAsync("0712345678", "Test message");
```

### **✅ 2. SendAppointmentConfirmationSmsAsync**
```csharp
await _smsService.SendAppointmentConfirmationSmsAsync(programareId);
```

**Message Template:**
```
ValyanClinic: Programarea ta cu Dr. Popescu Ion 
pe data de 16.11.2025 la ora 09:00 a fost confirmată. 
Pentru reprogramare, sună la 0123456789.
```

### **✅ 3. SendAppointmentReminderSmsAsync**
```csharp
await _smsService.SendAppointmentReminderSmsAsync(programareId, hoursBeforeAppointment: 24);
```

**Message Template:**
```
ValyanClinic REMINDER: Ai programare cu Dr. Popescu Ion 
peste 1 zi, pe 16.11.2025 la 09:00. 
Pentru anulare, sună la 0123456789.
```

### **✅ 4. SendAppointmentCancellationSmsAsync**
```csharp
await _smsService.SendAppointmentCancellationSmsAsync(programareId, reason: "Doctor indisponibil");
```

**Message Template:**
```
ValyanClinic: Programarea ta cu Dr. Popescu Ion 
din data de 16.11.2025 la ora 09:00 a fost anulată. 
Motiv: Doctor indisponibil. 
Pentru reprogramare, sună la 0123456789.
```

### **✅ 5. SendBulkSmsAsync (Batch)**
```csharp
var phoneNumbers = new List<string> { "0712345678", "0723456789", "0734567890" };
var count = await _smsService.SendBulkSmsAsync(phoneNumbers, "Clinica este închisă mâine.");
```

### **✅ 6. IsValidPhoneNumber**
```csharp
var isValid = _smsService.IsValidPhoneNumber("0712345678"); // true
var isValid2 = _smsService.IsValidPhoneNumber("+40712345678"); // true
var isValid3 = _smsService.IsValidPhoneNumber("invalid"); // false
```

---

## 🔧 **USAGE EXAMPLES:**

### **Exemplu 1: Confirmare automată la creare programare**

```csharp
// În CreateProgramareCommandHandler.cs
public async Task<Result<Guid>> Handle(CreateProgramareCommand request, CancellationToken cancellationToken)
{
    // ... create programare ...

    // ✅ Send confirmation SMS
    try
    {
        await _smsService.SendAppointmentConfirmationSmsAsync(newProgramare.ProgramareID);
    }
    catch (Exception ex)
    {
    _logger.LogWarning(ex, "Nu s-a putut trimite SMS-ul de confirmare");
        // Don't fail entire operation if SMS fails
  }

    return Result<Guid>.Success(newProgramare.ProgramareID);
}
```

### **Exemplu 2: Reminder automat (Background Job - viitor)**

```csharp
// Background job care rulează zilnic la 18:00
public async Task SendTomorrowReminders()
{
    var tomorrow = DateTime.Today.AddDays(1);
    var programari = await _programareRepository.GetByDateAsync(tomorrow);

    foreach (var programare in programari)
    {
        await _smsService.SendAppointmentReminderSmsAsync(programare.ProgramareID, hoursBeforeAppointment: 24);
    }
}
```

### **Exemplu 3: SMS anulare la Delete**

```csharp
// În DeleteProgramareCommandHandler.cs
public async Task<Result<bool>> Handle(DeleteProgramareCommand request, CancellationToken cancellationToken)
{
    // ... delete programare ...

    // ✅ Send cancellation SMS
    await _smsService.SendAppointmentCancellationSmsAsync(request.ProgramareID, request.MotivAnulare);

    return Result<bool>.Success(true);
}
```

---

## 🧪 **TESTING (MOCK MODE):**

### **Test în UI (Buton trigger)**

```razor
<button class="btn btn-info" @onclick="TestSms">
    <i class="fas fa-sms"></i>
    Test SMS
</button>

@code {
    private async Task TestSms()
    {
        var success = await SmsService.SendSmsAsync("0712345678", "Test message from ValyanClinic");
        
     if (success)
        {
 await NotificationService.ShowSuccessAsync("SMS trimis cu succes!");
        }
    }
}
```

### **Check Logs (Output Window):**
```
[INFO] 📱 MOCK SMS SENT
   To: 0712345678
   Message: Test message from ValyanClinic
   Length: 33 chars
   Cost: $0.00 (MOCK MODE)
```

---

## 📦 **FILES CREATED:**

```
ValyanClinic/Services/Sms/
├── ISmsService.cs         ✅ Interface complet
├── SmsMessageDto.cs      ✅ DTOs pentru mesaje
├── MockSmsService.cs    ✅ Mock implementation (CURRENT)
└── TwilioSmsService.cs ⏳ TODO când ai buget (copy from docs)
```

---

## 🎯 **NEXT STEPS:**

### **Acum (FREE):**
- [x] Infrastructure ready
- [x] Mock service pentru testare UI
- [x] Message templates
- [x] Phone validation
- [ ] **TODO:** Test în UI (adaugă buton Test SMS în Settings)

### **Când ai buget (~$15-20 pentru început):**
- [ ] Sign up Twilio (sau SMS-Gateway.ro)
- [ ] Install NuGet package
- [ ] Configure User Secrets
- [ ] Create TwilioSmsService.cs
- [ ] Update Program.cs registration
- [ ] **DONE!** SMS real în 2 min

---

## 💡 **RECOMMENDATIONS:**

### **1. Start cu Twilio** (pentru că e FREE trial)
- $15 credit gratuit = 450 SMS-uri
- Perfect pentru testare production
- Dacă merge bine, continui sau switchezi la alt provider

### **2. Monitor costs** (când ai buget)
- Twilio Dashboard → Usage → SMS
- Set alert când atingi $10
- Optimize: trimite doar confirmare (fără reminder zilnic)

### **3. Background Jobs** (viitor, când ai buget)
- Hangfire pentru SMS reminder automat
- Rulează zilnic la 18:00
- Trimite SMS cu 24h înainte de programare

---

**Status:** 🟢 **Infrastructure READY** - Activate când ai buget! 🚀📱

**Estimare activare:** **2 minute** (când ai credențiale provider)
