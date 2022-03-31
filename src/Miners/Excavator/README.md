# Excavator plugin
## Custom internal commands:
This example demonstrates how to use custom commands that are specific to the excavator miner. Excavator receives those commands via web api. NHM parses desired commands from the template file located inside `*plugin-uuid*/internals/CommandLineTemplate.json`. You can access this folder easily by navigating to "Plugins" tab inside NHM and clicking on `...` (three dots) -> `Show internals`. The contents of the file should look like this:
```yaml
[
  {
    "time": 0,
    "commands": [
      {
        "id": 1,
        "method": "subscribe",
        "params": [
          "__SUBSCRIBE_PARAM_LOCATION__",
          "__SUBSCRIBE_PARAM_USERNAME__"
        ]
      },
      {
        "id": 2,
        "method": "algorithm.add",
        "params": [
          "daggerhashimoto"
        ]
      },
      {
        "id": 3,
        "method": "worker.add",
        "params": [
          "daggerhashimoto",
          "GPU-1d07feec-f101-b824-e32e-a8d2f50eef75"
        ]
      },
      {
        "id": 4,
        "method": "worker.add",
        "params": [
          "daggerhashimoto",
          "GPU-7d9113ac-267b-11be-7c6c-ee77ba7f5e89"
        ]
      },
      {
        "id": 5,
        "method": "worker.add",
        "params": [
          "daggerhashimoto",
          "GPU-9bcf391e-3a9c-1b00-c527-d93edf7085bf"
        ]
      }
    ]
  },
  {
    "event": "on_quit",
    "commands": []
  }
]
```
In our example, we have 3 GPUs in our computer, and we will disable LHR unlock for two of them. Add the command of your choice as a new json object like following:
```yaml
[
  {
    "time": 0,
    "commands": [
      {
        "id": 1,
        "method": "subscribe",
        "params": [
          "__SUBSCRIBE_PARAM_LOCATION__",
          "__SUBSCRIBE_PARAM_USERNAME__"
        ]
      },
      {
        "id": 2,
        "method": "algorithm.add",
        "params": [
          "daggerhashimoto"
        ]
      },
      {
        "id": 3,
        "method": "worker.add",
        "params": [
          "daggerhashimoto",
          "GPU-1d07feec-f101-b824-e32e-a8d2f50eef75"
        ]
      },
      {
        "id": 4,
        "method": "worker.add",
        "params": [
          "daggerhashimoto",
          "GPU-7d9113ac-267b-11be-7c6c-ee77ba7f5e89"
        ]
      },
      {
        "id": 5,
        "method": "worker.add",
        "params": [
          "daggerhashimoto",
          "GPU-9bcf391e-3a9c-1b00-c527-d93edf7085bf"
        ]
      }
    ]
  },
  {
    "event": "on_quit",
    "commands": []
  }, // <------- DON'T FORGET THE COMMA HERE !!!!
  {
    "time": 1,
    "commands": [
      {
        "id": 1,
        "method": "device.lhr.disable",
        "params": [
          "GPU-1d07feec-f101-b824-e32e-a8d2f50eef75" // <- Each GPU is passed as a separate command!
        ]
      },
      {
        "id": 2,
        "method": "device.lhr.disable",
        "params": [
          "GPU-7d9113ac-267b-11be-7c6c-ee77ba7f5e89"
        ]
      }
    ]
  }
]
```
After you have entered your desired command, head to the root plugin folder, (where internals folder is located), and head to `bins/*version*/NHQM_*version*/` and delete all .json files starting with `cmd_...` Start NHM and re-benchmark excavator on your GPU's. Now, you should see new `cmd_...` files generated with all of the commands for a specific gpu from the template we mentioned previously (including your custom commands, if you've done everything correctly). 

Please note that you can enter GPU-specific commands to the template and NHM will cross reference it to the specific GPU using UUID's, so you don't have to worry a command for a single GPU will end up in the `cmd` file of every GPU you have.
