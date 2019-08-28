# Tweaking switch timing

We try to find the best values for switching time, however you may find that these do not work for you. If you are seeing too frequent switching, you can adjust these parameters so NHML is more resistant to profitability changes.

## Basic 

Three values are available on the Advanced settings tab:

#### Switch Profitability Threshold

This value dictates how much higher a new higher profit algorithm must be before it is switched to. The default is `0.02`, or 2%. Some people set this slightly higher, e.g. 5%. This would mean that NHML waits until it finds an algorithm that is 5% higher profitability than the current before switching to it.

Note this value is applied to global profit, i.e. if one card is 5% higher but 3 others are not, the switch will still not happen yet.

#### Switch Minimum/Maximum [s]

These two values decide how frequently new algorithm profits are checked. The default is 34 and 55. NHML picks a random number from within this range every time it checks algorithm profits to set the delay for when it checks next.

Note this time interval is not how often switches happen, it is how often new algorithm profits are checked with the profit normalization feature. These check "ticks" may need to happen for as little as 2 and as many as 13 times before the new profit is used, depending on how stable that algorithm price is deemed (see advanced section). For this reason it is recommended to adjust these values slowly. Also it is recommended to maintain a similar range between the upper and lower values.

## Advanced

Two other options are exposed in the `configs\General.json` file. These are `SwitchSmaTicksStable` (default [2,3]) and `SwitchSmaTicksUnstable` (default [5,13]). Like Switch Minimum/Maximum they both have a lower and upper bound. They define the range for how many ticks to take higher values are needed during profit normalization. These can be adjusted upward, however again make sure to keep a similar range and to adjust slowly. The unstable one is most likely to be of help, since unstable algorithms are usually what cause heavy swings in profitability. 

Whether an algorithm is stable or not is decided server side and updated dynamically. 