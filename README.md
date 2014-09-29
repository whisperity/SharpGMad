SharpGMad
=========

SharpGMad is a graphical tool used to manipulate [Garry's
Mod](http://en.wikipedia.org/wiki/Garry%27s_Mod)'s workshop addon packages
(aka: _gma_ files).

The program had been originally created as a straight reimplementation in
C# of Garry Newman's [GMad](http://github.com/garrynewman/gmad) tool, but
ever since it has been given more functionality.

(The current version implements and complies with
[`a121a70`](http://github.com/garrynewman/gmad/tree/a121a70e298ab6e07fa77a5e4f72018c7480f758).)

Usage
-----

### `gmad` mode

Because SharpGMad provides the legacy interface of gmad, all of its
operations are supported.

#### Create new GMA file from a folder

`SharpGMad.exe create -folder "C:\path\to\addon\folder\" -out
"C:\where\to\save\file\out.gma"`

#### Extract an existing file

`SharpGMad.exe extract -file
"C:\steam\etc\garrysmod\addons\my_addon_12345.gma" -out "C:\this\folder"`

### Conversion mode

Garry's Mod 12 _loose_ addon operations are also supported. This way,
SharpGMad will use an existing `info.txt` and ask the user for missing
metadata. Discovering that the source `-folder` is a loose structure is
done automatically.

To extract to the old layout, specifiy `-gmod12` as an argument to the
`extract` command.

The `gmad` and conversion modes are available in the GUI too, under
_Legacy operations_.

### Realtime mode

The main feature of SharpGMad is the realtime wrapper. With it, you can
load a GMA file like how you load an archive (zip, rar, tar) and
access/update the contents of the addon without having to do a full
extract and create run.

The realtime mode also supports _exporting_ a single file to a specific
location on the hard drive where you can edit it with an appropriate
editor. If a file is exported and then changed, it only takes two clicks
to _pull_ these changes and update the addon with them.

Exported files are shown with a blue filename, while changed (so-called
_pullable_) expors are written with purple.

Both a graphical and a command-line interface is available for realtime
mode.

#### GUI

`SharpGMad.exe [path]`

If the program is started with no command-line arguments (or the only
argument is a file path), a GUI will load. So you can **drag & drop** a
file in Explorer onto SharpGMad and it will be opened automatically.

![Screenshot of GUI mode](Screenshot.png)

#### Console

`SharpGMad.exe realtime`

Optionally, you can specify a file to be loaded initially

`SharpGMad.exe realtime -file "C:\steam\etc\garrysmod\addons\my_addon_12345.gma"`

You can execute the command `help` in the internal shell to get a list of
all supported commands. Every operation of the GUI is represented by a
corresponding command-line variant.

Compiling and requirements
--------------------------

SharpGMad is written using the .NET 4.0 framework. This is the only
requirement, you can compile the solution with any development environment
on any computer compatible with .NET 4.0.

Disclaimer
----------

The program _SharpGMad_ is provided "AS IS" without any expressed or
implied warranties. A general rule of thumb is that you should **NEVER**
meddle with files you hadn't made backup of beforehand. The creators
refuse liability to any damage caused by the software.
