# PLUGINS

- [What is a Plugin?](#plugin)
- [What is MinerPluginToolkitV1 used for?](#toolkit)

# <a name="plugin"></a> What is a Plugin?

A plugin is an external or internal dependcy (dll). The ABI (Application Binary Interface) is defined in <b>MinerPlugin</b> project and plugin developers must implement the following interfaces [IMinerPlugin](https://github.com/nicehash/NiceHashMiner/blob/18945346ce710eb691a0686ef9449fd1ddf70096/src/NHM.MinerPlugin/IMinerPlugin.cs) and [IMiner](https://github.com/nicehash/NiceHashMiner/blob/18945346ce710eb691a0686ef9449fd1ddf70096/src/NHM.MinerPlugin/IMiner.cs). This means that you create a new project inside Miners directory and implement all required functions from IMinerPlugin and IMiner interfaces. It is recommended that each one is in its own file (Plugin and Miner file).<br>

Each plugin project should implement at least 1 plugin. You can implement more, but good practice is to keep 1 plugin inside 1 project.<br>
<b>IMinerPlugin</b> is used for registering the plugin and there will be only 1 instance created. Its job is to give basic info such as name, UUID, version, etc.<br>
<b>IMiner</b> is the mandatory interface for all miners containing bare minimum functionalities and is being used as miner process instance created by IMinerPlugin.<br>

Bare minimum example of plugin is written in [Example Plugin](https://github.com/nicehash/NiceHashMiner/tree/18945346ce710eb691a0686ef9449fd1ddf70096/src/Miners/__DEV__ExamplePlugin) project. The [Plugin](https://github.com/nicehash/NiceHashMiner/blob/18945346ce710eb691a0686ef9449fd1ddf70096/src/Miners/__DEV__ExamplePlugin/ExamplePlugin.cs) file contains implementation of IMinerPlugin interface for registration and creation of the plugin instance. The [Miner](https://github.com/nicehash/NiceHashMiner/blob/18945346ce710eb691a0686ef9449fd1ddf70096/src/Miners/__DEV__ExamplePlugin/ExampleMiner.cs) file contains implementation of IMiner interface, providing required functionalities.

# <a name="toolkit"></a> What is MinerPluginToolkitV1 used for?

It is recommended to use <b>MinerPluginToolkitV1</b> as this will enable full integration with NiceHash Miner. It will save time developing it and enable implementation of additional advanced features. If you are writing a plugin we highly recommend that you use MinerPluginToolkitV1. All miner plugins that are developed by NiceHash miner dev team are using NHM.MinerPluginToolkitV1. For example you can check [GMiner Plugin](https://github.com/nicehash/NiceHashMiner/tree/18945346ce710eb691a0686ef9449fd1ddf70096/src/Miners/GMiner).<br>
MinerPluginToolkitV1 also enables creation of <b>Background Services</b>, check out [Ethlargement plugin](https://github.com/nicehash/NiceHashMiner/blob/18945346ce710eb691a0686ef9449fd1ddf70096/src/NHMCore/Mining/Plugins/EthlargementIntegratedPlugin.cs) for example.

<table style="width:100%">
<tr>
  <th>Advantages</th>
  <th>Disadvantages</th>
</tr>
<tr>
  <td><p>Implementation of all basic actions like Start/Stop mining, Start benchmarking, retrieve data from API, create command line, etc.<p>
  <p>Use of additional features like Configs and Extra Launch Parameters<p>

  Access to usefull [interfaces](https://github.com/nicehash/NiceHashMiner/tree/18945346ce710eb691a0686ef9449fd1ddf70096/src/NHM.MinerPluginToolkitV1/Interfaces) providing features like checking for missing files, device cross referencing, initializing internal settings, etc.
</td>
  <td>The current API is not final and might change in the future.</td> 
</tr>
</table> 
