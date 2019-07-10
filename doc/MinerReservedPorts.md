# Miner Reserved Ports

Located inside `miner_plugins/somePlugin/internals`, the **MinerReserverPorts.json** file allows user to reserve specific ports for each algorithm.

The file consists of `use_user_settings` and `algorithm_reserved_ports` sections.
The following section is a Dictionary with **algorithm name** (string) as a key and **sequence of ports** (List of ints) as value.

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
