# Algorithm Extra Launch Parameters (ELPs)

If you are an advanced user that wants to tweak the performance of your GPUs or CPUs you can set **supported** options in the **ExtraLaunchParameters** for selected Device and Algorithm.

If you have 3 AMD devices with the following **ExtraLaunchParameters** settings for algorithm A and B:
  - **device1** --xintensity 1024 --worksize 64 --gputhreads 2
  - **device2** --xintensity 512 --worksize 128 --gputhreads 2
  - **device3** --xintensity 512 --worksize 64 --gputhreads 4

If **algorithm A** is most profitable for device1 and device2 and **algorithm B** for device3, NiceHashMiner will run two sgminers for A and B like so:
  - sgminer .. --xintensity 1024,512 --worksize 64,128 --gputhreads 2,2 .. (device1 and device2)
  - sgminer .. --xintensity 512 --worksize 64 --gputhreads 4 .. (device3)

If **algorithm A** is most profitable for all three devices, NiceHashMiner will run two sgminers for A like so:
  - sgminer .. --xintensity 1024,512,512 --worksize 64,128,64 --gputhreads 2,2,4 .. (device1, device2, device3)

So when setting **ExtraLaunchParameters** set them **per device and algorithm** NiceHashMiner will group them accordingly.
If you leave **ExtraLaunchParameters** empty the defaults will be used or ignored if no parameters have been set.

## Supported options

Check the supported options in **Plugin** file located in [Miners](https://github.com/nicehash/NiceHashMiner/tree/master/src/Miners) directory. The ELPs are listed inside [MinerOptionsPackage](https://github.com/nicehash/NiceHashMiner/blob/4b3e80672cb08fbb6efe89a4e3d9ddf46ac2af8c/src/Miners/GMiner/GMinerPlugin.cs#L168) variable.

## How to add missing miner flags

You can add missing miner flags for certain miners. After first start of **NiceHash Miner (v1.9.1.5 and up)** there will be a **miner_plugins** folder created inside which there will be all supported plugins. <br>Inside each plugin folder there will be **internals** folder containing `MinerOptionsPackage.json`.<br> If there is a missing miner option you can edit the `general_options` for general optimizations or `temperature_options` for temperature settings. 

Each miner option consist of `type`, `id`, `short_name`, `long_name`, `default_value` and `delimeter.`
- **type** attribute defines type of ELP. There are 4 available types listed and explained in the *FlagType* section.
- **id** attribute defined id of that ELP. It must be unique.
- **short_name** and **long_name** attributes represent the actual flag that is being sent to the miner.
- **default_value** is the default value sent with the flag if default values are enabled and no other value is being sent.
- **delimeter** is the sign used for separating values (in case of multiple parameter options)

## FlagType 
This parameter indicates how to group multiple devices:
 - **OptionIsParameter** this means that the flag doesn't have any parameters 
 - **OptionWithSingleParameter** this means that the flag has only one parameter
 - **OptionWithMultipleParameters**  this means that the flag accepts more than one parameter (example two gpus with different intensity values)
 - **OptionWithDuplicateMultipleParameters** indicates that option takes one or more parameters but with repeated flag. 

## MinerOptionsPackage file

This is the example of GMiner `MinerOptionsPackage.json` file:

```
{
  "use_user_settings": false,
  "group_mining_pairs_only_with_compatible_options": true,
  "general_options": [
    {
      "type": "OptionWithSingleParameter",
      "id": "gminer_pers",
      "short_name": null,
      "long_name": "--pers",
      "default_value": null,
      "delimiter": null
    },
    //...//
  ],
  "temperature_options": [
    {
      "type": "OptionWithMultipleParameters",
      "id": "gminer_templimit",
      "short_name": "-t",
      "long_name": "--templimit",
      "default_value": "90",
      "delimiter": " "
    }
  ]
}
```

If you want to add an option just append miner option to `general_options` or `temperature_options` section.<br>
**Warning:** if value set inside NHM Extra Launch Parameter settings is same as `default_value`, the value will be ignored.

To enable changes inside this file, set `use_user_settings` to true.

The `group_mining_pairs_only_with_compatible_options` attribute is used in parser that checks if parsed options are compatible. It is set to **true** by default.<br>
This filter is checking options of type `OptionIsParameter` or `OptionWithSingleParameter`. <br>
For example:
  - **device1** --pers 5
  - **device2** --pers 4

These two options are incompatible and would be started in two different
instances.
