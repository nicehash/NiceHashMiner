Phases:

* **Phase 1 - Build a copy of the WinForms GUI**
* Phase 2 - Separate all business logic from `NiceHashMinerLegacy.csproj` into separate, business-only projects
* Phase 3 - Link new GUI to business-projects and make functional

## Phase 1 - In Progress

### Complications

* WPF does not have a LinkLabel so "Show my stats online!" is implemented as a button
* Will need a 3rd-party library for minimize to tray

### Re-implement form surfaces - In Progress

* `Form_Main` -> `MainWindow`
* `Form_Benchmark` -> `BenchmarkWindow`
* `Form_Settings` -> `SettingsWindow`
* and others...

### Re-implement GUI logic

* Minimize to tray
* Linking buttons to windows
* Setup application identity (name, icon, etc.)
* Translations
* and others...

## Phase 2

* ~~Break out miner logic~~
* and others...

## Phase 3

Will likely involve reimagining GUI-business interface to make use of data binding.
