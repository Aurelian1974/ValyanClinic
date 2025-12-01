# 🧪 E2E Testing Setup - Playwright Configuration

**Last Updated:** 2025-12-01  
**Status:** ✅ Active  
**Framework:** Playwright for .NET

---

## 📋 **Overview**

ValyanClinic folosește **Playwright** pentru testele End-to-End (E2E) care verifică fluxurile complete de utilizare ale aplicației Blazor Server.

---

## 🌐 **Application Ports**

### **Configured in `ValyanClinic/Properties/launchSettings.json`:**

| Profile | Protocol | URL | Usage |
|---------|----------|-----|-------|
| **https** (default) | HTTPS + HTTP | `https://localhost:7164`<br>`http://localhost:5007` | Development, E2E Tests |
| **http** | HTTP only | `http://localhost:5007` | Lightweight development |
| **http-localhost** | HTTP only | `http://localhost:5007` | Localhost-specific |

### **E2E Tests Configuration:**

```csharp
// ValyanClinic.Tests/Integration/PlaywrightTestBase.cs
protected virtual string BaseUrl { get; } = "https://localhost:7164";
```

**⚠️ Important:** E2E testele se conectează la **`https://localhost:7164`** (HTTPS). Aplicația **TREBUIE să ruleze** pe acest port pentru ca testele să funcționeze.

---

## 🚀 **Quick Start - Running E2E Tests**

### **Step 1: Install Playwright Browsers (One-time)**

```powershell
# Navigate to test output directory
cd "D:\Lucru\CMS\ValyanClinic.Tests\bin\Debug\net10.0"

# Install Chromium browser
.\playwright.ps1 install chromium

# ✅ Expected output:
# Chromium 129.0.6668.29 downloaded to C:\Users\{USER}\AppData\Local\ms-playwright\chromium-1134
```

### **Step 2: Start Blazor Application**

```powershell
# Terminal 1 - Start app on HTTPS (port 7164)
cd "D:\Lucru\CMS\ValyanClinic"
dotnet run --launch-profile https

# ✅ Wait for:
# Now listening on: https://localhost:7164
# Now listening on: http://localhost:5007
# Application started. Press Ctrl+C to shut down.
```

**IMPORTANT:** Nu închide acest terminal! Aplicația trebuie să ruleze în background pentru E2E teste.

### **Step 3: Run E2E Tests (în alt terminal)**

```powershell
# Terminal 2 - Run all E2E tests
cd "D:\Lucru\CMS"
dotnet test ValyanClinic.Tests\ValyanClinic.Tests.csproj --filter "FullyQualifiedName~E2ETests"

# ✅ Expected: All 13 tests should PASS
```

---

## 📁 **Test Structure**

```
ValyanClinic.Tests/
├── Integration/
│   ├── PlaywrightTestBase.cs         # Base class (browser setup, utilities)
│   └── VizualizarePacientiE2ETests.cs # E2E tests for VizualizarePacienti page
└── bin/Debug/net10.0/
    └── playwright.ps1                 # Playwright CLI (browser installation)
```

---

## 🧪 **Available E2E Tests**

### **VizualizarePacientiE2ETests.cs** (13 tests)

| Test Name | Description | Duration |
|-----------|-------------|----------|
| `PageLoad_DisplaysHeaderAndGrid` | Verifies page header and grid render | ~2s |
| `PageLoad_ShowsLoadingIndicatorDuringDataFetch` | Loading spinner appears during data load | ~2s |
| `GlobalSearch_TypeSearchText_FiltersResults` | Global search filters patient list | ~3s |
| `GlobalSearch_ClearButton_ResetsResults` | Clear button resets search | ~2s |
| `AdvancedFilters_ApplyJudetFilter_FiltersResults` | Judet filter works correctly | ~3s |
| `AdvancedFilters_ClearAllFilters_ResetsToFullList` | Clear all filters resets data | ~2s |
| `AdvancedFilters_FilterChip_RemovesSpecificFilter` | Filter chips remove individual filters | ~3s |
| `Pagination_ChangePageSize_UpdatesGridRows` | Page size selector updates rows | ~2s |
| `RowSelection_SelectPatient_EnablesActionButtons` | Row selection enables action buttons | ~2s |
| `ViewModal_OpenFromToolbar_DisplaysPatientDetails` | View modal opens with patient data | ~3s |
| `ViewModal_CloseButton_ClosesModal` | Close button closes modal | ~2s |
| `RefreshButton_Click_ReloadsData` | Refresh button reloads data | ~2s |
| `Sorting_ClickColumnHeader_SortsData` | Column sorting works (if implemented) | ~2s |

**Total Duration:** ~43s (all tests)

---

## 🎯 **Test Execution Modes**

### **Headless Mode (Default - Fast)**

```csharp
// PlaywrightTestBase.cs
protected virtual bool Headless { get; } = true; // No browser window
```

```powershell
# Run tests in headless mode (fast, CI/CD)
dotnet test --filter "FullyQualifiedName~E2ETests"
```

### **Headed Mode (Debugging - Visible Browser)**

```csharp
// Override in test class for debugging
protected override bool Headless => false; // Show browser window
protected override int SlowMo => 100; // Slow down actions (ms)
```

```powershell
# Run specific test with visible browser
dotnet test --filter "FullyQualifiedName~PageLoad_DisplaysHeaderAndGrid"
```

---

## 📸 **Test Artifacts**

### **Screenshots**

```csharp
// In test method
await TakeScreenshotAsync("page-load-state.png");

// Output: ValyanClinic.Tests/bin/Debug/net10.0/screenshots/page-load-state.png
```

### **Videos**

Playwright înregistrează **automat** video-uri pentru fiecare test:

```
ValyanClinic.Tests/bin/Debug/net10.0/videos/
├── test-1-{GUID}.webm
├── test-2-{GUID}.webm
└── ...
```

**⚠️ Videos sunt create DOAR când testul eșuează** (pentru debugging).

---

## 🔧 **Troubleshooting**

### **Error: `net::ERR_CONNECTION_REFUSED`**

**Cauză:** Aplicația Blazor **nu rulează** pe portul `https://localhost:7164`.

**Soluție:**
```powershell
# Verifică dacă aplicația rulează
netstat -ano | findstr :7164

# Dacă nu rulează, pornește-o:
cd "D:\Lucru\CMS\ValyanClinic"
dotnet run --launch-profile https
```

---

### **Error: `Playwright not installed`**

**Cauză:** Browser-ele Playwright nu sunt instalate.

**Soluție:**
```powershell
cd "D:\Lucru\CMS\ValyanClinic.Tests\bin\Debug\net10.0"
.\playwright.ps1 install chromium
```

---

### **Error: `Test timeout after 30 seconds`**

**Cauză:** Pagina se încarcă prea lent sau aplicația nu răspunde.

**Soluție:**
```csharp
// Increase timeout in test
await NavigateToAsync("/pacienti/vizualizare");
await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 60000 }); // 60s
```

---

### **Error: `Element not found`**

**Cauză:** Selectorul CSS nu găsește elementul (timing sau selector greșit).

**Soluție:**
```csharp
// Add explicit wait before interaction
await Page.WaitForSelectorAsync(".search-input", new() { State = WaitForSelectorState.Visible });
await Page.FillAsync(".search-input", "Popescu");
```

---

## 🎨 **Best Practices**

### **1. Use Semantic Selectors (Accessibility-First)**

```csharp
// ✅ GOOD - Accessible to screen readers
await Page.GetByRole(AriaRole.Button, new() { Name = "Reincarca" }).ClickAsync();
await Page.GetByLabel("Cautare rapida").FillAsync("Popescu");

// ⚠️ OK - CSS class selectors (but can break with style changes)
await Page.Locator(".btn-primary").ClickAsync();

// ❌ AVOID - XPath (fragile, hard to read)
await Page.Locator("//div[@class='modal']//button[1]").ClickAsync();
```

### **2. Add `data-testid` Attributes for Stability**

```razor
<!-- VizualizarePacienti.razor -->
<input type="text" 
       class="search-input" 
       data-testid="search-input"
       placeholder="Cautare rapida..." />
```

```csharp
// Test with stable selector
await Page.Locator("[data-testid='search-input']").FillAsync("Popescu");
```

### **3. Wait for Network Idle After Actions**

```csharp
// After search or filter action
await Page.FillAsync(".search-input", "Popescu");
await Page.WaitForLoadStateAsync(LoadState.NetworkIdle); // Wait for API response
```

### **4. Use Auto-Retry with Playwright Assertions**

```csharp
// ✅ Playwright automatically retries for 5 seconds
await Expect(Page.Locator(".grid-container")).ToBeVisibleAsync();

// ❌ Manual assertion (no retry)
Assert.True(await Page.IsVisibleAsync(".grid-container"));
```

---

## 🔄 **CI/CD Integration**

### **GitHub Actions Example**

```yaml
# .github/workflows/e2e-tests.yml
name: E2E Tests

on: [push, pull_request]

jobs:
  e2e-tests:
    runs-on: windows-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --configuration Release
      
      - name: Install Playwright browsers
        run: |
          cd ValyanClinic.Tests\bin\Release\net10.0
          .\playwright.ps1 install chromium
      
      - name: Start Blazor App (background)
        run: |
          cd ValyanClinic
          Start-Process -NoNewWindow dotnet -ArgumentList "run --launch-profile https"
          Start-Sleep -Seconds 10
      
      - name: Run E2E tests
        run: dotnet test --filter "FullyQualifiedName~E2ETests" --configuration Release
      
      - name: Upload test videos (on failure)
        if: failure()
        uses: actions/upload-artifact@v4
        with:
          name: e2e-test-videos
          path: ValyanClinic.Tests\bin\Release\net10.0\videos\
```

---

## 📊 **Test Metrics**

| Metric | Value | Target |
|--------|-------|--------|
| Total E2E Tests | 13 | 20+ (expand coverage) |
| Pass Rate | 100% | 100% |
| Average Duration | ~3.3s per test | <5s |
| Total Suite Duration | ~43s | <60s |
| Browser | Chromium | Multi-browser |

---

## 🚀 **Future Improvements**

- [ ] **Multi-browser testing** (Firefox, WebKit)
- [ ] **Parallel test execution** (reduce total time)
- [ ] **Visual regression testing** (screenshot comparison)
- [ ] **API mocking** (test error scenarios)
- [ ] **Performance metrics** (Lighthouse integration)
- [ ] **Accessibility testing** (axe-core integration)

---

## 📞 **Support**

**Framework Documentation:** https://playwright.dev/dotnet/  
**ValyanClinic E2E Tests Location:** `ValyanClinic.Tests/Integration/`  
**Issues:** Report to development team

---

*E2E Testing Infrastructure - ValyanClinic Medical Management System*
