# Changelog – Milex GMS1 Production Tuner

Alle wichtigen Aenderungen an diesem Mod sind hier aufgelistet.
Das Format folgt den Grundsaetzen von [Keep a Changelog](https://keepachangelog.com/).

---

## [1.0.1] – 2026-08-30

### Fehlerbehebungen & Verbesserungen

- **Anzeigenamen im Menue funktionieren jetzt korrekt**: Alle Einstellungen zeigen nun ihren uebersetzten Namen statt des internen Dateinamens an. Ursache war eine falsche Benennung der Config-Keys (die Keys waren auf Deutsch statt auf Englisch).
- **Einfacher / Erweiterter Modus jetzt pro Gruppe**: Jede der fuenf Gruppen hat einen eigenen Schalter. So kann man z. B. Gruppe 1 auf einfach lassen und Gruppe 3 auf erweitert schalten, ohne dass alle anderen Gruppen beeinflusst werden. Der globale Modus-Schalter wurde entfernt.
- **Slider in 0.5-Schritten**: Alle Multiplikator-Regler rasten jetzt in Schritten von 0.5 ein (0.5, 1.0, 1.5, … 10.0). Stufenlose Werte zwischen den Schritten sind nicht mehr moeglich.
- **Gruppe 5 (Anhaenger) hat keinen Gruppen-Multiplikator mehr**: Magnetitanhaenger und Kraftstoffanhaenger haben nichts miteinander zu tun und werden jetzt getrennt und unabhaengig voneinander eingestellt.
- **Einzelregler im einfachen Modus gesperrt**: Wenn der einfache Modus einer Gruppe aktiv ist, sind die Einzelregler dieser Gruppe ausgegraut und koennen nicht veraendert werden.

---

## [1.0.0] – 2026-08-30

### Neu

- **Fuenf Regler-Gruppen**: Handwerkzeuge & Mobile Waschanlagen, Baufahrzeuge & Mobiles Foerderband,
  Waschanlagen-Module (Tier 3–6), Feinverarbeitung sowie Anhaenger.
- **Einfacher Modus** (Standard): Ein gemeinsamer Multiplikator steuert alle Komponenten einer Gruppe gleichzeitig.
- **Erweiterter Modus**: Einzelregler pro Komponente und Parameter (Kapazitaet, Geschwindigkeit, Hydraulik)
  koennen unabhaengig voneinander angepasst werden.
- **Gruppen-Reset-Button**: Setzt alle Werte einer Gruppe auf den Standardwert (1.0) zurueck.
- **Gesamt-Reset-Button**: Setzt alle Werte aller Gruppen zurueck.
- **Kaskadenschutz** (`AutoScaleDependentInputs`, Standard: an): Verhindert Materialverlust,
  wenn ein grosser Eimer in ein kleineres Folgegeraet geleert wird. Pfanne, Wave Table,
  Magnetitabscheider und Anhaenger werden automatisch mindestens so gross wie der Eimer skaliert.
- **Volles Mehrsprachigkeits-System**: Deutsch und Englisch sind eingebaut. Weitere Sprachen
  koennen als Vorlage im Ingame-Menue erstellt und uebersetzt werden.
- **Ingame-Anzeige**: Der Mod erscheint mit eigenem Eintrag in der Sidebar des Milex Mod-Menues.
  Kann im laufenden Spiel ohne Neustart aktiviert und deaktiviert werden.
- **Phase 1 Architektur**: Vollstaendige Konfiguration, Domainlogik und Lokalisierung sind implementiert.
  Harmony-Patches fuer die direkten Spielklassen folgen in Phase 2 (nach Dekompilierung der Spiel-DLLs).
