Feature: Admin Authentication
  Als Administrator
  Möchte ich mich sicher einloggen können
  Damit ich das Easter Egg Hunt System verwalten kann

  Scenario: Erfolgreicher Admin Login
    Given ich bin auf der Admin Login Seite
    When ich gültige Anmeldedaten eingebe
    And ich auf "Anmelden" klicke
    Then sollte ich zur Admin Dashboard weitergeleitet werden
    And ich sollte eine erfolgreiche Login Nachricht sehen

  Scenario: Fehlgeschlagener Login mit ungültigen Daten
    Given ich bin auf der Admin Login Seite
    When ich ungültige Anmeldedaten eingebe
    And ich auf "Anmelden" klicke
    Then sollte ich eine Fehlermeldung sehen
    And ich sollte auf der Login Seite bleiben

  Scenario: Admin Logout
    Given ich bin als Administrator eingeloggt
    When ich auf "Abmelden" klicke
    Then sollte ich zur Login Seite weitergeleitet werden
    And meine Session sollte beendet sein
