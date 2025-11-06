# ========================================
# Testing Script: Utilizatori Management
# Descriere: Ghid de testare pentru funcționalitățile implementate
# Data: 2025-01-24
# ========================================

## 🧪 TEST PLAN - ADMINISTRARE UTILIZATORI

### **Pre-requisites:**
1. ✅ Database ValyanMed exists
2. ✅ Stored Procedures created (sp_Utilizatori_*)
3. ✅ Admin user created (run CreateAdminUser.sql)
4. ✅ Application running (dotnet run)
5. ✅ Navigate to: `/administrare/utilizatori`

---

## 📝 **TEST CASES**

### **Test 1: Initial Page Load**
**Steps:**
1. Navigate to `/administrare/utilizatori`
2. Wait for page to load

**Expected Results:**
- ✅ Loading spinner appears briefly
- ✅ Grid displays with data
- ✅ Admin user visible in grid
- ✅ Total records count shows: "Total: 1 utilizatori" (or more)
- ✅ No error messages
- ✅ All UI elements render correctly

**Status:** [ ] PASS [ ] FAIL

---

### **Test 2: Global Search**
**Steps:**
1. Click on search input
2. Type "Admin"
3. Press Enter or wait for auto-search

**Expected Results:**
- ✅ Grid filters to show only Admin user
- ✅ Total records updates: "Total: 1 utilizatori (filtrat)"
- ✅ Filter chip appears: "Cautare: Admin"
- ✅ Clear button (X) appears in search input

**Steps to Clear:**
1. Click X button in search input
2. OR Click X on filter chip

**Expected:** Search clears, all users visible again

**Status:** [ ] PASS [ ] FAIL

---

### **Test 3: Advanced Filters**
**Steps:**
1. Click "Filtre" button
2. Advanced filter panel expands
3. Select Status: "Activ"
4. Select Rol: "Administrator"
5. Click "Aplica Filtre"

**Expected Results:**
- ✅ Panel expands smoothly with animation
- ✅ Dropdowns populated correctly
- ✅ Grid filters to matching users
- ✅ Filter badges show: 2 active filters
- ✅ Filter chips display: "Status: Activ", "Rol: Administrator"
- ✅ "Sterge Filtre (2)" button enabled

**Steps to Clear:**
1. Click "Sterge Filtre (2)"

**Expected:** All filters clear, all users visible

**Status:** [ ] PASS [ ] FAIL

---

### **Test 4: Grid Selection**
**Steps:**
1. Click on Admin user row in grid
2. Observe toolbar changes

**Expected Results:**
- ✅ Row highlights with blue background
- ✅ Action toolbar activates (border changes color)
- ✅ "Selectat: Admin" displays in toolbar
- ✅ "Administrator" badge shows
- ✅ "Vizualizează" button enabled
- ✅ "Editeaza" button enabled
- ✅ "Sterge" button **DISABLED** (Admin protection)

**Status:** [ ] PASS [ ] FAIL

---

### **Test 5: View Modal (Read-Only)**
**Steps:**
1. Select Admin user (or any user)
2. Click "Vizualizează" button
3. Wait for modal to open

**Expected Results:**
- ✅ Modal overlay appears with blur effect
- ✅ Modal slides in smoothly
- ✅ Title: "Detalii Utilizator"
- ✅ 4 tabs visible: "Informatii Generale", "Securitate", "Personal Medical", "Audit"
- ✅ First tab active by default

**Tab 1: Informatii Generale**
- ✅ ID Utilizator displays (GUID format)
- ✅ Username displays emphasized
- ✅ Email clickable (mailto link)
- ✅ Rol badge color-coded (red for Administrator)
- ✅ Status badge (green "ACTIV")
- ✅ Data Creare displays
- ✅ Ultima Autentificare displays (or "Nu s-a autentificat niciodata")

**Tab 2: Securitate**
- ✅ PasswordHash masked
- ✅ Număr Încercări Eșuate: 0
- ✅ Data Blocare: "Nu este blocat" (green checkmark)
- ✅ No token reset section (if no active token)

**Tab 3: Personal Medical**
- ✅ Nume Complet displays
- ✅ Specializare: "Administrare Sistem" (badge)
- ✅ Departament: "IT"
- ✅ Pozitie: "Super Administrator"
- ✅ Email clickable (mailto link)

**Tab 4: Audit**
- ✅ Creat De: "System"
- ✅ Data Crearii displays
- ✅ Modificat De displays (or "-")
- ✅ Data Ultimei Modificari displays

**Footer Buttons:**
- ✅ "Inchide" button visible
- ✅ "Editeaza" button visible
- ✅ "Sterge" button visible
- ⚠️ For Admin user: Only "Inchide" visible (protection)

**Steps to Close:**
1. Click "Inchide" or overlay

**Expected:** Modal closes smoothly

**Status:** [ ] PASS [ ] FAIL

---

### **Test 6: Add New User (Form Modal)**
**Steps:**
1. Click "Adauga Utilizator" button (top right)
2. Wait for modal to open

**Expected Results:**
- ✅ Modal overlay appears
- ✅ Modal slides in
- ✅ Title: "Adauga Utilizator"
- ✅ 2 tabs: "Date Utilizator", "Securitate"
- ✅ First tab active

**Tab 1: Date Utilizator**
- ✅ PersonalMedical dropdown populated (searchable)
- ✅ Username input empty
- ✅ Email input empty
- ✅ Rol dropdown with 6 options (default: "Utilizator")
- ✅ "Activ" checkbox checked by default

**Fill Form:**
1. Select PersonalMedical: Choose one from list
2. Enter Username: "TestUser123"
3. Enter Email: "testuser@example.com"
4. Select Rol: "Doctor"
5. Keep "Activ" checked

**Tab 2: Securitate**
- ✅ Alert info: "Pentru utilizator nou, parola este obligatorie"
- ✅ Password input visible (with toggle eye icon)
- ✅ "Genereaza Parola" button visible

**Generate Password:**
1. Click "Genereaza Parola"

**Expected:**
- ✅ Generated password displays (12+ chars)
- ✅ Password input auto-filled
- ✅ Copy button appears
- ✅ Click copy button → toast "Copied!" (if implemented)

**Submit:**
1. Click "Salveaza" button

**Expected Results:**
- ✅ Loading spinner on button ("Se salveaza...")
- ✅ Modal closes
- ✅ Success toast: "Utilizator creat cu succes!"
- ✅ Grid reloads
- ✅ New user appears in list
- ✅ Password hashed with BCrypt (check in database)

**Database Verification:**
```sql
SELECT TOP 1 
    Username, 
    Email, 
    PasswordHash, 
    Salt, 
    Rol, 
    EsteActiv,
    LEN(PasswordHash) AS HashLength
FROM Utilizatori
WHERE Username = 'TestUser123'
ORDER BY DataCreare DESC
```

**Expected:**
- ✅ Username = "TestUser123"
- ✅ Email = "testuser@example.com"
- ✅ PasswordHash starts with "$2a$12$" (BCrypt format)
- ✅ Salt = "bcrypt_autogenerated"
- ✅ HashLength = 60
- ✅ Rol = "Doctor"
- ✅ EsteActiv = 1

**Status:** [ ] PASS [ ] FAIL

---

### **Test 7: Edit Existing User**
**Steps:**
1. Select "TestUser123" from grid
2. Click "Editeaza" button
3. Wait for modal to open (edit mode)

**Expected Results:**
- ✅ Modal title: "Editeaza Utilizator"
- ✅ Form pre-filled with existing data
- ✅ PersonalMedical dropdown **DISABLED** (cannot change)
- ✅ Username displays: "TestUser123"
- ✅ Email displays: "testuser@example.com"
- ✅ Rol displays: "Doctor"
- ✅ "Activ" checkbox state matches database

**Tab 2: Securitate (Edit Mode)**
- ✅ Alert warning: "Lasa parola goala daca nu doresti sa o schimbi"
- ✅ Password input empty
- ✅ "Genereaza Parola" button visible

**Edit Username:**
1. Change Username to: "TestUser123Updated"
2. Leave password empty (no change)
3. Click "Actualizeaza"

**Expected Results:**
- ✅ Loading spinner
- ✅ Modal closes
- ✅ Success toast: "Utilizator actualizat cu succes!"
- ✅ Grid reloads
- ✅ Username updated in grid

**Edit Password:**
1. Open edit again
2. Enter new password OR click "Genereaza Parola"
3. Click "Actualizeaza"

**Expected:**
- ✅ New password hashed with BCrypt
- ✅ Success toast
- ✅ Grid reloads

**Database Verification:**
```sql
SELECT 
    Username, 
    PasswordHash, 
    DataUltimeiModificari,
    ModificatDe
FROM Utilizatori
WHERE Username = 'TestUser123Updated'
```

**Expected:**
- ✅ Username updated
- ✅ PasswordHash updated (if password changed)
- ✅ DataUltimeiModificari recent
- ✅ ModificatDe = "CurrentUser" (or system default)

**Status:** [ ] PASS [ ] FAIL

---

### **Test 8: Delete Protection (Admin)**
**Steps:**
1. Select Admin user from grid
2. Observe "Sterge" button

**Expected Results:**
- ✅ "Sterge" button **DISABLED**
- ✅ Title tooltip: "Sterge utilizator"
- ✅ Cannot click button

**Steps (from ViewModal):**
1. Open Admin user in ViewModal
2. Check footer buttons

**Expected:**
- ✅ Only "Inchide" button visible
- ✅ No "Editeaza" or "Sterge" buttons for Admin

**Status:** [ ] PASS [ ] FAIL

---

### **Test 9: Pagination**
**Steps:**
(Requires multiple users - create a few test users first)

1. Set Page Size to 10
2. Navigate through pages

**Expected Results:**
- ✅ Page size changes immediately
- ✅ Grid shows 10 records max
- ✅ Pager info updates: "Afisate 1-10 din X"
- ✅ Page numbers display correctly
- ✅ "Prima pagina" / "Ultima pagina" buttons work
- ✅ "Pagina anterioara" / "Pagina urmatoare" buttons work
- ✅ Active page highlighted (blue background)
- ✅ Disabled buttons grayed out (at first/last page)

**Status:** [ ] PASS [ ] FAIL

---

### **Test 10: Responsive Design (Mobile)**
**Steps:**
1. Open Browser DevTools (F12)
2. Toggle device toolbar (Ctrl+Shift+M)
3. Select mobile device (iPhone 12, Pixel 5, etc.)
4. Test all features

**Expected Results:**
- ✅ Layout adapts to narrow screen
- ✅ Modal width: 95%
- ✅ Grid columns stack/scroll horizontally
- ✅ Tabs scroll horizontally (no wrap)
- ✅ Buttons stack vertically in modal footer
- ✅ Search and filters responsive
- ✅ All features functional on mobile

**Status:** [ ] PASS [ ] FAIL

---

## 🔐 **SECURITY TESTS**

### **Test 11: BCrypt Password Hashing**
**Manual Database Check:**

```sql
-- Check Admin user password hash
SELECT 
    Username,
    PasswordHash,
    Salt,
    LEN(PasswordHash) AS HashLength,
  SUBSTRING(PasswordHash, 1, 4) AS HashPrefix
FROM Utilizatori
WHERE Username = 'Admin'
```

**Expected Results:**
- ✅ PasswordHash = '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMesbjx.U4T6wgSJc4xE7iW.Im'
- ✅ Salt = 'bcrypt_autogenerated'
- ✅ HashLength = 60
- ✅ HashPrefix = '$2a$' (BCrypt identifier)

**Test Password Verification (Code):**
```csharp
// In UtilizatorRepository or test project
var passwordHasher = new BCryptPasswordHasher();
var testPassword = "admin123!@#";
var storedHash = "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMesbjx.U4T6wgSJc4xE7iW.Im";

bool isValid = passwordHasher.VerifyPassword(testPassword, storedHash);
Console.WriteLine($"Password verification: {isValid}"); // Should be TRUE
```

**Status:** [ ] PASS [ ] FAIL

---

### **Test 12: Password Generation**
**Steps:**
1. Open Add User modal
2. Navigate to "Securitate" tab
3. Click "Genereaza Parola" multiple times (5-10 times)

**Expected Results:**
- ✅ Each click generates **unique** password
- ✅ Length >= 12 characters
- ✅ Contains uppercase letters
- ✅ Contains lowercase letters
- ✅ Contains digits
- ✅ Contains special characters (!@#$%^&*()_+-)
- ✅ No predictable patterns
- ✅ Truly random each time

**Status:** [ ] PASS [ ] FAIL

---

## 📊 **PERFORMANCE TESTS**

### **Test 13: Large Dataset**
**Setup:**
Create 100+ test users

**Steps:**
1. Load page with 100+ users
2. Test search/filter performance
3. Test pagination speed
4. Test modal open/close speed

**Expected Results:**
- ✅ Initial load < 2 seconds
- ✅ Search/filter instant (< 200ms)
- ✅ Pagination instant
- ✅ Modal open/close smooth
- ✅ No lag or freezing

**Status:** [ ] PASS [ ] FAIL

---

## ✅ **TEST SUMMARY**

| Test ID | Test Name | Status | Notes |
|---------|-----------|--------|-------|
| Test 1 | Initial Page Load | [ ] | |
| Test 2 | Global Search | [ ] | |
| Test 3 | Advanced Filters | [ ] | |
| Test 4 | Grid Selection | [ ] | |
| Test 5 | View Modal | [ ] | |
| Test 6 | Add New User | [ ] | |
| Test 7 | Edit User | [ ] | |
| Test 8 | Delete Protection | [ ] | |
| Test 9 | Pagination | [ ] | |
| Test 10 | Responsive Design | [ ] | |
| Test 11 | BCrypt Hashing | [ ] | |
| Test 12 | Password Generation | [ ] | |
| Test 13 | Performance | [ ] | |

**PASS Rate:** 0 / 13 (0%)  
**FAIL Rate:** 0 / 13 (0%)  
**Not Tested:** 13 / 13 (100%)

---

## 🐛 **KNOWN ISSUES / BUGS**

1. [ ] Delete functionality not implemented (placeholder only)
2. [ ] ConfirmDeleteModal not created
3. [ ] "CurrentUser" hardcoded in commands (needs auth integration)
4. [ ] No actual authentication/authorization yet

---

## 📝 **TESTER NOTES**

**Date:** _____________  
**Tester:** _____________  
**Environment:** _____________  
**Browser:** _____________  
**Version:** _____________

**Additional Notes:**
_________________________________________________
_________________________________________________
_________________________________________________

---

**Creat:** 2025-01-24  
**Versiune:** 1.0  
**Status:** ⏳ **READY FOR TESTING**
