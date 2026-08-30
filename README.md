# Milex GMS1 Mods - Mod-Framework & Mod-Sammlung

Herzlich willkommen zum **Milex GMS1 Modding-System** für *Gold Mining Simulator* (*Gold Rush: The Game*). 

Dieses System bietet ein einfaches Ingame-Menü zur Verwaltung aller installierten Mods, automatische Sprachunterstützung und volle Kontrolle über deine Mod-Einstellungen direkt im laufenden Spiel.

---

## Schnell-Navigation

Hier findest du direkte Links zu den weiterführenden Anleitungen und Dokumentationen:

| Bereich | Beschreibung | Dokumentation |
|---|---|---|
| **CoreMod Framework** | Das Herzstück des Mod-Systems mit Ingame-Menü und Einstellungen | [CoreMod Anleitung](src/CoreMod/README.md) |
| **HelloMod (Beispiel-Mod)** | Ein einfacher Test-Mod zur Demonstration der Funktionen | [HelloMod Anleitung](src/Mods/HelloMod/README.md) |
| **Tastenbelegung** | Übersicht der verfügbaren Tasten und deren Anpassung | [Tasten-Übersicht](KEYBINDINGS.md) |
| **Änderungshistorie** | Übersicht aller Neuerungen und Verbesserungen | [Changelog ansehen](CHANGELOG.md) |

---

## Wichtigste Funktionen auf einen Blick

1. **Ingame Mod-Menü (Taste `Einfügen` / `Insert`)**:
   - Öffne jederzeit im Spiel das zentrale Menü, um deine Mods zu verwalten.
   - Die Spielwelt kann auf Wunsch automatisch pausiert werden, während das Menü offen ist.

2. **Mods im Spiel an- und ausschalten**:
   - Du kannst einzelne Erweiterungs-Mods direkt im Menü aktivieren oder deaktivieren.
   - Deaktivierte Mods stoppen sofort im Hintergrund, ohne dass das Spiel neu gestartet werden muss.

3. **Automatische Sprachunterstützung**:
   - Das Menü und alle unterstützten Mods passen sich automatisch an deine eingestellte Spielsprache an (Deutsch, Englisch und viele weitere).

4. **Kamera- & Steuerungsschutz**:
   - Während du Einstellungen im Menü anpasst, bleibt die Spielfigur ruhig stehen und die Kamera dreht sich nicht ungewollt mit.

---

## Installation & Erste Schritte

1. Stelle sicher, dass **BepInEx 5** im Hauptverzeichnis deines Spiels installiert ist.
2. Kopiere die Mod-Dateien (z. B. `Milex_GMS1_CoreMod.dll` und `Milex_GMS1_HelloMod.dll`) in den Ordner `BepInEx/plugins/`.
3. Starte das Spiel und drücke die Taste **`Einfügen`** (`Insert`), um das Mod-Menü zu öffnen.

---

## Für Mod-Entwickler & Mitwirkende

Das Projekt ist modular aufgebaut. Eigene Mods können einfach erstellt werden, indem sie vom `CoreMod` als Abhängigkeit Gebrauch machen. Alle Bau-Skripte und Pfade sind in der Solution `GMSModding.sln` vorbereitet.

```powershell
dotnet build GMSModding.sln
```
Der Befehl kompiliert das Projekt und kopiert die fertigen Mod-Dateien automatisch direkt in dein Spielverzeichnis.
