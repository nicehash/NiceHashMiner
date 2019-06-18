# MINERS

- [Introduction](#introduction)
- [What are the benefits?](#benefits)
- [What is a Plugin?](#plugin)
- [What is MinerPluginToolkitV1 used for?](#toolkit)

# <a name="introduction"></a> Introduction

Miners folder is home to all Plugins, Plugin interfaces and their helpers.

# <a name="benefits"></a> What are the benefits?

- Plugin implementation allows easier tweaking for advanced users like changing ExtraLaunchParameters, SystemEnvironmentVariables and settings that are miner specific (some miners have special settings).
- Adding the new miners or just updating old ones was never so easy, as now with the [MinerPlugin](MinerPlugin) and the [MinerPluginToolkitV1](MinerPluginToolkitV1).

# <a name="plugin"></a> What is a Plugin?

A plugin is a module that connects with main program to bring additional features. In most cases we are talking about Miner Plugins, but there are also Background Service Plugins. We know integrated and external plugins. 
Integrated ones come with the NHM distribution while external must be manually downloaded from plugin marketplace.

A plugin can be written by anyone with some knowledge of programming in C#.
[Here](Example) is also an example of how to implement Miner Plugin by yourself.
Each plugin must have its Plugin file. In our case this is [ExamplePlugin](Example/ExamplePlugin.cs).
Miner plugins also have Miner file. Example of this file is [ExampleMiner](Example/ExampleMiner.cs).
These two files are backbone for any Miner Plugin, while Background Service Plugins need only Plugin file to operate correctly (see [EthlargementPill](Ethlargement/EthlargementPlugin.cs) plugin for example).

# <a name="toolkit"></a> What is MinerPluginToolkitV1 used for?

Along the way you stumbled across [MinerPluginToolkitV1](MinerPluginToolkitV1). But what is its addition to MinerPlugin you would ask.
Miner plugin can be written completely without any use of MinerPluginToolkitV1, but there is a handfull of additional functionalities in it causing that using it is recommended.
MinerPluginToolkitV1 allows developer to use Configs, Extra Launch Parameters, different interfaces and a lot of helper classes. It is also home to MinerBase, supporting all basic actions like Start/Stop mining, Start benchmarking, retreive data from API, create command line, etc.
To sum it all, the MinerPluginToolkitV1 is a project with helper functionalities that can be used to easly implement your plugin.
