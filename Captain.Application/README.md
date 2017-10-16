# Captain.Application
![version: 0.6](https://img.shields.io/badge/version-0.6-blue.svg)
![license: BSD 2-Clause](https://img.shields.io/badge/license-BSD_2--Clause-brightgreen.svg)
> Native GUI application that implements most of the [Captain](https://github.com/CaptainApp) logic.

## What's this?
It's the _actual_ code for [Captain](https://github.com/CaptainApp), an extensible screen capturing application.

## Compatibility
The application is designed to work with Windows Vista SP2 onwards. Windows XP support is not present nor planned, as
we depend upon the .NET Framework 4.5, which is not compatible with this platform.

## Project structure
- `Content/` - Contains WPF assets and XAML files.
- `Resources/` - Contains images, icons and other assets used by the *non-WPF parts* of the application.
- `Source/` - Contains application logic and utilities not directly related with the user interface.
- `UI/` - Contains user interface logic, Windows Forms generated files and utility classes used by the UI.

## Localization
Captain uses .NET resource files (`*.resx`) which include strings and other assets that may be localized. For
translating the UI, you may use the built-in Windows Forms designer features in Visual Studio to modify strings and
other properties. You may want to look to the `Resources/Resources.resx` file, which contains other strings that
are used to display notifications, dialogs and other strings not directly bound to a Windows Forms control.

## Open-source code
This software depends upon awesome open-source software without which it could not be possible.

### Third-party
> Some directories contain a `README.md` file which briefly explains the contents and its original authority.
Additionally, some code snippets, lines and methods have been appropriately annotated to include their original sources
in most cases.

These open-source projects are also being used (albeit with no actual source code of these being included):
- [Microsoft/UWPCommunityToolkit](https://github.com/Microsoft/UWPCommunityToolkit), as a
  [NuGet package dependency](https://www.nuget.org/packages/Microsoft.Toolkit.Uwp/).  
  Copyright (c) .NET Foundation and Contributors
- [EasyHook/EasyHook](https://github.com/EasyHook/EasyHook/), as a
  [NuGet package dependency](https://www.nuget.org/packages/EasyHookNativePackage/).  
  Copyright (c) 2009 Christoph Husse & Copyright (c) 2012 Justin Stenning
- [sharpdx/SharpDX](https://github.com/sharpdx/SharpDX), as several
  [NuGet package dependencies](https://www.nuget.org/packages?q=Tags%3A%22SharpDX%22).  
  Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
- [Squirrel/Squirrel.Windows](https://github.com/Squirrel/Squirrel.Windows) and 
  [its dependencies](https://github.com/Squirrel/Squirrel.Windows/blob/master/src/Squirrel.nuspec#L11), as a
  [NuGet package dependency](https://www.nuget.org/packages/Squirrel.Windows).  
  Copyright (c) 2012 GitHub, Inc.
- [Ookii.Dialogs](http://www.ookii.org/software/dialogs/), as a
  [NuGet package dependency](https://www.nuget.org/packages/Ookii.Dialogs.WindowsForms/).  
  Copyright © Sven Groot (Ookii.org) 2009