# S1 — Real-client acceptance

Automated protocol and persistence checks cover authority, material economy, storage contention, and clean restart. This checklist proves that the **real game UI** presents the same behavior correctly.

> Run against an isolated `--saves` directory. Record date/time, player ID, server log, client log, and screenshots/video for each failure. Do not treat a passing packet test as evidence that a UI flow works.

## Setup

1. Build `server` and `test-client` in Debug.
2. Start server on normal local ports with a unique temporary save root:

   ```powershell
   dotnet server\bin\Debug\net9.0\DurangoServer.dll `
     --data server\data --saves $env:TEMP\durango-s1-manual-<guid> `
     --game-port 8191 --gateway-port 8190 `
     --enable-cheat --no-account-check --no-ip-bind --admin gm
   ```

3. Launch the development client with `DURANGO_AUTOCONNECT=127.0.0.1` using `tools/connect-game.ps1`.
4. Verify the server logged `[world] player joined` before beginning.

## A. Construction materials

```text
[ ] Open the building UI; a placed blueprint is visually pending, not a completed object.
[ ] Try to build with no materials: it remains pending, explains the problem, and unrelated inventory items do not change.
[ ] Add a wrong material / wrong slot if the UI permits it: it is rejected without inventory loss or UI lock-up.
[ ] Deposit the correct material set: every slot/count updates and inventory loses exactly those item IDs.
[ ] Build after all mandatory slots are complete: timer plays, model becomes completed, and the artifact is usable.
[ ] Stand away from the artifact and try building: it is rejected as out of reach.
```

## B. Refund and storage

```text
[ ] Destroy an incomplete structure: all reserved construction items return to inventory exactly once.
[ ] Build a structure, then destroy it: its completed construction ledger refunds exactly once.
[ ] Put a known item in a storage artifact, close/reopen the UI, and withdraw it: item count/ID is preserved.
[ ] Put a known item in storage, then destroy the storage artifact: the stored item and construction refund return to inventory.
[ ] While out of range, opening or transferring storage is rejected and the UI remains usable.
```

## C. Workbench and core loop

```text
[ ] Build or place a usable workbench/fire; standing nearby enables a recipe requiring it.
[ ] Move away; the same recipe is disabled/rejected.
[ ] Gather one natural resource, craft one non-food item, build/store/retrieve an item, then hunt/butchery.
[ ] Die, revive, and verify gauges/inventory/buildings stay coherent.
```

## D. Restart and relogin

```text
[ ] Leave one pending construction reservation and one item in storage.
[ ] Stop the server cleanly with Ctrl+C; wait for the clean-save completion log.
[ ] Restart with the same isolated save root and reconnect.
[ ] Pending material slots, built artifacts, owner access, storage contents, and player inventory are all visible and correct.
```

## E. Second real player

```text
[ ] On a separate workstation, a non-owner cannot open/read/withdraw storage or read/build/destroy the owner artifact.
[ ] If architect access is granted by the eventual product UI, the architect can access the shared artifact.
[ ] Two authorized real clients contend for one storage item: exactly one receives it; neither UI desynchronizes or disconnects.
```

This workstation supports only one `DurangoV2` process because client-local ports conflict. `--storage-workbench-check` is the required automated authority/race evidence locally; section E requires a second machine for UI confirmation.

## Automated evidence required with this checklist

```text
--building-economy-check       14/14
--storage-workbench-check      11/11
--blueprint-requirements-check 6/6
--world-persistence-check      9/9
--restart-persistence-check    13/13
```

Clean restart is covered. Crash consistency across independently written world/player/account files remains outside S1.
