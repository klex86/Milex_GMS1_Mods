# Milex GMS1 Mods - Mod-Framework & Mod-Sammlung

Herzlich willkommen zum **Milex GMS1 Modding-System** für *Gold Mining Simulator* (*Gold Rush: The Game*). 

Dieses System bietet ein einfaches Ingame-Menü zur Verwaltung aller installierten Mods, automatische Konfigurationsverwaltung, Sprachunterstützung und volle Kontrolle über deine Mod-Einstellungen direkt im laufenden Spiel.

---

## Schnell-Navigation

| Bereich | Beschreibung | Dokumentation |
|---|---|---|
| **CoreMod Framework** | Das Herzstück des Mod-Systems mit Ingame-Menü und Einstellungen | [CoreMod Anleitung](src/CoreMod/README.md) |
| **HelloMod (Beispiel-Mod)** | Ein einfacher Test-Mod zur Demonstration der Funktionen | [HelloMod Anleitung](src/Mods/HelloMod/README.md) |
| **Tastenbelegung** | Übersicht der verfügbaren Tasten und deren Anpassung | [Tasten-Übersicht](KEYBINDINGS.md) |
| **Änderungshistorie** | Übersicht aller Neuerungen und Verbesserungen | [Changelog ansehen](CHANGELOG.md) |

---

## Blick unter die Haube: Wie das Mod-System funktioniert

Alle Mods in diesem System folgen einem einheitlichen Standard für Einstellungen und Sprachdateien. Dadurch musst du Dateien nie manuell von Hand anlegen – alles wird beim ersten Start automatisch für dich eingerichtet.

### 1. Die Konfigurationsdateien (`.cfg`)

- **Speicherort**: `BepInEx/config/` im Spielverzeichnis.
- **Automatische Erstellung**: Sobald das Spiel mit installierten Mods startet, erstellt das System für jeden Mod eine eigene `.cfg`-Datei.
- **Benennung**: Jede Datei heißt exakt wie die dazugehörige Mod-Datei (z. B. `Milex_GMS1_CoreMod.cfg` oder `Milex_GMS1_HelloMod.cfg`).
- **Funktionsweise**: 
  - Du kannst Einstellungen bequem direkt im Ingame-Menü ändern – diese werden sofort live in die entsprechende `.cfg`-Datei geschrieben.
  - Alternativ kannst du die `.cfg`-Dateien auch mit einem normalen Texteditor (z. B. Notepad) bei geschlossenem Spiel bearbeiten.

### 2. Das Sprach- & Übersetzungssystem (`.json`)

- **Speicherort**: `BepInEx/plugins/Milex GMS1 Mod Localization/`
- **Automatische Vorlagen**: Beim ersten Start schreibt das System automatisch die englischen (`_en.json`) und deutschen (`_de.json`) Vorlagen aus den Mod-Dateien in diesen Ordner.
- **Automatische Spracherkennung**: Das System erkennt automatisch die eingestellte Sprache deines Spiels und wählt die passende Übersetzung.

---

## Anleitung: Eigene Sprachvorlagen für neue Sprachen erstellen

Wenn du das Spiel in einer Sprache spielst, für die ein Mod noch keine eigene Übersetzung mitbringt (z. B. Französisch, Polnisch, Spanisch), kannst du dir dafür direkt im Spiel Vorlagen erstellen lassen:

1. Öffne im Spiel mit der Taste **`Einfügen`** (`Insert`) das Mod-Menü.
2. Gehe in den Bereich **Allgemein** und wähle die Option **`Spiel-Sprache verwenden`** ab.
3. Wähle aus dem Klappmenü **`Sprache wählen`** deine gewünschte Zielsprache aus (z. B. *Français* oder *Polski*).
4. Das Spiel erkennt sofort, dass für diese Sprache noch keine Dateien existieren, und öffnet ein kleines Hinweisfenster:
   > *"Fehlende Sprachdateien erkannt. Möchtest du, dass wir dir dafür Vorlagen-Dateien zur Übersetzung anlegen?"*
5. Klicke auf **`Vorlagen erstellen`**.
6. Das System erstellt im Ordner `BepInEx/plugins/Milex GMS1 Mod Localization/` fertige JSON-Dateien (z. B. `Milex_GMS1_HelloMod_fr.json`).
7. Öffne die neu erstellten Dateien mit einem Texteditor. Die rechten Seiten der Textzeilen kannst du nun in deine Sprache übersetzen.
8. **Tipp für Community-Mitglieder**: Du kannst deine übersetzten JSON-Dateien gerne auf der NexusMods-Seite des jeweiligen Mods hochladen, damit sie in zukünftige Versionen integriert werden können!

---

## Installation & Erste Schritte

1. Stelle sicher, dass **BepInEx 5** (x64) im Hauptverzeichnis deines Spiels installiert ist.
2. Kopiere die fertigen Mod-Dateien (`Milex_GMS1_CoreMod.dll`, `Milex_GMS1_HelloMod.dll` usw.) in den Ordner `BepInEx/plugins/`.
3. Starte das Spiel und drücke die Taste **`Einfügen`** (`Insert`), um das Mod-Menü zu öffnen.

---

## Für Entwickler & Kompilierung

Das Repository ist als modulare Multi-Projekt-Solution aufgebaut.

```powershell
dotnet build GMSModding.sln
```
Der Befehl kompiliert alle Projekte und kopiert die fertigen `.dll`-Dateien automatisch direkt in den `BepInEx/plugins/`-Ordner deines Spiels.
