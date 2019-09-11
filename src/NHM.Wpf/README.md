# Overview of WPF project

There are two important main folders in the project: Views and ViewModels.

## Views

This folder contains all the XAML markup and some basic code-behind for the user interface. All the code in this folder 
should be directly related to the "View" part of MVVM. 

The XAML files in the folder root mostly have a direct correspondence to the form files in the WinForms project. 

### Settings

The `Settings` folder here contains all of the views for the settings window. In the root is `SettingsWindow.xaml` which 
declares the window that will host different settings pages, as well as a `TreeView` for navigation. The `Pages` folder
contains `UserControl` definitions for the different settings pages. Through data binding the selected entry in the `TreeView`
will have its page shown in the content host.

The `Settings` folder also has a `Controls` folder. Currently this contains a single file that declares a custom control
for setting entries, and is used by all pages as well as in `BenchmarkWindow.xaml`.

### Plugins

The `Plugins` folder contains the controls related to the plugins window. In a similar way to the settings window, the plugins
window hosts content from one of two `UserControl`s. Initially the plugin list control (`PluginList.xaml`) is shown, and it is 
switched to a plugin detail control (`PluginDetail.xaml`) when the details button is clicked.

The `Plugins` folder also contains a `Controls` folder for custom controls used only in the plugin views.

### Common

In the `Common` folder is 1) `WindowUtils.cs` that contains helper methods for the views, and 2) `Styles.xaml` that is a 
resource dictionary of various styles used throughout the GUI. 

## ViewModels

This folder contains the interface logic between backend code and UI. All the main windows have a corresponding ViewModel.
Each ViewModel exposes properties that can be data bound in markup. There are also several subfolders:

### Models

`Models` mostly contains wrapper classes of backend classes (`ComputeDevice`, etc.) for use with UI. Some of these models
implement `IDisposable` for the purpose of unhooking callbacks; it is imperative that they are disposed of properly or else
instances may not be garbage collected.

### Settings

All of the ViewModels related to the settings window are collected here. `SettingsVM` is the main VM used for the window.

Each page has its own ViewModel. In `SettingsWindow.xaml` data templates are used to describe a correspondence between 
the page VMs and their markups. Changing the `SelectedVM` property on `SettingsVM` will automatically update the content
to show the markup for that page.

`SettingsBaseVM` is the base class for all page VMs. This is the class used to populate the `TreeView` in the settings window.
Each page VM can optionally have children that will show nested in the `TreeView`. They all also have a title for the `TreeView`.

`SettingsContainerVM` is a special derivation of `SettingsBaseVM` that simply holds child VMs (thus it acts as a folder
in the `TreeView`). 

### Plugins

All of the ViewModels related to the plugin window are collected here. In a similar way to settings VMs, `PluginListVM` and
`PluginDetailVM` are used to select which markup will be displayed in the plugins window content. These two classes implement
`IPluginPageVM`.

## Misc. folders

Some misc. folders contain smaller classes.

### Converters

The classes in `Converters` implement `IValueConverter` for the use of type conversion in data binding. `ConverterBase` is an
abstract class that can be used for type-safe implementation of `IValueConverter`.

### Validators

Validators can be attached to data bindings of user inputs, e.g. `TextBox`s. They validate the input, and if an error is found, 
return a result with a friendly error message. This sets the `TextBox` to an invalid state (red outline). Through styling,
the error message can be displayed as a tooltip.
