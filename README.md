# PizzaClientLagFix

### What it do??

If you've ever played multiplayer, you may have noticed extreme lag with Mithrix's pizza (big spinny) attack. This mod aims to fix that.

### The problem

The rotation of the attack is technically simulated on clients, but badly, it does not take network delay into consideration at all, and since the damage check is serverside, this results in potentially massive desyncs between the visual position and where the damage is actually applied.

Vanilla sequence of events:
  * Attack starts, spinnies are spawned
  * Non-host dodges attack on their end
  * Host has a completely different rotation, so the non-host gets hit anyway
  * Everyone is sad :(

### How fix????

This mod makes the damage check clientside, so whether you take damage or not will always match with the visuals. But because damage is still serverside, this does introduce a slight delay before the damage is actually applied.

Mod sequence of events:
  * Attack starts, spinnies are spawned
  * Either, A:
    * Non-host dodges attack on their end
    * Host runs damage check, hits non-host player, but this is ignored, no damage is dealt
  * B:
    * Non-host gets hit on their end
    * Non-host sends message to host, signaling that they were hit
    * Noticeable, but slight delay for the damage message to be received, depending on ping
    * Host receives damage message, deals damage, regardless of normal damage check
  * Everyone is happy :)