# Milex GMS1 HelloMod

**Version:** `1.1.0`  
**GUID:** `com.milex.gms1.hellomod`  
**Autor:** Milex  
**Assembly:** `Milex_GMS1_HelloMod.dll`  
**Abhängigkeit:** `com.milex.gms1.core` (`Milex GMS1 CoreMod`)  

Ein Proof-of-Concept-Mod für **Gold Mining Simulator**, der die Anbindung an das `Milex GMS1 CoreMod`-Framework, Mehrsprachigkeit (Localization), Ingame-Config-Persistenz und anpassbare Hotkeys demonstriert.

---

## 📌 Features

- **Mehrsprachigkeit**: Bringt englische (`HelloMod_en.json`) und deutsche (`HelloMod_de.json`) Übersetzungen mit.
- **Auto-Template-Generierung**: Beim Start wird automatisch `BepInEx\plugins\Milex GMS1 Mod Localization\HelloMod_en.json` und `HelloMod_de.json` angelegt.
- **Konfigurierbar im Spiel**: Drücke **`Insert`** (Einfügen), um das Menü zu öffnen und:
  - Den Hotkey per Klick frei umzubelegen
  - Den Präfix `[Greeting]` an-/abzuschalten
  - Die Grußnachricht im Textfeld anzupassen
- **Auto-Persistenz**: Alle Änderungen werden sofort in `BepInEx/config/Milex_GMS1_HelloMod.cfg` gesichert.

---

## ⚙️ Konfigurationsoptionen

| Sektion | Schlüssel | Standardwert | Typ | Beschreibung |
|---|---|---|---|---|
| `Controls` | `TestHotkey` | `F2` | `KeyCode` | Taste zum Auslösen des Log-Eintrags |
| `General` | `ShowPrefix` | `true` | `bool` | Gibt an, ob der Präfix im Log ausgegeben wird |
| `General` | `GreetingMessage` | `Hello from Gold Mining Simulator Modding!` | `string` | Eigene Grußnachricht |
