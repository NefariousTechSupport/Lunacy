# Lunacy Level Editor

Level editor for the Ratchet and Clank PS3 games along with the Resistance series

## Building

* cd into the directory with the `Lunacy.sln` file
* Run `dotnet build`

## Running

### Lunacy

* Run `Lunacy.exe <path to folder>`, where the folder contains either the `main.dat` file or the `assetlookup.dat` file
* if `assetlookup.dat` is there, `highmips.dat` from `level_uncached.psarc` must be included as well
* Controls are as such:
  * hold right click + move mouse to look around
  * wasd to move, shift to move faster
  * p shows the names of all objects that the mouse is hovering over
  * select an object in either the regions or zones windows to teleport to that object

### AssetExtractor

* Run `AssetExtractor.exe <path to folder>`, where the folder contains either the `main.dat` file or the `assetlookup.dat` file
* if `assetlookup.dat` is there, `highmips.dat` from `level_uncached.psarc` must be included as well
* Assets will be found in the inputted folder

## Notes

* Including the `texstream.dat` file in the same place as `main.dat` will improve texture resolution (found in `level_textures.psarc`)
* Including the `debug.dat` file for a level in the same place as `main.dat` will include asset and instance names

## For Devs

* use the arg `--load-ufrags` to enable the loading of ufrags