These are established values for "sweetspots" of `-dcri` values for ClaymoreDual. The `-dcri` value adjusts how hard the miner works on the second coin, which can potentially eat into the Eth hashrate. For bigger cards like a 1080 Ti there is generally a lot of room to increase dcri without lowering Eth

These values can fluctuate based on the paying rates of Eth and the secondary coins, so it is best to try out for yourself.

The default value is `-dcri 30`. Instructions on how to adjust yourself at the end.

### NVIDIA

| Card            | Secondary Coin | `-dcri` | Eth Hashrate (MH/s) | Secondary Hashrate (MH/s) |
|:----------------|:---------------|--------:|--------------------:|--------------------------:|
|1080 Ti          |Sia             | 150     | 30.5                | 1550                      |

### AMD

| Card            | Secondary Coin | `-dcri` | Eth Hashrate (MH/s) | Secondary Hashrate (MH/s) |
|:----------------|:---------------|--------:|--------------------:|--------------------------:|
|Fury X           |SIA             | 40      | 30.5                | 1200                      |
|290X             |SIA             | 27      | 29                  | 800                       |
