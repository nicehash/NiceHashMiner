# Build Flags

The project can be built with different build flags, that enable or disable different features.

## Protocol build flags
- `TESTNET`
- `TESTNETDEV`
- `PRODUCTION_NEW`
- NO FLAG (old production  - **OBSOLETE**)

## External plugins flag

- `ENABLE_EXTERNAL_PLUGINS` is used to enable or disable external plugins. Not using this flag means that the *Plugins* menu will be unavailable, disallowing the download of external plugins. Also already installed external plugins won't be loaded.<br>
*Notice:* If any of `TESTNET`, `TESTNETDEV` or `PRODUCTION_NEW` is used, `ENABLE_EXTERNAL_PLUGINS` is automatically switched on.

## Debug flags

Allow user to enable debug features (more detailed logging, set benchmark speeds to 1, etc.).

- `DEBUG`
- `DEBUG_MARKETS`

## Integrated plugins flags

Allow external plugin to become integrated for testing purposes (useful when writing a plugin).

- `INTEGRATE_BMiner_PLUGIN`
- `INTEGRATE_BrokenMiner_PLUGIN`
- `INTEGRATE_CpuMinerOpt_PLUGIN`
- `INTEGRATE_CryptoDredge_PLUGIN`
- `INTEGRATE_Ethminer_PLUGIN`
- `INTEGRATE_ExamplePlugin_PLUGIN`
- `INTEGRATE_LolMinerBeam_PLUGIN`
- `INTEGRATE_MiniZ_PLUGIN`
- `INTEGRATE_NanoMiner_PLUGIN`
- `INTEGRATE_SRBMiner_PLUGIN`
- `INTEGRATE_WildRig_PLUGIN`
- `INTEGRATE_XMRig_PLUGIN`
- `INTEGRATE_ZEnemy_PLUGIN`