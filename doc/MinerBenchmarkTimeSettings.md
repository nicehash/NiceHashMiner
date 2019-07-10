# Miner Benchmark Time Settings

Located inside `miner_plugins/somePlugin/internals`, the **MinerBenchmarkTimeSettings.json** file allows user to set **general** and **per_algorithm** benchmark timeouts.

The file consists of `use_user_settings`, `time_general_s` and `time_per_algorithm_s` sections.



Example file: 
```JSON
{
    "use_user_settings": true,
    "algorithm_reserved_ports":  {
        "Beam": [4001],
        "CuckooCycle": [4005, 4010]
    }
}
```

Within the example file we reserved port [4001] for Beam and [4005, 4010] ports for CuckooCycle algorithm. You can set more than one port for each algorithm.<br>
To enable changes inside this file, change `use_user_settings` to true.