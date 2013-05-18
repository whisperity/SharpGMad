SharpGMad
=========

SharpGMad is a reimplementation of Garry Newman's 
[GMad](http://github.com/garrynewman/gmad) application.

_GMad_ is used to manipulate Garry's Mod (his game) addon files and is
written in C++. SharpGMod takes it to be in C#, written directly from the
code available by garry.

(The current version of SharpGMad implements [8b1c7b9011](http://github.com/garrynewman/gmad/tree/8b1c7b9011d81ef0f7378eae482a6a94a6536b0e).)

Usage
-----

This code is a straight mirror-implementation, so the command interface is the
same.

`SharpGMad <command> <options>`

To create a new .gma file

`SharpGMad.exe create -folder "C:\path\to\addon\folder\" -out
"C:\where\to\save\file\out.gma"`

To extract an existing .gma file into its parent folder

`SharpGMad.exe extract -file
"C:\steam\etc\garrysmod\addons\my_addon_12345.gma"`

To extract an existing .gma file into another folder

`SharpGMad.exe extract -file "C:\steam\etc\garrysmod\addons\my_addon_12345.gma"
-out "C:\this\folder"`

The realtime console
--------------------

The realtime console lets you "mount" an addon into the application and
use it like it was a simple archive.

To use it, simply start the program as

`SharpGMad.exe realtime`

Optionally, you can specify a .gma file to be loaded initially

`SharpGMad.exe realtime -file "C:\steam\etc\garrysmod\addons\my_addon_12345.gma"`

Once running, the following commands will be available to you

    load <filename>          Loads <filename> addon into the memory
    new <filename>           Creates a new, empty addon named <filename>
    list                     Lists the files in the memory
    remove <filename>        Removes <filename> from the archive
    push                     Writes the addon to the disk
    close                    Writes the addon and closes it
    abort                    Unloads the addon from memory, dropping all changes
    path                     Prints the full path of the current addon
    help                     Show the list of available commands
    exit                     Exits

Compiling and requirements
--------------------------

SharpGMad is written for the .NET 4.0 framework. This is the only
requirement, you can compile the solution with any development environment
on any computer compatible with .NET 4.0.
