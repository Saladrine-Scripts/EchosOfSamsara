Objectives:
Open dll files with ./CodemerxDecompile/CodemerxDecompile



When Player dies copy player inventory and save as a dat/json file.
When Player dies increase death count which changes file names for DeadDrop.
File name DeadDropLootx (x replaced with death count value)
Add DeadDrop Structure
One time use chest colapses when emptied.
Pull loot from a random DeadDropLoot file.

Base stuctures:
Identify player bases via different actions
Placing items like chests, anvils, farmland, Paths, Reinforced blocks, and doors identify base locations.
Dirt huts and bear pits are saved as structures
Needs a way to assign reasource values to different structures
If a room is made count it as a base. Adding all room walls to base structure.
player edits within a range of 3 blocks of a base are incorperated into the base.
3 block high edits count as the base for areas of 12 blocks of edits.
At death show a menu with a sample of each base with the following info
    Reasource values
    Structure size (Height width, length)
    Structure type (hut, farm, Megastructure, etc)
    Model of the base.
    Buttons with: "Preserve Memory", "Forsake Memory"


Items:
Fences of rememberance
    Defines an area that is saved as a base but is not included in base structure when spawned but ruin doesn't undergo erosion'.
Gate of rememberance
    Same as a Fence but can open like a gate.

Erosion:
Thematically there is an expectation that time passes between worlds to simulate time passing certain adjustments would be made.
Primitive survival furrows would be changed to dirt of the same quality.
low fert stays as low fert.
Med fert turns to low fert
high fert turns to med fert
teraprita turns to high fert

Farmland turns into dirt of the same quality
planted farmland will have wild crops on the dirt
Fertilized farmland will turn into dirt of a higher quality.
Reinforced blocks will not change unless it's and improvement
Unsuported stone will turn into gravel(over 3 block overhang)
Hardwood will turn into aged varients.
Softwoods will turn into dirt with mushrooms
Rooms will gain a layer of dirt on the floor.
food in containers will turn to rot
Red meat will turn into vintage meat.
Gravel will turn into sand.
Trees connected to leaves will remain the same.
seeds in inventories will stay the same.
crops that turn to rot will have a chance of turning into seeds.
Drystone above 3 blocks will have gravity applied.
Cobble stones will have a chance to turn into drystone
Cobble stone above 6 blocks will have gravity applied.
Polished stone will turn into stone
Ashlar blocks above 12 blocks will have gravity applied
Connection to a the side of a room that is up to 6 blocks wide(overhang distance x 2) will increase the hieght by 50% of cap height.
collaped rooms with fruit in storage in range will make a fruit tree in the middle of the room.

Maybe have multiple rounds of erossion for ruins from older worlds?


Implementation:
Step one: have mod command to save player inventory as a json or dat file with up to 16 slots each.
Step two: Have mod save inventory on death.
Step three: have mod spawn DeadDrop chests in world
Step four: have DeadDrop chest spawn with an inventory.
Step five: Have deaddrop chest spawn with a randomized inventory
Step Six: save a base structure with item placement triqqer.
Step seven: spawn structure in world
Step eight: Modify structure with errosion

