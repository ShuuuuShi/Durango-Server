# Alpha 0.0.1 Acceptance

## Required Lifecycle

Run this lifecycle with 10 distinct players:

1. Login
2. Spawn into the world
3. Gather a natural resource
4. Craft an item
5. Place and build an artifact
6. Hunt an animal
7. Logout cleanly
8. Restart the server
9. Login again
10. Verify that each character, inventory, crafted items, and built artifacts remain

The run passes only when all 10 players complete the lifecycle and all persisted state matches the pre-restart snapshot.

## State That Must Persist

- Player identity and display
- Player position
- Inventory and crafted items
- Equipped items, if changed during the run
- Skills and skill points changed during the run
- Built artifacts and ownership
- Storage contents inside built storage artifacts
- Gathered natural nodes that were fully removed

## Current Blockers

- Hunt now has a minimal server path (`UseBattleAction` against `animal_*`), but the 10-player acceptance run still needs to verify the real client action IDs, death animation, reward, and relog persistence.
- Partial generator depletion is held only in memory and is not in `WorldSave`.
- Crafting currently accepts client-supplied material IDs without validating required recipe materials.

These must be fixed before declaring Alpha 0.0.1 passed.

## Deferred After Alpha

The following work is intentionally stacked after the acceptance run and must not be used as Alpha evidence:

- Admin menu polish and additional admin buttons
- NPC dialogue suppression
- PlayerBot random farming behavior
- Direct `moveto` command polish
- Animal walk animation quality
- Private chat, auth hardening, client-authoritative level/display fixes
- Full combat AI, death/revive UX, pets, farming, party, and clan systems

## Evidence To Record

- Per-player login/spawn/logout/relogin log lines
- Inventory snapshot before logout and after relogin
- Artifact IDs, owners, and tiles before restart and after relogin
- Gathered-node state before restart and after relogin
- Hunt result and reward for each player
- Server restart timestamp and clean-save result
