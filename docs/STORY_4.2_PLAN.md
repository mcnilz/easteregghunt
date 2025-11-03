# Story 4.2: Session-Management Optimierung - Implementierungsplan

## 📋 Story-Übersicht

**Als** System  
**Möchte ich** Sessions effizient verwalten  
**Damit** Benutzer eine nahtlose Erfahrung haben  

**Aufwand:** 8 Story Points

## ✅ Aktuelle Implementierung (Analyse)

### Vorhanden:
1. **Session Entity** (`Domain/Entities/Session.cs`)
   - ✅ Id, UserId, CreatedAt, ExpiresAt, Data, IsActive
   - ✅ IsValid(), Extend(), Deactivate(), UpdateData()
   
2. **SessionRepository** (`Infrastructure/Repositories/SessionRepository.cs`)
   - ✅ CRUD-Operationen
   - ✅ `DeleteExpiredAsync()` - Methode vorhanden
   - ✅ `GetActiveAsync()` - Aktive Sessions abrufen
   
3. **SessionService** (`Application/Services/SessionService.cs`)
   - ✅ CreateSessionAsync, GetSessionByIdAsync, ValidateSessionAsync
   - ✅ ExtendSessionAsync, DeactivateSessionAsync, UpdateSessionDataAsync
   
4. **Cookie-Konfiguration** (`Web/Program.cs`)
   - ✅ HttpOnly = true
   - ✅ Secure (Development: None, Production: Always)
   - ✅ SameSite (Admin: Strict, Employee: Lax)
   - ✅ ExpireTimeSpan (Admin: 8h, Employee: 30d)
   - ✅ SlidingExpiration = true

5. **Tests**
   - ✅ SessionRepository Integration Tests
   - ⚠️ SessionService Tests fehlen
   - ⚠️ Cookie-Sicherheit Tests fehlen

### Fehlend/Verbesserungsbedarf:
1. **Session-Bereinigung** ❌
   - ❌ Keine automatische Bereinigung abgelaufener Sessions
   - ✅ Repository-Methode vorhanden, aber nicht automatisch aufgerufen
   
2. **Cookie-Sicherheit** ⚠️
   - ⚠️ Teilweise implementiert, aber Tests fehlen
   - ⚠️ Keine Validierung der Cookie-Konfiguration
   
3. **Session-Timeout** ✅
   - ✅ Bereits implementiert in Program.cs
   - ⚠️ Tests fehlen
   
4. **Geräte-übergreifende Behandlung** ✅
   - ✅ Bereits funktional (verschiedene Sessions pro User)
   - ⚠️ Tests fehlen
   
5. **GDPR-Compliance** ❌
   - ❌ Keine Löschung von Benutzerdaten
   - ❌ Keine Export-Funktion für Benutzerdaten
   - ❌ Keine Datenschutz-Richtlinien implementiert

6. **Tests** ❌
   - ❌ SessionService Tests fehlen komplett
   - ❌ Cookie-Konfiguration Tests fehlen
   - ❌ GDPR-Compliance Tests fehlen

## 🎯 Implementierungsplan

### Phase 1: Session-Bereinigung (2 Story Points)
**Ziel:** Automatische Bereinigung abgelaufener Sessions

**Aufgaben:**
1. Background Service für Session-Bereinigung erstellen
   - `SessionCleanupService` (IHostedService)
   - Konfigurierbarer Interval (z.B. alle 24h)
   - Verwendung von `SessionRepository.DeleteExpiredAsync()`
   
2. Tests schreiben
   - Unit Tests für Background Service
   - Integration Tests für automatische Bereinigung

**Akzeptanzkriterien:**
- [ ] Background Service läuft automatisch
- [ ] Abgelaufene Sessions werden regelmäßig gelöscht
- [ ] Service ist konfigurierbar
- [ ] 100% Test Coverage

### Phase 2: Cookie-Sicherheit verbessern (2 Story Points)
**Ziel:** Cookie-Sicherheit validieren und testen

**Aufgaben:**
1. Cookie-Konfiguration validieren
   - Prüfung: HttpOnly immer aktiv
   - Prüfung: Secure in Production
   - Prüfung: SameSite korrekt konfiguriert
   
2. Tests schreiben
   - Unit Tests für Cookie-Konfiguration
   - Integration Tests für Cookie-Verhalten

**Akzeptanzkriterien:**
- [ ] HttpOnly ist immer aktiv
- [ ] Secure ist in Production aktiv
- [ ] SameSite ist korrekt konfiguriert (Admin: Strict, Employee: Lax)
- [ ] 100% Test Coverage

### Phase 3: Session-Timeout validieren (1 Story Point)
**Ziel:** Session-Timeout korrekt implementiert und getestet

**Aufgaben:**
1. Session-Timeout validieren
   - Admin: 8 Stunden (bereits implementiert)
   - Employee: 30 Tage (bereits implementiert)
   - Sliding Expiration prüfen
   
2. Tests schreiben
   - Unit Tests für Session-Timeout
   - Integration Tests für ablaufende Sessions

**Akzeptanzkriterien:**
- [ ] Admin-Sessions laufen nach 8h ab
- [ ] Employee-Sessions laufen nach 30 Tagen ab
- [ ] Sliding Expiration funktioniert
- [ ] 100% Test Coverage

### Phase 4: Geräte-übergreifende Behandlung (1 Story Point)
**Ziel:** Testen und dokumentieren

**Aufgaben:**
1. Geräte-übergreifende Behandlung testen
   - Verschiedene Sessions pro User
   - Session-Isolation zwischen Geräten
   
2. Tests schreiben
   - Integration Tests für Multi-Device-Szenarien

**Akzeptanzkriterien:**
- [ ] Verschiedene Geräte erhalten verschiedene Sessions
- [ ] Sessions sind isoliert zwischen Geräten
- [ ] 100% Test Coverage

### Phase 5: GDPR-Compliance (2 Story Points)
**Ziel:** Datenschutz-Richtlinien implementieren

**Aufgaben:**
1. Benutzerdaten-Löschung
   - Löschung aller Sessions eines Benutzers
   - Löschung aller Funde eines Benutzers (optional)
   - API-Endpoint für Datenlöschung
   
2. Datenschutz-Dokumentation
   - Datenschutzerklärung
   - Cookie-Richtlinie
   - GDPR-Compliance-Hinweise

**Akzeptanzkriterien:**
- [ ] Benutzer können ihre Daten löschen lassen
- [ ] Alle Sessions werden gelöscht
- [ ] Datenschutz-Dokumentation vorhanden
- [ ] 100% Test Coverage

## 📊 Test-Strategie (Test-Pyramide)

### Unit Tests (70%)
- SessionService Tests
- Cookie-Konfiguration Tests
- Session-Timeout Tests

### Integration Tests (25%)
- Session-Bereinigung Tests
- Multi-Device-Szenarien Tests
- GDPR-Compliance Tests

### E2E Tests (5%)
- Session-Lifecycle Tests
- Cookie-Verhalten Tests

## 🚀 Implementierungs-Reihenfolge

1. **Phase 1:** Session-Bereinigung
2. **Phase 2:** Cookie-Sicherheit
3. **Phase 3:** Session-Timeout
4. **Phase 4:** Geräte-übergreifende Behandlung
5. **Phase 5:** GDPR-Compliance

## ✅ Definition of Done

- [ ] Alle Akzeptanzkriterien erfüllt
- [ ] 100% Test Coverage erreicht
- [ ] Code Review abgeschlossen
- [ ] Dokumentation aktualisiert
- [ ] Alle Tests laufen erfolgreich

