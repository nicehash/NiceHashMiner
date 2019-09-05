# Miner Reserved Ports

Located inside `miner_plugins/somePlugin/internals`, the **MinerReserverPorts.json** file allows user to reserve specific ports for each algorithm.

The file consists of `use_user_settings` and `algorithm_reserved_ports` sections.

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
- `algorithm_reserved_ports` holds list of reserved ports for specified algorithms

You can set more than one port for each algorithm.<br>
If none of the specified ports won't be available, fallback mechanism will be started, using the first free port in the default range.

To enable changes inside this file, set `use_user_settings` to true.
