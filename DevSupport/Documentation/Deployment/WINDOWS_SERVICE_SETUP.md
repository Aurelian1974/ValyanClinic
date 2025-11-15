# 🏗️ **WINDOWS SERVICE SETUP - ValyanClinic**

**Framework:** .NET 9 Blazor Server  
**Purpose:** Rulare permanentă în background (no manual start/stop)  
**Status:** ✅ **READY TO DEPLOY**

---

## 🎯 **CE FACE?**

Windows Service transformă aplicația **ValyanClinic** într-un **serviciu Windows** care:
- ✅ **Pornește automat** cu Windows-ul
- ✅ **Rulează în background** (fără UI vizibil)
- ✅ **Restart automat** dacă aplicația crapă
- ✅ **Management centralizat** prin Services Manager
- ✅ **Production-ready** - exact cum rulează pe servere

---

## 📦 **PREREQUISITES**

| Requirement | Status | Notes |
|-------------|--------|-------|
| **.NET 9 SDK** | ✅ Instalat | Verificat |
| **Windows 10/11 Pro** | ✅ | Or Windows Server |
| **Admin Rights** | ⚠️ Necesar | Pentru instalare service |
| **Package Installed** | ✅ | `Microsoft.Extensions.Hosting.WindowsServices` |
| **Code Updated** | ✅ | `Program.cs` cu `.UseWindowsService()` |

---

## 🚀 **INSTALARE WINDOWS SERVICE - PAS CU PAS**

### **Pas 1: Build Release Version**

Deschide **Command Prompt** (cmd) **ca Administrator**:

```cmd
REM Navighează la folder-ul proiectului
cd D:\Lucru\CMS\ValyanClinic

REM Publish Release (Self-Contained)
dotnet publish -c Release -r win-x64 --self-contained true -o D:\Services\ValyanClinic

REM SAU Framework-Dependent (mai mic size)
dotnet publish -c Release -r win-x64 --self-contained false -o D:\Services\ValyanClinic
```

**Output:** Toate fișierele aplicației în `D:\Services\ValyanClinic\`

---

### **Pas 2: Verifică Binarul**

```cmd
dir D:\Services\ValyanClinic\ValyanClinic.exe
```

✅ Ar trebui să vezi `ValyanClinic.exe` (executabil principal)

---

### **Pas 3: Instalare Windows Service**

**Folosind SC (Windows Service Controller):**

```cmd
REM Command Prompt ca Administrator

REM Creează service
sc create ValyanClinicService ^
  binPath="D:\Services\ValyanClinic\ValyanClinic.exe" ^
    start=auto ^
  DisplayName="ValyanClinic Service" ^
    obj="LocalSystem"

REM Descriere service
sc description ValyanClinicService "ValyanClinic - Sistem Management Clinica Medicala (.NET 9 Blazor Server)"
```

**Parametri:**
- `binPath`: Calea către executabilul aplicației
- `start=auto`: Pornire automată la boot Windows
- `obj=LocalSystem`: Rulează sub contul Local System (admin)

---

### **Pas 4: Pornește Service-ul**

```cmd
REM Start service
sc start ValyanClinicService

REM Check status
sc query ValyanClinicService
```

**Output așteptat:**
```
SERVICE_NAME: ValyanClinicService
    TYPE      : 10  WIN32_OWN_PROCESS
    STATE   : 4  RUNNING
    WIN32_EXIT_CODE    : 0  (0x0)
```

---

### **Pas 5: Verifică că Aplicația Rulează**

Deschide browser și navighează la:

```
http://localhost:5007
SAU
http://192.168.1.xxx:5007 (IP-ul PC-ului)
```

✅ Ar trebui să vezi aplicația ValyanClinic funcțională!

---

## 🛠️ **MANAGEMENT SERVICE**

### **Oprire Service:**
```cmd
sc stop ValyanClinicService
```

### **Pornire Service:**
```cmd
sc start ValyanClinicService
```

### **Restart Service:**
```cmd
sc stop ValyanClinicService
timeout /t 5
sc start ValyanClinicService
```

### **Check Status:**
```cmd
sc query ValyanClinicService
```

### **Delete Service** (dezinstalare):
```cmd
REM Stop mai întâi
sc stop ValyanClinicService

REM Apoi delete
sc delete ValyanClinicService
```

---

## 🖥️ **MANAGEMENT PRIN SERVICES.MSC (GUI)**

### **Acces Services Manager:**

1. Apasă `Win + R`
2. Scrie: `services.msc`
3. Apasă `Enter`

### **În lista de servicii:**

1. **Găsește:** "ValyanClinic Service"
2. **Right-click** → Properties
3. **General Tab:**
   - Startup type: **Automatic**
   - Service status: **Running**
4. **Recovery Tab (Restart automat):**
   - First failure: **Restart the Service**
   - Second failure: **Restart the Service**
   - Subsequent failures: **Restart the Service**
   - Restart service after: **1 minute**

**✅ SAVE**

---

## 📊 **VERIFICARE POST-INSTALARE**

### **Checklist:**

- [ ] ✅ Service apare în `services.msc`
- [ ] ✅ Status: **Running**
- [ ] ✅ Startup type: **Automatic**
- [ ] ✅ Recovery action: **Restart the Service**
- [ ] ✅ Browser acces: http://localhost:5007 → **funcționează**
- [ ] ✅ LAN access: http://192.168.1.xxx:5007 → **funcționează** (de pe tabletă)

### **Test Restart Automat:**

```cmd
REM Kill process manual (simulate crash)
taskkill /F /IM ValyanClinic.exe

REM Așteaptă 1 minut
timeout /t 60

REM Check dacă s-a repornit automat
sc query ValyanClinicService
```

✅ Ar trebui să vezi STATUS: **RUNNING** (auto-restart după 1 min)

---

## 🔧 **TROUBLESHOOTING**

### **Problem 1: Service nu pornește**

**Cauze:**
- Port 5007 deja ocupat
- Conexiune DB eșuează
- Missing dependencies

**Soluție:**

```cmd
REM Check Event Viewer pentru detalii eroare
eventvwr.msc
   → Windows Logs
    → Application
 → Caută "ValyanClinic" sau "Error"

REM Check firewall
netsh advfirewall firewall show rule name="ValyanClinic HTTP"
```

**Fix:** Update `appsettings.json` cu setări corecte DB, Port, etc.

---

### **Problem 2: Service pornește dar nu pot accesa aplicația**

**Cauze:**
- Firewall block
- Aplicația ascultă doar pe localhost (nu 0.0.0.0)

**Soluție:**

```cmd
REM Verifică că aplicația ascultă pe toate interfețele
netstat -an | findstr 5007

REM Ar trebui să vezi:
REM TCP    0.0.0.0:5007           0.0.0.0:0     LISTENING

REM Dacă vezi doar:
REM TCP    127.0.0.1:5007         0.0.0.0:0    LISTENING
REM Atunci trebuie să modifici launchSettings.json să asculte pe 0.0.0.0
```

**Fix:** Update `appsettings.Production.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5007"
      }
    }
  }
}
```

Apoi **republish** și **restart service**:

```cmd
dotnet publish -c Release -r win-x64 -o D:\Services\ValyanClinic
sc stop ValyanClinicService
sc start ValyanClinicService
```

---

### **Problem 3: Service se oprește după câteva minute**

**Cauză:** IIS Application Pool timeout sau Windows Service timeout

**Soluție:**

```cmd
REM Increase service timeout (Registry edit - ADVANCED)
reg add HKLM\SYSTEM\CurrentControlSet\Control /v ServicesPipeTimeout /t REG_DWORD /d 180000 /f

REM Apoi restart service
sc stop ValyanClinicService
sc start ValyanClinicService
```

---

## 🔒 **SECURITY CONSIDERATIONS**

### **User Account:**

**Current:** `LocalSystem` (full admin rights - OK pentru development)

**Production Recommendation:**

```cmd
REM Create dedicated user
net user ValyanClinicService ComplexPassword123! /add

REM Grant user rights to folder
icacls D:\Services\ValyanClinic /grant ValyanClinicService:(OI)(CI)F

REM Update service to run as this user
sc config ValyanClinicService obj=".\ValyanClinicService" password="ComplexPassword123!"
```

### **Firewall:**

```cmd
REM Windows Defender Firewall exception deja adăugată
netsh advfirewall firewall add rule ^
    name="ValyanClinic HTTP" ^
    dir=in ^
    action=allow ^
    protocol=TCP ^
    localport=5007
```

---

## 📝 **LOGS & MONITORING**

### **Application Logs:**

Logs sunt scrise de Serilog în:

```
D:\Services\ValyanClinic\Logs\
├── valyan-clinic-20250120.log
├── valyan-clinic-20250121.log
└── errors-20250120.log
```

**View logs:**

```cmd
REM Tail latest log
powershell Get-Content D:\Services\ValyanClinic\Logs\valyan-clinic-*.log -Tail 50 -Wait

REM Caută errors
findstr /C:"ERROR" D:\Services\ValyanClinic\Logs\valyan-clinic-*.log
```

### **Windows Event Viewer:**

```
eventvwr.msc → Application Logs
  Filtrare:
    Source: ValyanClinicService
 Event Level: Error, Warning
```

---

## 🚀 **DEPLOYMENT FLOW**

### **Development → Production:**

#### **1. Local Testing:**
```cmd
cd D:\Lucru\CMS\ValyanClinic
dotnet run --environment Development
```

#### **2. Build Release:**
```cmd
dotnet publish -c Release -r win-x64 -o D:\Services\ValyanClinic
```

#### **3. Stop Service (dacă rulează):**
```cmd
sc stop ValyanClinicService
```

#### **4. Update Files:**
```cmd
REM Backup old version
xcopy D:\Services\ValyanClinic D:\Services\ValyanClinic_backup_%date:~-4,4%%date:~-10,2%%date:~-7,2% /E /I

REM Copy new files
xcopy D:\Lucru\CMS\ValyanClinic\bin\Release\net9.0\win-x64\publish\* D:\Services\ValyanClinic /E /Y
```

#### **5. Start Service:**
```cmd
sc start ValyanClinicService
```

#### **6. Verify:**
```
http://localhost:5007
```

---

## 📊 **PERFORMANCE MONITORING**

### **CPU & Memory:**

```powershell
# PowerShell script pentru monitoring
while ($true) {
    $proc = Get-Process ValyanClinic -ErrorAction SilentlyContinue
    if ($proc) {
        Write-Host "CPU: $($proc.CPU)s | Memory: $([math]::Round($proc.WorkingSet64/1MB, 2)) MB"
    }
    Start-Sleep -Seconds 5
}
```

### **Connection Count:**

```cmd
REM Active connections pe portul 5007
netstat -an | findstr 5007 | findstr ESTABLISHED | find /c /v ""
```

---

## ✅ **PRODUCTION CHECKLIST**

### **Pre-Deployment:**
- [x] Package `Microsoft.Extensions.Hosting.WindowsServices` instalat
- [x] `Program.cs` updated cu `.UseWindowsService()`
- [x] `appsettings.Production.json` configurat corect
- [x] Connection string DB production
- [x] Firewall rules adăugate
- [ ] **TODO:** Test pe mediu staging
- [ ] **TODO:** Backup plan (rollback procedure)

### **Post-Deployment:**
- [ ] Service installed și **Running**
- [ ] Startup type: **Automatic**
- [ ] Recovery action: **Restart the Service**
- [ ] Browser access: **funcționează**
- [ ] LAN access: **funcționează**
- [ ] Logs: **se scriu corect**
- [ ] Performance: **<1s response time**
- [ ] Memory: **<500MB usage**

---

## 🔄 **UPDATE PROCEDURE**

### **Cum să Actualizezi Service-ul cu Versiune Nouă:**

```cmd
REM 1. Stop service
sc stop ValyanClinicService

REM 2. Backup current version
xcopy D:\Services\ValyanClinic D:\Services\ValyanClinic_backup_%date:~-4,4%%date:~-10,2%%date:~-7,2% /E /I

REM 3. Build new version
cd D:\Lucru\CMS\ValyanClinic
dotnet publish -c Release -r win-x64 -o D:\Services\ValyanClinic_new

REM 4. Replace files
robocopy D:\Services\ValyanClinic_new D:\Services\ValyanClinic /MIR

REM 5. Start service
sc start ValyanClinicService

REM 6. Verify
curl http://localhost:5007/health
```

---

## 🆚 **ALTERNATIVE: IIS Express (Not Recommended for Production)**

**Dacă preferi IIS Express în loc de Windows Service:**

```cmd
REM Install IIS
dism /online /enable-feature /featurename:IIS-WebServerRole

REM Install ASP.NET Core Hosting Bundle
REM Download: https://dotnet.microsoft.com/download/dotnet/9.0

REM Create IIS site
iisreset
```

**❌ Dezavantaj:** Mai complicat, necesită mai multe configurări, mai greu de manage.

---

## 📚 **RESOURCES**

### **Official Docs:**
- **Windows Services:** https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/windows-service
- **Kestrel Configuration:** https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel

### **Troubleshooting:**
- **Event Viewer:** `eventvwr.msc`
- **Services Manager:** `services.msc`
- **Task Manager:** `Ctrl+Shift+Esc` → Services tab

---

## 🎯 **QUICK START (TL;DR)**

```cmd
REM 1. Publish
cd D:\Lucru\CMS\ValyanClinic
dotnet publish -c Release -r win-x64 -o D:\Services\ValyanClinic

REM 2. Install Service (CMD ca Administrator)
sc create ValyanClinicService binPath="D:\Services\ValyanClinic\ValyanClinic.exe" start=auto DisplayName="ValyanClinic Service"
sc description ValyanClinicService "ValyanClinic - Sistem Management Clinica (.NET 9)"

REM 3. Start
sc start ValyanClinicService

REM 4. Verifică
curl http://localhost:5007
```

✅ **DONE! Service rulează permanent în background!**

---

**Document Status:** ✅ **COMPLETE & PRODUCTION READY**  
**Last Updated:** 2025-01-20  
**Version:** 1.0

---

*Pentru suport, check logs în `D:\Services\ValyanClinic\Logs\` sau Event Viewer (`eventvwr.msc`)*
