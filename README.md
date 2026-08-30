# Milex GMS1 Mods - Mod-Framework & Mod-Sammlung

Ein modulares Mod-System für **Gold Mining Simulator** (*Gold Rush: The Game*), basierend auf BepInEx 5 und Harmony. Es beinhaltet ein zentrales Ingame-Konfigurationsmenü, eine automatische Konfigurationsverwaltung und ein vollständiges Mehrsprachigkeits-System.

---

## Dokumentation & Navigation

Die folgenden Links führen direkt zu den Anleitungen und Änderungshistorien (Changelogs) der einzelnen Komponenten:

| Thema / Mod | Beschreibung | Dokumentation | Changelog |
|---|---|---|---|
| **Gesamtes Projekt** | Gesamt-Übersicht und Haupt-Anleitung | [Haupt-Dokumentation](README.md) | [Gesamt-Changelog](CHANGELOG.md) |
| **Milex GMS1 CoreMod** | Zentrales Verwaltungssystem, Ingame-Menü & Sprachverwaltung | [CoreMod Anleitung](src/CoreMod/README.md) | [CoreMod Changelog](src/CoreMod/CHANGELOG.md) |
| **Milex GMS1 HelloMod** | Beispiel-Mod zur Demonstration der Funktionen | [HelloMod Anleitung](src/Mods/HelloMod/README.md) | [HelloMod Changelog](src/Mods/HelloMod/CHANGELOG.md) |
| **Tastenbelegung** | Übersicht der Hotkeys und deren Umbelegung im Spiel | [Tasten-Übersicht](KEYBINDINGS.md) | - |

---

## Installation & Erste Schritte

1. **Voraussetzung**: Stelle sicher, dass **BepInEx 5** (x64) im Hauptverzeichnis deines Spiels installiert ist.
2. **Mods kopieren**: Kopiere die fertigen Mod-Dateien (z. B. `Milex_GMS1_CoreMod.dll` und `Milex_GMS1_HelloMod.dll`) in den Ordner `BepInEx/plugins/`.
3. **Spiel starten**: Starte das Spiel wie gewohnt.
4. **Mod-Menü öffnen**: Drücke im Spiel die Taste **`Einfügen`** (`Insert`), um das Mod-Menü zu öffnen.

---

## Bedienung im Spiel

- **Mod-Menü öffnen/schließen**: Taste `Einfügen` (`Insert`) drücken.
- **Mods an- und ausschalten**: Im Tab *Geladene Mods* kann jeder Mod über einen eigenen Schalter im laufenden Spiel aktiviert oder deaktiviert werden.
- **Einstellungen anpassen**: Tastenbelegungen, Texte und Optionen können direkt im Menü geändert werden.
- **Sprache ändern**: Im Tab *Allgemein* kann die Sprache manuell gewählt werden, falls die automatische Erkennung der Spielsprache deaktiviert wird.

---

## Konfiguration & Sprachdateien

Alle Mods dieses Systems verwalten ihre Einstellungen und Texte nach einem einheitlichen Standard. Es müssen keine Dateien manuell angelegt werden – das System richtet alles beim ersten Start automatisch ein.

### Konfigurationsdateien (`.cfg`)

- **Speicherort**: `BepInEx/config/`
- **Automatische Erstellung**: Beim ersten Spielstart erstellt das System für jeden Mod eine eigene Einstellungsdatei.
- **Dateibenennung**: Jede Datei ist exakt nach der Mod-Datei benannt (z. B. `Milex_GMS1_CoreMod.cfg` oder `Milex_GMS1_HelloMod.cfg`).
- **Anpassung**: Einstellungen können direkt im Ingame-Menü oder bei geschlossenem Spiel mit einem Texteditor in den `.cfg`-Dateien bearbeitet werden.

### Sprachdateien & Lokalisierung (`.json`)

- **Speicherort**: `BepInEx/plugins/Milex GMS1 Mod Localization/`
- **Automatische Vorlagen**: Beim Mod-Start werden die englischen (`_en.json`) und deutschen (`_de.json`) Sprachdateien automatisch in diesen Ordner entpackt.

---

## Anleitung: Vorlagen für neue Sprachen erstellen

Wenn du das Spiel in einer bisher nicht enthaltenen Sprache spielst (z. B. Französisch, Polnisch oder Spanisch), kannst du dir dafür direkt im Spiel Vorlagen erstellen lassen:

1. Drücke im Spiel die Taste **`Einfügen`** (`Insert`), um das Mod-Menü zu öffnen.
2. Navigiere zum Bereich **Allgemein** und wähle die Option **`Spiel-Sprache verwenden`** ab.
3. Wähle im Feld **`Sprache wählen`** deine gewünschte Zielsprache aus.
4. Das System erkennt fehlende Sprachdateien und zeigt ein Hinweisfenster an.
5. Klicke auf **`Vorlagen erstellen`**.
6. Im Ordner `BepInEx/plugins/Milex GMS1 Mod Localization/` werden nun editierbare JSON-Vorlagen (z. B. `Milex_GMS1_HelloMod_fr.json`) angelegt.
7. Diese Dateien können mit einem beliebigen Texteditor geöffnet und übersetzt werden.
8. Übersetzte Sprachdateien können gerne auf NexusMods bereitgestellt werden, um sie offiziell in zukünftige Versionen zu übernehmen.

---

## Entwicklung & Kompilierung

Projekt-Erstellung über die zentrale Solution `GMSModding.sln`:

```powershell
dotnet build GMSModding.sln
```
Der Build-Prozess kopiert fertige Mod-Dateien automatisch in das `BepInEx/plugins/`-Verzeichnis des Spiels.
