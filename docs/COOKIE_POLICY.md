# Cookie-Richtlinie

## Übersicht

Das Easter Egg Hunt System verwendet Cookies zur Session-Verwaltung und Benutzer-Authentifizierung.

## Verwendete Cookies

### Admin-Authentifizierungs-Cookie
- **Name**: `EasterEggHunt.Admin`
- **Zweck**: Authentifizierung für Admin-Benutzer
- **Gültigkeitsdauer**: 8 Stunden
- **Sliding Expiration**: Ja (Session wird bei Aktivität verlängert)
- **HttpOnly**: Ja (Schutz vor XSS-Angriffen)
- **Secure**: Ja (in Production, nur HTTPS)
- **SameSite**: Strict (Schutz vor CSRF-Angriffen)

### Employee-Authentifizierungs-Cookie
- **Name**: `EasterEggHunt.Employee`
- **Zweck**: Authentifizierung für Mitarbeiter
- **Gültigkeitsdauer**: 30 Tage
- **Sliding Expiration**: Ja (Session wird bei Aktivität verlängert)
- **HttpOnly**: Ja (Schutz vor XSS-Angriffen)
- **Secure**: Ja (in Production, nur HTTPS)
- **SameSite**: Lax (ermöglicht QR-Code-Scans von anderen Domains)

### Session-Cookie
- **Name**: `.AspNetCore.Session`
- **Zweck**: Session-Verwaltung (zustandsbehaftete Daten)
- **Gültigkeitsdauer**: 8 Stunden (Idle-Timeout)
- **HttpOnly**: Ja
- **Secure**: Ja (in Production, nur HTTPS)
- **SameSite**: Strict
- **IsEssential**: Ja (erforderlich für grundlegende Funktionalität)

## Cookie-Konfiguration nach Umgebung

### Development
- **Secure**: Deaktiviert (ermöglicht HTTP)
- **SameSite**: Wie oben konfiguriert

### Production
- **Secure**: Aktiviert (nur HTTPS)
- **SameSite**: Wie oben konfiguriert

## Zweck der Cookies

Cookies werden ausschließlich zu folgenden Zwecken verwendet:

1. **Authentifizierung**: Identifikation angemeldeter Benutzer
2. **Session-Management**: Verwaltung von Benutzer-Sitzungen
3. **Sicherheit**: Schutz vor XSS- und CSRF-Angriffen

## Deaktivierung von Cookies

Die Deaktivierung von Cookies ist möglich, führt jedoch dazu, dass das System nicht vollständig funktionsfähig ist, da Session-Management erforderlich ist.

## Cookie-Verwaltung

### Automatische Bereinigung
- Abgelaufene Sessions werden automatisch bereinigt
- Interval: Konfigurierbar (Standard: 24 Stunden)
- Start-Delay: 30 Sekunden nach App-Start

### Cookie-Löschung
Bei GDPR-Datenlöschung werden alle Cookies und Session-Daten eines Benutzers gelöscht.

## Kontakt

Bei Fragen zur Cookie-Verwendung kontaktieren Sie bitte das Easter Egg Hunt Team.

---

**Stand:** Oktober 2025  
**Version:** 1.0


