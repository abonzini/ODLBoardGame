const CACHE_NAME = 'odl-game-cache-v1';
const IMAGE_CACHE_NAME = 'odl-images-cache-v1';
const AUDIO_CACHE_NAME = 'odl-audio-cache-v1';

// Files to cache immediately
const CORE_ASSETS = [
  '/',
  '/index.html',
  '/src/main.jsx',
  '/src/App.jsx',
  '/src/App.css',
  '/src/index.css'
];

// Install event - cache core assets
self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then((cache) => cache.addAll(CORE_ASSETS))
  );
});

// Fetch event - handle image and audio caching
self.addEventListener('fetch', (event) => {
  const { request } = event;
  const url = new URL(request.url);

  // Handle image requests
  if (request.destination === 'image' || 
      url.pathname.includes('.png') || 
      url.pathname.includes('.jpg') || 
      url.pathname.includes('.jpeg') || 
      url.pathname.includes('.gif') || 
      url.pathname.includes('.webp')) {
    
    event.respondWith(
      caches.open(IMAGE_CACHE_NAME)
        .then((cache) => {
          return cache.match(request)
            .then((response) => {
              if (response) {
                // Return cached image
                return response;
              }
              
              // Fetch from network and cache
              return fetch(request)
                .then((networkResponse) => {
                  if (networkResponse.status === 200) {
                    cache.put(request, networkResponse.clone());
                  }
                  return networkResponse;
                })
                .catch(() => {
                  // Return a placeholder if network fails
                  return new Response('', {
                    status: 404,
                    statusText: 'Image not found'
                  });
                });
            });
        })
    );
  }
  // Handle audio requests
  else if (request.destination === 'audio' || 
           url.pathname.includes('.wav') || 
           url.pathname.includes('.mp3') || 
           url.pathname.includes('.ogg') || 
           url.pathname.includes('.m4a')) {
    
    event.respondWith(
      caches.open(AUDIO_CACHE_NAME)
        .then((cache) => {
          return cache.match(request)
            .then((response) => {
              if (response) {
                // Return cached audio
                return response;
              }
              
              // Fetch from network and cache
              return fetch(request)
                .then((networkResponse) => {
                  if (networkResponse.status === 200) {
                    cache.put(request, networkResponse.clone());
                  }
                  return networkResponse;
                })
                .catch(() => {
                  // Return a placeholder if network fails
                  return new Response('', {
                    status: 404,
                    statusText: 'Audio not found'
                  });
                });
            });
        })
    );
  } else {
    // For non-image/audio requests, try network first, then cache
    event.respondWith(
      fetch(request)
        .catch(() => {
          return caches.match(request);
        })
    );
  }
});

// Activate event - clean up old caches
self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys()
      .then((cacheNames) => {
        return Promise.all(
          cacheNames.map((cacheName) => {
            if (cacheName !== CACHE_NAME && 
                cacheName !== IMAGE_CACHE_NAME && 
                cacheName !== AUDIO_CACHE_NAME) {
              return caches.delete(cacheName);
            }
          })
        );
      })
  );
}); 