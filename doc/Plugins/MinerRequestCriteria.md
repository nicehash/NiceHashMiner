# Miner request criteria

We welcome suggestions to add new miners to NHML. You can make a request by opening an issue, but please ensure the miners you suggest follow some basic requirements:

* The miner is compatible with the NiceHash stratum protocol. You can usually find this in the miner readme, or if it has a NiceHash compatibility option. Without this, the miner is likely to submit invalid shares

* The miner has an HTTP API for getting hashrates. If this is supported, there will be options to configure the API port or instructions in the readme. This is used to integrate speed stats into the NHML GUI

* The miner is benchmarkable. Preferably, this means it includes an offline benchmark mode (e.g. `-benchmark` option). However at the least it must support frequent automatic hashrate reporting

* The miner has stable releases. Miners that are in development or have unstable releases will usually be held off until a stable version is released

* The miner is actually needed. If it does not provide speed bonuses compared to current miners, it will not be included so that NHML does not get bloated with obsolete miners

The miner does not have to be open source, closed source miners such as ClaymoreDual are included as 3rd party miners. It does however have to allow distribution (i.e. the automatic downloading NHML does on setup). 

After these requirements are checked, you can open an issue and explain what kind of speed improvements you are seeing with it. 

Note miner suggestions based off algorithms not supported by NiceHash will not be considered. NHML is designed to work with NH pools, and if there is no pool for an algorithm then there is no point in including miners for it.