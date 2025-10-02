Feature: Mitarbeiter Registrierung
  Als Mitarbeiter
  Möchte ich mich beim ersten QR-Code Scan registrieren können
  Damit ich am Easter Egg Hunt teilnehmen kann

  Scenario: Erste Registrierung beim QR-Code Scan
    Given ich scanne einen QR-Code zum ersten Mal
    And ich habe noch keine Session
    When die QR-Code Seite lädt
    Then sollte ich ein Registrierungsformular sehen
    And ich sollte aufgefordert werden, meinen Namen einzugeben

  Scenario: Erfolgreiche Registrierung
    Given ich bin auf der Registrierungsseite
    When ich meinen Namen "Max Mustermann" eingebe
    And ich auf "Teilnehmen" klicke
    Then sollte eine Session für mich erstellt werden
    And ich sollte zur Fundbestätigung weitergeleitet werden
    And mein Name sollte in einem Cookie gespeichert werden

  Scenario: Registrierung mit leerem Namen
    Given ich bin auf der Registrierungsseite
    When ich keinen Namen eingebe
    And ich auf "Teilnehmen" klicke
    Then sollte ich eine Fehlermeldung sehen
    And ich sollte auf der Registrierungsseite bleiben

  Scenario: Bereits registrierter Benutzer scannt QR-Code
    Given ich bin bereits registriert als "Anna Schmidt"
    And ich habe eine aktive Session
    When ich einen QR-Code scanne
    Then sollte ich direkt zur Fundbestätigung weitergeleitet werden
    And ich sollte keine Registrierung durchführen müssen
