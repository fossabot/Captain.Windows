# Captain.Application.Native
## What's this?
These files contain native Windows API struct, type, enum and P/Invoke function declarations.
Files in the current directory are related to the [cn2helper](https://github.com/CaptainApp/cn2helper),
[cn2rthelper](https://github.com/CaptainApp/cn2rthelper) and [cn2shared](https://github.com/CaptainApp/cn2shared)
projects.

## Authors
### [Andrew Arnott (AArnott)](https://github.com/AArnott)
Some of these files have been adapted from sources from the [AArnott/pinvoke](https://github.com/AArnott/pinvoke)
repository.

- `Gdi32`
  - `Gdi32.cs`
- `User32`
  - `POINT.cs`
  - `RECT.cs`
  - `User32.cs`
  - `WINDOWPOS.cs`

### [Microsoft Reference Source for .NET Framework](https://referencesource.microsoft.com/)
> Microsoft Avalon  
> Copyright (c) Microsoft Corporation, All Rights Reserved

- `PropIdl`
  - `PROPVARIANT.cs`
    - [`PropVariant.Init(string[] value, bool fAscii)`](https://referencesource.microsoft.com/#PresentationCore/Core/CSharp/System/Windows/Media/Imaging/PropVariant.cs,209)