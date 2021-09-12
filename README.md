# ✒🌈 Inkify
A .NET 5.0 command line tool to reduce image colors, dither them, and size them to fit nicely on [Pimoroni eInk Displays](https://shop.pimoroni.com/collections/pimoroni?filter=e-ink+Displays)

## Project State
It's rough and fresh out of my brain. Build and runs on Windows & macOS. I haven't tested it yet, but I intend to run this on a Raspberry Pi, but just haven't got to that point yet. There's probably plenty that doesn't work right, so feel free to open an issue or PR if you find something.

## 💡 Inspiration
For more about what inspired this, check out [this post](https://github.com/pimoroni/inky/issues/126) over on pimoroni/inky!

## Image Output Format
Right now, images are output at PNGs. However, they're not being created as 8bpp indexed pngs. (There is code to generate the images in that format, but `System.Drawing.Bitmap` seems to always write them as 32 bit PNGs for now.

## Usage
Build it up, then invoke it from the command line. You can use the option `--help` to see the built in help, but a general use would look something like these:

```shell
$ # General help and command argument/option info
$ dotnet Inkify.dll --help

$ # Transform test.jpg using implied options:
$ # Inky Impression display,
$ # Stucki dithering algorithm,
$ # Saturated color palette
$ dotnet Inkify.dll transform "test.jpg"

$ # List available dithering algorithms (pass them to -e)
$ dotnet Inkify.dll ditherers

$ # Transform test.jpg with all available dithering algorithms,
$ # using the implicitly implied Inky Impression display as the target,
$ # with a color saturation percent of 0.7.
$ # (Will produce one output file per dithering algorithm)
$ dotnet Inkify.dll transform "test.jpg" -e All -s 0.7

$ # Transform test.jpg using the Floyd-Steinberg dithering algorithm,
$ # targeting the Inky pHat with the Black/White/Yellow display.
$ dotnet Inkify.dll transform "test.jpg" -d PhatYellow -e FloydSteinbergDithering
```

## 🌄 Screenshots
Certainly not entirely representitive, but if you just want to take a peek at the kinds of images Inkify can spit out, here you go:

![image](https://user-images.githubusercontent.com/602691/132973032-2a6bd37b-08bf-4baa-a01a-5d24d50dc9f6.png)
![image](https://user-images.githubusercontent.com/602691/132973192-8266861d-87d2-4bb9-833d-f3792b39c777.png)

## 📝 License

Inkify is licensed under the AGPL 3.0 (SPDX-License-Identifier: AGPL-3.0-only). If you're interested in Inkify under other terms, please contact the authors. Inkify makes use of several open source packages. Those packages are each covered by their own copyrights and licenses, which are available via the tooling you use to restore the packages when building.

Copyright © 2021 [Chris March Dailey](https://cmd.wtf)
This program is free software: you can redistribute it and/or modify it under the terms of the GNU Affero General Public License as published by the Free Software Foundation, version 3.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with this program. If not, see <https://www.gnu.org/licenses/>

### Licensed Software

Inkify makes uses of several licensed portions of code, each licensed under their own terms by their authors. In particular, some of those software licenses require the following notices.

 - The nuget package `cmdwtf.Dithering` is licensed under the MIT license from [cmdwtf/Dithering](https://github.com/cmdwtf/Dithering/blob/main/LICENSE). It is based on work by Cyotek Ltd.
 
