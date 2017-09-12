# cn2rthelper
![license: BSD 2-Clause](https://img.shields.io/badge/license-BSD_2--Clause-brightgreen.svg)
> **Cyanogen Helper Library for Remote Targets** - Dynamic library injected to remote processes in order to modify window behaviours for implementing diverse [Captain](https://github.com/CaptainApp/Captain) features.

## What's this?
In order to support certain Captain features (i.e. displaying capture HUD on fullscreen DirectX applications, or attaching windows to Captain), we need to execute code in remote processes.
To achieve this, a DLL is injected onto the target process, which modifies its behaviour.

## Why *cn2*?
Originally, Captain used C++/CLI libraries for mixing native and managed code, but in favour of portability these libraries have been superseeded by pure native ones.
These *revamped* libraries are part of the *Captain Native Support Libraries Version 2*.