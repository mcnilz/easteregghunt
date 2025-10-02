Feature: Kampagnen Verwaltung
  Als Administrator
  Möchte ich Kampagnen erstellen und verwalten können
  Damit ich verschiedene Easter Egg Hunt Events organisieren kann

  Scenario: Neue Kampagne erstellen
    Given ich bin als Administrator eingeloggt
    When ich auf "Neue Kampagne" klicke
    And ich einen Kampagnennamen eingebe
    And ich eine Beschreibung hinzufüge
    And ich auf "Kampagne erstellen" klicke
    Then sollte die neue Kampagne in der Übersicht erscheinen
    And ich sollte eine Bestätigungsnachricht sehen

  Scenario: Kampagne bearbeiten
    Given ich bin als Administrator eingeloggt
    And es existiert eine Kampagne "Ostern 2025"
    When ich auf "Bearbeiten" bei der Kampagne klicke
    And ich den Namen zu "Ostern 2025 - Büro Hamburg" ändere
    And ich auf "Speichern" klicke
    Then sollte der neue Name in der Übersicht angezeigt werden

  Scenario: Kampagne löschen
    Given ich bin als Administrator eingeloggt
    And es existiert eine Kampagne "Test Kampagne"
    When ich auf "Löschen" bei der Kampagne klicke
    And ich die Löschung bestätige
    Then sollte die Kampagne nicht mehr in der Übersicht erscheinen

  Scenario: Kampagnen Übersicht anzeigen
    Given ich bin als Administrator eingeloggt
    And es existieren mehrere Kampagnen
    When ich die Kampagnen Übersicht öffne
    Then sollte ich alle Kampagnen mit ihren Details sehen
    And ich sollte die Anzahl der QR-Codes pro Kampagne sehen
