# Datenschutzerklärung (GDPR-Compliance)

## Einleitung

Diese Datenschutzerklärung informiert Sie über die Art, den Umfang und Zweck der Verarbeitung von personenbezogenen Daten in unserem Easter Egg Hunt System.

## Verantwortlicher

Der Verantwortliche für die Datenverarbeitung ist das Easter Egg Hunt Team.

## Erhobene Daten

Das System erhebt und verarbeitet folgende personenbezogene Daten:

### Benutzerdaten
- **Name**: Name des Mitarbeiters (wird beim ersten QR-Code-Scan erfasst)
- **Registrierungszeitpunkt**: Zeitpunkt der ersten Registrierung (FirstSeen)
- **Letzte Aktivität**: Zeitpunkt der letzten Aktivität (LastSeen)

### Session-Daten
- **Session-IDs**: Eindeutige Identifikatoren für Benutzer-Sessions
- **Session-Expiration**: Ablaufzeitpunkt der Session
- **Session-Daten**: Zusätzliche Daten, die in der Session gespeichert werden

### Automatisch erfasste Daten
- **IP-Adresse**: Wird bei QR-Code-Scans erfasst
- **User-Agent**: Browser-/Geräteinformationen bei QR-Code-Scans

## Zweck der Datenverarbeitung

Die Datenverarbeitung erfolgt ausschließlich zu folgenden Zwecken:

1. **Spiel-Funktionalität**: 
   - Registrierung von Mitarbeitern
   - Tracking von QR-Code-Funden
   - Anzeige von Statistiken und Ranglisten

2. **Session-Verwaltung**:
   - Bereitstellung einer nahtlosen Benutzererfahrung
   - Sicherstellung der Benutzer-Authentifizierung

3. **Statistiken**:
   - Aggregierte Statistiken über Spiel-Teilnahme
   - Anonymisierte Auswertungen

## Rechtsgrundlage

Die Verarbeitung personenbezogener Daten erfolgt auf Grundlage von:
- **Einwilligung**: Durch die Registrierung und Teilnahme am Spiel geben Benutzer ihre Einwilligung zur Datenverarbeitung
- **Berechtigtes Interesse**: Erhebung von Statistiken für interne Zwecke

## Speicherdauer

### Benutzerdaten
- Benutzerdaten werden gespeichert, solange der Benutzer aktiv ist
- Nach Inaktivität: Daten werden gemäß Unternehmensrichtlinien aufbewahrt

### Session-Daten
- **Admin-Sessions**: Ablauf nach 8 Stunden
- **Employee-Sessions**: Ablauf nach 30 Tagen
- **Automatische Bereinigung**: Abgelaufene Sessions werden automatisch gelöscht (konfigurierbares Interval, Standard: 24 Stunden)

### QR-Code-Funde
- Funde werden dauerhaft gespeichert für Statistiken (optional löschbar bei GDPR-Anfrage)

## Ihre Rechte (DSGVO)

Als betroffene Person haben Sie folgende Rechte:

### Recht auf Auskunft (Art. 15 DSGVO)
Sie haben das Recht, Auskunft über Ihre gespeicherten personenbezogenen Daten zu erhalten.

### Recht auf Berichtigung (Art. 16 DSGVO)
Sie haben das Recht, die Berichtigung unrichtiger Daten zu verlangen.

### Recht auf Löschung (Art. 17 DSGVO - "Recht auf Vergessenwerden")
Sie haben das Recht, die Löschung Ihrer personenbezogenen Daten zu verlangen.

**API-Endpoint für Datenlöschung:**
```
POST /api/users/gdpr/delete
```

**Request:**
```json
{
  "userId": 1,
  "deleteFinds": false
}
```

**Response:**
```json
{
  "deletedSessions": 5,
  "deletedFinds": 0,
  "userDeleted": true,
  "totalDeleted": 6
}
```

### Recht auf Einschränkung der Verarbeitung (Art. 18 DSGVO)
Sie haben das Recht, die Einschränkung der Verarbeitung Ihrer Daten zu verlangen.

### Recht auf Datenübertragbarkeit (Art. 20 DSGVO)
Sie haben das Recht, Ihre Daten in einem strukturierten Format zu erhalten.

### Recht auf Anonymisierung
Sie können auch die Anonymisierung Ihrer Daten beantragen, anstatt die vollständige Löschung.

**API-Endpoint für Anonymisierung:**
```
POST /api/users/{id}/gdpr/anonymize
```

## Datenübertragung und Drittanbieter

Das System speichert Daten ausschließlich lokal (SQLite-Datenbank) und überträgt keine Daten an Drittanbieter.

## Cookies

Das System verwendet Cookies für Session-Management. Details finden Sie in der [Cookie-Richtlinie](COOKIE_POLICY.md).

## Sicherheitsmaßnahmen

Das System implementiert folgende Sicherheitsmaßnahmen:

- **HttpOnly-Cookies**: Verhindert Zugriff auf Cookies über JavaScript
- **Secure-Cookies**: In Production nur über HTTPS
- **SameSite-Attribute**: Schutz vor CSRF-Angriffen
- **Session-Timeout**: Automatischer Ablauf von Sessions
- **Automatische Bereinigung**: Regelmäßige Löschung abgelaufener Sessions

## Kontakt

Bei Fragen zum Datenschutz oder zur Ausübung Ihrer Rechte kontaktieren Sie bitte das Easter Egg Hunt Team.

## Änderungen dieser Datenschutzerklärung

Diese Datenschutzerklärung kann von Zeit zu Zeit aktualisiert werden. Die aktuelle Version ist jederzeit in der Dokumentation verfügbar.

---

**Stand:** Oktober 2025  
**Version:** 1.0


