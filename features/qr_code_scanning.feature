Feature: QR-Code Scannen und Funde
  Als Mitarbeiter
  Möchte ich QR-Codes scannen und meine Funde verfolgen können
  Damit ich am Easter Egg Hunt teilnehmen kann

  Background:
    Given ich bin registriert als "Lisa Weber"
    And es existiert eine aktive Kampagne mit QR-Codes

  Scenario: Ersten QR-Code erfolgreich scannen
    Given ich habe noch keine QR-Codes gefunden
    When ich einen QR-Code "Versteck am Drucker" scanne
    Then sollte ich eine Fundbestätigung sehen
    And der Fund sollte mit Zeitstempel gespeichert werden
    And mein Fortschritt sollte aktualisiert werden

  Scenario: Bereits gefundenen QR-Code erneut scannen
    Given ich habe den QR-Code "Versteck in der Küche" bereits gefunden
    When ich denselben QR-Code erneut scanne
    Then sollte ich eine Nachricht sehen "Du hast dieses Versteck bereits gefunden!"
    And der Fund sollte trotzdem erneut gespeichert werden
    And ich sollte das Datum meines ersten Fundes sehen

  Scenario: Ungültigen QR-Code scannen
    Given ich scanne einen QR-Code der nicht zur aktuellen Kampagne gehört
    When die Seite lädt
    Then sollte ich eine Fehlermeldung sehen
    And ich sollte einen Link zur Hauptseite erhalten

  Scenario: QR-Code scannen ohne aktive Kampagne
    Given es gibt keine aktive Kampagne
    When ich einen QR-Code scanne
    Then sollte ich eine Nachricht sehen "Derzeit läuft keine Kampagne"
    And ich sollte Informationen über kommende Events erhalten
