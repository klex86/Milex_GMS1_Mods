# Changelog - Milex GMS1 CoreMod

All notable changes to `Milex_GMS1_CoreMod` will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.2.0] - 2026-08-29

### Added
- **Ultimate Input Freeze Matrix**:
  - **Native Game Input Blocker**: Hooked `InputManager.SetPauseMenuBlocked(true)` via reflection directly into the Mod Menu toggle to utilize the game's own menu freeze logic.
  - **Unity Native Input Patches**: Blocked `UnityEngine.Input.GetAxis` and `UnityEngine.Input.GetMouseButton` completely to stop legacy scripts from receiving mouse wheel deltas.
  - **Cursor State Deception**: Patched `Cursor.lockState` GETTER so any game script checking the lock state is fooled into thinking the mouse is already unlocked.
  - **Physics Gliding Fix**: Zeroed out the Rigidbody `velocity` and `angularVelocity` in `FixedUpdate` so the player stops instantly without gliding.
- **Player & Tool Update Freezing**:
  - Blocks `Player.Update()` and `PlayerTool.MouseLeft` / `MouseRight`.
- **Rewired Hardware Mouse Patches**:
  - Hooks directly into `Rewired.Mouse` hardware driver (`get_screenPositionDelta`, `GetAxis`, `GetAxis2D`, `GetButton`) to silence mouse rotation at the lowest engine level.
- **Full Camera Controller Coverage**:
  - Hooks `MouseOrbit`, `MachineOrbit`, `MouseLook`, `CabinMouseLook`, `CameraController_DogCollieFull`, `RepairMouseLook`, and `PanningCamera`.
- **Step Button UI Scaling**:
  - Replaced slider with clean `[ ➖ -5 % ]` and `[ ➕ -5 % ]` stepper buttons with a direct 100% reset.
- **Native Endonym Language Dropdown**:
  - 21 languages with native names and template generation dialog.

---

## [1.1.0] - 2026-08-29

### Added
- Multi-Assembly separation and `ModRegistry`.
- IMGUI In-Game Mod Configuration Menu on `Insert`.

---

## [1.0.0] - 2026-08-29

### Added
- Initial `ModBase` class with automated Harmony patch/unpatch lifecycle.
