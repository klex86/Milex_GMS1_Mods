# Changelog - Milex GMS1 CoreMod

Alle Änderungen am zentralen Mod-Framework `Milex GMS1 CoreMod`.

---

## [1.2.2] - 2026-09-02

### Fehlerbehebungen & Stabilität
- **Mauszeiger-Wiederherstellung im Spielmenü behoben**:
  - Beim Schließen des Mod-Menüs wird der vorherige Mauszeiger-Zustand nun exakt wiederhergestellt. Wenn das Menü im Pausenmenü oder Inventar geöffnet wird, bleibt der Mauszeiger nach dem Schließen sichtbar und frei beweglich.
- **UI-Texturen gegen Szenenwechsel-Entladung geschützt**:
  - Prozedurale Menü- und Slider-Texturen sind nun mit `HideFlags.HideAndDontSave` vor der Unity-Garbage-Collection bei Szenenwechseln geschützt und werden bei Bedarf automatisch regeneriert.

---

## [1.2.1] - 2026-08-30

### Verbesserungen & Fehlerbehebungen
- **Deutlich sichtbare Schieberegler (Slider)**:
  - Die Schieberegler im Menü heben sich nun mit einer klar abgegrenzten Rinne und einem goldenen Schiebegriff deutlich vom dunklen Hintergrund ab.
- **Automatische Gruppensperre im einfachen Modus**:
  - Wenn in einer Gruppe der einfache Modus aktiv ist, werden die spezifischen Einzelregler automatisch ausgegraut und gesperrt, da der Gruppenregler die Führung übernimmt.
- **Gruppen-Reset-Knopf**:
  - Jede Gruppe im Menü besitzt nun einen eigenen Knopf `[ Gruppe zurücksetzen ]`, um die Werte dieser Gruppe direkt auf die Standardwerte zurückzustellen.
- **Englische Konfigurations-Beschreibungen**:
  - Alle internen Beschreibungen in der Konfigurationsdatei (`.cfg`) sind nun einheitlich auf Englisch verfasst. Menü-Texte im Spiel richten sich weiterhin nach den Lokalisierungsdateien.
- **Live-Aktualisierung der Statusleiste**:
  - Die Anzahl aktiver Mods in der Fußzeile wird nun bei jedem Aktivieren oder Deaktivieren eines Mods sofort in Echtzeit aktualisiert (Format: `Aktive Mods: X / Y`).

---

## [1.2.0] - 2026-08-30

### Neue Funktionen
- **Live An-/Ausschalten von Sub-Mods**:
  - Jeder Erweiterungs-Mod besitzt jetzt einen An/Aus-Schalter im Ingame-Menü.
  - Das Deaktivieren stoppt alle Mod-Funktionen und Spielanpassungen sofort im laufenden Spiel.
  - Der Core-Mod selbst bleibt als Kern des Mod-Systems immer aktiv.
- **Entwickler-Schalter für Sprachdateien**:
  - Einstellung *"Externe Sprachdateien ignorieren"* hinzugefügt. Lädt Texte direkt aus den internen Mod-Ressourcen für einfachere Tests.

### Fehlerbehebungen & Optimierungen
- **Fix für Spiel-Pause**:
  - Behebt ein Problem, bei dem das Spiel nach dem Schließen des Menüs pausiert blieb, wenn die Pause-Option im offenen Menü umgestellt wurde.
- **Fokussierte Menü-Skalierung**:
  - Das Menü bleibt beim Skalieren an seiner oberen linken Ecke verankert und wandert nicht mehr über den Bildschirm.
- **Eingabe- & Kamerasperre**:
  - Verhindert zuverlässig, dass beim Bedienen des Menüs im Hintergrund die Kamera gedreht oder Werkzeuge gewechselt werden.
- **Bereinigte Schrift- & Symbol-Darstellung**:
  - Entfernung unsauber dargestellter Sonderzeichen für ein übersichtliches und gut lesbares Layout.

---

## [1.1.0] - 2026-08-29

### Neue Funktionen
- Trennung in Kern-Framework und Erweiterungs-Mods.
- Aufrufbares Ingame-Menü über die Taste `Einfügen` (`Insert`).

---

## [1.0.0] - 2026-08-29

### Erstveröffentlichung
- Grundstein für das Mod-System und automatische Verwaltung von Einstellungen.
