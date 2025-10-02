Feature: QR-Code Verwaltung
  Als Administrator
  Möchte ich QR-Codes für eine Kampagne erstellen und verwalten können
  Damit Mitarbeiter diese im Büro finden können

  Background:
    Given ich bin als Administrator eingeloggt
    And es existiert eine aktive Kampagne "Ostern 2025"

  Scenario: Neuen QR-Code erstellen
    Given ich bin in der Kampagne "Ostern 2025"
    When ich auf "Neuer QR-Code" klicke
    And ich den Titel "Versteck am Drucker" eingebe
    And ich die interne Notiz "Unter dem HP Drucker im 2. Stock" eingebe
    And ich auf "QR-Code erstellen" klicke
    Then sollte der QR-Code in der Liste erscheinen
    And der QR-Code sollte eine eindeutige URL enthalten
    And ich sollte eine Bestätigungsnachricht sehen

  Scenario: QR-Code bearbeiten
    Given es existiert ein QR-Code "Versteck in der Küche"
    When ich auf "Bearbeiten" bei dem QR-Code klicke
    And ich den Titel zu "Versteck am Kühlschrank" ändere
    And ich die interne Notiz aktualisiere
    And ich auf "Speichern" klicke
    Then sollten die Änderungen gespeichert werden
    And der aktualisierte Titel sollte angezeigt werden

  Scenario: QR-Code löschen
    Given es existiert ein QR-Code "Test Versteck"
    When ich auf "Löschen" bei dem QR-Code klicke
    And ich die Löschung bestätige
    Then sollte der QR-Code nicht mehr in der Liste erscheinen

  Scenario: QR-Codes einer Kampagne anzeigen
    Given die Kampagne "Ostern 2025" hat 5 QR-Codes
    When ich die QR-Code Übersicht öffne
    Then sollte ich alle 5 QR-Codes sehen
    And jeder QR-Code sollte Titel und interne Notiz anzeigen
    And ich sollte sehen können, wie oft jeder Code gefunden wurde
