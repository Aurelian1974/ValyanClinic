# 📱 PWA Implementation - Valyan Clinic

Implementare completă Progressive Web App (PWA) cu suport offline, service workers și sincronizare în background.

## ✅ Ce a fost implementat

### 1. **Manifest PWA** (`wwwroot/manifest.json`)
   - Metadata aplicație (nume, icoane, culori)
   - Shortcuts către pagini importante
   - Configurare standalone mode

### 2. **Service Workers**
   - **Development** (`service-worker.js`) - cu logging detaliat
   - **Production** (`service-worker.published.js`) - optimizat, cache agresiv
   - Strategii de cache:
     - **Cache-First**: Static assets (CSS, JS, imagini)
     - **Network-First**: API calls și pagini HTML
     - **Stale-While-Revalidate**: Conținut dinamic

### 3. **Offline Sync** (`js/offline-sync.js`)
   - IndexedDB pentru stocare locală
   - Queue pentru request-uri offline
   - Auto-sync când revine conexiunea
   - Retry logic (3 încercări)

### 4. **PWA Installer** (`js/pwa-installer.js`)
   - Install prompt management
   - Update notifications
   - Background sync registration
   - Push notifications support

### 5. **Blazor Service** (`Services/PWAService.cs`)
   - C# interop pentru JavaScript
   - Events pentru Blazor components
   - Helpers pentru IndexedDB operations

### 6. **UI Components**
   - `PWAInstallButton.razor` - Exemplu de install button
   - Offline indicator
   - Sync status display

---

## 🚀 Cum să folosești

### Pasul 1: Adaugă icon-urile PWA

**IMPORTANT**: Aplicația are nevoie de 2 icon-uri pentru a funcționa complet:

1. `wwwroot/icon-192.png` (192x192 px)
2. `wwwroot/icon-512.png` (512x512 px)

Vezi `wwwroot/PWA-ICONS-README.md` pentru detalii despre cum să le creezi.

**Quick placeholder** (pentru testare):
```bash
cd ValyanClinic/wwwroot
# Folosește favicon existent ca placeholder
cp favicon.png icon-192.png
cp favicon.png icon-512.png
```

### Pasul 2: Adaugă componenta de instalare

În `MainLayout.razor` sau unde vrei să apară install prompt:

```razor
@using ValyanClinic.Components.Shared

<PWAInstallButton />
```

### Pasul 3: Build și rulează

```bash
dotnet build
dotnet run
```

### Pasul 4: Testează PWA

1. **Desktop (Chrome/Edge)**:
   - Navighează la aplicație
   - Click pe iconul "Install" din address bar
   - SAU click pe butonul de instalare din UI

2. **Mobile**:
   - Navighează la aplicație (TREBUIE HTTPS sau localhost)
   - Menu → "Add to Home Screen"

3. **Verificare instalare**:
   - F12 → Application → Manifest
   - F12 → Application → Service Workers
   - Lighthouse audit (PWA score)

---

## 💻 Utilizare în cod

### Injectează PWAService

```razor
@inject PWAService PWA

@code {
    protected override async Task OnInitializedAsync()
    {
        await PWA.InitializeAsync();

        // Check status
        var status = await PWA.GetStatusAsync();
        if (status.IsInstalled)
        {
            Console.WriteLine("App is installed!");
        }
    }
}
```

### Instalează PWA programatic

```csharp
var installed = await PWA.InstallAsync();
if (installed)
{
    // Success!
}
```

### Verifică dacă e online

```csharp
var isOnline = await PWA.IsOnlineAsync();
if (!isOnline)
{
    // Show offline UI
}
```

### Queue request pentru offline

```csharp
await PWA.QueueRequestAsync(
    type: "create_pacient",
    endpoint: "pacienti",
    method: "POST",
    data: pacientData
);
```

### Salvează date în IndexedDB

```csharp
// Save
await PWA.StoreDataAsync("pacienti", pacient);

// Get
var pacient = await PWA.GetDataAsync<Pacient>("pacienti", pacientId);

// Get all
var pacienti = await PWA.GetAllDataAsync<Pacient>("pacienti");

// Delete
await PWA.DeleteDataAsync("pacienti", pacientId);
```

### Listen pentru evenimente

```csharp
PWA.OnSyncComplete += async (sender, args) =>
{
    Console.WriteLine($"Synced {args.Success} items, {args.Failed} failed");
    await InvokeAsync(StateHasChanged);
};

PWA.OnAppInstalled += (sender, args) =>
{
    Console.WriteLine("App installed!");
};
```

### Afișează notificări

```csharp
// Request permission
var granted = await PWA.RequestNotificationPermissionAsync();

if (granted)
{
    await PWA.ShowNotificationAsync("Consultație nouă", new NotificationOptions
    {
        Body = "Pacient X la ora 10:00",
        Icon = "/icon-192.png",
        Data = new { consultationId = 123 }
    });
}
```

---

## 🎯 Use Cases

### 1. Salvare consultație offline

```csharp
@inject PWAService PWA

private async Task SalveazaConsultatie()
{
    var isOnline = await PWA.IsOnlineAsync();

    if (isOnline)
    {
        // Normal API call
        await Http.PostAsJsonAsync("/api/consultatii", consultatie);
    }
    else
    {
        // Queue pentru sync ulterior
        await PWA.QueueRequestAsync(
            "create_consultation",
            "consultatii",
            "POST",
            consultatie
        );

        // Save local pentru vizualizare
        await PWA.StoreDataAsync("consultatii", consultatie);

        ToastService.ShowInfo("Consultație salvată offline. Va fi sincronizată automat.");
    }
}
```

### 2. Cache pacienți pentru acces rapid

```csharp
private async Task<List<Pacient>> IncarcaPacienti()
{
    var isOnline = await PWA.IsOnlineAsync();

    if (isOnline)
    {
        // Fetch de la server
        var pacienti = await Http.GetFromJsonAsync<List<Pacient>>("/api/pacienti");

        // Cache local
        await PWA.StoreDataAsync("pacienti", pacienti);

        return pacienti;
    }
    else
    {
        // Load din cache
        var pacienti = await PWA.GetAllDataAsync<Pacient>("pacienti");
        return pacienti ?? new List<Pacient>();
    }
}
```

### 3. Sync status indicator

```razor
@inject PWAService PWA

<div class="sync-status">
    @if (_queueCount > 0)
    {
        <span class="badge badge-warning">
            @_queueCount în așteptare de sincronizare
        </span>
        <button @onclick="SyncNow">Sincronizează acum</button>
    }
</div>

@code {
    private int _queueCount;

    protected override async Task OnInitializedAsync()
    {
        await UpdateQueueStatus();

        PWA.OnSyncComplete += async (s, e) =>
        {
            await UpdateQueueStatus();
            await InvokeAsync(StateHasChanged);
        };
    }

    private async Task UpdateQueueStatus()
    {
        var status = await PWA.GetQueueStatusAsync();
        _queueCount = status?.Count ?? 0;
    }

    private async Task SyncNow()
    {
        await PWA.SyncNowAsync();
    }
}
```

---

## 🔧 Configurare avansată

### Personalizare cache strategy

Editează `service-worker.published.js`:

```javascript
// Adaugă mai multe rute la cache
const CACHE_ASSETS = [
  '/',
  '/css/app.css',
  // ... adaugă mai multe
];

// Preîncarcă rute populare
async function precachePopularRoutes() {
  const routes = [
    '/consultatii',
    '/vizualizarepacienti',
    '/calendar',
    '/statistici'  // ← Adaugă aici
  ];
  // ...
}
```

### Actualizare versiune cache

În `service-worker.published.js`, schimbă versiunea:

```javascript
const CACHE_NAME = 'valyan-clinic-v1.0.1'; // ← Incrementează
```

Apoi rebuild și refresh pentru a actualiza cache-ul.

### Timeout pentru API calls

```javascript
// În service-worker.published.js
event.respondWith(networkFirstWithTimeout(request, 3000)); // 3 secunde
```

---

## 🧪 Testing

### Testare offline

1. **Chrome DevTools**:
   - F12 → Network → Offline (checkbox)
   - Refresh pagina - ar trebui să funcționeze

2. **Lighthouse**:
   - F12 → Lighthouse → Run audit
   - PWA score ar trebui 90+

3. **Service Worker**:
   - F12 → Application → Service Workers
   - Verifică "Status: activated and is running"

### Testare sync

```javascript
// În consolă
offlineSync.queueRequest('test', 'test-endpoint', 'POST', { test: true });

// Check queue
offlineSync.getQueueStatus().then(console.log);

// Force sync
offlineSync.syncNow();
```

### Debug service worker

```javascript
// În service-worker.js, adaugă mai mult logging
console.log('[SW] Cache hit:', request.url);
console.log('[SW] Network response:', response.status);
```

---

## 🐛 Troubleshooting

### PWA nu se instalează

**Cauze posibile**:
1. Nu e HTTPS (excepție: localhost)
2. Icon-urile lipsesc
3. Service Worker nu e înregistrat
4. Manifest invalid

**Soluții**:
```bash
# Check manifest
curl https://your-app.com/manifest.json

# Check service worker
# F12 → Application → Service Workers

# Verifică erori în consolă
```

### Service Worker nu se actualizează

```javascript
// Forțează update
navigator.serviceWorker.getRegistrations().then(registrations => {
  registrations.forEach(registration => {
    registration.update();
  });
});

// SAU clear cache complet
caches.keys().then(names => {
  names.forEach(name => caches.delete(name));
});
```

### Datele nu se sincronizează

1. **Check queue**:
```javascript
offlineSync.getQueueStatus().then(console.log);
```

2. **Check online status**:
```javascript
console.log('Online:', navigator.onLine);
```

3. **Force sync**:
```javascript
offlineSync.syncNow();
```

4. **Check background sync support**:
```javascript
console.log('Sync API:', 'sync' in navigator.serviceWorker.registration);
```

---

## 📊 Performance

### Cache sizes

- **Development**: ~2-5 MB (CSS, JS, fonts)
- **Production**: ~5-10 MB (cu rute pre-cached)
- **IndexedDB**: Limitat doar de storage disponibil (50-100 MB typical)

### Network savings

- **First load**: 100% network (download assets)
- **Subsequent loads**: ~80-90% cache hit (doar API calls)
- **Offline**: 100% cache (zero network)

### Best practices

1. **Cache doar ce e necesar** - evită cache excesiv
2. **Versioning** - incrementează versiunea la schimbări majore
3. **Cleanup** - șterge date vechi periodic
4. **Compression** - folosește gzip/brotli pentru assets

---

## 🔐 Security

### HTTPS Requirement

PWA funcționează **DOAR** pe HTTPS (excepție: localhost pentru dev).

**Production deployment**:
```bash
# În appsettings.Production.json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://*:443",
        "Certificate": {
          "Path": "/path/to/cert.pfx",
          "Password": "your-password"
        }
      }
    }
  }
}
```

### Data Security

- **Tokens**: Nu salva tokens în IndexedDB fără encryption
- **Sensitive data**: Folosește encryption pentru date medicale
- **Cache expiry**: Configurează TTL pentru date sensibile

```csharp
// Exemplu: Nu salva token în IndexedDB
// ❌ BAD
await PWA.StoreDataAsync("auth", new { token = authToken });

// ✅ GOOD - folosește sessionStorage sau secure cookies
await JSRuntime.InvokeVoidAsync("sessionStorage.setItem", "token", authToken);
```

---

## 📈 Monitoring

### Metrics importante

1. **Install rate**: Câți utilizatori instalează PWA
2. **Cache hit rate**: % requests servite din cache
3. **Offline usage**: Frecvență utilizare offline
4. **Sync queue size**: Număr mediu items în queue
5. **Sync success rate**: % sync reușit

### Logging

```csharp
// În PWAService.cs
Logger.LogInformation("PWA installed by user {UserId}", userId);
Logger.LogWarning("Offline queue has {Count} pending items", queueCount);
Logger.LogError("Sync failed for {Count} items", failedCount);
```

### Analytics

Integrează cu Google Analytics sau Application Insights:

```javascript
// În service-worker.js
self.addEventListener('install', () => {
  // Track install
  fetch('/api/analytics/pwa-install', { method: 'POST' });
});
```

---

## 🎓 Resurse

- [PWA Documentation - MDN](https://developer.mozilla.org/en-US/docs/Web/Progressive_web_apps)
- [Service Workers - Google](https://developers.google.com/web/fundamentals/primers/service-workers)
- [IndexedDB Guide](https://developer.mozilla.org/en-US/docs/Web/API/IndexedDB_API)
- [Workbox - Google's PWA Library](https://developers.google.com/web/tools/workbox)

---

## ❓ FAQ

**Q: Redis vs Service Workers - care e diferența?**
A: **Redis** = server-side cache (în RAM pe server), **Service Workers** = client-side cache (în browser). Pentru PWA offline, trebuie Service Workers. Redis nu ajută offline.

**Q: Funcționează pe iOS?**
A: Da, din iOS 11.3+. Limitări: No push notifications, storage limits mai mici.

**Q: Cât storage am disponibil?**
A: **Desktop**: ~60% din disk space disponibil. **Mobile**: varies by device, typical 50-100MB.

**Q: Pot folosi PWA împreună cu Redis?**
A: **DA!** Best practice:
- **Redis**: Cache API responses pe server
- **Service Worker**: Cache static assets în browser
- **IndexedDB**: Offline data storage

**Q: Ce se întâmplă dacă storage-ul e plin?**
A: Browser va șterge automat cache-ul cel mai vechi. Implementează cleanup periodic pentru control.

---

## 📝 TODO (Viitor)

- [ ] Push notifications pentru reminder consultații
- [ ] Background sync pentru photos/documents
- [ ] Offline-first CRUD pentru toate entitățile
- [ ] Share API pentru partajare documente
- [ ] Shortcuts dinamice către consultații recente
- [ ] Badging API pentru notificări neacceptate

---

**Implementat de**: Claude
**Data**: 2025-12-19
**Versiune**: 1.0.0
