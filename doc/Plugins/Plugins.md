# Plugins

<ul>
<li><a href="#plugin">What is a Plugin?</a></li>
<li><a href="#toolkit">What is MinerPluginToolkitV1 used for?</a></li>
<li><a href="#listOfPlugins">List of included plugins</a></li>
</ul>

<h1 id="plugin">What is a Plugin?</h1>
<p>A plugin is an external or internal dependcy (dll). The ABI (Application Binary Interface) is defined in <b>MinerPlugin</b> project and plugin developers must implement the following interfaces <a href="../../src/NHM.MinerPlugin/IMinerPlugin.cs">IMinerPlugin</a> and <a href="../../src/NHM.MinerPlugin/IMiner.cs">IMiner</a>. This means that you create a new project inside Miners directory and implement all required functions from IMinerPlugin and IMiner interfaces. It is recommended that each one is in its own file (Plugin and Miner file).</p>
<p>Each plugin project should implement at least 1 plugin. You can implement more, but good practice is to keep 1 plugin inside 1 project.
<br><b>IMinerPlugin</b> is used for registering the plugin and there will be only 1 instance created. Its job is to give basic info such as name, UUID, version, etc.
<br><b>IMiner</b> is the mandatory interface for all miners containing bare minimum functionalities and is being used as miner process instance created by IMinerPlugin.</p>
<p>Bare minimum example of plugin is written in <a href="../../src/Miners/__DEV__ExamplePlugin">Example Plugin</a> project. The <a href="../../src/Miners/__DEV__ExamplePlugin/ExamplePlugin.cs">Plugin</a> file contains implementation of IMinerPlugin interface for registration and creation of the plugin instance. The <a href="../../src/Miners/__DEV__ExamplePlugin/ExampleMiner.cs">Miner</a> file contains implementation of IMiner interface, providing required functionalities.</p>

<h1 id="toolkit">What is MinerPluginToolkitV1 used for?</h1>
<p>It is recommended to use <b>MinerPluginToolkitV1</b> as this will enable full integration with NiceHash Miner. It will save time developing it and enable implementation of additional advanced features. If you are writing a plugin we highly recommend that you use MinerPluginToolkitV1. All miner plugins that are developed by NiceHash miner dev team are using MinerPluginToolkitV1. For example you can check <a href="../../src/Miners/GMiner">GMiner Plugin</a>.</p>
<p>MinerPluginToolkitV1 also enables creation of <b>Background Services</b>, check out <a href="../../src/NHMCore/Mining/Plugins/EthlargementIntegratedPlugin.cs">Ethlargement plugin</a> for example.</p>

<table style="width:100%">
<tr>
  <th>Advantages</th>
  <th>Disadvantages</th>
</tr>
<tr>
  <td><p>Implementation of all basic actions like Start/Stop mining, Start benchmarking, retrieve data from API, create command line, etc.</p>
  <p>Use of additional features like Configs and Extra Launch Parameters</p>
  <p>Access to usefull <a href="../../src/NHM.MinerPluginToolkitV1/Interfaces">interfaces</a> providing features like checking for missing files, device cross referencing, initializing internal settings, etc.</p>
</td>
  <td>The current API is not final and might change in the future.</td> 
</tr>
</table> 
<br>
<h2 id="listOfPlugins">List of Included Plugins</h2>

## Miner Plugins

### All devices

* [Xmr-Stak](./PluginDocs/XmrStak.md)

### NVIDIA and AMD

* BMiner
* ClaymoreDual
* GMiner
* Phoenix
* LolMiner
* NanoMiner

### NVIDIA

* CCMiner
* NBMiner
* T-Rex
* TT-Miner
* CryptoDredge
* MiniZ
* Z-Enemy

### AMD

* SGMiner
* TeamRedMiner
* WildRig

### CPU

* XMRig

## Background Services

* Ethlargement Pill


