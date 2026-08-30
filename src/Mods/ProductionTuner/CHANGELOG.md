# Changelog – Milex GMS1 Production Tuner

Alle wichtigen Aenderungen an diesem Mod sind hier aufgelistet.
Das Format folgt den Grundsaetzen von [Keep a Changelog](https://keepachangelog.com/).

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
