# Milex GMS1 CoreMod

**Version:** `1.2.0`  
**GUID:** `com.milex.gms1.core`  
**Autor:** Milex  
**Assembly:** `Milex_GMS1_CoreMod.dll`  

Das zentrale Framework und Management-Modul für alle **Milex GMS1 Mods** in **Gold Mining Simulator** (*Gold Rush: The Game*).

---

## 📌 Features & Aufgaben

1. **`ModBase` Basisklasse**:
   - Standardisierte Basis für alle Unity-Mods (`BaseUnityPlugin`).
   - Automatisches Harmony-Patching im `Awake()` und Unpatching im `OnDestroy()`.
   - Mod-Prefix-Logging (`LogInfo`, `LogWarning`, `LogError`).
   - Automatische Registrierung im zentralen Menü und bei der Lokalisierungs-Engine.
   - `Translate(key, fallback)`-Methode für einfachen Zugriff auf Mod-Sprachdateien.
2. **Mehrsprachigkeits- & Lokalisierungssystem (`LocalizationManager`)**:
   - Legt automatisch den Ordner `BepInEx\plugins\Milex GMS1 Mod Localization\` an.
   - Schreibt fehlende Sprachdateien (`%Modname%_en.json`, `%Modname%_de.json`) beim Mod-Start automatisch als editierbare Vorlagen aus den eingebetteten DLL-Ressourcen auf die Festplatte.
   - Unterstützt 21 Sprachen (`fr`, `en`, `de`, `es`, `ru`, `pl`, `it`, `pt`, `tr`, `nl`, `sv`, `da`, `no`, `ro`, `cs`, `bg`, `el`, `ja`, `ko`, `zh-CN`, `zh-TD`).
   - Fällt bei fehlenden Übersetzungen immer strikt auf **Englisch (`en`)** zurück.
3. **Ingame Mod-Menü (`ModMenuUI`)**:
   - Standardmäßig aufrufbar mit Taste **`Insert`** (Einfügen).
   - **Core-Optionen Tab**:
     - Checkbox `Use Game Language` (Spiel-Sprache automatisch erkennen, Standard: `true`).
     - Manuelle Sprachauswahl (nur aktiv, wenn `Use Game Language` deaktiviert ist).
     - Hotkey-Rebinding für die Menü-Taste selbst.
   - **Geladene Mods Tab / Sidebar**:
     - Dynamisch übersetzte Sektionen, Optionstitel, Beschreibungen und Werte.
     - **Echtzeit-Persistenz**: Jede UI-Interaktion speichert direkt in die jeweilige Mod-`.cfg`-Datei unter `BepInEx/config/`.

---

## ⚙️ Konfiguration

Die Einstellungen des Core-Mods befinden sich in `BepInEx/config/Milex_GMS1_CoreMod.cfg`:

| Sektion | Schlüssel | Standardwert | Beschreibung |
|---|---|---|---|
| `General` | `MenuToggleKey` | `Insert` | Taste zum Öffnen/Schließen des Mod-Menüs |
| `Localization` | `UseGameLanguage` | `true` | Automatische Erkennung der Spiel-/System-Sprache |
| `Localization` | `SelectedLanguage` | `en` | Manuell gewählter Sprachcode (z. B. `de`, `en`, `fr`...) |
