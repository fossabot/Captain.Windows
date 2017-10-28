# Captain.Common
![version: 0.1](https://img.shields.io/badge/version-0.1-blue.svg)
![license: Unlicense](https://img.shields.io/badge/license-Unlicense-brightgreen.svg)
> Code shared between [Captain](https://github.com/CaptainApp) and its plugins

## What's this?
[Captain](https://github.com/CaptainApp) is an extensible tray app that acts as a foundation for implementing
custom screen capturers. This is a shared library for creating plugins.

## Installing
```
PM> Install-Package Captain.Common
```

## Distributing your plugin
Just ship the assembly DLL and Captain will take care of the rest. Do not include a copy of the `Captain.Common`
assembly alongside yours, as Captain will automatically hook on the assembly load event and use its own copy.

## Documentation
_None yet (:_

## Examples
- [Captain.Plugins.BuiltIn](https://github.com/CaptainApp/Captain/tree/master/Captain.Plugins.BuiltIn): Built-in plugin
  implementing basic funcionality.