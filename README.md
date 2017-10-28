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
to the Internet or whatever.

Did I mention it ~~can~~ is meant to capture accelerated graphics such as those in games, fullscreen or not? Well it
~~might~~ might eventually do that, too.

## Why not ShareX?
The author **loves** [ShareX](https://github.com/ShareX/ShareX) and has used it for a looong time, but they're also a
maniatical snob who can't cope with those raw Windows Forms clinging around the screen and threatening kids with those
innocent yet ugly `Microsoft Sans Serif` labels.

Captain **does not aim to be an alternative to ShareX**, but a simple app to suit its author's current and future needs
by emphasizing extensibility.

## Building
```$ git
 clone --recursive https://github.com/CaptainApp/Captain
$ cd Captain
$ nuget restore
$ devenv Captain.sln /Build
```

## What's done?
_(In order of priority)_
- [x] Bare-bones [GUI application](https://github.com/CaptainApp/Captain/tree/master/Captain.Application) (WIP)
- [x] Basic [extensibility](https://github.com/CaptainApp/Captain/tree/master/Captain.Common) support (WIP)
- [x] Basic [functionality](https://github.com/CaptainApp/Captain/tree/master/Captain.Plugins.BuiltIn) (WIP)
- [x] Screen [capture foundations](https://github.com/CaptainApp/Captain/tree/master/Captain.Application/Source/Capture)
- [x] User-defined [tasks](https://github.com/CaptainApp/Captain/tree/master/Captain.Application/Source/Tasks) (WIP)
- [ ] Migrating remaining WPF to Windows Forms - reimplement grabber UI in
      [cn2rthelper](https://github.com/CaptainApp/Captain/tree/master/cn2rthelper) with Direct2D
- [ ] Getting rid of [cn2helper](https://github.com/CaptainApp/Captain/tree/master/cn2helper)
- [ ] Consider moving [Captain.Plugins.BuiltIn](https://github.com/CaptainApp/Captain/tree/master/Captain.Plugins.BuiltIn)
      to [Captain.Application](https://github.com/CaptainApp/Captain/tree/master/Captain.Application)
- [ ] Recording and encoding desktop audio and video
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

## What's with that version number?
~~The author likes pretending to have released a lots of versions for a product.~~
This app is actually the sucessor of [another application](https://github.com/sanlyx/cup) with the same concept made by
the same author.

This project results from the intent of providing the community a **functional** product, rather than some buggy,
unmaintained app tailored to personal usage.

## Extending Captain
Refer to the [Captain.Common](https://github.com/CaptainApp/Captain.Common) source tree for more on this.
You can also take a look to the [built-in plugin](https://github.com/CaptainApp/Captain/tree/master/Captain.Plugins.BuiltIn)
for a gist onhow encoders and output streams work.

## Contributing
_To-do :)_

## Licensing
This software is made up of different components that may not necessarily be licensed under the same terms. Refer to the `LICENSE.md` file in the top-level directory of each project to find further details about their licensing.

Major components are licensed under the [simplified BSD license](https://opensource.org/licenses/BSD-2-Clause) (a.k.a. BSD 2-Clause), with other smaller parts released [into the public domain](http://unlicense.org/).