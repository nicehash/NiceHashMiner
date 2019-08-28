# How to fix missing miner downloads

The most common factors that result in missing miner files are:
1. Anti-Virus (**AV**) removing files
2. Failed autodownload: 
    * AV preventing download
    * ISP/geo blocking links

### Anti-Virus removing downloaded miner files

To fix this issue, you have to add your NHM installation path as an exception in your AV.<br>
For example if you have Windows Defender, add exception inside Virus & threat protection settings.

If this doesn't work, try to fix the issue with the following method.

### Failed autodownload

For each release there is a `miner_bins_urls.json` file that contains miner download links.<br>
Example of file:
```
{
  "use_file_settings": false,
  "plugin_bins_urls": {
    ...
    "BMiner": [
      "https://www.bminercontent.com/releases/bminer-lite-v15.5.3-747d98e-amd64.zip"
    ],
    "CCMinerMTP": [
      "https://github.com/nicehash/ccminer/releases/download/1.1.14/ccminer_mtp.7z"
    ],
    "CCMinerTpruvot": [
      "https://github.com/tpruvot/ccminer/releases/download/2.3.1-tpruvot/ccminer-2.3.1-cuda10.7z"
    ],
    "ClaymoreDual": [
      "https://mega.nz/#F!O4YA2JgD!n2b4iSHQDruEsYUvTQP5_w?2sBXjCTY"
    ],
    "ClaymoreDual14+": [
      "https://mega.nz/#F!O4YA2JgD!n2b4iSHQDruEsYUvTQP5_w?ngYyTARY"
    ],
    ...
  }
}
```

Use these links to download miner bins.<br>
For example if download of **ClaymoreDual14+** fails, download files from link manually and extract them inside `ClaymoreDual14+/bins`.
```
├───ClaymoreDual14+
│   ├───bins
│   └───internals
```