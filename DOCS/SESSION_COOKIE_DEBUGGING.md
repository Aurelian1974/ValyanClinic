# 🔍 Session Cookie Debugging Guide

## Problema

Cookie-ul de autentificare persistă după închiderea browser-ului, chiar dacă am setat:
- `IsPersistent = false`
- `ExpiresUtc = null`
- `Cookie.MaxAge = null`

---

## 🧪 Pași de Debugging

### 1. Verifică Cookie-ul în Browser (F12)

#### Chrome/Edge DevTools
1. Deschide `https://localhost:5001/`
2. Press `F12` → Application Tab
3. Expand "Cookies" → Click `https://localhost:5001`
4. Găsește cookie-ul `ValyanClinic.Auth`

#### Verifică următoarele proprietăți:

| Proprietate | Valoare Așteptată | Valoare Actuală | Status |
|-------------|-------------------|-----------------|--------|
| **Name** | `ValyanClinic.Auth` | ? | |
| **Value** | `<encrypted-string>` | ? | |
| **Domain** | `localhost` | ? | |
| **Path** | `/` | ? | |
| **Expires / Max-Age** | **Session** | ? | ⚠️ CRITICAL |
| **Size** | ~500-1000 bytes | ? | |
| **HttpOnly** | ✓ | ? | |
| **Secure** | (depends on HTTPS) | ? | |
| **SameSite** | `Lax` | ? | |

**🚨 Cel mai important:** `Expires / Max-Age` trebuie să fie **"Session"** sau să lipsească complet!

---

### 2. Verifică Response Headers la Login

#### Folosind Browser DevTools
1. Deschide `F12` → Network Tab
2. Fă login
3. Găsește request-ul `POST /api/authentication/login`
4. Click pe request → Headers Tab → Response Headers

#### Caută header-ul `Set-Cookie`:

```http
Set-Cookie: ValyanClinic.Auth=<value>; path=/; httponly; samesite=lax
```

**⚠️ NU ar trebui să conțină:**
- `expires=<date>`
- `max-age=<seconds>`

**Dacă vezi `expires` sau `max-age` → PROBLEMA ESTE AICI!**

---

### 3. Verifică Browser-ul

#### Chrome/Edge - "Continue where you left off"

**Locație:** `chrome://settings/onStartup`

Verifică dacă este activat:
- ✅ "Continue where you left off"
- ❌ "Open the New Tab page" (RECOMANDAT pentru testare)

#### Firefox - "Restore previous session"

**Locație:** `about:preferences#general`

Verifică:
- ✅ "Restore previous session"
- ❌ "Show your windows and tabs from last time"

---

### 4. Test Complet - Închidere Browser

#### Windows - Chrome/Edge

**Pas 1:** Deschide Task Manager (`Ctrl+Shift+Esc`)

**Pas 2:** Înainte de închidere:
```
Processes → Google Chrome / Microsoft Edge
Detalii → Vezi număr de procese (ex: 15 procese)
```

**Pas 3:** Închide browser-ul (`Alt+F4` sau click X)

**Pas 4:** Verifică Task Manager imediat:
```
Google Chrome / Microsoft Edge - toate procesele TREBUIE să dispară!
```

**Pas 5:** Dacă procesele rămân → Click dreapta → End Task pe fiecare

**Pas 6:** Aștept 10 secunde

**Pas 7:** Deschide din nou browser-ul

**Pas 8:** Navighează manual la `https://localhost:5001/`

**Pas 9:** Verifică dacă ești încă autentificat (ar trebui redirect la `/login`)

---

### 5. Test cu Incognito/Private Mode

#### Chrome Incognito
1. `Ctrl+Shift+N` → Incognito window
2. Navighează la `https://localhost:5001/login`
3. Login cu `admin` / `admin123`
4. Verifică cookie în DevTools (ar trebui Session)
5. **Închide COMPLET browser-ul** (inclusiv fereastra normală)
6. Deschide Incognito din nou (`Ctrl+Shift+N`)
7. Navighează la `https://localhost:5001/`
8. **Expected:** Redirect la `/login` (sesiunea dispărută)

---

### 6. Debugging Logs Server-Side

#### Verifică în logs ce proprietăți se setează

**Locație logs:** Console unde rulează `dotnet run`

**Caută după login:**
```
✅ SESSION COOKIE CREATED:
  - IsPersistent: FALSE (session-only)
  - ExpiresUtc: NULL (no expiration - will expire when browser closes)
  - IssuedUtc: <timestamp>
  - Cookie will be deleted when ALL browser windows are closed
```

**Dacă vezi `ExpiresUtc: <date>` → PROBLEMA!**

---

### 7. Test Manual Cookie Properties

#### Adaugă logging în `AuthenticationController.cs`

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
// ... existing code ...

    var authProperties = new AuthenticationProperties
    {
        IsPersistent = false,
     ExpiresUtc = null,
        AllowRefresh = false,
        IssuedUtc = DateTimeOffset.UtcNow
    };

    // ✅ LOG EXACT PROPERTIES
    _logger.LogInformation("========== COOKIE PROPERTIES ==========");
    _logger.LogInformation("IsPersistent: {IsPersistent}", authProperties.IsPersistent);
    _logger.LogInformation("ExpiresUtc: {ExpiresUtc}", authProperties.ExpiresUtc?.ToString() ?? "NULL");
    _logger.LogInformation("AllowRefresh: {AllowRefresh}", authProperties.AllowRefresh);
    _logger.LogInformation("IssuedUtc: {IssuedUtc}", authProperties.IssuedUtc);
    _logger.LogInformation("=======================================");

    await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal,
  authProperties);

    // ... rest of code ...
}
```

**Verifică logs-urile exact:**
- ExpiresUtc TREBUIE să fie `NULL`
- IsPersistent TREBUIE să fie `False`

---

### 8. Verifică Evenimentul OnSigningIn din Program.cs

#### Asigură-te că nu modifică proprietățile

```csharp
OnSigningIn = context =>
{
    _logger.LogInformation("OnSigningIn TRIGGERED");
    
    if (context.Properties != null)
    {
        _logger.LogInformation("BEFORE modification:");
        _logger.LogInformation("  IsPersistent: {IP}", context.Properties.IsPersistent);
        _logger.LogInformation("  ExpiresUtc: {Exp}", context.Properties.ExpiresUtc?.ToString() ?? "NULL");
     
        // Force session-only
   context.Properties.IsPersistent = false;
  context.Properties.ExpiresUtc = null;
  context.Properties.AllowRefresh = false;
        
 _logger.LogInformation("AFTER modification:");
        _logger.LogInformation("  IsPersistent: {IP}", context.Properties.IsPersistent);
     _logger.LogInformation("  ExpiresUtc: {Exp}", context.Properties.ExpiresUtc?.ToString() ?? "NULL");
    }
    
    return Task.CompletedTask;
}
```

---

## 🔧 Posibile Soluții

### Soluția 1: Force Clear All Cookies la Logout

```javascript
// wwwroot/js/auth-api.js
logout: async function () {
    try {
        // Server-side logout
        await fetch('/api/authentication/logout', {
            method: 'POST',
            credentials: 'include'
      });
  
        // Client-side: Clear ALL cookies
        document.cookie.split(";").forEach(function(c) {
        document.cookie = c.replace(/^ +/, "").replace(/=.*/, "=;expires=" + new Date().toUTCString() + ";path=/");
        });
        
        // Clear storage
  localStorage.clear();
 sessionStorage.clear();
      
        return { success: true };
    } catch (error) {
 return { success: false, message: error.message };
    }
}
```

### Soluția 2: Set Expiration Foarte Scurt (1 minut)

```csharp
// Program.cs - În loc de ExpiresUtc = null
options.Events = new CookieAuthenticationEvents
{
    OnSigningIn = context =>
  {
        if (context.Properties != null)
        {
 context.Properties.IsPersistent = false;
        
            // Set expiration la 1 minut (pentru testare)
     context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(1);
            
            context.Properties.AllowRefresh = false;
        }
    return Task.CompletedTask;
    }
};
```

### Soluția 3: Disable Cookie Persistence Complet

```csharp
// Program.cs
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
    options.Cookie.Name = "ValyanClinic.Auth";
        
        // Force session-only
        options.Cookie.MaxAge = null;
 options.Cookie.Expiration = null;
    options.SlidingExpiration = false;
        
        // Timeout FOARTE scurt pentru testare
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        
        options.Events = new CookieAuthenticationEvents
    {
            OnSigningIn = context =>
            {
      // CRITICAL: Force toate proprietățile
        context.Properties.IsPersistent = false;
      context.Properties.ExpiresUtc = null;
         context.Properties.AllowRefresh = false;
       
         // Log pentru debugging
   var logger = context.HttpContext.RequestServices
      .GetRequiredService<ILogger<Program>>();
                
    logger.LogWarning("=== COOKIE SETARE ===");
   logger.LogWarning("IsPersistent: {IP}", context.Properties.IsPersistent);
           logger.LogWarning("ExpiresUtc: {Exp}", context.Properties.ExpiresUtc?.ToString() ?? "NULL");
       logger.LogWarning("=====================");
        
       return Task.CompletedTask;
         }
   };
    });
```

---

## 📊 Rezultate Așteptate

### Test 1: Login → Close Browser → Reopen
```
1. Login success
2. Cookie created (Session)
3. Close ALL browser windows
4. Wait 10 seconds
5. Open browser
6. Navigate to https://localhost:5001/
7. EXPECTED: Redirect to /login
```

### Test 2: Cookie Inspection
```
F12 → Application → Cookies → ValyanClinic.Auth
Expires/Max-Age: "Session" (NOT a date!)
```

### Test 3: Response Headers
```
POST /api/authentication/login
Response Headers:
Set-Cookie: ValyanClinic.Auth=xxx; path=/; httponly; samesite=lax
(NO expires or max-age!)
```

---

## 🚨 Probleme Comune

### Problema 1: Browser nu se închide complet
**Simptom:** Procese rămân în Task Manager  
**Soluție:** Force close sau disable "Continue where you left off"

### Problema 2: Cookie are Expires setat
**Simptom:** `Expires: Thu, 24 Jan 2025 10:00:00 GMT`  
**Soluție:** Verifică `OnSigningIn` event - nu seta `ExpiresUtc`

### Problema 3: Sliding Expiration activ
**Simptom:** Cookie se reînnoiește automat  
**Soluție:** `options.SlidingExpiration = false`

### Problema 4: RememberMe afectează cookie-ul
**Simptom:** Cookie devine persistent când RememberMe = true  
**Soluție:** Ignoră `RememberMe` în `AuthenticationController` - folosește-l doar pentru localStorage

---

## ✅ Checklist Final

- [ ] Cookie `ValyanClinic.Auth` are `Expires/Max-Age: Session` în DevTools
- [ ] Response header `Set-Cookie` NU conține `expires` sau `max-age`
- [ ] Logs arată `ExpiresUtc: NULL`
- [ ] Logs arată `IsPersistent: False`
- [ ] Task Manager arată că browser-ul se închide complet
- [ ] După reîncărcare → Redirect la `/login`
- [ ] Test în Incognito mode funcționează
- [ ] Logout șterge cookie-ul imediat

---

## 📞 Next Steps

Dacă problemele persistă:
1. Trimite screenshot-uri din DevTools (Cookie properties)
2. Trimite logs de la login (toate lini ile cu "COOKIE")
3. Verifică versiunea browser-ului
4. Test în alt browser (Firefox vs Chrome)

---

**Status:** 🔍 **DEBUGGING IN PROGRESS**  
**Expected Resolution:** Session cookies should work correctly after verification  
**Alternative:** Force expire after 1 minute for testing

