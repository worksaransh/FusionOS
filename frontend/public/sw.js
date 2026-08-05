// Minimal PWA app-shell cache — deliberately does NOT cache /api/* responses.
// This app shows live financial/operational data; caching API responses would
// mean a user offline could see stale numbers with no indication they're stale.
// The only thing this caches is the static shell so the app can at least LOAD
// (and show its own "offline" UI) without a network connection.
const CACHE_NAME = 'fusionos-shell-v1';
const SHELL_ASSETS = ['/', '/manifest.json', '/icon-192.png', '/icon-512.png', '/favicon.svg'];

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => cache.addAll(SHELL_ASSETS))
  );
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((keys) =>
      Promise.all(keys.filter((key) => key !== CACHE_NAME).map((key) => caches.delete(key)))
    )
  );
});

self.addEventListener('fetch', (event) => {
  const url = new URL(event.request.url);

  // Never intercept API calls — always go to the network, and let a real
  // network failure surface as a real error rather than stale cached data.
  if (url.pathname.startsWith('/api/')) return;

  // Only handle same-origin GET requests for the app shell itself.
  if (event.request.method !== 'GET' || url.origin !== self.location.origin) return;

  event.respondWith(
    caches.match(event.request).then((cached) => {
      const network = fetch(event.request)
        .then((response) => {
          if (response.ok) {
            const copy = response.clone();
            caches.open(CACHE_NAME).then((cache) => cache.put(event.request, copy));
          }
          return response;
        })
        .catch(() => cached);
      return cached || network;
    })
  );
});
