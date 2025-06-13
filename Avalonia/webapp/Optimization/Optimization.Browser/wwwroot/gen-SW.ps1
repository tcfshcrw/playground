# 設定根目錄與檔名
$ScriptPath = $MyInvocation.MyCommand.Path
$Root = Split-Path $ScriptPath -Parent
Set-Location $Root

$ServiceWorker = "service-worker.js"
$CacheName = "v1-ps-auto-cache"

# 遞迴尋找所有檔案，排除 service-worker.js 本身
$files = Get-ChildItem -Path $Root -File -Recurse | Where-Object { $_.Name -ne $ServiceWorker }

# 產生「不以 / 開頭」的相對路徑
$offlineUrls = $files | ForEach-Object {
    $relativePath = $_.FullName.Substring($Root.Length)
    $relativePath = $relativePath -replace "^[\\/]+", ""    # 移除開頭所有/或\
    $relativePath = $relativePath -replace "\\", "/"         # 統一用正斜線
    $relativePath
}

# 正確分行產生 OFFLINE_URLS
$offlineUrlsString = $offlineUrls | ForEach-Object { "  '$_',"} | Out-String

# 組成 Service Worker 內容
$sw = @"
const CACHE_NAME = '$CacheName';
const OFFLINE_URLS = [
$offlineUrlsString
];

self.addEventListener('install', event => {
  self.skipWaiting();
  event.waitUntil(
    caches.open(CACHE_NAME).then(cache => cache.addAll(OFFLINE_URLS))
  );
});

self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(keys =>
      Promise.all(
        keys.filter(key => key !== CACHE_NAME).map(key => caches.delete(key))
      )
    )
  );
  self.clients.claim();
});

self.addEventListener('fetch', event => {
  event.respondWith(
    caches.match(event.request, { ignoreSearch: true }).then(response => {
      return response || fetch(event.request);
    })
  );
});
"@

# 輸出 Service Worker 檔案
$sw | Set-Content -Encoding UTF8 $ServiceWorker

Write-Host ""
Write-Host "service-worker.js done"


