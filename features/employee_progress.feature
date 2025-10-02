Feature: Mitarbeiter Fortschritt
  Als Mitarbeiter
  Möchte ich meinen Fortschritt im Easter Egg Hunt sehen können
  Damit ich weiß, wie viele Verstecke ich noch finden muss

  Background:
    Given ich bin registriert als "Tom Müller"
    And es existiert eine Kampagne "Ostern 2025" mit 10 QR-Codes

  Scenario: Fortschritt nach erstem Fund anzeigen
    Given ich habe 1 QR-Code gefunden
    When ich meine Fortschrittsseite öffne
    Then sollte ich sehen "1 von 10 Verstecken gefunden"
    And ich sollte eine Fortschrittsleiste sehen
    And ich sollte den Titel des gefundenen Verstecks sehen

  Scenario: Alle gefundenen Verstecke anzeigen
    Given ich habe 5 QR-Codes gefunden
    When ich meine Fortschrittsseite öffne
    Then sollte ich eine Liste aller 5 gefundenen Verstecke sehen
    And jedes Versteck sollte mit Fundzeit angezeigt werden
    And die Verstecke sollten chronologisch sortiert sein

  Scenario: Noch zu findende Verstecke anzeigen
    Given ich habe 7 von 10 QR-Codes gefunden
    When ich meine Fortschrittsseite öffne
    Then sollte ich sehen "Noch 3 Verstecke zu finden!"
    And ich sollte motivierende Nachrichten sehen
    And die gefundenen Verstecke sollten markiert sein

  Scenario: Kampagne vollständig abgeschlossen
    Given ich habe alle 10 QR-Codes gefunden
    When ich meine Fortschrittsseite öffne
    Then sollte ich eine Glückwunschnachricht sehen
    And ich sollte meine Gesamtzeit sehen
    And ich sollte eine Übersicht aller Funde mit Zeiten sehen

  Scenario: Fortschritt ohne Funde
    Given ich habe noch keine QR-Codes gefunden
    When ich meine Fortschrittsseite öffne
    Then sollte ich sehen "0 von 10 Verstecken gefunden"
    And ich sollte Hinweise zum Starten erhalten
    And ich sollte ermutigt werden, den ersten QR-Code zu suchen
