# Miner Api Max Timeout Settings

Located inside `miner_plugins/somePlugin/internals`, the **MinerApiMaxTimeoutSettings.json** file allows user to set **general**, **per_algorithm** and **per_device_type** api max timeouts.

The file consists of `use_user_settings`, `enabled`, `general_timeout`, `timeout_per_algorithm` and `timeout_per_device_type` sections.

Example file: 
```JSON
{
  "use_user_settings": false,
  "enabled": true,
  "general_timeout": "00:05:00",
  "timeout_per_algorithm": {
    "Lyra2z": "00:03:00",
    "CryptonightV8": "00:04:00"
  },
  "timeout_per_device_type": {
    "CPU": "00:08:30",
  }
}
```

- `general_timeout` section holds general timeouts for all algorithms on all device types.
- `timeout_per_algorithm` section holds timeouts for specified algorithms.
- `timeout_per_device_type` section holds timeouts for specified **device types**.

Device Types are CPU, NVIDIA and AMD

When checking for api max timeout, time from `timeout_per_device_type` has priority over `timeout_per_algorithm`, which has priority over `general_timeout`.<br>

To enable this functionality, set `enabled` to true.<br>
To enable changes inside this file, set `use_user_settings` to true.<br>
*Note:* All times are in `hh:mm:ss` format.<br>
