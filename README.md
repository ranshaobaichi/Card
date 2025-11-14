# Card 
## What is this?
A card game made with Unity, combines **"Stacklands"**'s craft system and **Auto Chess**'s battle system.

## The CardSlot Update Logic
* Validate input check
* Update the linking status
* Update the cardSlot reference and transform parent
* Update the identity ID
* Update the transform position
* Change the last card's placement state
* Detect whether need to start production

## The Battle System Logic
* Each battle creatur will add it's invoke action to BattleWorldManager(short for `engine`)'s `Tick` event due to its lineup
* Start the battle
* Engine will invoke the event `Tick` every certain time interval or mannually trigger it
* Each battle creature will decrease its internal cooldown timer by tick interval
* When the internal cooldown timer reaches zero, the battle creature will perform its action by delegate its action to engine's according event:
  * first add debuffs to the target(s)
  * then deal damage to the target(s)
* After all creatures have performed their action, the engine will invoke all the buff events and damage events to process the effects
* Repeat the above steps until one side wins