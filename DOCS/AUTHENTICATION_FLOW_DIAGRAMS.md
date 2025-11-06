# 📊 Diagrame Flow Autentificare ValyanClinic

## 🔄 Flow General - Vizualizare Completă

```
┌──────────────────────────────────────────────────────────────┐
│       PORNIRE APLICAȚIE        │
│           https://localhost:5001/  │
└────────────────────────┬─────────────────────────────────────┘
  │
       ▼
┌──────────────────────────────────────────────────────────────┐
│            Index.razor (/)                │
│  ┌────────────────────────────────────────────────────────┐  │
│  │ AuthenticationStateProvider.GetAuthenticationState()   │  │
│  └────────────────────────┬───────────────────────────────┘  │
└────────────────────────────┼──────────────────────────────────┘
               │
          ┌─────────┴─────────┐
    │         │
 DA ◄───┤ Autentificat?    ├───► NU
        │          │
   └─────┬───────┬─────┘
             │       │
                ▼       ▼
         ┌─────────────────────────────┐
    │    /dashboard    │    /login
            │    (Home.razor)             │    (Login.razor)
└─────────────────────────────┘
  │
          │
        ┌────────────┴────────────┐
            │        │
          ▼  ▼
┌──────────────────┐     ┌──────────────────────┐
│  Navigare App    │     │  Logout Click        │
│  (cu sesiune)    │     │  (Header dropdown)   │
└──────────────────┘     └──────┬───────────────┘
          │
   ▼
              ┌──────────────────────┐
  │   Logout.razor   │
        │   Clear Session      │
      └──────┬───────────────┘
           │
         ▼
    ┌──────────────────────┐
          │   /login (redirect)  │
   └──────────────────────┘
```

---

## 🔐 Flow Autentificare Detalizat

```
┌──────────────────────────────────────────────────────────────┐
│            Login.razor (/login)       │
│    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ Formular:       │  │
│  │  • Username: [___________]      │    │
│  │  • Password: [___________] 👁️     │    │
││  • [✓] Ține-mă minte    │  │
│  │  • [✓] Resetare parolă la prima logare     │    │
│  │  • [Conectare]          │    │
│  └─────────────────────────────────────────────────────┘    │
└───────────────────────────┬──────────────────────────────────┘
            │ Click "Conectare"
          ▼
┌──────────────────────────────────────────────────────────────┐
│   Login.razor.cs - HandleLogin()       │
│   │
│  1. IsLoading = true         │
│  2. Create LoginCommand       │
│  3. Send to MediatR              │
└───────────────────────────┬──────────────────────────────────┘
│
             ▼
┌──────────────────────────────────────────────────────────────┐
│           LoginCommandHandler (MediatR)        │
││
│  1. Caută utilizator în DB (UtilizatorRepository)            │
│  2. Verifică dacă utilizatorul există          │
│  3. Verifică dacă contul este activ    │
│  4. Validează parola (BCrypt)     │
│  5. Returnează LoginResult      │
└───────────────────────────┬──────────────────────────────────┘
           │
                ┌───────────┴───────────┐
        │   │
   SUCCESS ◄─────────────────────► FAIL
     │       │
          ▼            ▼
┌──────────────────────────┐  ┌───────────────────────┐
│ CustomAuthStateProvider  │  │ Afișează ErrorMessage │
│ .MarkUserAsAuthenticated │  │ IsLoading = false     │
│                    │  └───────────────────────┘
│ • Salvează UserSession   │
│   în Protected Storage   │
│ • Notify Auth Changed    │
└──────────┬───────────────┘
    │
   ▼
┌──────────────────────────┐
│ Remember Me?      │
│ DA: SaveUsername()       │
│ NU: ClearUsername()  │
└──────────┬───────────────┘
           │
      ▼
┌──────────────────────────┐
│ NavigateTo("/dashboard") │
└──────────────────────────┘
```

---

## 🗄️ Structura UserSession în Protected Storage

```
┌──────────────────────────────────────────────────────────┐
│      PROTECTED SESSION STORAGE (Client-Side Encrypted)   │
│          │
│  Key: "UserSession"          │
│  Value: {         │
│    UtilizatorId: "12345678-1234-1234-1234-123456789012"  │
│Username: "admin"        │
│    Email: "admin@valyanclinic.ro"            │
│    Role: "Administrator"       │
│    LoginTime: "2024-01-15T10:30:00"  │
│    ExpirationTime: "2024-01-15T18:30:00" ← 8 ore         │
│  }    │
└──────────────────────────────────────────────────────────┘
```

**Verificare Valabilitate:**
```csharp
public bool IsValid()
{
    return DateTime.Now < ExpirationTime;
}
```

**Conversie în ClaimsPrincipal:**
```csharp
Claims:
  - NameIdentifier: "12345678-1234-1234-1234-123456789012"
  - Name: "admin"
  - Email: "admin@valyanclinic.ro"
  - Role: "Administrator"
  - LoginTime: "2024-01-15T10:30:00"
  - ExpirationTime: "2024-01-15T18:30:00"
```

---

## 🚪 Flow Logout Detaliat

```
┌──────────────────────────────────────────────────────────┐
│     Header.razor - User Dropdown Menu           │
│      │
│  ┌────────────────────────────────────────────────────┐ │
││ [👤 Dr. Admin]         │ │
│  │↓ (click)    │ │
│  │ ┌──────────────────────────────────────────────┐   │ │
│  │ │ 🔹 Dr. Admin - Administrator                 │   │ │
│  │ │ ─────────────────────────────────────────    │   │ │
│  │ │ 👤 Profil          │   │ │
│  │ │ ⚙️  Setări       │   │ │
│  │ │ ─────────────────────────────────────────    │   │ │
│  │ │ 🚪 Deconectare ◄── CLICK         │   │ │
│  │ └──────────────────────────────────────────────┘   │ │
│  └────────────────────────────────────────────────────┘ │
└────────────────────────────┬─────────────────────────────┘
         │
          ▼
┌──────────────────────────────────────────────────────────┐
│       NavigateTo("/logout")       │
└────────────────────────────┬─────────────────────────────┘
         │
          ▼
┌──────────────────────────────────────────────────────────┐
│    Logout.razor + Logout.razor.cs        │
│          │
│  UI:   │
│  ┌────────────────────────────────────────────────────┐ │
│  │  [🚪]              │ │
│  │  Te deconectăm...          │ │
│  │  Vei fi redirecționat către pagina de login       │ │
│  │  [Loading Spinner]            │ │
│  └────────────────────────────────────────────────────┘ │
│  │
│  Code Behind:    │
│  1. AuthStateProvider.MarkUserAsLoggedOut()      │
│  2. await Task.Delay(2000)   │
│  3. NavigateTo("/login", forceLoad: true)        │
└────────────────────────────┬─────────────────────────────┘
  │
          ▼
┌──────────────────────────────────────────────────────────┐
│   CustomAuthenticationStateProvider         │
│   .MarkUserAsLoggedOut()         │
│           │
│   1. ClearUserSession()   │
│      → DeleteAsync("UserSession")     │
│   2. NotifyAuthenticationStateChanged()│
│      → new ClaimsPrincipal(new ClaimsIdentity())          │
└────────────────────────────┬─────────────────────────────┘
       │
   ▼
┌──────────────────────────────────────────────────────────┐
│    NavigateTo("/login")          │
│    Redirect automat        │
└──────────────────────────────────────────────────────────┘
```

---

## 🔄 Flow Session Expiration

```
┌──────────────────────────────────────────────────────────┐
│    Utilizator navighează în aplicație │
│                (ex: /dashboard → /personal)     │
└────────────────────────────┬─────────────────────────────┘
       │
    ▼
┌──────────────────────────────────────────────────────────┐
│   CustomAuthenticationStateProvider  │
│       .GetAuthenticationStateAsync()             │
│        │
│   1. Citește UserSession din Protected Storage           │
│   2. Verifică userSession.IsValid()  │
└────────────────────────────┬─────────────────────────────┘
        │
              ┌──────────────┴──────────────┐
           │    │
       VALID  ▼       ▼  EXPIRED
┌────────────────────────┐    ┌────────────────────────────┐
│ DateTime.Now <      │    │ DateTime.Now >=  │
│ ExpirationTime         │    │ ExpirationTime             │
│  │    │  │
│ → Return Authenticated │    │ → ClearUserSession()       │
│   ClaimsPrincipal      │    │ → Return Anonymous         │
└────────────────────────┘    │   ClaimsPrincipal       │
    └──────────┬─────────────────┘
     │
      ▼
         ┌──────────────────────────┐
            │ Index.razor detectează   │
 │ utilizator neautentificat│
              │ → /login          │
      └──────────────────────────┘
```

**Timeline Expirare (8 ore):**
```
Login:    10:00 AM → ExpirationTime = 6:00 PM
─────────────────────────────────────────────────
10:00 AM  ✅ Login Success
11:00 AM  ✅ Sesiune Valid
12:00 PM  ✅ Sesiune Valid
...
5:59 PM   ✅ Sesiune Valid
6:00 PM   ❌ Sesiune EXPIRED → Redirect /login
```

---

## 🎯 State Diagram - Authentication States

```
┌──────────────────────────────────────────────────────────┐
│         │
│             [NOT AUTHENTICATED]        │
│               │         │
│      │ Login Success       │
│       ▼     │
│          [AUTHENTICATED]     │
│    │      │
│        ┌───────────┼───────────┐         │
│   │           │           │    │
│         Logout│     8h Expired    Navigate    │
│      │           │ │        │
│       ▼      ▼           │    │
│       [NOT AUTHENTICATED] ◄───────────┘                │
│      │      │
│  └─► Redirect /login           │
│   │
└──────────────────────────────────────────────────────────┘
```

**State Details:**

| State        | Protected Storage | ClaimsPrincipal | UI Behavior         |
|---------------------|-------------------|-----------------|--------------------------|
| NOT AUTHENTICATED   | Empty           | Anonymous| Redirect → `/login`      |
| AUTHENTICATED       | UserSession     | With Claims     | Access all routes  |
| SESSION EXPIRED     | Deleted           | Anonymous   | Redirect → `/login`  |
| LOGGING OUT      | Deleting...       | Anonymous       | Show logout page         |

---

## 📊 Component Interaction Diagram

```
┌─────────────────────────────────────────────────────────────┐
│    APLICAȚIE      │
││
│  ┌──────────────┐     ┌───────────────────────────────┐   │
│  │ Index.razor  │────▶│ AuthenticationStateProvider│   │
│  └──────────────┘     └───────────┬───────────────────┘   │
│   ││           │
│         │    │    │
│  ┌──────▼──────┐    ┌───────▼────────────┐          │
│  │Login.razor  │◄─────────│ Protected Storage  │   │
│  └──────┬──────┘    └──────────┬─────────┘    │
│       │        │          │
│  │          │           │
│  ┌──────▼──────────────┐    ┌────────▼───────────┐        │
│  │ LoginCommandHandler │    │ UserSession Model  │        │
│  └──────┬──────────────┘    └────────────────────┘        │
│      │         │
│  ┌──────▼──────────────┐    │
│  │ UtilizatorRepository│           │
│  └──────┬──────────────┘  │
│         │   │
│  ┌──────▼──────────────┐      │
│  │   SQL Database      │        │
│  └─────────────────────┘                │
└─────────────────────────────────────────────────────────────┘
```

**Data Flow:**
1. **Index.razor** → Check Auth State → Redirect Logic
2. **Login.razor** → Credentials → **LoginCommand** → **MediatR**
3. **LoginCommandHandler** → Validate → **UtilizatorRepository** → **SQL DB**
4. **LoginCommandHandler** → Success → **CustomAuthStateProvider**
5. **CustomAuthStateProvider** → Save **UserSession** → **Protected Storage**
6. **CustomAuthStateProvider** → Notify → **All Components**

---

## ✅ Security Flow - Password Hashing

```
┌──────────────────────────────────────────────────────────┐
│       USER REGISTRATION           │
│      (Prima creare cont)          │
│       │
│  Plain Password: "admin123"   │
│         │    │
│    ▼          │
│  BCrypt.HashPassword("admin123")  │
│         │ │
│ ▼       │
│  Hash: "$2a$11$..."  ◄── Salvat în DB           │
└──────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────┐
│          LOGIN   │
│            │
│  Input Password: "admin123"    │
│ │  │
│       ▼     │
│  BCrypt.Verify("admin123", storedHash)          │
│ │            │
│         ├── TRUE  → Login Success  │
│  └── FALSE → Login Fail           │
└──────────────────────────────────────────────────────────┘
```

**Security Features:**
- ✅ Parola NICIODATĂ în plain text în DB
- ✅ BCrypt salt automat pentru fiecare parolă
- ✅ Hash diferit chiar pentru parole identice
- ✅ Nu se poate face reverse engineering din hash

---

## 🎉 Concluzie - Flow Complet

**Aplicația ValyanClinic are un sistem complet de autentificare cu:**

✅ Redirect automat la `/login` la pornire  
✅ Validare credențiale cu BCrypt  
✅ Sesiune criptată în Protected Storage  
✅ Expirare automată după 8 ore  
✅ Logout funcțional cu clear session  
✅ Protected routes cu verificare auth  
✅ UI modern și user-friendly  

**Flow-ul este production-ready! 🚀**
