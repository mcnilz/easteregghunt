Feature: QR-Code Drucklayout
  Als Administrator
  Möchte ich QR-Codes in einem druckfreundlichen Layout anzeigen können
  Damit ich sie ausdrucken und im Büro verstecken kann

  Background:
    Given ich bin als Administrator eingeloggt
    And es existiert eine Kampagne "Ostern 2025" mit QR-Codes

  Scenario: Druckansicht für alle QR-Codes öffnen
    Given die Kampagne "Ostern 2025" hat 8 QR-Codes
    When ich auf "Druckansicht" klicke
    Then sollte sich eine neue Seite mit Drucklayout öffnen
    And alle QR-Codes sollten in einem Raster angeordnet sein
    And jeder QR-Code sollte seinen Titel anzeigen
    And die Seite sollte für A4 Format optimiert sein

  Scenario: Einzelne QR-Codes für Druck auswählen
    Given die Kampagne hat mehrere QR-Codes
    When ich 3 spezifische QR-Codes auswähle
    And ich auf "Ausgewählte drucken" klicke
    Then sollte die Druckansicht nur die 3 ausgewählten QR-Codes zeigen

  Scenario: Drucklayout anpassen
    Given ich bin in der Druckansicht
    When ich die Druckoptionen öffne
    Then sollte ich die Anzahl der QR-Codes pro Seite einstellen können
    And ich sollte die Größe der QR-Codes anpassen können
    And ich sollte wählen können, ob Titel angezeigt werden sollen

  Scenario: QR-Codes drucken
    Given ich bin in der Druckansicht
    When ich auf "Drucken" klicke
    Then sollte der Browser Druckdialog geöffnet werden
    And die QR-Codes sollten korrekt formatiert sein
    And die Qualität sollte zum Scannen ausreichend sein
