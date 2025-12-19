// 📱 Valyan Clinic - PWA Installer & Service Worker Registration

class PWAInstaller {
    constructor() {
        this.deferredPrompt = null;
        this.isInstalled = false;
        this.swRegistration = null;

        this.init();
    }

    async init() {
        // Check dacă PWA e deja instalată
        if (window.matchMedia('(display-mode: standalone)').matches ||
            window.navigator.standalone === true) {
            this.isInstalled = true;
            console.log('[PWA] Running as installed app ✅');
        }

        // Înregistrează Service Worker
        if ('serviceWorker' in navigator) {
            this.registerServiceWorker();
        }

        // Listen pentru install prompt
        window.addEventListener('beforeinstallprompt', (e) => {
            e.preventDefault();
            this.deferredPrompt = e;
            this.showInstallButton();
            console.log('[PWA] Install prompt ready');
        });

        // Listen pentru app installed
        window.addEventListener('appinstalled', () => {
            this.isInstalled = true;
            this.deferredPrompt = null;
            this.hideInstallButton();
            console.log('[PWA] App installed ✅');
        });

        // Check pentru update la focus
        document.addEventListener('visibilitychange', () => {
            if (!document.hidden && this.swRegistration) {
                this.swRegistration.update();
            }
        });
    }

    // 🔧 Înregistrează Service Worker
    async registerServiceWorker() {
        try {
            const isDevelopment = location.hostname === 'localhost' ||
                                location.hostname === '127.0.0.1';

            const swPath = isDevelopment
                ? '/service-worker.js'
                : '/service-worker.published.js';

            this.swRegistration = await navigator.serviceWorker.register(swPath, {
                scope: '/'
            });

            console.log('[PWA] Service Worker registered:', swPath);

            // Listen pentru update
            this.swRegistration.addEventListener('updatefound', () => {
                const newWorker = this.swRegistration.installing;

                newWorker.addEventListener('statechange', () => {
                    if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
                        this.showUpdateNotification();
                    }
                });
            });

            // Check for updates
            setInterval(() => {
                this.swRegistration.update();
            }, 60 * 60 * 1000); // Check every hour

        } catch (error) {
            console.error('[PWA] Service Worker registration failed:', error);
        }
    }

    // 📥 Install PWA
    async installPWA() {
        if (!this.deferredPrompt) {
            console.warn('[PWA] No install prompt available');
            return false;
        }

        try {
            this.deferredPrompt.prompt();
            const { outcome } = await this.deferredPrompt.userChoice;

            if (outcome === 'accepted') {
                console.log('[PWA] User accepted installation');
                return true;
            } else {
                console.log('[PWA] User dismissed installation');
                return false;
            }
        } catch (error) {
            console.error('[PWA] Installation error:', error);
            return false;
        } finally {
            this.deferredPrompt = null;
        }
    }

    // 🔔 Show install button
    showInstallButton() {
        const installBtn = document.getElementById('pwa-install-btn');
        if (installBtn) {
            installBtn.style.display = 'block';
            installBtn.addEventListener('click', () => this.installPWA());
        }

        // Notifică Blazor
        if (window.DotNet && window.pwaInterop) {
            window.pwaInterop.invokeMethodAsync('OnInstallPromptReady');
        }
    }

    // 🙈 Hide install button
    hideInstallButton() {
        const installBtn = document.getElementById('pwa-install-btn');
        if (installBtn) {
            installBtn.style.display = 'none';
        }

        if (window.DotNet && window.pwaInterop) {
            window.pwaInterop.invokeMethodAsync('OnAppInstalled');
        }
    }

    // 🔄 Show update notification
    showUpdateNotification() {
        console.log('[PWA] Update available');

        const notification = document.createElement('div');
        notification.id = 'pwa-update-notification';
        notification.innerHTML = `
            <div style="
                position: fixed;
                bottom: 20px;
                right: 20px;
                background: #0066cc;
                color: white;
                padding: 1rem 1.5rem;
                border-radius: 0.5rem;
                box-shadow: 0 4px 12px rgba(0,0,0,0.15);
                z-index: 10000;
                display: flex;
                align-items: center;
                gap: 1rem;
            ">
                <span>O versiune nouă este disponibilă</span>
                <button id="pwa-reload-btn" style="
                    background: white;
                    color: #0066cc;
                    border: none;
                    padding: 0.5rem 1rem;
                    border-radius: 0.25rem;
                    font-weight: 600;
                    cursor: pointer;
                ">Actualizează</button>
                <button id="pwa-dismiss-btn" style="
                    background: transparent;
                    color: white;
                    border: 1px solid white;
                    padding: 0.5rem 1rem;
                    border-radius: 0.25rem;
                    cursor: pointer;
                ">Mai târziu</button>
            </div>
        `;

        document.body.appendChild(notification);

        document.getElementById('pwa-reload-btn').addEventListener('click', () => {
            window.location.reload();
        });

        document.getElementById('pwa-dismiss-btn').addEventListener('click', () => {
            notification.remove();
        });

        // Notifică Blazor
        if (window.DotNet && window.pwaInterop) {
            window.pwaInterop.invokeMethodAsync('OnUpdateAvailable');
        }
    }

    // 📊 Get PWA status
    getStatus() {
        return {
            isInstalled: this.isInstalled,
            canInstall: this.deferredPrompt !== null,
            hasServiceWorker: this.swRegistration !== null,
            isOnline: navigator.onLine
        };
    }

    // 🔔 Request notification permission
    async requestNotificationPermission() {
        if (!('Notification' in window)) {
            console.warn('[PWA] Notifications not supported');
            return false;
        }

        if (Notification.permission === 'granted') {
            return true;
        }

        if (Notification.permission !== 'denied') {
            const permission = await Notification.requestPermission();
            return permission === 'granted';
        }

        return false;
    }

    // 📬 Show notification
    async showNotification(title, options = {}) {
        if (Notification.permission !== 'granted') {
            console.warn('[PWA] Notification permission not granted');
            return;
        }

        const defaultOptions = {
            icon: '/icon-192.png',
            badge: '/icon-192.png',
            vibrate: [200, 100, 200],
            ...options
        };

        if (this.swRegistration) {
            await this.swRegistration.showNotification(title, defaultOptions);
        } else {
            new Notification(title, defaultOptions);
        }
    }

    // 🔄 Force sync
    async forceSync() {
        if (this.swRegistration && 'sync' in this.swRegistration) {
            try {
                await this.swRegistration.sync.register('sync-offline-queue');
                console.log('[PWA] Sync registered');
                return true;
            } catch (error) {
                console.error('[PWA] Sync registration failed:', error);
                return false;
            }
        }
        return false;
    }
}

// Initialize
const pwaInstaller = new PWAInstaller();

// Export pentru Blazor interop
window.pwaInstaller = {
    install: () => pwaInstaller.installPWA(),
    getStatus: () => pwaInstaller.getStatus(),
    requestNotifications: () => pwaInstaller.requestNotificationPermission(),
    showNotification: (title, options) => pwaInstaller.showNotification(title, options),
    forceSync: () => pwaInstaller.forceSync()
};

// Auto-install în development dacă e localhost
if (location.hostname === 'localhost' || location.hostname === '127.0.0.1') {
    console.log('[PWA] Development mode - PWA features enabled');
}

console.log('[PWA] Installer loaded ✅');
