Description of files:

blockDifficulty - 
[{"level":1},{"level":2},{"level":3},{"level":4}]
This actually encodes the difficulty of each period/block.  The example is kind of bad, but there could be any number of blocks with a "level"
[{"level":4},{"level":4},{"level":3},{"level":1},{"level":1},{"level":3}]
The game ends when it runs of blocks from this list
What do the 1-4 indicate? 
Level 1: equation with a single unknown, answer is likely to be spawned
Level 2: equation with a single unknown, random spawning where answer is 
                        more likely to be produced by combinations
Level 3: two unknowns, answer is likely to be spawned
Level 4: two unknowns, random spawning
At the start of each block, a new equations and blockDifficulty will be loaded in.  Specifically each file will load in like:
Level 1: equations1 & spawningDiffiulty1
Level 2: equations1 & spawningDifficulty2
Level 3: equations2 & spawningDifficulty1
Level 4: equations2 & spawningDifficulty2
Right now these values are just hardcoded in a switch, but it would be easy to change them if you want more/less files.

spawningDifficullty(1&2) - 
[{"MAX_ATOM_NUMBER_ON_SCREEN":2,"ALL_SOLUTIONS_PRIORITY":2,"CURRENT_SOLUTIONS_PRIORITY":4,"ADJUSTED_GENERAL_SOLUTIONS_PRIORITY":1}]
Loaded in for each block.  Value affect probability of which bubble number will be spawned

equations(1&2) - 
[{"leftNum":null,"rightNum":3,"solution":4,"operation":0}]
The equations list.  Glad you like it!  I think you can leave any leftNum/rightNum or solution off and it will default it to null.  Good example though.
Unfortunately for equations2, smart spawning does not work if there is no solution.  So most equations should have a solution.

timeConfig - 
[{"gameTime":30.0,"plusTime":10.0,"bubbleDestroyTime":19.0,"numPeriods":3,"bubbleSpawnTime":2.0,"maxBubbles":7}]
gameTime/plusTime - number of seconds of play vs fixation periood
bubbleDestroyTime - how many seconds a bubble lasts before it goes away
numPeriods - this is redundant with block difficulty file
bubbleSpawnTime - time between each bubble spawning initially.  could set this to high & bubbleDestroyTime to low to have more frequent bubble cycles/
maxBubbles - max number of bubbles on screen at one time

pauseTimes & numberSpawning are indeed obsolete.