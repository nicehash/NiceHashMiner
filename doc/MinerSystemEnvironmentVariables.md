# Miner System Environment Variables

Located inside `miner_plugins/somePlugin/internals`, the **MinerSystemEnvironmentVariables.json** file allows user to set system environment variables.

The file consists of `use_user_settings`, `default_system_environment_variables` and `custom_system_environment_variables` sections.

Example file for Xmr-Stak: 
```JSON
{
  "use_user_settings": false,
  "default_system_environment_variables": {
    "XMRSTAK_NOWAIT": "1",
    "GPU_MAX_ALLOC_PERCENT": "100",
    "GPU_SINGLE_ALLOC_PERCENT": "100",
    "GPU_MAX_HEAP_SIZE": "100",
    "GPU_FORCE_64BIT_PTR": "1"
  },
  "custom_system_environment_variables": null
}
```
- `default_system_environment_variables` sections holds system variables set by the plugin and can be changed.
- `custom_system_environment_variables` sections is not yet implemented.

To enable changes inside this file, set `use_user_settings` to true.
