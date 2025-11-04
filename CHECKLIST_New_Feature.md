# ✅ Checklist: Adăugare Feature Nou (Repository + CQRS)

Folosește acest checklist de fiecare dată când adaugi un feature nou cu repository și handler-e MediatR.

---

## 📋 Checklist Complet

### 1️⃣ Domain Layer (ValyanClinic.Domain)

- [ ] **Entity Class** creat în `Domain/Entities/`
  ```csharp
  public class NouEntity { ... }
  ```

- [ ] **Repository Interface** creat în `Domain/Interfaces/Repositories/`
  ```csharp
  public interface INouRepository { ... }
  ```

---

### 2️⃣ Infrastructure Layer (ValyanClinic.Infrastructure)

- [ ] **Repository Implementation** creat în `Infrastructure/Repositories/`
  ```csharp
  public class NouRepository : BaseRepository, INouRepository { ... }
  ```

- [ ] **Stored Procedures** create în database
  - [ ] `sp_Nou_GetAll`
  - [ ] `sp_Nou_GetById`
  - [ ] `sp_Nou_Create`
  - [ ] `sp_Nou_Update`
  - [ ] `sp_Nou_Delete`
  - [ ] `sp_Nou_GetCount` (pentru paginare)

---

### 3️⃣ Application Layer (ValyanClinic.Application)

#### Queries

- [ ] **Query Class** creat
  ```
  Application/Features/NouManagement/Queries/GetNouList/GetNouListQuery.cs
  ```

- [ ] **Query Handler** creat
  ```
  Application/Features/NouManagement/Queries/GetNouList/GetNouListQueryHandler.cs
  ```

- [ ] **DTO Class** creat
  ```
  Application/Features/NouManagement/Queries/GetNouList/NouListDto.cs
  ```

#### Commands

- [ ] **Create Command** + Handler
  ```
  Application/Features/NouManagement/Commands/CreateNou/CreateNouCommand.cs
  Application/Features/NouManagement/Commands/CreateNou/CreateNouCommandHandler.cs
  ```

- [ ] **Update Command** + Handler
  ```
  Application/Features/NouManagement/Commands/UpdateNou/UpdateNouCommand.cs
  Application/Features/NouManagement/Commands/UpdateNou/UpdateNouCommandHandler.cs
  ```

- [ ] **Delete Command** + Handler
  ```
  Application/Features/NouManagement/Commands/DeleteNou/DeleteNouCommand.cs
  Application/Features/NouManagement/Commands/DeleteNou/DeleteNouCommandHandler.cs
  ```

---

### 4️⃣ ⚠️ **CRITICAL: Dependency Injection (Program.cs)**

#### ✅ MANDATORY STEP!

- [ ] **Repository înregistrat în Program.cs**
  ```csharp
  builder.Services.AddScoped<INouRepository, NouRepository>();
  ```

**⚠️ IMPORTANT:** Fără acest pas, aplicația va crăpa la startup!

---

### 5️⃣ Presentation Layer (ValyanClinic/Components)

#### Blazor Page

- [ ] **Razor Component** creat
  ```
  Components/Pages/Administrare/Nou/AdministrareNou.razor
  Components/Pages/Administrare/Nou/AdministrareNou.razor.cs
  ```

#### Dialog-uri

- [ ] **Add Dialog** creat (dacă e necesar)
  ```
  Components/Dialogs/Nou/AddNouDialog.razor
  ```

- [ ] **Edit Dialog** creat (dacă e necesar)
  ```
  Components/Dialogs/Nou/EditNouDialog.razor
  ```

---

### 6️⃣ Testing & Verification

#### Build & Compile

- [ ] **Build reușit** fără erori
  ```powershell
  dotnet build
  ```

- [ ] **Nicio eroare de compilare**
  - [ ] Zero CS errors
  - [ ] Zero warnings critice

#### Runtime Testing

- [ ] **Aplicația pornește** fără crash
- [ ] **Navigare la pagina nouă** funcționează
- [ ] **Grid se încarcă** cu date
- [ ] **Filtrare** funcționează
- [ ] **Sortare** funcționează
- [ ] **Paginare** funcționează
- [ ] **Add** funcționează
- [ ] **Edit** funcționează
- [ ] **Delete** funcționează

#### Log Verification

- [ ] **Verificat log-urile** pentru erori
  ```
  ValyanClinic/Logs/errors-*.log
  ```

- [ ] **Nicio eroare în runtime**
- [ ] **Nicio excepție SQL**

---

### 7️⃣ Database

- [ ] **Tabele create** în database
- [ ] **Stored Procedures create**
- [ ] **Foreign Keys** configurate corect
- [ ] **Indexes** adăugate pentru performanță
- [ ] **Test data** inserat pentru testing

---

### 8️⃣ Navigation & UI

- [ ] **Menu item** adăugat în NavMenu
  ```razor
  <NavLink href="/administrare/nou">
      <i class="fa fa-icon"></i> Nou
  </NavLink>
  ```

- [ ] **Route** configurat corect
  ```razor
  @page "/administrare/nou"
  ```

- [ ] **Breadcrumb** funcționează

---

## 🚨 Common Pitfalls (Greșeli Frecvente)

### ❌ Top 3 Erori

1. **❌ Uitat să înregistrezi Repository-ul în Program.cs**
   - Simptom: Aplicația crăpează la startup
   - Fix: Adaugă `builder.Services.AddScoped<IRepository, Repository>()`

2. **❌ Stored Procedure nu există în DB**
   - Simptom: SQL Exception la runtime
   - Fix: Creează SP în database

3. **❌ Wrong namespace în using statements**
   - Simptom: CS0246 "Type not found"
   - Fix: Verifică namespace-urile

---

## 📊 Quick Reference

### Repository Lifetime Scopes

| Service Type | Lifetime | Usage |
|--------------|----------|-------|
| Repository | `AddScoped` | ✅ Recomandat (per-request) |
| DbConnection Factory | `AddSingleton` | ✅ OK (stateless) |
| Cache Service | `AddSingleton` | ✅ OK (shared state) |
| Business Service | `AddScoped` | ✅ Recomandat |

---

## 🔍 Verification Commands

### Build Check
```powershell
dotnet build ValyanClinic.sln
```

### Run Application
```powershell
dotnet run --project ValyanClinic/ValyanClinic.csproj
```

### Check Logs
```powershell
Get-Content ValyanClinic/Logs/errors-*.log -Tail 50
```

---

## ✅ Final Checklist

Înainte de commit:

- [ ] ✅ Build successful
- [ ] ✅ Application runs without crash
- [ ] ✅ All CRUD operations tested
- [ ] ✅ No errors in logs
- [ ] ✅ Code reviewed
- [ ] ✅ Repository registered in DI
- [ ] ✅ Database objects created
- [ ] ✅ UI navigation works

---

## 📚 Documentation

După finalizare:

- [ ] README actualizat (dacă e necesar)
- [ ] API documentation
- [ ] User guide (dacă e complex)
- [ ] Code comments pentru logică complexă

---

## 🎯 Success Criteria

✅ Feature este considerat complet când:

1. ✅ Build-ul este success
2. ✅ Aplicația pornește normal
3. ✅ Toate operațiile CRUD funcționează
4. ✅ UI este responsive și user-friendly
5. ✅ Zero erori în logs
6. ✅ Performance este acceptabilă
7. ✅ Code este clean și documented

---

*Folosește acest checklist pentru fiecare feature nou!*  
*Previne bug-uri de tipul "aplicația se închide instant"*
