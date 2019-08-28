# Manual Benchmarking

Sometimes you may not be able to benchmark specific algorithms. The following steps will allow you to benchmark them manually (and if the benchmark failure is due to a crash, better pinpoint the problem):

1. Open settings, and make sure `Advanced -> Hide Mining Windows` is unchecked.

2. Go to the `Devices/Algorithms` tab and find the algorithm you wish to benchmark.

3. Right click on the algorithm and click "Enable only this". This will disable all other algorithms. If the algorithm doesn't have a speed, set it to 1 inside `Benchmark Speed` field.

5. Run the miner and the one you selected should pop up (NOTE: if you have more than one identical card and a different algorithm is popping up, disable your other cards). 

6. Let the miner run long enough to get a stable hashrate. Remember this number, then go back to `Settings -> Devices/Algorithms` and enter it manually in the `Benchmark Speed` field. Note miners will generally show hashrates in larger units "e.g. GH/s" so you will have to add zeroes to adjust for this (look at the speed column to see it formatted with larger units).

7. Re-enable the other algorithms that were disabled in step 3. Once you've saved those settings, NHML will remember the speed you've found and use it for auto-switching.