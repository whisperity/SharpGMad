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

Compiling and requirements
--------------------------

SharpGMad is written for the .NET 4.0 framework. This is the only
requirement, you can compile the solution with any development environment
on any computer compatible with .NET 4.0.
