# 📋 Setări Aplicație - Lista Completă de Implementat

**Aplicație:** ValyanClinic - Sistem Medical Management  
**Framework:** .NET 9 Blazor Server  
**Data:** 2025-01-15  
**Status:** 📝 **PLANNING**

---

## 🎯 Categorii Principale de Setări

### 1️⃣ **AUTENTIFICARE ȘI SECURITATE**
- [x] Politici parole (lungime minimă, complexitate, expirare)
- [ ] Autentificare multi-factor (2FA/MFA)
- [x] Timeout sesiune utilizator (inactivitate)
- [x] Lockout cont după N încercări eșuate
- [ ] IP whitelisting/blacklisting
- [x] Audit log pentru accesări sistem
- [ ] Politici de criptare date sensibile (CNP, date medicale)
- [ ] GDPR compliance settings (consimțământ prelucrare date)
- [x] **Securitate parolă implicită pentru conturi noi**
- [ ] **Forțare schimbare parolă la prima autentificare**
- [x] **Istoric parole (nu permite refolosirea ultimelor N parole)**
- [ ] **Sign-out automat la inactivitate (toate dispozitivele)**
- [ ] **Sesiuni concurente maxime per utilizator**
- [ ] **Restricții orare acces (program de lucru)**

### 2️⃣ **ROLURI ȘI PERMISIUNI**
- [ ] Definire roluri sistem (Admin, Doctor, Asistent, Recepție, Manager, etc.)
- [ ] Matrice permisiuni per rol (CRUD pe module)
- [ ] Grupuri utilizatori (Departamente, Echipe, Ture)
- [ ] Permisiuni granulare (vizualizare, creare, editare, ștergere, export)
- [ ] Delegare temporară permisiuni (vacanțe, înlocuiri)
- [ ] Restricții acces date pacienți (medic de familie vs. specialist)
- [ ] Acces emergență (bypass temporar pentru urgențe medicale)
- [ ] **Ierarhie roluri (moștenire permisiuni)**
- [ ] **Permisiuni la nivel de câmp (mascare date sensibile)**
- [ ] **Permisiuni geografice (acces doar pacienți din regiunea X)**
- [ ] **Permisiuni temporale (acces doar în timpul programului)**
- [ ] **Approval workflow pentru acțiuni critice (ștergere, export bulk)**
- [ ] **Segregation of duties (un utilizator nu poate aproba propria acțiune)**

### 3️⃣ **CONFIGURĂRI MEDICALE SPECIFICE**
- [ ] Setări cabinet medical (nume, CIF, registru comerț)
- [ ] Parafa medicală electronică
- [ ] Configurare protocoale medicale
- [ ] Template-uri rețete medicale
- [ ] Template-uri scrisori medicale
- [ ] Template-uri diagnostic
- [ ] Coduri ICD-10 folosite frecvent
- [ ] Nomenclator servicii medicale
- [ ] Tarife consultații și investigații
- [ ] Contract CNAS (Casa Națională de Asigurări de Sănătate)
- [ ] Setări facturare CNAS vs. privat
- [ ] Nomenclator medicamente (prescripții)
- [ ] Interacțiuni medicamentoase (alerte)
- [ ] Alergii frecvente (database)
- [ ] **Coduri CPT/HCPCS (pentru facturare internațională)**
- [ ] **Protocoale de triaj (urgență, severitate)**
- [ ] **Setări consimțământ informat (template-uri per procedură)**
- [ ] **Contraindicații standard per procedură)**
- [ ] **Valori normale analize (range-uri per vârstă/sex)**
- [ ] **Unități de măsură medicale (mg/dl vs. mmol/l)**
- [ ] **Template-uri concediu medical**
- [ ] **Template-uri certificate medicale**
- [ ] **Template-uri adeverințe medicale**
- [ ] **Setări prescripții (max. medicamente per rețetă, validitate)**
- [ ] **Coduri CAEN pentru activități medicale**

### 4️⃣ **PROGRAMĂRI ȘI CALENDAR**
- [ ] Ore funcționare cabinet
- [ ] Durata implicită consultație (15, 30, 45 min)
- [ ] Zile libere și sărbători
- [ ] Ore pauză (prânz, pauze)
- [ ] Notificări automate pacienți (SMS, email, WhatsApp)
- [ ] Reminder programări (cu X ore/zile înainte)
- [ ] Politici anulare/reprogramare
- [ ] Overbooking toleranță (0%, 10%, 20%)
- [ ] Culori codificare status programări
- [ ] **Timp buffer între consultații (dezinfecție, documentație)**
- [ ] **Programări recurente (control periodic)**
- [ ] **Blackout periods (vacanțe medic, training)**
- [ ] **Setări prioritizare (urgențe, VIP-uri)**
- [ ] **Limită reprogramări per pacient**
- [ ] **Penalizare no-show (restricții programări viitoare)**
- [ ] **Check-in automat (QR code, SMS confirm)**
- [ ] **Sala de așteptare virtuală (estimare timp așteptare)**
- [ ] **Sincronizare cu calendare externe (Google, Outlook)**

### 5️⃣ **NOTIFICĂRI ȘI ALERTE**
- [ ] Email SMTP settings (server, port, credențiale)
- [ ] SMS gateway integration
- [ ] Push notifications (desktop, mobile)
- [ ] Template-uri email (confirmare programare, reminder, rețete)
- [ ] Template-uri SMS
- [ ] Alerte stoc medicamente (nivel minim)
- [ ] Alerte expirare acte medicale (autorizații, avize)
- [ ] Alerte expirare acte personal (CI, permise, atestate)
- [ ] Alerte deadlines raportări CNAS/MS
- [ ] Notificări rezultate investigații
- [ ] **Alerte valori critice analize (panic values)**
- [ ] **Alerte interacțiuni medicamentoase**
- [ ] **Alerte alergii pacient la prescripție**
- [ ] **Alerte duplicate investigații (în ultimele X zile)**
- [ ] **Notificări zi de naștere pacient**
- [ ] **Notificări recall consultații (mamografie, PAP test, etc.)**
- [ ] **Alerte echipamente medicale (calibrare, mentenanță)**
- [ ] **Escalation alerts (management pentru situații critice)**
- [ ] **Quiet hours (nu trimite notificări între 22:00-08:00)**

### 6️⃣ **RAPORTARE ȘI STATISTICI**
- [ ] Rapoarte predefinite (zilnice, săptămânale, lunare)
- [ ] Exporturi format (PDF, Excel, CSV)
- [ ] Setări date afișate în dashboard
- [ ] Statistici medicale (KPI-uri)
- [ ] Raportări legale (MS, CNAS, ANMDM)
- [ ] Arhivare automată rapoarte
- [ ] Retentie date (câți ani se păstrează)
- [ ] **Report scheduling (generare automată și email)**
- [ ] **Custom reports builder (drag & drop)**
- [ ] **Dashboards per rol (medic vs. manager vs. admin)**
- [ ] **Benchmarking cu alte clinici (anonimizat)**
- [ ] **Previziuni (ocupare, venituri, stocuri)**
- [ ] **Raportare incidente medicale**
- [ ] **Statistici satisfacție pacienți (NPS, CSAT)**
- [ ] **Raportare infecții nosocomiale**
- [ ] **Statistici outcome-uri tratamente**

### 7️⃣ **INTEGRĂRI EXTERNE**
- [ ] Platformă CNAS (raportări online)
- [ ] Platformă SIUI (Sistemul Informatic Unic Integrat)
- [ ] RETETAR (prescripții electronice)
- [ ] DES (Dosarul Electronic de Sănătate)
- [ ] Laboratoare externe (rezultate investigații)
- [ ] Sisteme imagistică (PACS - radiologie)
- [ ] E-Factura (ANAF)
- [ ] Sisteme contabilitate (Saga, WizOne, etc.)
- [ ] Gateway-uri plată online
- [ ] **HL7/FHIR integration (interoperabilitate)**
- [ ] **DICOM integration (imagistică medicală)**
- [ ] **Farmacie online (verificare disponibilitate medicamente)**
- [ ] **Call center integration (programări telefonice)**
- [ ] **WhatsApp Business API**
- [ ] **Google My Business (review-uri, programări)**
- [ ] **Platforme telemedicină terțe**
- [ ] **Sisteme ERP pentru aprovizionare**
- [ ] **API public pentru aplicații mobile terțe**

### 8️⃣ **UI/UX PERSONALIZARE**
- [ ] Tema aplicație (light/dark mode)
- [ ] Culori principale (branding)
- [ ] Logo clinică
- [ ] Font size (accesibilitate)
- [ ] Limba interfață (română, engleză, maghiară)
- [ ] Format dată (dd.MM.yyyy vs. MM/dd/yyyy)
- [ ] Format oră (24h vs. 12h AM/PM)
- [ ] Timezone
- [ ] Număr rânduri per pagină (grid-uri)
- [ ] Coloane vizibile default (personalizabile per utilizator)
- [ ] **High contrast mode (accesibilitate)**
- [ ] **Screen reader optimization**
- [ ] **Keyboard shortcuts personalizabile**
- [ ] **Layout preferences (sidebar left/right/collapsed)**
- [ ] **Homepage customizare (widget-uri drag & drop)**
- [ ] **Favorite pages/quick access**
- [ ] **Recent items history**
- [ ] **Bookmark-uri interne**
- [ ] **Notificări desktop position (corner preference)**
- [ ] **Animation speed (fast/normal/slow/off)**

### 9️⃣ **BACKUP ȘI RECUPERARE**
- [ ] Frecvență backup automat (zilnic, săptămânal)
- [ ] Locație backup (local, cloud, hybrid)
- [ ] Retenție backup-uri (câte copii se păstrează)
- [ ] Backup incremental vs. full
- [ ] Testare automată recuperare date
- [ ] Disaster recovery plan settings
- [ ] Encryption backup-uri
- [ ] **Backup verificare integritate (checksums)**
- [ ] **Point-in-time recovery (restore la oră specifică)**
- [ ] **Backup versioning (multiple restore points)**
- [ ] **Off-site backup (geografic separat)**
- [ ] **Backup compression level**
- [ ] **Backup prioritizare (date critice mai des)**
- [ ] **Backup notification (success/failure)**
- [ ] **Automated backup testing (restore simulat lunar)**

### 🔟 **GDPR ȘI COMPLIANCE**
- [ ] Consimțământ prelucrare date (template-uri)
- [ ] Drept de acces date (export date personale)
- [ ] Drept de ștergere (anonimizare după X ani)
- [ ] Data retention policies
- [ ] Audit trail (cine a accesat ce date și când)
- [ ] Portabilitate date (export standard format)
- [ ] Privacy policy și Terms of Service
- [ ] Cookie policy (dacă există parte web publică)
- [ ] Raportare breach-uri de securitate (72h ANSPDCP)
- [ ] **Drept de rectificare (pacient cere corectare date)**
- [ ] **Drept de restricționare prelucrare**
- [ ] **Drept de opoziție**
- [ ] **Data Protection Impact Assessment (DPIA)**
- [ ] **Registrul activităților de prelucrare**
- [ ] **Contracte DPO (Data Protection Officer)**
- [ ] **Minimizare date (collect doar ce e necesar)**
- [ ] **Pseudonimizare date pentru cercetare**
- [ ] **Consimțământ minori (parental consent)**
- [ ] **Transfer date internațional (GDPR compliance)**

### 1️⃣1️⃣ **FACTURARE ȘI FINANCIAR**
- [ ] Date fiscale clinică (CUI, J, cont IBAN)
- [ ] Serie și număr factură (auto-increment)
- [ ] TVA aplicabilă (19%, 9%, scutit)
- [ ] Moduri de plată acceptate (cash, card, transfer)
- [ ] Terminal POS integration
- [ ] Facturare recurentă (abonamente)
- [ ] Discount-uri și promoții
- [ ] Evidență încasări/plăți casa de marcat
- [ ] Raportare Z zilnică
- [ ] **Facturare split (asigurare + pacient)**
- [ ] **Co-pay calculation (contribuție pacient)**
- [ ] **Deductible tracking (franșiză anuală)**
- [ ] **Payment plans (plată în rate)**
- [ ] **Refund policies (rambursări)**
- [ ] **Bad debt management (datorii nerecuperabile)**
- [ ] **Invoice templates per tip serviciu**
- [ ] **Multi-currency support**
- [ ] **Tax exemptions (scutiri medicale)**
- [ ] **Charity care policies (tratamente gratuite)**

### 1️⃣2️⃣ **INVENTAR ȘI STOC**
- [ ] Nivel minim stoc (alerte)
- [ ] Nivel optim stoc
- [ ] Furnizori preferați
- [ ] Comandă automată când stoc < minim
- [ ] Tracking loturi medicamente
- [ ] Tracking expirare (alerte cu X luni înainte)
- [ ] Inventar periodic (lunar, trimestrial)
- [ ] Categorii produse (medicamente, consumabile, echipamente)
- [ ] **FIFO/LIFO/FEFO policies (First Expired First Out)**
- [ ] **Cold chain monitoring (temperatura frigorifice)**
- [ ] **Quarantine area management (produse în așteptare aprobare)**
- [ ] **Recalled items tracking (produse retrase)**
- [ ] **Waste management (disposal policies)**
- [ ] **Barcode/QR code integration**
- [ ] **Inventory cycle counting**
- [ ] **Consignment inventory (stoc furnizor)**
- [ ] **Usage patterns analysis (forecast consum)**

### 1️⃣3️⃣ **RESURSE UMANE**
- [ ] Evidență personal (contracte, pontaj)
- [ ] Concedii medicale și vacanțe
- [ ] Ture și schimburi (planning)
- [ ] Evaluare performanță
- [ ] Training și certificări (reînnoire)
- [ ] Dosare personale electronice
- [ ] **Onboarding workflow (angajați noi)**
- [ ] **Offboarding workflow (plecare angajați)**
- [ ] **Competency management (skills matrix)**
- [ ] **Credentialing și privileging (medici)**
- [ ] **Peer review process**
- [ ] **Incident reporting pentru personal**
- [ ] **Payroll integration**
- [ ] **Benefits administration**
- [ ] **Vaccination tracking (personal medical)**
- [ ] **Background checks tracking**

### 1️⃣4️⃣ **COMUNICARE INTERNĂ**
- [ ] Mesagerie internă (între medici, cu recepția)
- [ ] Anunțuri sistem (broadcast messages)
- [ ] Chat pentru cazuri urgente
- [ ] Partajare documente (protocoale, proceduri)
- [ ] Comentarii pe fișă pacient (istoric intern)
- [ ] **Handoff communication (predare-primire ture)**
- [ ] **Consult requests între specialități**
- [ ] **Team huddles (meeting-uri scurte zilnice)**
- [ ] **Knowledge base (wiki intern)**
- [ ] **Forums/Discussion boards**
- [ ] **Video conferencing intern**
- [ ] **Screen sharing pentru training**
- [ ] **Emergency broadcast system**

### 1️⃣5️⃣ **PERFORMANȚĂ ȘI MONITORIZARE**
- [ ] Logging level (Debug, Info, Warning, Error)
- [ ] Health checks endpoints
- [ ] Performance metrics (response time, throughput)
- [ ] Database connection pooling settings
- [ ] Cache settings (Redis, Memory)
- [ ] Rate limiting (API calls)
- [ ] Throttling pentru operații heavy
- [ ] **Application Performance Monitoring (APM)**
- [ ] **Real User Monitoring (RUM)**
- [ ] **Error tracking și alerting (Sentry, Raygun)**
- [ ] **Slow query logging**
- [ ] **Memory leak detection**
- [ ] **Resource usage alerts (CPU, RAM, Disk)**
- [ ] **Uptime monitoring și SLA tracking**
- [ ] **Load balancing configuration**

### 1️⃣6️⃣ **MOBILE ȘI MULTI-DEVICE**
- [ ] Responsive settings
- [ ] PWA (Progressive Web App) enable
- [ ] Offline mode (sync când revine online)
- [ ] Setări sincronizare mobile-desktop
- [ ] Biometric authentication (mobile)
- [ ] **Device management (register/deregister)**
- [ ] **Mobile-specific UI optimizations**
- [ ] **Gesture controls settings**
- [ ] **Camera/Scanner integration (prescripții, documente)**
- [ ] **Voice input settings**
- [ ] **Location services (check-in geographic)**
- [ ] **Mobile data usage optimization**

### 1️⃣7️⃣ **CALITATE ȘI ACREDITARE**
- [ ] Indicatori calitate (ISO, JCI)
- [ ] Formulare satisfacție pacient
- [ ] Management reclamații
- [ ] Acțiuni corective și preventive (CAPA)
- [ ] Documente proceduri operaționale (SOP)
- [ ] Audit intern (planificare și execuție)
- [ ] **Clinical quality measures (CQM)**
- [ ] **Patient safety indicators (PSI)**
- [ ] **Mortality and morbidity tracking**
- [ ] **Sentinel events reporting**
- [ ] **Root cause analysis (RCA)**
- [ ] **Failure Mode and Effects Analysis (FMEA)**
- [ ] **Benchmarking cu standarde naționale/internaționale**
- [ ] **Accreditation documentation management**
- [ ] **Continuous quality improvement (CQI) tracking**

### 1️⃣8️⃣ **TELEMEDICINĂ** (dacă aplicabil)
- [ ] Video conferencing integration (Zoom, Teams, Google Meet)
- [ ] Consultație la distanță settings
- [ ] Prescripție electronică remotă
- [ ] Partajare ecran/documente în consultație
- [ ] Recording consultații (cu consimțământ)
- [ ] **Virtual waiting room**
- [ ] **Remote patient monitoring (RPM) integration**
- [ ] **Wearables data integration (smartwatch, fitness tracker)**
- [ ] **Asynchronous consultations (store-and-forward)**
- [ ] **Telemetry settings (ECG, blood pressure remote)**
- [ ] **Virtual physical exam tools**
- [ ] **Bandwidth adaptation (video quality)**
- [ ] **HIPAA-compliant platforms enforcement**

### 1️⃣9️⃣ **EXPORT ȘI IMPORT DATE**
- [ ] Import date pacienți (Excel, CSV)
- [ ] Import istoric medical (migrare din alt sistem)
- [ ] Export rapoarte predefinite
- [ ] Export bulk date (pentru analiză)
- [ ] Template-uri import/export
- [ ] **Data mapping configuration (field matching)**
- [ ] **Import validation rules**
- [ ] **Duplicate detection on import**
- [ ] **Merge strategies (import conflicts)**
- [ ] **Import rollback (undo import)**
- [ ] **Scheduled exports (automated)**
- [ ] **Export encryption (date sensibile)**
- [ ] **Export audit log**

### 2️⃣0️⃣ **SETĂRI AVANSATE SISTEM**
- [ ] Feature flags (enable/disable funcționalități)
- [ ] A/B testing pentru UI
- [ ] Maintenance mode (mesaj în timp ce se fac update-uri)
- [ ] API versioning
- [ ] Webhook-uri pentru evenimente (pacient nou, programare, etc.)
- [ ] Rate limiting per utilizator/rol
- [ ] Geo-restriction (access doar din anumite țări/regiuni)
- [ ] **Blue-green deployment settings**
- [ ] **Canary releases configuration**
- [ ] **Circuit breaker patterns (resilience)**
- [ ] **Retry policies (transient failures)**
- [ ] **Chaos engineering settings (test resilience)**
- [ ] **Service mesh configuration**
- [ ] **API gateway settings**

### 2️⃣1️⃣ **PACIENT ENGAGEMENT** ⭐ **NOU**
- [ ] **Portal pacient (self-service)**
- [ ] **Acces istoric medical propriu**
- [ ] **Solicitare programări online**
- [ ] **Plată online (factură, consultații viitoare)**
- [ ] **Download rezultate investigații**
- [ ] **Comunicare securizată cu medicul (messaging)**
- [ ] **Educație pacient (articole, video-uri)**
- [ ] **Medication adherence tracking (reminder medicamente)**
- [ ] **Symptom checker/Health risk assessment**
- [ ] **Family account management (acces pentru familie)**
- [ ] **Proxy access (reprezentant legal pentru minor/senior)**
- [ ] **Referral management (recomandări specialiști)**

### 2️⃣2️⃣ **RESEARCH ȘI ANALYTICS** ⭐ **NOU**
- [ ] **De-identification engine pentru cercetare**
- [ ] **Cohort builder (grupare pacienți după criterii)**
- [ ] **Clinical trial management**
- [ ] **IRB (Institutional Review Board) compliance**
- [ ] **Data warehouse integration**
- [ ] **Business Intelligence (BI) tools integration**
- [ ] **Machine Learning model deployment**
- [ ] **Predictive analytics (readmission risk, etc.)**
- [ ] **Natural Language Processing (NLP) pentru note medicale**
- [ ] **Data export pentru registre naționale/studii**

### 2️⃣3️⃣ **EMERGENCY PREPAREDNESS** ⭐ **NOU**
- [ ] **Disaster recovery protocols**
- [ ] **Emergency contact tree**
- [ ] **Mass casualty incident (MCI) mode**
- [ ] **Pandemic preparedness settings**
- [ ] **Emergency supplies inventory**
- [ ] **Evacuation procedures documentation**
- [ ] **Communication backup (radio, satellite phone)**
- [ ] **Generator/Power backup integration**
- [ ] **Paper-based backup forms (downtime procedures)**

### 2️⃣4️⃣ **REVENUE CYCLE MANAGEMENT** ⭐ **NOU**
- [ ] **Pre-authorization management**
- [ ] **Insurance eligibility verification**
- [ ] **Claims submission (electronic)**
- [ ] **Claims tracking și status**
- [ ] **Denial management workflow**
- [ ] **Appeals process**
- [ ] **Payment posting (EOB reconciliation)**
- [ ] **Patient responsibility estimation**
- [ ] **Financial counseling workflows**
- [ ] **Collection agency integration**

### 2️⃣5️⃣ **CLINICAL DECISION SUPPORT** ⭐ **NOU**
- [ ] **Evidence-based guidelines integration**
- [ ] **Drug-drug interaction alerts**
- [ ] **Drug-allergy alerts**
- [ ] **Dosage calculation assistance**
- [ ] **Clinical pathway protocols**
- [ ] **Diagnostic decision trees**
- [ ] **Order sets (pre-defined treatments)**
- [ ] **Best practice advisories (BPA)**
- [ ] **Duplicate test alerts**
- [ ] **Critical value notifications**

---

## 📊 Prioritizare Implementare (ACTUALIZATĂ)

### 🔴 **CRITICAL (P0)** - Fără acestea aplicația nu poate funcționa
1. Roluri și permisiuni de bază (Admin, Doctor, Asistent)
2. Autentificare securizată (parole, sesiuni, 2FA)
3. Configurări medicale de bază (cabinet, tarife, template-uri)
4. Backup și recuperare date
5. **Audit log complet (compliance)**
6. **GDPR mandatory (consimțământ, portabilitate)**

### 🟠 **HIGH (P1)** - Esențiale pentru funcționare normală
7. Notificări pacienți (email, SMS)
8. Programări și calendar (ore funcționare, durate)
9. GDPR compliance extins (toate drepturile)
10. Raportări de bază (zilnice, lunare)
11. **Patient engagement basic (portal pacient)**
12. **Clinical decision support basic (alerte interacțiuni)**
13. **Revenue cycle management basic (facturare, claims)**

### 🟡 **MEDIUM (P2)** - Îmbunătățesc semnificativ UX
14. UI/UX personalizare (teme, limba)
15. Facturare și financiar avansat
16. Inventar și stoc
17. Comunicare internă
18. **Calitate și acreditare (raportări, indicatori)**
19. **Telemedicină basic**
20. **HR management (pontaj, concedii)**

### 🟢 **LOW (P3)** - Nice to have
21. Integrări externe avansate (DES, PACS)
22. Telemedicină avansată (wearables, RPM)
23. A/B testing și feature flags
24. Mobile advanced features
25. **Research și analytics (AI/ML)**
26. **Emergency preparedness**

---

## 🛠 Structură Tehnică Propusă (ACTUALIZATĂ)

### Database Tables
```sql
-- Tabele noi necesare (Key-Value Pattern - EAV):
- Setari_Sistem (Categorie, Cheie, Valoare, TipDate, Descriere, ValoareDefault, EsteEditabil)
- Setari_Utilizator (UtilizatorID, Categorie, Cheie, Valoare, TipDate)
- Roluri (role definitions)
- Permisiuni (permissions matrix)
- Roluri_Permisiuni (many-to-many)
- Utilizatori_Roluri (many-to-many)
- Grupuri_Utilizatori (teams, departments)
- Audit_Log (who, what, when, where, IP, device)
- Notificari_Template (email, SMS templates cu placeholders)
- GDPR_Consimtaminte (patient consent tracking cu versioning)
- Feature_Flags (enable/disable features)
- Setari_Backup (backup policies și history)
- Setari_Integrari (API keys, endpoints, credentials encrypted)
- Clinical_Decision_Rules (CDS rules engine)
- Patient_Portal_Settings (portal configuration)
- Emergency_Protocols (disaster recovery plans)
```

### Blazor Pages Necesare (ACTUALIZATĂ)
```
/Administrare/Setari/
  ├── SetariGenerale.razor (sistem global)
  ├── SetariMedicale.razor (cabinet, nomenclatoare)
  ├── SetariRoluri.razor (roles management cu matrix UI)
  ├── SetariPermisiuni.razor (permissions matrix drag-drop)
  ├── SetariGrupuri.razor (user groups management)
  ├── SetariNotificari.razor (email, SMS config + templates)
  ├── SetariIntegrari.razor (CNAS, SIUI, etc. + test connection)
  ├── SetariBackup.razor (backup policies + manual trigger)
  ├── SetariGDPR.razor (compliance settings + audit viewer)
  ├── SetariUtilizator.razor (user preferences cu preview)
  ├── SetariProgramari.razor (calendar, ore, reminder) ⭐ NOU
  ├── SetariFacturare.razor (TVA, POS, payment methods) ⭐ NOU
  ├── SetariInventar.razor (stoc, alerte, furnizori) ⭐ NOU
  ├── SetariClinicalDecision.razor (CDS rules, alerts) ⭐ NOU
  ├── SetariPortalPacient.razor (patient engagement) ⭐ NOU
  ├── SetariTelemedicina.razor (video, remote monitoring) ⭐ NOU
  ├── SetariEmergenta.razor (disaster recovery, protocols) ⭐ NOU
  └── SetariAvansate.razor (feature flags, API, webhooks) ⭐ NOU
```

### Application Layer (ACTUALIZATĂ)
```
ValyanClinic.Application/
  └── Features/
      └── Settings/
          ├── Commands/
│   ├── UpdateSystemSettings/
      │   ├── UpdateUserSettings/
 │   ├── CreateRole/
       │   ├── UpdatePermissions/
    │   ├── ConfigureNotification/
  │   ├── TestIntegration/ ⭐ NOU
       │   ├── TriggerBackup/ ⭐ NOU
          │   ├── EnableFeatureFlag/ ⭐ NOU
          │   └── ...
          └── Queries/
   ├── GetSystemSettings/
      ├── GetUserSettings/
     ├── GetRolesWithPermissions/
    ├── GetAuditLog/ ⭐ NOU
              ├── GetFeatureFlags/ ⭐ NOU
        ├── GetClinicalDecisionRules/ ⭐ NOU
     └── ...
```

### Domain Layer (ACTUALIZATĂ)
```
ValyanClinic.Domain/
  └── Entities/
      ├── SystemSetting.cs (Key-Value pair cu Type)
      ├── UserSetting.cs (UserId + Key-Value)
      ├── Role.cs (Name, Description, IsSystemRole)
      ├── Permission.cs (Resource, Action, Scope)
      ├── UserGroup.cs (Name, Type, ParentGroupId)
      ├── AuditLog.cs (UserId, Action, Entity, Old/New Value, IP, Timestamp)
      ├── FeatureFlag.cs (Name, IsEnabled, RolloutPercentage) ⭐ NOU
      ├── ClinicalDecisionRule.cs (Condition, Action, Severity) ⭐ NOU
      ├── NotificationTemplate.cs (Type, Subject, Body, Placeholders) ⭐ NOU
      ├── BackupPolicy.cs (Schedule, Retention, Type) ⭐ NOU
    └── IntegrationConfig.cs (Provider, Endpoint, ApiKey encrypted) ⭐ NOU
```

### Services Layer (ACTUALIZATĂ) ⭐ NOU
```
ValyanClinic.Application/Services/
  ├── ISettingsService.cs (centralizat Get/Set settings)
  ├── IAuditService.cs (log toate acțiunile critice)
  ├── IPermissionService.cs (check permissions cu caching)
  ├── IFeatureFlagService.cs (evaluate feature flags)
  ├── IClinicalDecisionService.cs (evaluate CDS rules)
  ├── INotificationTemplateService.cs (render templates)
  ├── IBackupService.cs (trigger, monitor backups)
  └── IIntegrationService.cs (test, execute integrations)
```

---

## 📝 Next Steps (ACTUALIZAȚI)

1. **Review și prioritizare** - Discutăm ce setări implementăm ACUM vs. LATER
2. **Database design detailed** - Schema completă cu indexuri, constraints
3. **UI/UX mockups** - Figma/Sketch pentru toate paginile de setări
4. **Security review** - Encryption, access control, audit
5. **Implementation plan** - Planificare sprint-uri (8-12 săptămâni)
6. **Testing strategy** - Unit tests, integration tests, security tests, UAT
7. **Migration strategy** - Plan pentru date existente
8. **Documentation** - User manual, admin manual, API docs
9. **Training plan** - Pentru utilizatori și administratori
10. **Rollout plan** - Phased deployment cu rollback strategy

---

## 🎯 Metrici de Succes

### Pentru Implementare Completă:
- ✅ **100% coverage** pentru setări P0 și P1
- ✅ **Audit log** pentru toate acțiunile critice (99.9% uptime)
- ✅ **GDPR compliance** 100% (toate drepturile implementate)
- ✅ **Security audit** passed (penetration testing)
- ✅ **Performance** - sub 500ms response time pentru load settings
- ✅ **User satisfaction** - >4.5/5 pentru UI/UX setări
- ✅ **Zero downtime** deployment pentru settings updates
- ✅ **Backup success rate** >99.9%
- ✅ **Settings documentation** completă și up-to-date

---

## 📋 Checklist Final (Pre-Production)

### Înainte de Go-Live:
- [ ] **Toate setările P0 și P1 implementate și testate**
- [ ] **Security audit complet (third-party)**
- [ ] **GDPR compliance verificat (legal review)**
- [ ] **Backup/Restore testat în mediu production-like**
- [ ] **Disaster recovery plan testat**
- [ ] **Performance testing la scară (load testing)**
- [ ] **User acceptance testing (UAT) cu medici și administratori**
- [ ] **Documentation completă (user + admin + developer)**
- [ ] **Training sessions finalizate**
- [ ] **Rollback plan documented și rehearsed**
- [ ] **Support team pregătit (on-call schedule)**
- [ ] **Monitoring și alerting active**
- [ ] **Initial data migration successful**
- [ ] **Go-live communication plan executat**

---

**Status:** 📋 **LISTA COMPLETĂ EXTINSĂ - READY FOR PLANNING**  
**Total Setări:** **~350 setări** (față de ~200 inițial)  
**Estimare timp total:** 8-12 săptămâni (pentru toate setările P0-P2)  
**Recomandare:** Implementare iterativă, un modul la 1-2 săptămâni, cu focus pe P0 → P1 → P2 → P3

---

## 🆕 Actualizări Majore Adăugate

### Categorii Noi Complete:
1. **Patient Engagement** (Portal pacient, self-service)
2. **Research și Analytics** (AI/ML, cohort builder)
3. **Emergency Preparedness** (disaster recovery, MCI mode)
4. **Revenue Cycle Management** (claims, denials, collections)
5. **Clinical Decision Support** (alerte, guidelines, order sets)

### Setări Noi Critice Adăugate:
- ✅ **~150 setări noi** distribuite în categoriile existente
- ✅ **Security enhancements** (sesiuni concurente, restricții orare)
- ✅ **Advanced permissions** (field-level, temporal, geographic)
- ✅ **Medical-specific** (panic values, recall consultații, cold chain)
- ✅ **Compliance** (credentialing, IRB, clinical trials)
- ✅ **Integration** (HL7/FHIR, DICOM, WhatsApp Business)

---

*Document actualizat: 2025-01-15 (Review Complete)*  
*Versiune: 2.0 (Extended)*  
*Aplicație: ValyanClinic - .NET 9 Blazor Server*  
*Review Status: ✅ **COMPREHENSIVE & PRODUCTION-READY***
