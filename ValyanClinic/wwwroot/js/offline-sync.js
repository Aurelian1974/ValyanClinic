// 🔄 Valyan Clinic - Offline Sync Manager
// Gestionează stocarea locală și sincronizarea datelor

class OfflineSyncManager {
    constructor() {
        this.dbName = 'ValyanClinicDB';
        this.dbVersion = 2;
        this.db = null;
        this.isOnline = navigator.onLine;
        this.syncInProgress = false;

        this.init();
    }

    async init() {
        // Deschide database
        this.db = await this.openDatabase();

        // Listen pentru evenimente online/offline
        window.addEventListener('online', () => this.handleOnline());
        window.addEventListener('offline', () => this.handleOffline());

        // Listen pentru mesaje de la Service Worker
        if ('serviceWorker' in navigator && navigator.serviceWorker.controller) {
            navigator.serviceWorker.addEventListener('message', (event) => {
                this.handleServiceWorkerMessage(event.data);
            });
        }

        // Sync periodic (dacă e online)
        setInterval(() => {
            if (this.isOnline && !this.syncInProgress) {
                this.syncOfflineData();
            }
        }, 60000); // 1 minut

        console.log('[OfflineSync] Initialized ✅');
    }

    // 🗄️ Database Operations
    openDatabase() {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(this.dbName, this.dbVersion);

            request.onerror = () => {
                console.error('[OfflineSync] DB open error:', request.error);
                reject(request.error);
            };

            request.onsuccess = () => {
                console.log('[OfflineSync] DB opened');
                resolve(request.result);
            };

            request.onupgradeneeded = (event) => {
                const db = event.target.result;

                // Store pentru request-uri offline
                if (!db.objectStoreNames.contains('offlineQueue')) {
                    const queueStore = db.createObjectStore('offlineQueue', {
                        keyPath: 'id',
                        autoIncrement: true
                    });
                    queueStore.createIndex('timestamp', 'timestamp', { unique: false });
                    queueStore.createIndex('type', 'type', { unique: false });
                }

                // Store pentru date cached
                if (!db.objectStoreNames.contains('cachedData')) {
                    const dataStore = db.createObjectStore('cachedData', { keyPath: 'key' });
                    dataStore.createIndex('timestamp', 'timestamp', { unique: false });
                    dataStore.createIndex('category', 'category', { unique: false });
                }

                // Store pentru pacienți offline
                if (!db.objectStoreNames.contains('pacienti')) {
                    const pacientiStore = db.createObjectStore('pacienti', { keyPath: 'id' });
                    pacientiStore.createIndex('nume', 'nume', { unique: false });
                    pacientiStore.createIndex('cnp', 'cnp', { unique: true });
                }

                // Store pentru consultații offline
                if (!db.objectStoreNames.contains('consultatii')) {
                    const consultatiiStore = db.createObjectStore('consultatii', { keyPath: 'id' });
                    consultatiiStore.createIndex('pacientId', 'pacientId', { unique: false });
                    consultatiiStore.createIndex('timestamp', 'timestamp', { unique: false });
                }

                console.log('[OfflineSync] DB schema created');
            };
        });
    }

    // 💾 Store data
    async storeData(storeName, data) {
        try {
            const tx = this.db.transaction([storeName], 'readwrite');
            const store = tx.objectStore(storeName);

            if (Array.isArray(data)) {
                for (const item of data) {
                    await store.put(item);
                }
            } else {
                await store.put(data);
            }

            await tx.complete;
            console.log(`[OfflineSync] Data stored in ${storeName}`);
            return true;
        } catch (error) {
            console.error('[OfflineSync] Store error:', error);
            return false;
        }
    }

    // 📖 Get data
    async getData(storeName, key) {
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction([storeName], 'readonly');
            const store = tx.objectStore(storeName);
            const request = store.get(key);

            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    }

    // 📋 Get all data
    async getAllData(storeName) {
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction([storeName], 'readonly');
            const store = tx.objectStore(storeName);
            const request = store.getAll();

            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    }

    // 🗑️ Delete data
    async deleteData(storeName, key) {
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction([storeName], 'readwrite');
            const store = tx.objectStore(storeName);
            const request = store.delete(key);

            request.onsuccess = () => resolve();
            request.onerror = () => reject(request.error);
        });
    }

    // 🔍 Query by index
    async queryByIndex(storeName, indexName, value) {
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction([storeName], 'readonly');
            const store = tx.objectStore(storeName);
            const index = store.index(indexName);
            const request = index.getAll(value);

            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    }

    // 📤 Queue request pentru offline
    async queueRequest(type, endpoint, method, data) {
        const item = {
            type: type,
            endpoint: endpoint,
            method: method,
            data: data,
            timestamp: Date.now(),
            retries: 0
        };

        try {
            const tx = this.db.transaction(['offlineQueue'], 'readwrite');
            const store = tx.objectStore('offlineQueue');
            const id = await store.add(item);

            console.log(`[OfflineSync] Request queued: ${type} - ID: ${id}`);

            // Notifică UI
            this.notifyUI('request_queued', { id, type });

            // Încearcă sync dacă e online
            if (this.isOnline) {
                setTimeout(() => this.syncOfflineData(), 1000);
            }

            return id;
        } catch (error) {
            console.error('[OfflineSync] Queue error:', error);
            throw error;
        }
    }

    // 🔄 Sync offline data
    async syncOfflineData() {
        if (this.syncInProgress || !this.isOnline) {
            return;
        }

        this.syncInProgress = true;
        console.log('[OfflineSync] Starting sync...');

        try {
            const queue = await this.getAllData('offlineQueue');

            if (queue.length === 0) {
                console.log('[OfflineSync] Queue empty');
                return;
            }

            let successCount = 0;
            let failCount = 0;

            for (const item of queue) {
                try {
                    const response = await fetch(`/api/${item.endpoint}`, {
                        method: item.method,
                        headers: {
                            'Content-Type': 'application/json',
                            'Authorization': `Bearer ${this.getAuthToken()}`
                        },
                        body: JSON.stringify(item.data)
                    });

                    if (response.ok) {
                        await this.deleteData('offlineQueue', item.id);
                        successCount++;
                        console.log(`[OfflineSync] Synced: ${item.type}`);
                    } else {
                        failCount++;
                        console.warn(`[OfflineSync] Failed: ${item.type} - Status: ${response.status}`);

                        // Increment retries
                        item.retries++;
                        if (item.retries < 3) {
                            await this.storeData('offlineQueue', item);
                        } else {
                            // Șterge după 3 încercări
                            await this.deleteData('offlineQueue', item.id);
                            console.error(`[OfflineSync] Abandoned after 3 retries: ${item.type}`);
                        }
                    }
                } catch (error) {
                    failCount++;
                    console.error(`[OfflineSync] Sync error for ${item.type}:`, error);
                }
            }

            // Notifică UI
            this.notifyUI('sync_complete', {
                success: successCount,
                failed: failCount,
                total: queue.length
            });

            console.log(`[OfflineSync] Sync complete: ${successCount} success, ${failCount} failed`);
        } catch (error) {
            console.error('[OfflineSync] Sync failed:', error);
        } finally {
            this.syncInProgress = false;
        }
    }

    // 🌐 Online/Offline handlers
    handleOnline() {
        console.log('[OfflineSync] Online ✅');
        this.isOnline = true;
        this.notifyUI('status_changed', { online: true });
        this.syncOfflineData();
    }

    handleOffline() {
        console.log('[OfflineSync] Offline ⚠️');
        this.isOnline = false;
        this.notifyUI('status_changed', { online: false });
    }

    // 📬 Service Worker messages
    handleServiceWorkerMessage(data) {
        console.log('[OfflineSync] SW message:', data);

        switch (data.type) {
            case 'SYNC_COMPLETE':
                this.notifyUI('sync_complete', data);
                break;
            case 'CACHE_UPDATED':
                this.notifyUI('cache_updated', data);
                break;
        }
    }

    // 🔔 Notify UI (Blazor)
    notifyUI(eventType, data) {
        if (window.DotNet && window.offlineSyncInterop) {
            window.offlineSyncInterop.invokeMethodAsync('OnOfflineEvent', eventType, data);
        }

        // Dispatch custom event
        window.dispatchEvent(new CustomEvent('offlineSync', {
            detail: { type: eventType, data: data }
        }));
    }

    // 🔑 Get auth token
    getAuthToken() {
        return localStorage.getItem('authToken') || '';
    }

    // 📊 Get queue status
    async getQueueStatus() {
        const queue = await this.getAllData('offlineQueue');
        return {
            count: queue.length,
            items: queue
        };
    }

    // 🧹 Clear old data
    async clearOldData(maxAgeDays = 30) {
        const maxAge = maxAgeDays * 24 * 60 * 60 * 1000;
        const now = Date.now();

        const stores = ['cachedData', 'consultatii'];

        for (const storeName of stores) {
            try {
                const data = await this.getAllData(storeName);

                for (const item of data) {
                    if (item.timestamp && (now - item.timestamp > maxAge)) {
                        await this.deleteData(storeName, item.id || item.key);
                    }
                }

                console.log(`[OfflineSync] Cleaned old data from ${storeName}`);
            } catch (error) {
                console.error(`[OfflineSync] Cleanup error for ${storeName}:`, error);
            }
        }
    }
}

// Initialize global instance
window.offlineSyncManager = new OfflineSyncManager();

// Export pentru Blazor interop
window.offlineSync = {
    queueRequest: (type, endpoint, method, data) =>
        window.offlineSyncManager.queueRequest(type, endpoint, method, data),

    getQueueStatus: () =>
        window.offlineSyncManager.getQueueStatus(),

    syncNow: () =>
        window.offlineSyncManager.syncOfflineData(),

    storeData: (storeName, data) =>
        window.offlineSyncManager.storeData(storeName, data),

    getData: (storeName, key) =>
        window.offlineSyncManager.getData(storeName, key),

    getAllData: (storeName) =>
        window.offlineSyncManager.getAllData(storeName),

    deleteData: (storeName, key) =>
        window.offlineSyncManager.deleteData(storeName, key),

    isOnline: () =>
        window.offlineSyncManager.isOnline
};

console.log('[OfflineSync] Module loaded ✅');
