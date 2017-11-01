<center><img src="https://i.imgur.com/Hg5XOIF.png" alt="About Dialog screenshot" /></center>

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

## Why mixing C#/C/C++?
~~Why not?~~ Actually, the original application was completely written in C. I decided to rewrite it from scratch using
managed code because of reasons but some legacy code (for capturing the screen, "_advanced_" native procedures...)
remained as a C++/CLI assembly referenced by the managed application (!)

Motivated by the need of linking native stuff statically in favour of portability, all these snippets are being slowly
reimplemented in purely managed code. In the meantime, there's a pure native library
([cn2helper](https://github.com/CaptainApp/Captain/tree/master/cn2helper)) that implements native stuff that is a bit
too complicated to be implemented in C# (see: COM objects, weird macros...) but that, again, will be eventually
replaced by a managed counterpart.

There are, however, parts of the application that would be impractical to reimplement, like the remote hook DLL
([cn2rthelper](https://github.com/CaptainApp/Captain/tree/master/cn2helper)); and others that just do their job and do
not benefit at all from the perks of .NET (see [cn2wowbr](https://github.com/CaptainApp/Captain/tree/master/cn2wowbr).)

## Building
```
$ git clone --recursive https://github.com/CaptainApp/Captain
$ cd Captain
$ nuget restore
$ devenv Captain.sln /Build
```

## What's done?
_(In order of priority)_
- [x] Bare-bones [GUI application](https://github.com/CaptainApp/Captain/tree/master/Captain.Application) (WIP)
- [x] Basic [extensibility](https://github.com/CaptainApp/Captain/tree/master/Captain.Common) support (WIP)
- [x] Basic [functionality](https://github.com/CaptainApp/Captain/tree/master/Captain.Plugins.BuiltIn) (WIP)
- [x] Screen [capture foundations](https://github.com/CaptainApp/Captain/tree/master/Captain.Application/Source/Capture/VideoProviders)
- [x] User-defined [tasks](https://github.com/CaptainApp/Captain/tree/master/Captain.Application/Source/Tasks) (WIP)
  - [ ] Import from existing [ShareX](https://github.com/ShareX/ShareX) installation
  - [ ] Import from [ShareX Custom Uploader](https://github.com/ShareX/CustomUploaders) files
- [ ] Migrating remaining WPF to Windows Forms - reimplement grabber UI in
      [cn2rthelper](https://github.com/CaptainApp/Captain/tree/master/cn2rthelper) with Direct2D
- [ ] Getting rid of [cn2helper](https://github.com/CaptainApp/Captain/tree/master/cn2helper)
- [x] Consider moving [Captain.Plugins.BuiltIn](https://github.com/CaptainApp/Captain/tree/master/Captain.Plugins.BuiltIn)
      to [Captain.Application](https://github.com/CaptainApp/Captain/tree/master/Captain.Application)
- [x] Recording and encoding desktop audio and
      [video](https://github.com/CaptainApp/Captain/tree/master/Captain.Application/Source/Capture/Encoders/H264CaptureEncoder.cs)
      (still a PoC)
- [ ] Command-line interface
- [ ] Built-in support for [ShareX](https://github.com/ShareX/ShareX)
      [custom uploaders](https://getsharex.com/docs/custom-uploader)
- [ ] Documentation for plugin developers
- [ ] Basic built-in image/video editor (filters, text, shapes...)
- [x] Installer/[updater](https://github.com/CaptainApp/Captain/tree/master/Captain.Application/Source/Update) UX (WIP)
- [ ] Portable variant of the application that *very unperformantly* could extract dependencies, user options and
      installed plugins in a temporary path and, with a helper utility, *repack* all these files in the application
      executable ('cause why not?)
- [ ] Tests. Lots of them.
- [ ] Consider splitting all the projects in the solution across different repositories.
- [ ] Localization
- [ ] Capturing exclusive-mode DirectX applications via hooks

## What's with that version number?
~~The author likes pretending to have released a lots of versions for a product.~~
This app is actually the sucessor of [another application](https://github.com/sanlyx/cup) with the same concept made by
the same author.

This project results from the intent of providing the community a **functional** product, rather than some buggy,
unmaintained app tailored to personal usage.

## Extending Captain
Refer to the [Captain.Common](https://github.com/CaptainApp/Captain.Common) source tree for more on this.  
There will be documentation, eventually.

## Contributing
_To-do :)_

## Licensing
This software is made up of different components that may not necessarily be licensed under the same terms.
Refer to the `LICENSE.md` file in the top-level directory of each project to find further details about their
licensing.

Major components are licensed under the [simplified BSD license](https://opensource.org/licenses/BSD-2-Clause)
(a.k.a. BSD 2-Clause), with other smaller parts released [into the public domain](http://unlicense.org/).