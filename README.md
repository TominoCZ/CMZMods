# CastleMiner Z Modding
This is a CastleMinerZ Mod Loader I built using the [Harmony](https://github.com/pardeike/Harmony) library.

At the moment it is very barebones but it's proof of concept

## How to use
The Mod Loader itself is in the ``@LOADER`` folder. To install it, paste the files into the game's directory.
To install mods, create a ``mods`` folder in the game's directory (it creates itself) and paste your mod DLL inside (along with any library DLLs your mod is using, the loader will recognize mod files).

## The Example Mod
I've included my Resource Pack and an Example Mod.
To install the Resource Pack mod, copy both ``ResourcePacks.dll`` and ``FastBitmapLib.dll`` into the ``mods`` folder.

To install resource packs, create a ``resourcepacks`` folder (it creates itself) in the game's directory and paste in the example packs from ``@PACKS``.

You can switch between packs by pressing DELETE while playing.
