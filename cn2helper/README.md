# cn2helper
![license: BSD 2-Clause](https://img.shields.io/badge/license-BSD_2--Clause-brightgreen.svg)
> **Helper Utility Library** - Contains native routines used by
> [Captain.Application](https://github.com/CaptainApp/Captain.Application).

⚠ **Do not add more features to this library. Whenever possible, implement these using managed code in
[Captain.Application](https://github.com/CaptainApp/Captain.Application).**

## What's this?
These are a set of utility routines used by [Captain.Application](https://github.com/CaptainApp/Captain.Application) UI.
Some of these functions are nothing but a **quick hack **to get stuff done and focus in major features, and **are to be
reimplemented** within a managed assembly whenever possible to reduce the number of dependencies for the sake of
portability.

## Why *cn2*?
Originally, Captain used C++/CLI libraries for mixing native and managed code, but in favour of portability these
libraries have been superseeded by pure native ones. These *revamped* libraries are part of the *Captain Native Support
Libraries Version 2*.