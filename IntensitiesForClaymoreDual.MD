These are established values for "sweetspots" of `-dcri` values for ClaymoreDual. The `-dcri` value adjusts how hard the miner works on the second coin, which can potentially eat into the Eth hashrate. For bigger cards like a 1080 Ti there is generally a lot of room to increase dcri without lowering Eth

These values are rough and can fluctuate based on the paying rates of Eth and the secondary coins, so it is best to try out for yourself.

The default value is `-dcri 30` which is usually a good value for cards similar to 290/390/480/580. Instructions on how to adjust yourself at the end.

### NVIDIA

| Card            | Secondary Coin | `-dcri` | Eth Hashrate (MH/s) | Secondary Hashrate (MH/s) |
|:----------------|:---------------|--------:|--------------------:|--------------------------:|
|1080 Ti          |Sia             | 150     | 30.5                | 1550                      |
|          |Lbry            | 80     | 30                | 60                      |
|          |Pascal             | 90     | 32                | 950                      |

### AMD

| Card            | Secondary Coin | `-dcri` | Eth Hashrate (MH/s) | Secondary Hashrate (MH/s) |
|:----------------|:---------------|--------:|--------------------:|--------------------------:|
|Fury X           |SIA             | 40      | 30.5                | 1200                      |
|290X             |SIA             | 27      | 29                  | 800                       |

### Instructions

1. Uncheck hide miners in settings and start mining with the dual algo you want to tweak (e.g. DaggerSia)

2. While mining, highlight the miner window and hit + or - on your keyboard. You will see a message come up "-dcri set to ##". Keep playing with that value and watch the NHM window to see if your profit goes up or down. 

3. When you find the right number go to the settings for that algorithm and put `-dcri <your number>` in the additional launch options. E.g. the optimal value I've found for a 1080 Ti is `-dcri 150`. Now whenever NHM launches that dual algo, it will automatically set to that intensity. Don't forget to update your hashrates after you've found a good intensity!
