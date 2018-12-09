# 作りかけで申し訳ない
4k intro released at TokyoDemoFest2018

[pouet page](http://www.pouet.net/prod.php?which=79364)

### Requirements
* Windows 8.1 or newer
* OpenGL 4.5

### Required softwares to build
* [Crinkler](http://crinkler.net/)
* [Nim 0.19.9 or newer](https://github.com/nim-lang/Nim)
* [oldwinapi](https://github.com/nim-lang/oldwinapi)
* [opengl](https://github.com/nim-lang/opengl)
* Microsoft Visual Studio Community 2015 (Microsoft Visual C++ 2015)

### How to build
Install all required softwares and set PATH environment variable
so that nim and crinkler can be called from command line.
Build debug build before building release build.

Debug build:
```console
cd Tukurikake/src
nim c zrks.nim
```

Release build:
Execute following commands from VS2015 x86 Native Tools command prompt.
```console
cd Tukurikake/src
nim e build4kintros.nims
```

If you want to make 4k intro using Nim language like this demo, check [nim-4k-intro-sample](https://github.com/demotomohiro/nim-4k-intro-sample).
