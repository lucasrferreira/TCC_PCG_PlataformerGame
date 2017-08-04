The Corgi Engine is a complete Unity 2D platformer engine.
Basically it'll allow you to create your own platformer game. 
It comes complete with lots of assets, animations, particle effects, etc...

I HAVE A LOT OF QUESTIONS !
---------------------------

Have you read the FAQ at www.moremountains.com/corgi-engine-best-unity-2d-platformer#faq ?
You should read it.

WHAT'S IN THE CORGI ENGINE ?
----------------------------

The Corgi Engine contains quite a lot of stuff. 
Everything is in the Assets folder, and in it you'll find the following folders :

- /Common : a bunch of stuff (scenes, sprites, animations...) that don't belong to any particular demo, but may be used by any demo. Stuff like the startscreen, loading screen, etc, will go in there. /Common also contains the main /Scripts folder, which is the actual engine : all the scripts that make it work, grouped by theme. This includes camera scripts, character controllers, enemy AI, health management, ladders, jumpers, level management, etc... It's all sorted for you in the right folders.
- /Demos : grouped by demo types (pixel, 2d, 3d...), it contains everything from sprite to prefabs that make these demo work

In the demos/common folders you'll usually find the following structure :
- Animations : contains all the animations in the game
- Fonts : the fonts used in the GUI
- Materials : all the materials and associated sprites used in the particle effects
- Resources : all the game's prefabs. You just have to drag one of these in your scene and they'll be ready
- Scenes : a start screen and some demo levels (from the start screen press A on an xbox controller, or space on your keyboard)
- Scripts : specific scripts
- Sprites : all the sprites and spritesheets used in the game. Feel free to reuse them in your own game.

WHAT AM I SUPPOSED TO DO WITH IT ?
----------------------------------

The Corgi Engine includes everything you need to start your own platformer game.
You can either start fresh and pick the scripts you need from it, or modify the existing demo levels brick by brick, adding your own sprites and scripts as you go.

MOBILE INPUT
------------

You can activate / desactivate mobile inputs via the menu in your editor's top bar (the bar with File, Edit, etc...).
There's a "Mobile Input" entry there, just enable or disable it. Disabling it will hide all mobile inputs.
It should also automatically enable or disable it depending on your target platform when building.
Note that these mobile controls are just here as placeholders, it's just the Unity Standard Asset's inputs. You should replace them with your own.

DOCUMENTATION
-------------

A complete documentation is available for this asset, go to www.moremountains.com/corgi-engine/docs/index.html


