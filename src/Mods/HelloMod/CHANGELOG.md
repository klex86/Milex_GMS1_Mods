# Changelog - Milex GMS1 HelloMod

All notable changes to `Milex_GMS1_HelloMod` will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.1.0] - 2026-08-29

### Added
- **Multi-Language Support**: Embedded `HelloMod_en.json` and `HelloMod_de.json` resources for automated disk template extraction to `BepInEx\plugins\Milex GMS1 Mod Localization\HelloMod_<lang>.json`.
- Dynamic translated log output and localized section/key names in the Core Mod Menu.

### Changed
- Rebranded to **Milex GMS1 HelloMod** (`Milex_GMS1_HelloMod.dll`, namespace `Milex.GMS1.Mods.HelloMod`, GUID `com.milex.gms1.hellomod`).
- Updated dependency reference to `Milex_GMS1_CoreMod`.

---

## [1.0.0] - 2026-08-29

### Added
- Initial standalone release of `HelloMod` with persistent config entries and customizable `F2` hotkey.
