// 🔧 Valyan Clinic Service Worker - Development Version
// Cache-first strategy pentru assets statice, Network-first pentru API

const CACHE_NAME = 'valyan-clinic-cache-v1';
const OFFLINE_URL = '/offline';

// Assets critice pentru funcționare offline
const CRITICAL_ASSETS = [
  '/',
  '/css/app.css',
  '/css/consultatie-tabs.css',
  '/js/sidebar-manager.js',
  '/js/auth-api.js',
  '/js/consultatii.js',
  '/manifest.json',
  '/icon-192.png',
  '/icon-512.png',
  '/_framework/blazor.web.js'
];

// Install event - pre-cache assets critice
self.addEventListener('install', event => {
  console.log('[SW] Installing service worker...');

  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => {
        console.log('[SW] Caching critical assets');
        return cache.addAll(CRITICAL_ASSETS.map(url => new Request(url, { cache: 'reload' })));
      })
      .then(() => self.skipWaiting())
      .catch(err => console.error('[SW] Install failed:', err))
  );
});

// Activate event - curăță cache-uri vechi
self.addEventListener('activate', event => {
  console.log('[SW] Activating service worker...');

  event.waitUntil(
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames
          .filter(name => name !== CACHE_NAME)
          .map(name => {
            console.log('[SW] Deleting old cache:', name);
            return caches.delete(name);
          })
      );
    })
    .then(() => self.clients.claim())
  );
});

// Fetch event - strategii de cache
self.addEventListener('fetch', event => {
  const { request } = event;
  const url = new URL(request.url);

  // Skip pentru requests non-GET
  if (request.method !== 'GET') {
    return;
  }

  // Skip pentru SignalR connections
  if (url.pathname.includes('/_blazor') || url.pathname.includes('/negotiate')) {
    return;
  }

  // Skip pentru API calls - folosim network-first
  if (url.pathname.startsWith('/api/')) {
    event.respondWith(networkFirstStrategy(request));
    return;
  }

  // Cache-first pentru static assets
  if (isStaticAsset(url.pathname)) {
    event.respondWith(cacheFirstStrategy(request));
    return;
  }

  // Network-first pentru HTML pages
  event.respondWith(networkFirstStrategy(request));
});

// 📡 Network-First Strategy (pentru API și pagini dinamice)
async function networkFirstStrategy(request) {
  try {
    const networkResponse = await fetch(request);

    // Cache doar răspunsuri OK
    if (networkResponse.ok) {
      const cache = await caches.open(CACHE_NAME);
      cache.put(request, networkResponse.clone());
    }

    return networkResponse;
  } catch (error) {
    console.log('[SW] Network failed, trying cache:', request.url);
    const cachedResponse = await caches.match(request);

    if (cachedResponse) {
      return cachedResponse;
    }

    // Fallback pentru HTML - arată pagină offline
    if (request.headers.get('accept').includes('text/html')) {
      return caches.match(OFFLINE_URL) || new Response(
        createOfflinePage(),
        { headers: { 'Content-Type': 'text/html' } }
      );
    }

    throw error;
  }
}

// 💾 Cache-First Strategy (pentru static assets)
async function cacheFirstStrategy(request) {
  const cachedResponse = await caches.match(request);

  if (cachedResponse) {
    return cachedResponse;
  }

  try {
    const networkResponse = await fetch(request);

    if (networkResponse.ok) {
      const cache = await caches.open(CACHE_NAME);
      cache.put(request, networkResponse.clone());
    }

    return networkResponse;
  } catch (error) {
    console.error('[SW] Failed to fetch:', request.url, error);
    throw error;
  }
}

// Helper: verifică dacă e static asset
function isStaticAsset(pathname) {
  const staticExtensions = ['.css', '.js', '.png', '.jpg', '.jpeg', '.svg', '.woff', '.woff2', '.ttf', '.eot'];
  return staticExtensions.some(ext => pathname.endsWith(ext)) ||
         pathname.includes('/_framework/') ||
         pathname.includes('/_content/');
}

// 🔄 Background Sync pentru queue de requests offline
self.addEventListener('sync', event => {
  console.log('[SW] Background sync triggered:', event.tag);

  if (event.tag === 'sync-offline-data') {
    event.waitUntil(syncOfflineData());
  }
});

// Sincronizează datele offline când revine conexiunea
async function syncOfflineData() {
  console.log('[SW] Syncing offline data...');

  try {
    // Ia toate request-urile din IndexedDB queue
    const db = await openDatabase();
    const requests = await getQueuedRequests(db);

    // Trimite toate request-urile
    for (const req of requests) {
      try {
        await fetch(req.url, {
          method: req.method,
          headers: req.headers,
          body: req.body
        });

        // Șterge din queue după succes
        await deleteQueuedRequest(db, req.id);
        console.log('[SW] Synced:', req.url);
      } catch (error) {
        console.error('[SW] Failed to sync:', req.url, error);
      }
    }

    // Notifică clientul că sync-ul s-a terminat
    const clients = await self.clients.matchAll();
    clients.forEach(client => {
      client.postMessage({ type: 'SYNC_COMPLETE', count: requests.length });
    });
  } catch (error) {
    console.error('[SW] Sync failed:', error);
  }
}

// IndexedDB helpers
function openDatabase() {
  return new Promise((resolve, reject) => {
    const request = indexedDB.open('ValyanClinicDB', 1);

    request.onerror = () => reject(request.error);
    request.onsuccess = () => resolve(request.result);

    request.onupgradeneeded = (event) => {
      const db = event.target.result;
      if (!db.objectStoreNames.contains('offlineQueue')) {
        db.createObjectStore('offlineQueue', { keyPath: 'id', autoIncrement: true });
      }
    };
  });
}

async function getQueuedRequests(db) {
  return new Promise((resolve, reject) => {
    const transaction = db.transaction(['offlineQueue'], 'readonly');
    const store = transaction.objectStore('offlineQueue');
    const request = store.getAll();

    request.onsuccess = () => resolve(request.result);
    request.onerror = () => reject(request.error);
  });
}

async function deleteQueuedRequest(db, id) {
  return new Promise((resolve, reject) => {
    const transaction = db.transaction(['offlineQueue'], 'readwrite');
    const store = transaction.objectStore('offlineQueue');
    const request = store.delete(id);

    request.onsuccess = () => resolve();
    request.onerror = () => reject(request.error);
  });
}

// Pagină offline HTML
function createOfflinePage() {
  return `
    <!DOCTYPE html>
    <html lang="ro">
    <head>
      <meta charset="utf-8" />
      <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      <title>Offline - Valyan Clinic</title>
      <style>
        body {
          font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
          display: flex;
          align-items: center;
          justify-content: center;
          min-height: 100vh;
          margin: 0;
          background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
          color: white;
        }
        .container {
          text-align: center;
          padding: 2rem;
        }
        .icon {
          font-size: 4rem;
          margin-bottom: 1rem;
        }
        h1 {
          font-size: 2rem;
          margin: 0 0 1rem 0;
        }
        p {
          font-size: 1.1rem;
          opacity: 0.9;
          margin: 0.5rem 0;
        }
        .retry-btn {
          margin-top: 2rem;
          padding: 0.75rem 2rem;
          font-size: 1rem;
          background: white;
          color: #667eea;
          border: none;
          border-radius: 0.5rem;
          cursor: pointer;
          font-weight: 600;
        }
        .retry-btn:hover {
          transform: scale(1.05);
        }
      </style>
    </head>
    <body>
      <div class="container">
        <div class="icon">📡</div>
        <h1>Offline</h1>
        <p>Nu există conexiune la internet</p>
        <p>Aplicația va sincroniza datele când revine conexiunea</p>
        <button class="retry-btn" onclick="location.reload()">Reîncearcă</button>
      </div>
    </body>
    </html>
  `;
}

// 📬 Push notifications (pentru viitor)
self.addEventListener('push', event => {
  if (!event.data) return;

  const data = event.data.json();
  const options = {
    body: data.body,
    icon: '/icon-192.png',
    badge: '/icon-192.png',
    vibrate: [200, 100, 200],
    data: data.data
  };

  event.waitUntil(
    self.registration.showNotification(data.title, options)
  );
});

self.addEventListener('notificationclick', event => {
  event.notification.close();
  event.waitUntil(
    clients.openWindow(event.notification.data.url || '/')
  );
});

console.log('[SW] Service Worker loaded ✅');
