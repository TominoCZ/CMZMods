# CastleMiner Z Modding
This is a CastleMinerZ Mod Loader I built using the [Harmony](https://github.com/pardeike/Harmony) library. At the moment it is very barebones, it's a proof of concept.
![modloader](https://github.com/TominoCZ/CMZMods/assets/24359011/00f0e890-1552-47b4-af55-133166741912)

## How to use
- To install the **Mod Loader**, copy ``Modding.dll``, ``Services.Client.dll`` and ``0Harmony.dll`` into the game's directory.
- To install the **mods**, create a ``@mods`` folder in the game's directory (it creates itself) and paste your mod DLL inside (along with any library DLLs your mod is using, the loader will recognize mod files).

## Mods
I've included some example mods I made:

### IntroSkip (ExampleMod)
- This mod greatly reduces the splash screen length (the loader has a custom splash image)

### ZombieNerf
- This mod removes the player slowdown when colliding with enemies.

### Resource Packs
- To install the Resource Pack mod, copy __both__ ``ResourcePacks.dll`` and ``FastBitmapLib.dll`` into the ``@mods`` folder.
- To install resource packs, create a ``@resourcepacks`` folder (it creates itself) in the game's directory and paste in the example packs from ``@PACKS``.
- At the moment you can switch between packs by pressing DELETE while playing but there is also a **Resource Packs** tab in Options.
