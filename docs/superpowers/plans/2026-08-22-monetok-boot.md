# Monetok Boot Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Boot scene loads save (or defaults), then opens a non-AR Game scene where Monetok can work and snack with persistent coins.

**Architecture:** Pure C# `GameState` + `SaveService` + `PetActions` with a static `GameSession`. Boot coroutine initializes session then `LoadScene("Game")`. Game HUD binds to session and saves after actions.

**Tech Stack:** Unity 6, uGUI + TextMeshPro, `JsonUtility`, NUnit EditMode tests, Unity MCP for scene wiring.

## Global Constraints

- Boot is build index 0; Game is 1; AR is never auto-loaded.
- Save file name is exactly `monetok-save.json`.
- Default pet name `Монеток`, coins `100`, hunger `80`.
- Work +15 coins; snack −10 coins and +15 hunger, no negative coins.
- Minimum Boot splash 0.5s.
- No AR code in Boot/Game scripts.

---

### Task 1: Save and pet actions (TDD)

**Files:**
- Create: `Assets/_Game/_Tests/Editor/PetActionsTests.cs`
- Create: `Assets/_Game/_Tests/Editor/SaveServiceTests.cs`
- Create: `Assets/_Game/_Scripts/Core/GameState.cs`
- Create: `Assets/_Game/_Scripts/Core/PetActions.cs`
- Create: `Assets/_Game/_Scripts/Core/SaveService.cs`
- Create: `Assets/_Game/_Scripts/Core/GameSession.cs`

**Interfaces:**
- Consumes: none
- Produces: `GameState.CreateDefault()`, `PetActions.TryWork/TrySnack`, `SaveService.LoadOrCreateDefault/Save`, `GameSession.Initialize/Persist`

- [ ] Write failing EditMode tests, then implementation, then run `run_tests` EditMode for Monetok tests.

### Task 2: Boot + Game runtime

**Files:**
- Create: `Assets/_Game/_Scripts/Boot/LoadingView.cs`
- Create: `Assets/_Game/_Scripts/Boot/BootController.cs`
- Create: `Assets/_Game/_Scripts/Game/GameHud.cs`

- [ ] Boot never loads Game until session is ready. HUD calls PetActions + Persist.

### Task 3: Scenes and build order

**Files:**
- Create: `Assets/_Game/_Scenes/Boot.unity`
- Create: `Assets/_Game/_Scenes/Game.unity`
- Modify: Build Settings scene list

- [ ] Wire camera/light/canvas via Unity MCP `execute_code`. Build order Boot, Game, AR.
