# MINERS

- [Introduction](#introduction)
- [What are the benefits?](#benefits)
- [What is a Plugin?](#plugin)

# <a name="introduction"></a> Introduction

Miners folder is home to all Plugins, Plugin interfaces and their helpers.

# <a name="benefits"></a> What are the benefits?

- Plugin implementation allows easier tweaking for advanced users like changing ExtraLaunchParameters, SystemEnvironmentVariables and settings that are miner specific (some miners have special settings).
- Adding the new miners or just updating old ones was never so easy, as now with the [MinerPlugin](MinerPlugin) and the [MinerPluginToolkitV1](MinerPluginToolkitV1).

# <a name="plugin"></a> What is a Plugin?

A plugin is addition to program with additional features. In most cases we are talking about MinerPlugins. These are miners writen as plugins. We know integrated and external plugins. 
Integrated come with the NHM distribution while external must be manually downloaded from plugin marketplace.

A plugin can be written by anyone with some knowledge of programming in C#.
[Here](#example) is also an example of how to implement one by yourself.
