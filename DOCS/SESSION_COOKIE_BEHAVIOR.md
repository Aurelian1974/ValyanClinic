# 🍪 Session Cookie Behavior - ValyanClinic

## 📋 Overview

Aplicația **ValyanClinic** folosește **TRUE session cookies** pentru autentificare, care se șterg automat când browser-ul se închide complet.

---

## ✅ Implementare Session Cookies

### 1. **Cookie Configuration (Program.cs)**

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
     options.Cookie.Name = "ValyanClinic.Auth";
        
        // ✅ Session cookie settings
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Maximum session duration
        options.SlidingExpiration = false; // NU reseta timeout-ul
        
        // ✅ CRITICAL: MaxAge = null → TRUE session cookie
    options.Cookie.MaxAge = null; // Se șterge când browser-ul se închide
        
        options.Events = new CookieAuthenticationEvents
 {
            OnSigningIn = context =>
        {
   // ✅ Force session-only behavior
    context.Properties.IsPersistent = false;
             context.Properties.ExpiresUtc = null; // NU seta expirare
                context.Properties.AllowRefresh = false;
     return Task.CompletedTask;
      },
            OnValidatePrincipal = async context =>
   {
           // Verificare expirare după 8 ore
     if (context.Properties?.IssuedUtc.HasValue == true)
       {
     var elapsed = DateTimeOffset.UtcNow - context.Properties.IssuedUtc.Value;
      if (elapsed > TimeSpan.FromHours(8))
              {
 context.RejectPrincipal();
      await context.HttpContext.SignOutAsync();
        }
       }
            }
   };
    });
```

### 2. **Login Implementation (AuthenticationController.cs)**

```csharp
await HttpContext.SignInAsync(
    CookieAuthenticationDefaults.AuthenticationScheme,
    principal,
    new AuthenticationProperties
    {
        IsPersistent = false,      // ✅ Session cookie
        ExpiresUtc = null,      // ✅ NU seta expirare
  AllowRefresh = false,      // ✅ NU permite refresh
  IssuedUtc = DateTimeOffset.UtcNow // Pentru tracking
    });
```

---

## 🔍 Cum Funcționează Session Cookies?

### **Session Cookie vs Persistent Cookie**

| Aspect | Session Cookie | Persistent Cookie |
|--------|----------------|-------------------|
| **MaxAge/Expires** | ❌ NU are | ✅ Are valoare setată |
| **IsPersistent** | `false` | `true` |
| **Expirare** | Când închizi browser-ul | La data specificată |
| **Salvare pe disk** | ❌ NU | ✅ DA |
| **Comportament** | Memorie RAM | Salvat permanent |

### **ValyanClinic Cookie Headers**

**Login Request Response Headers:**
```http
Set-Cookie: ValyanClinic.Auth=<encrypted-value>; path=/; samesite=lax; httponly
```

**Observă că LIPSESC:**
- ❌ `Max-Age=xxx`
- ❌ `Expires=xxx`

**Aceasta înseamnă că este un TRUE session cookie!**

---

## 🧪 Testare Session Cookie Behavior

### **Test 1: Cookie Creation**

1. **Login:** Autentifică-te cu `admin` / `admin123`
2. **Verificare Browser DevTools (F12):**
   - Application → Cookies → `https://localhost:5001`
   - Găsește `ValyanClinic.Auth`
   - **Session:** `Yes` sau `Session` (nu are Expires)
   - **Expires/Max-Age:** `Session` sau gol

### **Test 2: Close & Reopen Browser**

1. **Close Browser Completely:**
   - Windows: Închide **TOATE** ferestrele/tab-urile Chrome/Edge
   - Verifică în Task Manager că procesul browser-ului s-a închis complet
   
2. **Reopen Browser:**
   - Deschide `https://localhost:5001/`
   - **Expected:** Redirect automat la `/login` (sesiunea a expirat)
   - **Verificare DevTools:** Cookie-ul `ValyanClinic.Auth` **LIPSEȘTE**

### **Test 3: Session Timeout (8 ore)**

1. **Login Success**
2. **Wait 8+ hours** (sau modifică manual `IssuedUtc` în cookie)
3. **Navigate to any page**
4. **Expected:** Redirect automat la `/login` (sesiune expirată)

### **Test 4: Multiple Tabs (Same Session)**

1. **Login în Tab 1**
2. **Deschide Tab 2** (acelasi browser): `https://localhost:5001/dashboard`
3. **Expected:** Tab 2 vede sesiunea activă (același cookie)
4. **Close Tab 1**, **Close Tab 2** → Cookie se șterge

---

## 🚨 Known Browser Behaviors

### **Chrome/Edge "Continue where you left off"**

**Problema:**
- Chrome/Edge cu setarea "Continue where you left off" poate **PĂSTRA tab-urile deschise**
- Procesul browser-ului **NU se închide complet**
- Cookie-urile de sesiune **rămân active**

**Soluție:**
1. **Verifică procesul în Task Manager:**
   - Windows: `Ctrl+Shift+Esc` → Caută `chrome.exe` sau `msedge.exe`
   - Asigură-te că procesul s-a închis complet
   
2. **Disable "Continue where you left off":**
   - Chrome: Settings → On startup → Open the New Tab page
   - Edge: Settings → On startup → Open a new tab

3. **Force Close:**
- Windows: `Alt+F4` în loc de `X` (close button)
   - Sau: `chrome://restart` în address bar

### **Firefox "Restore Previous Session"**

**Problema similară:** Firefox poate restora sesiunea anterioară

**Soluție:**
- Firefox: Settings → General → Startup → "Show your windows and tabs from last time" → **Unchecked**

### **Safari "Reopen windows when logging back in"**

**macOS specific:** Safari poate păstra sesiuni între restart-uri OS

**Soluție:**
- Safari → Preferences → General → "Safari opens with" → **A new window**

---

## 🔐 Security Notes

### **Session Cookie Security**

✅ **Advantages:**
- Se șterge automat când browser-ul se închide
- Nu poate fi modificat din JavaScript (`HttpOnly`)
- Nu persistă pe disk (doar în RAM)
- Reduce riscul de session hijacking (dacă cineva accesează PC-ul mai târziu)

✅ **Implemented:**
- `HttpOnly = true` → NU poate fi citit din JavaScript
- `Secure = SameAsRequest` → HTTPS în producție
- `SameSite = Lax` → Protecție CSRF
- `IsEssential = true` → Funcționează chiar dacă utilizatorul refuză cookies

### **Server-Side Session Timeout**

✅ **8 ore maximum session:**
- Verificat în `OnValidatePrincipal` event
- Bazat pe `IssuedUtc` timestamp
- Se invalidează automat după 8 ore, chiar dacă browser-ul păstrează cookie-ul

```csharp
var elapsed = DateTimeOffset.UtcNow - context.Properties.IssuedUtc.Value;
if (elapsed > TimeSpan.FromHours(8))
{
    context.RejectPrincipal(); // Force logout
}
```

---

## 📊 Cookie Lifecycle - Visual

```
┌───────────────────────────────────────────────────────────┐
│       LOGIN REQUEST     │
│  POST /api/authentication/login         │
│  { username, password }   │
└──────────────────────┬────────────────────────────────────┘
            │
     ▼
        ┌────────────────────────────┐
    │  Validate Credentials      │
          │  (LoginCommandHandler)     │
        └────────────┬───────────────┘
   │
            ▼
      ┌────────────────────────────┐
          │  Create Claims Principal   │
  │  (ClaimsIdentity)          │
        └────────────┬───────────────┘
   │
     ▼
   ┌────────────────────────────┐
  │  SignInAsync()             │
      │  IsPersistent = false      │
          │  ExpiresUtc = null         │
        └────────────┬───────────────┘
           │
        ▼
     ┌────────────────────────────┐
          │  Set-Cookie RESPONSE       │
          │  ValyanClinic.Auth=xxx     │
      │  (NO Max-Age/Expires)      │
          └────────────┬───────────────┘
          │
    ▼
┌──────────────────────────────────────────────────────────┐
│      BROWSER STORES IN RAM    │
│      (NOT on disk - memory only)         │
└──────────────────────┬───────────────────────────────────┘
      │
     ┌───────────────┴───────────────┐
  │           │
 ▼       ▼
┌──────────────┐       ┌──────────────────┐
│ Navigate     │            │ Close Browser    │
│ in App       │   │ (All windows)    │
└──────┬───────┘       └────────┬─────────┘
       │               │
       │  ▼
       │             ┌──────────────────┐
       │      │ Cookie DELETED   │
       │        │ from RAM         │
       │  └────────┬─────────┘
       │      │
       │         ▼
     │        ┌──────────────────┐
       │      │ Next Login       │
    │         │ Required         │
       │          └──────────────────┘
       │
       ▼
┌──────────────────┐
│ Cookie Sent in   │
│ Request Headers  │
│ Cookie:          │
│ ValyanClinic.    │
│ Auth=xxx         │
└────────┬─────────┘
   │
         ▼
┌──────────────────┐
│ Auth Middleware  │
│ Validates Cookie │
└────────┬─────────┘
         │
    ┌────┴─────┐
    │    │
    ▼       ▼
┌────────┐  ┌──────────┐
│ Valid  │  │ Invalid  │
│ (< 8h) │  │ (> 8h)   │
└───┬────┘  └────┬─────┘
    │ │
    ▼  ▼
┌────────┐  ┌──────────┐
│ Allow  │  │ Redirect │
│ Access │  │ /login   │
└────────┘  └──────────┘
```

---

## 🛠️ Troubleshooting

### **Problema: Cookie persistă după închiderea browser-ului**

**Cauze posibile:**

1. **Browser nu s-a închis complet**
   - Verifică Task Manager (procesul încă rulează)
   - Forțează închidere: `taskkill /F /IM chrome.exe` (Windows)

2. **"Continue where you left off" activat**
   - Dezactivează în setările browser-ului

3. **Cookie are `Max-Age` setat (bug)**
   - Verifică în DevTools → Application → Cookies
   - Ar trebui să fie `Session`, NU o dată

4. **ExpiresUtc setat în AuthenticationProperties (bug)**
   - Verifică `AuthenticationController.cs`
   - `ExpiresUtc` trebuie să fie `null`

### **Problema: Sesiune expiră prea repede**

**Verificare:**
- Timpul de expirare este setat la **8 ore** în `Program.cs`
- Verifică log-urile pentru `OnValidatePrincipal` events
- Verifică `IssuedUtc` timestamp în cookie

### **Problema: Cookie nu se creează deloc**

**Verificare:**
1. `AddAuthentication()` și `AddCookie()` în `Program.cs` ✅
2. `app.UseAuthentication()` în middleware pipeline ✅
3. `credentials: 'include'` în JavaScript fetch ✅
4. Verifică Response Headers pentru `Set-Cookie`

---

## 📝 Developer Notes

### **RememberMe Checkbox**

**IMPORTANT:** În `Login.razor`, checkbox-ul "Remember Me" **NU afectează cookie-ul de sesiune!**

```csharp
// RememberMe DOAR salvează username-ul în localStorage (pentru convenience)
if (LoginModel.RememberMe)
{
    await SaveUsername(LoginModel.Username); // localStorage
}
```

**Cookie-ul de autentificare este ÎNTOTDEAUNA session-only**, indiferent de starea checkbox-ului.

### **Future Enhancement: Persistent Sessions**

Dacă în viitor vrei să implementezi sesiuni persistente ("Remember Me" real):

1. **Creează un cookie separat pentru "Remember Me"**
   ```csharp
   if (rememberMe)
   {
       authProperties.IsPersistent = true;
       authProperties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30);
   }
   ```

2. **Salvează un refresh token în baza de date**
3. **Implementează token rotation** pentru securitate

---

## ✅ Verification Checklist

După implementare, verifică:

- [ ] Cookie-ul `ValyanClinic.Auth` are **Session** ca Expires/Max-Age în DevTools
- [ ] Cookie-ul dispare când închizi **complet** browser-ul
- [ ] La redeschiderea browser-ului, ești redirecționat la `/login`
- [ ] Sesiunea expiră după 8 ore (verificat în logs)
- [ ] Multiple tab-uri împărtășesc aceeași sesiune
- [ ] Logout șterge cookie-ul imediat
- [ ] Cookie-ul este `HttpOnly`, `SameSite=Lax`

---

## 🎯 Summary

**ValyanClinic folosește TRUE session cookies:**

✅ **IsPersistent = false** în `AuthenticationProperties`  
✅ **ExpiresUtc = null** (nu setăm expirare explicită)  
✅ **Cookie.MaxAge = null** în `Program.cs`  
✅ **Expires after 8 hours** (server-side validation)  
✅ **Deleted when browser closes** (toate ferestrele)  

**Rezultat:**
- Sesiune securizată, se șterge automat la închiderea browser-ului
- Verificare server-side pentru timeout după 8 ore
- Nu persistă pe disk, doar în RAM

---

**Aplicația ValyanClinic are acum TRUE session cookies implementate corect!** 🚀
