---
description: You do an big steppy
---

# Movement

Movement is a critical component in EVERY first person shooter.\
Without movement, you can't get anywhere. And if you can't get move, how can you find people to shoot?

## What makes your movement different?

To be honest? Nothing really does. I'm not after movement that feels unique or breaks any boundaries of what already exists. I'm trying to make movement that feels good: fluid and responsive. So far, I'm doing that pretty well.\
My character controller takes a full physics to movement, with zero friction on the player, using forces and linear damping to control it. While this might be more expensive (as though it _\*reeeeally\*_ matters) than a regular character controller, there's some nice things that using a Rigidbody lets me do that using Unity's Character Controller for would make me develop some kind of incurable headache.

### So what can you do?

Currently, the player has all the "basic" movement stuffs.\
You can walk, sprint, crouch, jump (with optional multi-jumps).\
You can aim to zoom in, and there's an optional alternate aim for weapons that might use it. Aiming slows down movement by default, but I could change that if I wanted to. \
Additionally, standing on moving/rotating platforms can optionally.\
Sliding has now been implemented. Strafing and pushing left and right while sliding also tilts the camera. Isn't that cool?

### What's planned?

The player will, in future, be able to

* ~~Slide - this will also have an optional "steer force" to allow players to redirect their slides.~~
* Mantle - Climbing ledges is a feature that I consider to be damn near a _necessity_ in games where movement matters. Not being able to climb a ledge can be the difference between getting away and losing your legs. Maybe your head too.
* Vault - The player might be able to move over low obstacles although this might be included in mantling.
* Climb steps - Not like stairs, but small ledges at ground level. I might work around this with geometry, honestly.
* Ladders - I hate hate HATE not being to climb ladders in games. I also hate that I have no clue how to make my character climb a ladder. I shall figure this out, I'm sure.
* Custom Movement - This is anything that doesn't quite fit into any of the others. This could be grappling hooks or zip-lines, jump pads, teleportation - any of that sort of stuff.



