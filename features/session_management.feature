Feature: Session Management
  Als System
  Möchte ich Benutzer-Sessions über Cookies verwalten
  Damit Mitarbeiter ohne Login teilnehmen können

  Scenario: Neue Session beim ersten Besuch erstellen
    Given ein Mitarbeiter besucht die Seite zum ersten Mal
    When er einen QR-Code scannt
    Then sollte eine neue Session erstellt werden
    And ein Session-Cookie sollte gesetzt werden
    And die Session sollte eine eindeutige ID haben

  Scenario: Bestehende Session wiedererkennen
    Given ein Mitarbeiter hat bereits eine Session
    And sein Session-Cookie ist noch gültig
    When er einen neuen QR-Code scannt
    Then sollte seine bestehende Session verwendet werden
    And seine vorherigen Funde sollten verfügbar sein

  Scenario: Abgelaufene Session handhaben
    Given ein Mitarbeiter hat eine abgelaufene Session
    When er einen QR-Code scannt
    Then sollte eine neue Session erstellt werden
    And er sollte erneut seinen Namen eingeben müssen
    And seine alten Funde sollten nicht verloren gehen

  Scenario: Session-Daten speichern
    Given ein Mitarbeiter ist registriert
    When er QR-Codes findet
    Then sollten alle Funde mit seiner Session verknüpft werden
    And die Session sollte seinen Namen speichern
    And die Session sollte seine Kampagnen-Teilnahme speichern

  Scenario: Session zwischen verschiedenen Geräten
    Given ein Mitarbeiter nutzt verschiedene Geräte
    When er von einem neuen Gerät einen QR-Code scannt
    Then sollte er eine neue Session auf diesem Gerät erhalten
    And seine Funde von anderen Geräten sollten separat bleiben
    And er sollte seinen Namen erneut eingeben müssen
