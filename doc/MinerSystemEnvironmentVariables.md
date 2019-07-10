# Miner System Environment Variables

Located inside `miner_plugins/somePlugin/internals`, the **MinerSystemEnvironmentVariables.json** file allows user to set **default** and **custom** system environment variables.

The file consists of `use_user_settings`, `default_system_environment_variables` and `custom_system_environment_variables` sections.

Both `system_environment_variables` sections use same notation: `"name": "value"`.

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
  "custom_system_environment_variables": {
    "CUSTOM_SETTING_NAME": "55",
  }
}
```

Within the `custom_system_environment_variables` you should set wanted custom variables.<br>
To enable changes inside this file, change `use_user_settings` to true.
