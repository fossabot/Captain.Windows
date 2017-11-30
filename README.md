<p align="center"><img src="https://i.imgur.com/Hg5XOIF.png" alt="About Dialog screenshot" /></p>

# Captain
![version: 0.6](https://img.shields.io/badge/version-0.6-blue.svg)
> The minimal, extensible screen capturer.

🚧 **This is an ongoing project under active development, and is not yet meant to be usable.** 🚧

Expect this repository to introduce **lots** of breaking changes with each commit and not to comply with open-source
licenses of its dependencies (i.e. credits to original authors of libraries and code used in this software may not be
added until a release candidate.)

## What's this?
Captain is an extensible screen capturing app that Just Works™ (on Windows).
It is intended to take screenshots and record video from your screen and perhaps save captures to a file or upload them
to the Internet or whatever you'd like as long as there is a plugin for that (and if it's not go ahead and roll your own!)

Did I mention it ~~can~~ is meant to capture accelerated graphics such as those in games, fullscreen or not? Well it
~~might~~ might eventually do that, too.

## Building
```
$ git clone https://github.com/CaptainApp/Captain.Windows
$ cd Captain.Windows
$ nuget restore
$ devenv Captain.sln /Build
```

## What's with that version number?
~~The author likes pretending to have released a lots of versions for a product.~~
This app is actually the sucessor of [another application](https://github.com/sanlyx/cup) with the same concept made by
the same author.

This project results from the intent of providing the community a **functional** product, rather than some buggy,
unmaintained app tailored to personal usage.

## Extending Captain
Refer to the [Captain.Common](https://github.com/CaptainApp/Captain.Windows/Captain.Common) project tree.
There will be documentation, eventually.

## Contributing
_To-do :)_

## Licensing
This software is made up of different components that may not necessarily be licensed under the same terms.
Refer to the `LICENSE.md` file in the top-level directory of each project to find further details about their
licensing.

Major components are licensed under the [simplified BSD license](https://opensource.org/licenses/BSD-2-Clause)
(a.k.a. BSD 2-Clause), with other smaller parts released [into the public domain](http://unlicense.org/).

## What's done?
_(Note that most unchecked features are still work in progress, but have already been started.)_

- [X] Still image capture
- [ ] Video capture
  - [ ] Audio capture
- [ ] UI
  - [X] About dialog
  - [ ] Options
    - [X] General
    - [X] Tasks
    - [ ] Capture
    - [ ] Advanced
  - [ ] Post-capture graphics tooling
  - [X] HUD
    - [ ] In-game HUD
  - [ ] CLI
- [ ] Extensibility
  - [ ] Stable API
  - [ ] Documentation
  - [ ] Compatibility with ShareX Custom Uploaders
- [ ] Installer/updater UX
- [ ] Screen capture foundations
  - [X] GDI
  - [X] DXGI desktop duplication
  - [ ] DWM SharedSurface's
  - [ ] DirectX hooks on platforms without DXGI 1.5/desktop duplication
- [ ] Post-capture
  - [X] Actions
    - [X] Pre-encoding actions
    - [ ] Filters