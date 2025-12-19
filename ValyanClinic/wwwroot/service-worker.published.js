// 🚀 Valyan Clinic Service Worker - Production Version
// Optimized for performance with aggressive caching

const CACHE_NAME = 'valyan-clinic-v1.0.0';
const OFFLINE_URL = '/offline';
const SYNC_INTERVAL = 30000; // 30 secunde

// Extended cache pentru production
const CACHE_ASSETS = [
  '/',
  '/css/app.css',
  '/css/consultatie-tabs.css',
  '/js/sidebar-manager.js',
  '/js/auth-api.js',
  '/js/consultatii.js',
  '/js/fileDownload.js',
  '/js/dom-removal-monitor.js',
  '/js/navigationGuard.js',
  '/js/login-monitor.js',
  '/js/calendar-tooltips.js',
  '/manifest.json',
  '/icon-192.png',
  '/icon-512.png',
  '/favicon.png',
  '/_framework/blazor.web.js'
];

// Install - aggressive caching
self.addEventListener('install', event => {
  console.log('[SW-PROD] Installing...');

  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => {
        console.log('[SW-PROD] Caching all assets');
        return cache.addAll(CACHE_ASSETS.map(url =>
          new Request(url, { cache: 'reload' })
        ));
      })
      .then(() => self.skipWaiting())
      .catch(err => {
        console.error('[SW-PROD] Install failed:', err);
        // Nu blocăm instalarea pentru erori de cache
        return self.skipWaiting();
      })
  );
});

// Activate
self.addEventListener('activate', event => {
  console.log('[SW-PROD] Activating...');

  event.waitUntil(
    Promise.all([
      // Curăță cache-uri vechi
      caches.keys().then(names =>
        Promise.all(
          names
            .filter(name => name !== CACHE_NAME)
            .map(name => {
              console.log('[SW-PROD] Deleting cache:', name);
              return caches.delete(name);
            })
        )
      ),
      // Preîncarcă pagini populare
      precachePopularRoutes(),
      // Claim control
      self.clients.claim()
    ])
  );
});

// Preîncarcă rute populare
async function precachePopularRoutes() {
  const routes = [
    '/consultatii',
    '/vizualizarepacienti',
    '/calendar'
  ];

  const cache = await caches.open(CACHE_NAME);

  for (const route of routes) {
    try {
      const response = await fetch(route);
      if (response.ok) {
        await cache.put(route, response);
      }
    } catch (err) {
      console.log('[SW-PROD] Failed to precache:', route);
    }
  }
}

// Fetch strategy
self.addEventListener('fetch', event => {
  const { request } = event;
  const url = new URL(request.url);

  // Skip non-GET
  if (request.method !== 'GET') {
    return;
  }

  // Skip SignalR
  if (url.pathname.includes('/_blazor') ||
      url.pathname.includes('/negotiate') ||
      url.searchParams.has('id')) {
    return;
  }

  // API - Network first with timeout
  if (url.pathname.startsWith('/api/')) {
    event.respondWith(networkFirstWithTimeout(request, 5000));
    return;
  }

  // Static assets - Cache first
  if (isStaticAsset(url.pathname)) {
    event.respondWith(cacheFirst(request));
    return;
  }

  // HTML pages - Stale-while-revalidate
  event.respondWith(staleWhileRevalidate(request));
});

// 📡 Network First with Timeout
async function networkFirstWithTimeout(request, timeout) {
  try {
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), timeout);

    const response = await fetch(request, { signal: controller.signal });
    clearTimeout(timeoutId);

    if (response.ok) {
      const cache = await caches.open(CACHE_NAME);
      cache.put(request, response.clone());
    }

    return response;
  } catch (error) {
    const cached = await caches.match(request);
    if (cached) {
      return cached;
    }

    // Queue pentru sync ulterior
    if (request.method === 'POST' || request.method === 'PUT') {
      await queueRequest(request);
    }

    throw error;
  }
}

// 💾 Cache First
async function cacheFirst(request) {
  const cached = await caches.match(request);
  if (cached) {
    return cached;
  }

  try {
    const response = await fetch(request);
    if (response.ok) {
      const cache = await caches.open(CACHE_NAME);
      cache.put(request, response.clone());
    }
    return response;
  } catch (error) {
    console.error('[SW-PROD] Fetch failed:', request.url);
    throw error;
  }
}

// 🔄 Stale While Revalidate
async function staleWhileRevalidate(request) {
  const cached = await caches.match(request);

  const fetchPromise = fetch(request).then(response => {
    if (response.ok) {
      const cache = caches.open(CACHE_NAME);
      cache.then(c => c.put(request, response.clone()));
    }
    return response;
  }).catch(() => {
    // Fallback la offline page pentru HTML
    if (request.headers.get('accept')?.includes('text/html')) {
      return caches.match(OFFLINE_URL) || new Response(
        createOfflinePage(),
        { headers: { 'Content-Type': 'text/html' } }
      );
    }
    throw new Error('Network failed');
  });

  return cached || fetchPromise;
}

// Static asset checker
function isStaticAsset(pathname) {
  const extensions = [
    '.css', '.js', '.png', '.jpg', '.jpeg', '.svg', '.gif',
    '.woff', '.woff2', '.ttf', '.eot', '.ico', '.webp'
  ];

  return extensions.some(ext => pathname.endsWith(ext)) ||
         pathname.includes('/_framework/') ||
         pathname.includes('/_content/');
}

// 🔄 Background Sync
self.addEventListener('sync', event => {
  console.log('[SW-PROD] Sync event:', event.tag);

  if (event.tag === 'sync-offline-queue') {
    event.waitUntil(processOfflineQueue());
  }
});

// Process offline queue
async function processOfflineQueue() {
  try {
    const db = await openDB();
    const requests = await getAllFromStore(db, 'offlineQueue');

    let successCount = 0;
    let failCount = 0;

    for (const item of requests) {
      try {
        const response = await fetch(item.url, {
          method: item.method,
          headers: item.headers,
          body: item.body
        });

        if (response.ok) {
          await deleteFromStore(db, 'offlineQueue', item.id);
          successCount++;
        } else {
          failCount++;
        }
      } catch (error) {
        console.error('[SW-PROD] Sync failed for:', item.url, error);
        failCount++;
      }
    }

    // Notifică clienții
    const clients = await self.clients.matchAll();
    clients.forEach(client => {
      client.postMessage({
        type: 'SYNC_COMPLETE',
        success: successCount,
        failed: failCount,
        total: requests.length
      });
    });

    console.log(`[SW-PROD] Sync complete: ${successCount} success, ${failCount} failed`);
  } catch (error) {
    console.error('[SW-PROD] Queue processing failed:', error);
  }
}

// Queue request pentru sync ulterior
async function queueRequest(request) {
  try {
    const body = await request.clone().text();
    const db = await openDB();

    const item = {
      url: request.url,
      method: request.method,
      headers: Object.fromEntries(request.headers.entries()),
      body: body,
      timestamp: Date.now()
    };

    await addToStore(db, 'offlineQueue', item);
    console.log('[SW-PROD] Request queued:', request.url);

    // Înregistrează sync
    await self.registration.sync.register('sync-offline-queue');
  } catch (error) {
    console.error('[SW-PROD] Failed to queue request:', error);
  }
}

// IndexedDB helpers
function openDB() {
  return new Promise((resolve, reject) => {
    const request = indexedDB.open('ValyanClinicDB', 2);

    request.onerror = () => reject(request.error);
    request.onsuccess = () => resolve(request.result);

    request.onupgradeneeded = (event) => {
      const db = event.target.result;

      // Offline queue
      if (!db.objectStoreNames.contains('offlineQueue')) {
        db.createObjectStore('offlineQueue', { keyPath: 'id', autoIncrement: true });
      }

      // Cached data
      if (!db.objectStoreNames.contains('cachedData')) {
        const store = db.createObjectStore('cachedData', { keyPath: 'key' });
        store.createIndex('timestamp', 'timestamp', { unique: false });
      }
    };
  });
}

function getAllFromStore(db, storeName) {
  return new Promise((resolve, reject) => {
    const tx = db.transaction([storeName], 'readonly');
    const store = tx.objectStore(storeName);
    const request = store.getAll();

    request.onsuccess = () => resolve(request.result);
    request.onerror = () => reject(request.error);
  });
}

function addToStore(db, storeName, data) {
  return new Promise((resolve, reject) => {
    const tx = db.transaction([storeName], 'readwrite');
    const store = tx.objectStore(storeName);
    const request = store.add(data);

    request.onsuccess = () => resolve(request.result);
    request.onerror = () => reject(request.error);
  });
}

function deleteFromStore(db, storeName, key) {
  return new Promise((resolve, reject) => {
    const tx = db.transaction([storeName], 'readwrite');
    const store = tx.objectStore(storeName);
    const request = store.delete(key);

    request.onsuccess = () => resolve();
    request.onerror = () => reject(request.error);
  });
}

// Offline page
function createOfflinePage() {
  return `
    <!DOCTYPE html>
    <html lang="ro">
    <head>
      <meta charset="utf-8" />
      <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      <title>Offline - Valyan Clinic</title>
      <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
          font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
          background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
          color: white;
          min-height: 100vh;
          display: flex;
          align-items: center;
          justify-content: center;
          padding: 2rem;
        }
        .container {
          text-align: center;
          max-width: 500px;
        }
        .icon {
          font-size: 5rem;
          margin-bottom: 1.5rem;
          animation: pulse 2s ease-in-out infinite;
        }
        @keyframes pulse {
          0%, 100% { opacity: 1; }
          50% { opacity: 0.5; }
        }
        h1 {
          font-size: 2.5rem;
          margin-bottom: 1rem;
          font-weight: 700;
        }
        p {
          font-size: 1.2rem;
          margin-bottom: 0.75rem;
          opacity: 0.95;
        }
        .status {
          background: rgba(255, 255, 255, 0.2);
          padding: 1rem;
          border-radius: 0.75rem;
          margin: 2rem 0;
        }
        .btn {
          display: inline-block;
          margin-top: 1.5rem;
          padding: 1rem 2.5rem;
          font-size: 1.1rem;
          background: white;
          color: #667eea;
          border: none;
          border-radius: 0.75rem;
          cursor: pointer;
          font-weight: 700;
          text-decoration: none;
          transition: transform 0.2s;
        }
        .btn:hover {
          transform: scale(1.05);
        }
        .btn:active {
          transform: scale(0.95);
        }
      </style>
    </head>
    <body>
      <div class="container">
        <div class="icon">📡</div>
        <h1>Offline</h1>
        <p>Nu există conexiune la internet</p>
        <div class="status">
          <p>✅ Datele sunt salvate local</p>
          <p>🔄 Sincronizare automată la reconectare</p>
        </div>
        <button class="btn" onclick="location.reload()">Reîncearcă conexiunea</button>
      </div>
      <script>
        // Auto-reload când revine conexiunea
        window.addEventListener('online', () => {
          setTimeout(() => location.reload(), 1000);
        });
      </script>
    </body>
    </html>
  `;
}

// 📬 Push Notifications
self.addEventListener('push', event => {
  if (!event.data) return;

  const data = event.data.json();
  const options = {
    body: data.body || 'Notificare nouă',
    icon: '/icon-192.png',
    badge: '/icon-192.png',
    vibrate: [200, 100, 200],
    data: data.data || {},
    actions: data.actions || [],
    tag: data.tag || 'default',
    requireInteraction: data.requireInteraction || false
  };

  event.waitUntil(
    self.registration.showNotification(data.title || 'Valyan Clinic', options)
  );
});

self.addEventListener('notificationclick', event => {
  event.notification.close();

  if (event.action) {
    // Handle action clicks
    console.log('[SW-PROD] Notification action:', event.action);
  }

  event.waitUntil(
    clients.matchAll({ type: 'window', includeUncontrolled: true })
      .then(clientList => {
        // Focus pe window existent
        for (const client of clientList) {
          if ('focus' in client) {
            return client.focus();
          }
        }
        // Deschide window nou
        if (clients.openWindow) {
          return clients.openWindow(event.notification.data.url || '/');
        }
      })
  );
});

// Periodic cleanup
setInterval(async () => {
  try {
    const db = await openDB();
    const data = await getAllFromStore(db, 'cachedData');
    const now = Date.now();
    const maxAge = 7 * 24 * 60 * 60 * 1000; // 7 zile

    for (const item of data) {
      if (now - item.timestamp > maxAge) {
        await deleteFromStore(db, 'cachedData', item.key);
      }
    }
  } catch (error) {
    console.error('[SW-PROD] Cleanup failed:', error);
  }
}, 60 * 60 * 1000); // O dată pe oră

console.log('[SW-PROD] Service Worker loaded ✅');
