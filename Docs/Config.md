# Configuration

Here's a little description of what each setting does. Note that these options may change!
```json
{
  "Version": "latest", //one of 'latest','latest-ptb' or 'latest-canary'
  "AssetCacheLocation": "assets_cache/$VERSION/", //location for asset cache, $VERSION changes based on the option selected above
  "Client": {
    "DebugOptions": {
      "DumpWebsocketTrafficToBrowserConsole": false, //dump websocket data to browser console, used for debug
      "DumpWebsocketTraffic": false, //dump websocket data to server, used for debug
      "Patches": {
        "SomePatch": true, //defines wether a certain client patch is enabled. We recommend using the defaults for all of these!
        //...
      }
    }
  },
  "Cache": {
    "Disk": true, //Should cache be stored on disk
    "Memory": true, //Should cache be kept in memory
    "WipeOnStart": false, //Should cache be cleared on startup
    "PreloadFromDisk": true, //Should cache be loaded from disk into memory on startup
    "PreloadFromWeb": false //Should missing JS files be downloaded on startup, slows down startup considerably
  },
  "Debug": {
    //used during debug, proxies a global env json, recommended to leave undefined/null
    "ClientEnvProxyUrl": "http://localhost:2000/api/_fosscord/v1/global_env"
  }
}
```
