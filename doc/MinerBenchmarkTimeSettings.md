# Miner Benchmark Time Settings

Located inside `miner_plugins/somePlugin/internals`, the **MinerBenchmarkTimeSettings.json** file allows user to set **general** and **per_algorithm** benchmark timeouts.

The file consists of `use_user_settings`, `time_general_s` and `time_per_algorithm_s` sections.


Example file: 
```JSON
{
    "use_user_settings": true,
    "time_general_s":  {
        "0": 20,
        "1": 60,
        "2": 120,
    },
    "time_per_algorithm_s":  {
        "0": {
            "X16R": 40,
            "DaggerHashimoto": 60
        },
        "1": {
            "DaggerHashimoto": 120
        },
        "2": {
            "DaggerHashimoto": 180,
        },
    },
}
```

- `time_general_s` section holds general benchmark timeouts of all algorithms for each **Benchmark Performance Type**.<br>
- `time_per_algorithm_s` section holds benchmark timeouts for specified algorithms. Each **Benchmark Performance Type** specifies benchmark time for stated algorithm. <br>

*Benchmark Performance Types:*
- 0 -> Quick
- 1 -> Standard
- 2 -> Precise

When checking for benchmark timeout, time from `time_per_algorithm_s` has priority over `time_general_s`.<br>

To enable changes inside this file, set `use_user_settings` to true.<br>
*Note:* All times are in seconds.<br>
