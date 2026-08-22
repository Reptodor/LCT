# Monetok boot, scenes, and save

Date: 2026-08-22

## Goal

Ship a first-playable loop for a virtual piggy-bank pet named Monetok: the player always starts in a dedicated Boot scene, waits until save data is loaded, then lands in a non-AR Game scene. Progress (coins, hunger) persists between Play sessions.

## Out of scope

- AR scene gameplay and XR loaders
- Pet naming / first-run wizard
- Real money, accounts, or backend
- Full economy / shop UI (later slice)

## Scene flow

1. `Assets/_Game/_Scenes/Boot.unity` is build index 0.
2. `Assets/_Game/_Scenes/Game.unity` is build index 1.
3. `Assets/_Game/_Scenes/AR.unity` stays in the project and may remain in Build Settings after Game, but is never auto-loaded by Boot.

Boot does not load Game until initialization finishes. Game never talks to AR.

## Initialization

Boot shows a simple loading canvas (title "Монеток", status text, progress bar). Steps:

1. Create `SaveService` pointed at `Application.persistentDataPath`.
2. `LoadOrCreateDefault()` into `GameSession`.
3. Keep the splash visible at least 0.5 seconds.
4. `SceneManager.LoadScene("Game")` (Single).

If Game is missing from Build Settings, Boot stays on screen and shows an error status. It does not load AR as a fallback.

## Save

File: `monetok-save.json` under `persistentDataPath`.

Fields:

- `petName` string, default `Монеток`
- `coins` int, default `100`
- `hunger` int 0–100, default `80`
- `lastSaveUtc` ISO-8601 UTC string

Corrupt, empty, or missing file → default state. IO errors on save log and do not crash. Persist after successful actions and on `OnApplicationPause` / `OnApplicationQuit`.

## Gameplay (first slice)

Non-AR scene: camera, directional light, capsule pet, HUD.

- **Подработать**: +15 coins, always succeeds, then save.
- **Перекус**: if coins >= 10, spend 10, hunger += 15 capped at 100, then save. Otherwise no change.

HUD shows pet name, coins, hunger.

## Error handling

- Missing save → defaults.
- Bad JSON → defaults.
- Failed write → log error, keep playing with in-memory state.
- Missing Game scene → Boot error text, no scene change.

## Testing

EditMode tests cover save load/create/corrupt and work/snack rules.

Manual: Play from Boot (not AR). See loading, then Game. Work increases coins. Stop Play, Play again, coins remain. Snack with 0 coins does nothing. AR does not open.
