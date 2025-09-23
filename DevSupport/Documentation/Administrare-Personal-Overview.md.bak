# Documentație Administrare Personal - ValyanClinic

## Prezentare Generală

Modulul **Administrare Personal** este o componentă centrală a sistemului ValyanClinic, destinată gestionării întregului personal non-medical al clinicii. Această pagină oferă funcționalități complete pentru administrarea angajaților din departamentele de suport: administrativ, financiar, IT, întreținere, logistică și altele.

## Cuprins

### 📋 Pentru Utilizatori Finali
- **[Ghid Utilizator - Administrare Personal](ForApplicationUsers/Ghid-Utilizator-Administrare-Personal.md)** - Ghid complet pentru utilizarea paginii de administrare personal
- **[Ghid Utilizator - Adăugare Personal](ForApplicationUsers/Ghid-Utilizator-Adaugare-Personal.md)** - Cum să adăugați personal nou în sistem
- **[Ghid Utilizator - Editare Personal](ForApplicationUsers/Ghid-Utilizator-Editare-Personal.md)** - Cum să modificați informațiile personalului existent
- **[Ghid Utilizator - Vizualizare Detalii Personal](ForApplicationUsers/Ghid-Utilizator-Vizualizare-Detalii-Personal.md)** - Dashboard pentru vizualizarea informațiilor complete
- **[Ghid Utilizator - Filtrare și Căutare Personal](ForApplicationUsers/Ghid-Utilizator-Filtrare-Cautare-Personal.md)** - Cum să folosiți sistemul de filtrare avansată

### 🔧 Pentru Dezvoltatori
- **[Documentație Tehnică - Administrare Personal](Development/Administrare-Personal-Technical-Documentation.md)** - Documentație tehnică completă pentru pagina principală
- **[Documentație Tehnică - Modal Adăugare/Editare Personal](Development/Add-Edit-Personal-Modal-Technical-Documentation.md)** - Implementarea formularului de adăugare/editare
- **[Documentație Tehnică - Modal Vizualizare Personal](Development/View-Personal-Modal-Technical-Documentation.md)** - Dashboard-ul de vizualizare detalii
- **[Documentație Tehnică - Sistem Filtrare Personal](Development/Personal-Filtering-System-Technical-Documentation.md)** - Sistemul avansat de filtrare și căutare
- **[Documentație Tehnică - Kebab Menu și UI Components](Development/Kebab-Menu-UI-Components-Technical-Documentation.md)** - Componente UI și interacțiuni JavaScript

### 📊 Arhitectură și Design
- **[Arhitectura Modulului Personal](Technical/Personal-Module-Architecture.md)** - Arhitectura completă a modulului
- **[State Management - Personal](Technical/Personal-State-Management.md)** - Managementul stării aplicației
- **[Database Schema - Personal](Technical/Personal-Database-Schema.md)** - Schema bazei de date și stored procedures
- **[API Endpoints - Personal](Technical/Personal-API-Endpoints.md)** - Documentația API-urilor
- **[Security și Audit - Personal](Technical/Personal-Security-Audit.md)** - Securitate și audit trail

## Funcționalități Cheie

### 🏢 Gestionare Completă Personal
- **CRUD Operations**: Create, Read, Update, Delete pentru înregistrările de personal
- **Validare Avansată**: CNP românesc, email, telefon, date de identitate
- **Gestionare Departamente**: Administratie, Financiar, IT, Întreținere, etc.
- **Tracking Status**: Activ/Inactiv cu audit trail

### 🔍 Căutare și Filtrare Avansată
- **Filtrare Multi-Criteriu**: Departament, Status, Perioada activitate
- **Căutare Text**: În nume, prenume, email, telefon
- **Export Date**: Funcționalități de export pentru raportare
- **Grupare Inteligentă**: Organizare automată după departament

### 📱 Interfață Modernă
- **Responsive Design**: Optimizat pentru desktop, tablet și mobile
- **Syncfusion DataGrid**: Grid profesional cu funcții avansate
- **Toast Notifications**: Feedback instant pentru operații
- **Kebab Menu**: Acces rapid la funcții secundare

### 🔐 Securitate și Audit
- **Validare Server-Side**: Toate validările se fac pe server
- **Audit Logging**: Înregistrarea tuturor modificărilor
- **Role-Based Access**: Acces bazat pe roluri și permisiuni
- **Data Protection**: Protecția datelor personale conform GDPR

## Componente Principale

### 1. **AdministrarePersonal.razor** - Componenta Principală
```
Locație: ValyanClinic\Components\Pages\Administrare\Personal\
Responsabilități:
- Management principal al paginii
- Coordonarea modalelor și componentelor
- Gestionarea stării aplicației
- Event handling și comunicarea cu serviciile
```

### 2. **AdaugaEditezaPersonal.razor** - Modal Add/Edit
```
Locație: ValyanClinic\Components\Pages\Administrare\Personal\
Responsabilități:
- Formulare pentru adăugare/editare personal
- Validare client și server-side
- Gestionarea lookup-urilor (județe, localități)
- CNP validation și auto-calculare data nașterii
```

### 3. **VizualizeazaPersonal.razor** - Modal Vizualizare
```
Locație: ValyanClinic\Components\Pages\Administrare\Personal\
Responsabilități:
- Dashboard pentru vizualizarea detaliilor complete
- Layout card-based pentru informații organizate
- Read-only mode cu opțiuni de editare
- Export și print functionality
```

### 4. **LocationDependentGridDropdowns.razor** - Componente Lookup
```
Locație: ValyanClinic\Components\Shared\
Responsabilități:
- Dropdown-uri dependente (Județ → Localitate)
- Încărcare asincronă a datelor
- State management pentru selecții
- Error handling și retry logic
```

## Stack Tehnologic

### Frontend
- **Framework**: Blazor Server (.NET 9)
- **UI Components**: Syncfusion Blazor Enterprise Suite
- **Rendering**: InteractiveServer mode
- **CSS**: Custom CSS cu specificitate maximă
- **JavaScript**: Helper functions pentru event handling

### Backend
- **Architecture**: Clean Architecture
- **ORM**: Dapper pentru high-performance data access
- **Database**: SQL Server cu stored procedures
- **Validation**: FluentValidation pentru validări complexe
- **Logging**: Serilog pentru structured logging

### Infrastructure
- **Caching**: MemoryCache pentru optimizare
- **State Management**: Custom state classes
- **Grid State Persistence**: Salvarea preferințelor utilizatorilor
- **Disposal Pattern**: Memory leak prevention

## Workflow Tipic

### 1. **Accesarea Paginii**
```
1. Navigare la /administrare/personal
2. Încărcare date personal din baza de date
3. Inițializare componente UI (grid, filtre, statistici)
4. Setup JavaScript helpers pentru kebab menu
```

### 2. **Adăugare Personal Nou**
```
1. Click pe "Adaugă Personal" → Deschidere modal
2. Auto-generare cod angajat unic
3. Completare formular cu validare real-time
4. Validare CNP și calculare automată data nașterii
5. Salvare cu validare server-side completă
```

### 3. **Editare Personal Existent**
```
1. Selectare din grid → Click "Edit"
2. Pre-populare formular cu date existente
3. Modificare câmpuri cu validare
4. Salvare cu audit trail
```

### 4. **Vizualizare Detalii**
```
1. Click pe "View" din grid
2. Afișare dashboard cu toate informațiile
3. Organizare în carduri tematice
4. Opțiuni pentru editare directă
```

## Considerații de Performanță

### Grid Performance
- **Lazy Loading**: Încărcare pe pagini pentru volume mari de date
- **Virtual Scrolling**: Pentru liste foarte lungi
- **Column Virtualization**: Optimizare pentru ecrane mici
- **State Persistence**: Salvarea preferințelor utilizatorului

### Memory Management
- **Proper Disposal**: Curățarea tuturor resurselor
- **Event Listener Cleanup**: Prevenirea memory leaks
- **Component Lifecycle**: Management corect al ciclului de viață

### Database Optimization
- **Stored Procedures**: Pentru operații complexe
- **Indexing**: Pe coloanele frecvent căutate
- **Connection Pooling**: Optimizarea conexiunilor
- **Async Operations**: Pentru toate operațiile I/O

## Integrări

### Cu Alte Module
- **Personal Medical**: Legătura cu modulul de personal medical
- **Utilizatori**: Sincronizarea cu conturile de utilizator
- **Raportare**: Generarea de rapoarte pentru personal
- **Audit**: Înregistrarea în log-urile de audit

### Cu Servicii Externe
- **Email Service**: Pentru notificări
- **SMS Service**: Pentru alerte urgente
- **Export Service**: Pentru generarea fișierelor
- **Backup Service**: Pentru arhivarea datelor

## Quick Reference - Comenzi și Shortcut-uri

### Navigare Rapidă
- **URL Direct**: `/administrare/personal`
- **Keyboard Shortcut**: `Alt + A` → `P` (dacă sunt activate)
- **Din meniu**: Administrare → Administrare Personal

### Operații Principale
- **Adaugă Personal**: Click butonul verde "Adaugă Personal"
- **Editează**: Click butonul portocaliu ✏️ din grid
- **Vizualizează**: Click butonul albastru 👁️ din grid
- **Șterge**: Click butonul roșu 🗑️ din grid (doar Admin)

### Filtrare și Căutare
- **Meniu Kebab**: Click pe ⋮ pentru opțiuni avansate
- **Statistici**: Toggle din kebab menu
- **Filtre Avansate**: Toggle din kebab menu
- **Căutare Rapidă**: Folosește bara de căutare din filtre

### Keyboard Shortcuts în Modal
- **Salvare**: `Ctrl + S` (în formulare)
- **Anulare**: `Escape` (închide modalul)
- **Tab Navigation**: Pentru navigarea între câmpuri

## Troubleshooting Rapid

### Probleme Comune
| Problemă | Cauză Probabilă | Soluție Rapidă |
|----------|----------------|----------------|
| Nu se încarcă datele | Conexiune/Permisiuni | `Ctrl + F5` pentru refresh |
| CNP nu se validează | Format incorect | Verifică 13 cifre și algoritm |
| Modal nu se deschide | JavaScript blocat | Dezactivează AdBlock |
| Filtrarea nu funcționează | Cache corupt | Curăță filtrele și reload |

### Contacte Urgente
- **Suport Tehnic**: +40 373 XXX XXX
- **Email Suport**: suport.urgent@valyanmed.ro
- **Chat Intern**: Butonul "Ajutor" din aplicație

## Planuri de Dezvoltare

### Următoarea Versiune (v2.1)
- **Export PDF**: Generare rapoarte PDF pentru personal
- **Import Bulk**: Import masiv din Excel/CSV
- **Mobile App**: Aplicație mobilă pentru manageri
- **Advanced Analytics**: Dashboard-uri analitice

### Viitor (v3.0)
- **AI Integration**: Asistent AI pentru completarea formularelor
- **Document Scanner**: Scanare automată CI/CV
- **Biometric Integration**: Pontaj cu amprente
- **API Public**: API REST pentru integrări externe

---

**📧 Contact Echipa de Dezvoltare**: development@valyanmed.ro  
**📞 Suport Tehnic**: +40 373 XXX XXX  
**🌐 Documentație Online**: https://docs.valyanmed.ro

**Versiune**: 2.0  
**Data ultimei actualizări**: Decembrie 2024  
**Responsabil documentație**: Echipa ValyanMed Development

## 📜 Changelog

### v2.0 (Decembrie 2024)
- ✅ Documentație completă și atotcuprinzătoare
- ✅ Toate modalele documentate în detaliu
- ✅ Sistem de filtrare avansat
- ✅ Kebab menu cu JavaScript integration
- ✅ Accessibility și responsive design complet

### v1.9 (Noiembrie 2024)
- ✅ Implementare AdministrarePersonal.razor
- ✅ Modal Add/Edit cu validare CNP
- ✅ Modal vizualizare cu dashboard
- ✅ Sistema de lookup dependente

### v1.8 (Octombrie 2024)
- ✅ Integrare Syncfusion DataGrid
- ✅ Bază de date și stored procedures
- ✅ Logging cu Serilog
- ✅ Clean Architecture implementation
