SharpGMad
=========

SharpGMad is a reimplementation of Garry Newman's 
[GMad](http://github.com/garrynewman/gmad) application.

_GMad_ is used to manipulate Garry's Mod (his game) addon files and is
written in C++. SharpGMod takes it to be in C#, originally written directly
from the code available by garry.

(The current version of SharpGMad implements and complies with
[`f2a0de4`](http://github.com/garrynewman/gmad/tree/f2a0de42f5d124221ea080f18f338cf8fc23c15f).)

Ever since, SharpGMad's code has been refurbished to match more with
development style used in C# applications, and to, of course, support new
opportunities.

Usage
-----

Currently three operation modes are supported:

### [`gmad`](http://github.com/garrynewman/gmad) (legacy) mode

This code is a straight mirror-implementation, so the command interface is the
same.

`SharpGMad.exe <command> <options>`

To create a new .gma file

`SharpGMad.exe create -folder "C:\path\to\addon\folder\" -out
"C:\where\to\save\file\out.gma"`

To extract an existing .gma file into its parent folder

`SharpGMad.exe extract -file
"C:\steam\etc\garrysmod\addons\my_addon_12345.gma"`

To extract an existing .gma file into another folder

`SharpGMad.exe extract -file "C:\steam\etc\garrysmod\addons\my_addon_12345.gma"
-out "C:\this\folder"`

### Realtime access

What is the main feature of SharpGMad, however, is the so-called realtime
wrapper. With it, you can load a GMA file like any other archive (zip,
rar, tar, ...) and update the contents of the files in it without having
to fully extract and then repack the file.

In realtime, you can also _export_ a single file to a specific location in
your system and see/run/edit. If a file is exported and then edited
"outside", SharpGMad offers the possibility to _pull_ the changes and
instantly update the addon with it. Exported files are indicated with
blue file name, whilst changed files are purple. 

The realtime wrapper comes in two flavours.

#### GUI

`SharpGMad.exe [path]`

If the program is started with no command-line arguments (or the only
argument is a file path), a GUI will load. (Because of this, dragging and
dropping a file onto SharpGMad in Explorer is supported.)

![Screenshot of GUI mode](Screenshot.png)

#### Console

`SharpGMad.exe realtime`

Optionally, you can specify a .gma file to be loaded initially

`SharpGMad.exe realtime -file "C:\steam\etc\garrysmod\addons\my_addon_12345.gma"`

#### Realtime functionality

The following operations are supported in realtime mode.
(The `command` is applicable in CLI-mode. Most features have a corresponding, 
easy operation in GUI.)

* _Addon operations_
 * `new <filename>`: Create new addon and save it as `<filename>`
 * `load <filename>`: Load an existing addon saved as `<filename>`
 * `push`: Save changes to the disk
 * **Console only!** `close`: Drop all changes and close the addon without
saving
* **Console only!** _Filesystem operations_
 * `path`: Print the full file path of the currently opened addon
 * `pwd`: Print current working directory
 * `cd <folder>`: Change working directory to `<folder>`
 * `ls`: List all files in the current folder
* _Addon file contents_
 * `add <filename>`: Add `<filename>`
 * **Console only!** `addfolder <folder>`: Add all files from `<folder>`
 * `list`: List the files currently added
 * `remove <filename>`: Remove `<filename>`
 * `extract <filename> [path]`: Extract `<filename>` to the current folder
(or to `[path]` if specified). Unlike `export`, a plain `extract` does not
set up a realtime change watch.
 * `mget <folder> <f1> [f2...]`: Extract all specified files to `<folder>`
* _Exporting and pulling changes_
 * **Console only!** `export`: List all currently handled exports
 * `export <filename> [path]`: Export `<filename>` for editing to the
current folder (or to `[path]` if specified)
 * `pull`: Pull all changes from all exported files
 * `pull <filename>`: Pull changes of `<filename>` (the parameter
indicates the path of the file **within** the addon, not the path of the
exported file)
 * `drop <filename>`: Drop the export and delete the exported file
(`<filename>` is file path **within** the addon)
* _Metadata operations_
 * **Console only!** `get` or `set`: List the handled metadata parameters
 * `get <parameter>`: Print the value of `<parameter>`
 * `set <parameter> <value>`: Sets the value of `<parameter>` to `<value>`
* **Console only!** `gui`: Load a GUI window
* **Console only!** `help`: Show the list of available commands
* `exit`: Exit the application

Compiling and requirements
--------------------------

SharpGMad is written for the .NET 4.0 framework. This is the only
requirement, you can compile the solution with any development environment
on any computer compatible with .NET 4.0.
