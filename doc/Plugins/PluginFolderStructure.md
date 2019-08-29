# Plugin Folder Structure

After first start of **NiceHash Miner (v1.9.1.5 and up)** there will be a **miner_plugins** folder created inside which all supported plugins will be located.<br>
In each plugin folder there will be **bins** and **internals** folder.<br>

```
├───miner_plugins
│   ├───XmrStak
│   │   ├───bins
│   │   └───internals
```

- Bins folder contains downloaded miner files
- Internals folder contains Internal settings

## Internal settings

Internal settings are configs implemented by **MinerPluginToolkitV1**.<br>
Currently following settings are implemented:
- [Miner System Environment Variables](./InternalSettings/MinerSystemEnvironmentVariables.md)
- [Miner Options Package](./InternalSettings/ExtraLaunchParameters.md)
- [Miner Reserved Ports](./InternalSettings/MinerReservedPorts.md)
- [Miner Api Max Timeout Setting](./InternalSettings/MinerApiMaxTimeoutSettings.md)
- [Miner Benchmark Time Settings](./InternalSettings/MinerBenchmarkTimeSettings.md)

Each of them resides in its own *JSON* file.

```
├───internals
|        MinerApiMaxTimeoutSetting.json
|        MinerBenchmarkTimeSettings.json
|        MinerOptionsPackage.json
|        MinerReservedPorts.json
|        MinerSystemEnvironmentVariables.json
```

All of them are allowed to be modified by users for more personalized settings.
The changes inside the file are used by miner, if `use_user_settings` field in each changed file is set to `true`.

*Note:* Change of the settings won't do anything while the NHM is running. You have to stop the NHM, change the files and then start it, for settings to take effect.