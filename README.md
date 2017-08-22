# Captain
![version: 0.6.0.0](https://img.shields.io/badge/version-0.6.0.0-blue.svg)
> The minimal, extensible screen capturer.

## What's this?
Captain is an extensible screen capturing app that Just Works™ (on Windows).
It is intended to take screenshots and record video from your screen and perhaps save captures to a file or upload them to the Internet or whatever.

Did I mention it can capture accelerated graphics such as those in games, fullscreen or not? Well it does that, too.

## Why not ShareX?
The author **loves** [ShareX](https://github.com/ShareX/ShareX) and has used it for a looong time, but they're also a maniatical snob who can't cope
with those raw Windows Forms clinging around the screen and threatening kids with those innocent yet ugly `Microsoft Sans Serif` labels.

Captain does not aim to be an alternative to ShareX, but a simple app to suit its author's current and future needs by emphasizing extensibility.

## Building
```
$ git clone --recursive https://github.com/CaptainApp/Captain
$ cd Captain
$ nuget restore
$ devenv Captain.sln /Build
```

## What's done?
_(In order of priority)_
- [x] Bare-bones [GUI application](https://github.com/CaptainApp/Captain.Application) (WIP)
- [x] Basic [extensibility](https://github.com/CaptainApp/Captain.Common) support (WIP)
- [x] Basic [functionality](https://github.com/CaptainApp/Captain.Plugins.BuiltIn) (WIP)
- [ ] Documentation for plugin developers
- [ ] Command-line interface
- [ ] Built-in support for [ShareX](https://github.com/ShareX/ShareX) [custom uploaders](https://getsharex.com/docs/custom-uploader)

## What's with that version number?
~~The author likes pretending to have released a lots of versions for a product.~~
This app is actually the sucessor of [another application](https://github.com/sanlyx/cup) with the same concept
made by the same author.

This project results from the intent of providing the community a **functional** product, rather than some buggy,
unmaintained app tailored to personal usage.

## Extending Captain
Refer to the [Captain.Common](https://github.com/CaptainApp/Captain.Common) repository for more on this.
You can also take a look to the [built-in plugin](https://github.com/CaptainApp/Captain.Plugins.BuiltIn) for a gist on how encoders and output streams work.