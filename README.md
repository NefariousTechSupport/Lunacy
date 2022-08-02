# Lunacy Level Editor

Level editor for the Ratchet and Clank PS3 games along with the Resistance series

## Building

* cd into the directory with Lunacy.sln
* Run `dotnet build`

## Running

* Run `Lunacy.exe <path to folder>`, where the folder contains either the main.dat file or the assetlookup.dat file
* Controls are as such:
  * hold right click + move mouse to look around
  * wasd to move, shift to move faster
  * p shows the names of all objects that the mouse is hovering over
  * select an object in either the regions or zones windows to teleport to that object
 
## For Devs

* use the arg `--load-ufrags` to enable the loading of ufrags