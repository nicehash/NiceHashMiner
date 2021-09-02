# About
These installer scripts are based off the [electron-builder](https://github.com/electron-userland/electron-builder) NSIS template scripts: 
  - https://github.com/electron-userland/electron-builder/tree/v19.43.0
  - https://github.com/electron-userland/electron-builder/tree/v19.43.0/packages/electron-builder/templates/nsis


The scripts are highly modified for nhm2, but the original template and electron-builder sources should serve as a reference until a complete re-write is done. For better understanding make sure to read the [NSIS Users Manual](http://nsis.sourceforge.net/Docs/). 

## Features
  - Only 1 instance running check,
  - Support for multilanguage install,
  - Creates Desktop and Start Menu shortcuts,
  - Per user and all users (Admin) install,
  - Check if existing instance is running before update
  - Registers the aplication (can be uninstalled from "Program files")
  - Checks and allows installation only on 64bit Windows7 OS
  - Prompt for clean uninstall (deletes all user data like configs, benchmarks, miner files, ...)
  - Run on finish prompt,
  - Copies over installer to user path for attribution meta-data extraction
  - ... TODO what is worth mentioning

## Remarsks
 - heavy use of macros (we should reduce this wherever needed)
 - remove unused functionalitly
 - manual execution, automate as much as possible
 - no tests

# Required 3rd party plugins:
  - nsDialogs,
  - nsis7z,
  - nsProcess (v1_6),
  - [StdUtils](http://nsis.sourceforge.net/StdUtils_plug-in),
  - System,
  - UAC,
  - WinShell

# How to use
The NSIS installer is built out of the binary package and an uninstaller.

## Step #1
Package the (signed) executable with all needed assets and dll's with 7z LZMA2 algorithm. The installer expects the package to have the `${BASE_NAME}.exe` in the root of the archive. It expects an archive file path `dist\nhm.7z`.

## Step #2
To build the uninstaller define **BUILD_UNINSTALLER** flag and extract the final package you should have `dist\__uninstaller-nsis-nhm2.exe` in the root.

## Step #3
With first two steps completed remove the **BUILD_UNINSTALLER** flag and build the installer.

# TODO
Check `TODO.md`