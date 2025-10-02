Feature: Admin Statistiken und Übersicht
  Als Administrator
  Möchte ich Statistiken über die Funde sehen können
  Damit ich den Erfolg der Kampagne verfolgen kann

  Background:
    Given ich bin als Administrator eingeloggt
    And es existiert eine aktive Kampagne "Ostern 2025"

  Scenario: Gesamtstatistiken anzeigen
    Given die Kampagne hat 15 registrierte Teilnehmer
    And es wurden insgesamt 45 Funde gemacht
    When ich die Statistik-Übersicht öffne
    Then sollte ich die Anzahl der Teilnehmer sehen
    And ich sollte die Gesamtzahl der Funde sehen
    And ich sollte die durchschnittliche Anzahl Funde pro Teilnehmer sehen

  Scenario: QR-Code spezifische Statistiken
    Given es existieren QR-Codes mit unterschiedlichen Fundanzahlen
    When ich die QR-Code Statistiken öffne
    Then sollte ich für jeden QR-Code sehen:
      | QR-Code Titel | Anzahl Funde | Letzte Finder |
    And ich sollte die beliebtesten Verstecke identifizieren können
    And ich sollte noch nicht gefundene QR-Codes hervorheben können

  Scenario: Teilnehmer Rangliste
    Given mehrere Teilnehmer haben unterschiedlich viele QR-Codes gefunden
    When ich die Teilnehmer Rangliste öffne
    Then sollte ich eine sortierte Liste aller Teilnehmer sehen
    And jeder Teilnehmer sollte mit seiner Fundanzahl angezeigt werden
    And ich sollte sehen können, wer alle QR-Codes gefunden hat

  Scenario: Zeitbasierte Statistiken
    Given Funde wurden über mehrere Tage gemacht
    When ich die Zeitstatistiken öffne
    Then sollte ich ein Diagramm der Funde pro Tag sehen
    And ich sollte Spitzenzeiten für Aktivität identifizieren können
    And ich sollte die Entwicklung der Teilnahme über Zeit sehen

  Scenario: Detaillierte Fund-Historie
    Given es wurden viele Funde gemacht
    When ich die Fund-Historie öffne
    Then sollte ich eine chronologische Liste aller Funde sehen
    And jeder Eintrag sollte enthalten: Teilnehmer, QR-Code, Zeitstempel
    And ich sollte nach Teilnehmer oder QR-Code filtern können
