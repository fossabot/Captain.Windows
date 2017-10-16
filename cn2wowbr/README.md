# cn2wowbr
![license: BSD 2-Clause](https://img.shields.io/badge/license-BSD_2--Clause-brightgreen.svg)
> **Windows 32-bit on Windows 64-bit Bridge** - Small internal utility for performing library injection on
> 32-bit processes under 64-bit platforms.

## What's this?
*See [issue #94](https://github.com/EasyHook/EasyHook/issues/94) for
[EasyHook/EasyHook](https://github.com/EasyHook/EasyHook).*

When we need to inject native libraries onto remote 32-bit processes under 64-bit platforms, we can not do so directly
from a 64-bit process (like [Captain.Application](https://github.com/CaptainApp/Captain.Application)). In these cases,
we call this little utility which handles the injection for us, and since it's a 32-bit process the injection can be
performed just fine.

This also lets Captain inject its helper library onto itself, allowing to capture its own windows even with EasyHook
being not able to inject code onto the caller process.

## Why *cn2*?
Originally, Captain used C++/CLI libraries for mixing native and managed code, but in favour of portability these
libraries have been superseeded by pure native ones. These *revamped* libraries are part of the *Captain Native Support
Libraries Version 2*.