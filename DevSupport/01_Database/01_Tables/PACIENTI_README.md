# Tabela Pacienti - Documentație

## Descriere Generală

Tabela `Pacienti` stochează informațiile complete despre pacienții clinicii medicale ValyanMed. Include date personale, date de contact, informații despre asigurări medicale, date medicale de bază, contacte de urgență și informații administrative.

## Caracteristici Tehnice

- **Database**: ValyanMed
- **Schema**: dbo
- **Tip Primary Key**: UNIQUEIDENTIFIER cu NEWSEQUENTIALID()
- **Encoding**: NVARCHAR pentru suport Unicode complet
- **Audit**: Complet cu timestamp-uri și utilizatori pentru creare/modificare

## Structura Tabelei

### Identificare Primară

| Coloană | Tip | Descriere |
|---------|-----|-----------|
| `Id` | UNIQUEIDENTIFIER | Identificator unic cu NEWSEQUENTIALID() |
| `Cod_Pacient` | NVARCHAR(20) | Cod intern generat automat format PACIENT00000001 |

### Date Personale

| Coloană | Tip | Descriere |
|---------|-----|-----------|
| `CNP` | NVARCHAR(13) | Cod numeric personal (opțional, unic) |
| `Nume` | NVARCHAR(100) | Numele pacientului (obligatoriu) |
| `Prenume` | NVARCHAR(100) | Prenumele pacientului (obligatoriu) |
| `Data_Nasterii` | DATE | Data nașterii (obligatoriu) |
| `Sex` | NVARCHAR(1) | 'M' pentru Masculin, 'F' pentru Feminin |

### Date de Contact

| Coloană | Tip | Descriere |
|---------|-----|-----------|
| `Telefon` | NVARCHAR(15) | Telefon principal |
| `Telefon_Secundar` | NVARCHAR(15) | Telefon secundar/alternativ |
| `Email` | NVARCHAR(100) | Adresă email |

### Adresă

| Coloană | Tip | Descriere |
|---------|-----|-----------|
| `Judet` | NVARCHAR(50) | Județul |
| `Localitate` | NVARCHAR(100) | Localitatea/Orașul |
| `Adresa` | NVARCHAR(255) | Adresa completă |
| `Cod_Postal` | NVARCHAR(10) | Cod poștal |

### Informații Asigurare

| Coloană | Tip | Descriere |
|---------|-----|-----------|
| `Asigurat` | BIT | Flag dacă pacientul este asigurat |
| `CNP_Asigurat` | NVARCHAR(13) | CNP al asiguratului (poate diferi pentru copii) |
| `Nr_Card_Sanatate` | NVARCHAR(20) | Număr card național de sănătate |
| `Casa_Asigurari` | NVARCHAR(100) | Denumire casă de asigurări |

### Date Medicale de Bază

| Coloană | Tip | Descriere |
|---------|-----|-----------|
| `Alergii` | NVARCHAR(MAX) | Listă alergii cunoscute |
| `Boli_Cronice` | NVARCHAR(MAX) | Listă boli cronice |
| `Medic_Familie` | NVARCHAR(150) | Nume medic de familie |

### Contact Urgență

| Coloană | Tip | Descriere |
|---------|-----|-----------|
| `Persoana_Contact` | NVARCHAR(150) | Nume persoană de contact |
| `Telefon_Urgenta` | NVARCHAR(15) | Telefon pentru urgențe |
| `Relatie_Contact` | NVARCHAR(50) | Relația cu persoana de contact |

### Informații Administrative

| Coloană | Tip | Descriere |
|---------|-----|-----------|
| `Data_Inregistrare` | DATETIME2(7) | Data înregistrării pacientului |
| `Ultima_Vizita` | DATE | Data ultimei vizite |
| `Nr_Total_Vizite` | INT | Număr total vizite (calculat automat) |
| `Activ` | BIT | Status activ/inactiv (pentru soft delete) |
| `Observatii` | NVARCHAR(MAX) | Observații generale |

### Audit

| Coloană | Tip | Descriere |
|---------|-----|-----------|
| `Data_Crearii` | DATETIME2(7) | Data creării înregistrării |
| `Data_Ultimei_Modificari` | DATETIME2(7) | Data ultimei modificări |
| `Creat_De` | NVARCHAR(100) | Utilizator care a creat înregistrarea |
| `Modificat_De` | NVARCHAR(100) | Utilizator care a modificat ultima dată |

## Constrainte

### Primary Key
- **PK_Pacienti**: pe coloana `Id`

### Unique Constraints
- **UK_Pacienti_Cod_Pacient**: pe coloana `Cod_Pacient`
- **UK_Pacienti_CNP**: pe coloana `CNP` (permite NULL dar trebuie unic dacă este setat)

### Check Constraints
- **CK_Pacienti_Sex**: `Sex IN ('M', 'F')`

## Indexuri

1. **IX_Pacienti_Nume_Prenume**: Index compus pe (Nume, Prenume) - căutări după nume
2. **IX_Pacienti_CNP**: Index pe CNP - căutări după CNP
3. **IX_Pacienti_Cod_Pacient**: Index pe Cod_Pacient - căutări după cod intern
4. **IX_Pacienti_Data_Nasterii**: Index pe Data_Nasterii - filtrări după vârstă
5. **IX_Pacienti_Telefon**: Index pe Telefon - căutări după telefon
6. **IX_Pacienti_Email**: Index pe Email - căutări după email
7. **IX_Pacienti_Activ**: Index pe Activ - filtrări după status

## Triggere

### TR_Pacienti_UpdateTimestamp
- **Tip**: AFTER UPDATE
- **Funcție**: Actualizează automat `Data_Ultimei_Modificari` și `Modificat_De` la fiecare modificare

## Stored Procedures

### Operații CRUD

1. **sp_Pacienti_GetAll** - Obține lista paginată cu filtrare și sortare
   - Parametri: PageNumber, PageSize, SearchText, Judet, Asigurat, Activ, SortColumn, SortDirection
   
2. **sp_Pacienti_GetCount** - Obține numărul total cu filtrare
   - Parametri: SearchText, Judet, Asigurat, Activ

3. **sp_Pacienti_GetById** - Obține pacient după ID
   - Parametri: Id (UNIQUEIDENTIFIER)

4. **sp_Pacienti_GetByCodPacient** - Obține pacient după cod intern
   - Parametri: CodPacient

5. **sp_Pacienti_GetByCNP** - Obține pacient după CNP
   - Parametri: CNP

6. **sp_Pacienti_Create** - Creează pacient nou
   - Generează automat `Cod_Pacient` dacă nu este furnizat
   - Validează unicitatea CNP și Cod_Pacient

7. **sp_Pacienti_Update** - Actualizează pacient existent
   - Validează unicitatea CNP

8. **sp_Pacienti_Delete** - Soft delete (marchează ca inactiv)
   - Parametri: Id, ModificatDe

9. **sp_Pacienti_HardDelete** - Ștergere fizică (folosire cu precauție)
   - Parametri: Id

### Operații Specifice

10. **sp_Pacienti_GenerateNextCodPacient** - Generează următorul cod disponibil
    - Returnează: NextCodPacient (format PACIENT00000001)

11. **sp_Pacienti_CheckUnique** - Verifică unicitatea CNP și Cod_Pacient
    - Parametri: CNP, Cod_Pacient, ExcludeId
    - Returnează: CNP_Exists, CodPacient_Exists

12. **sp_Pacienti_UpdateUltimaVizita** - Actualizează ultima vizită și incrementează numărul de vizite
    - Parametri: Id, DataVizita, ModificatDe

### Operații Lookup și Raportare

13. **sp_Pacienti_GetJudete** - Obține lista județelor unice cu număr de pacienți

14. **sp_Pacienti_GetDropdownOptions** - Opțiuni pentru dropdown-uri
    - Format: Nume + Prenume (Cod_Pacient)

15. **sp_Pacienti_GetStatistics** - Statistici pentru dashboard
    - Total pacienți, asigurați, neasigurați, pe sex

16. **sp_Pacienti_GetBirthdays** - Pacienți cu ziua de naștere în perioadă specificată
    - Parametri: StartDate, EndDate

## Instalare

### 1. Instalare Tabelă
```sql
-- Varianta 1: Script complet cu ștergere
:r Pacienti_Complete.sql

-- Varianta 2: Script instalare (verifică existența)
:r Pacienti_Install.sql
```

### 2. Instalare Stored Procedures
```sql
:r sp_Pacienti.sql
```

### 3. Verificare Instalare
```sql
:r Pacienti_Verify.sql
```

## Exemple de Utilizare

### Creare Pacient Nou
```sql
EXEC sp_Pacienti_Create
    @Nume = 'Popescu',
    @Prenume = 'Ion',
    @Data_Nasterii = '1980-05-15',
    @Sex = 'M',
    @CNP = '1800515123456',
    @Telefon = '0721234567',
    @Email = 'ion.popescu@email.com',
    @Judet = 'Bucuresti',
    @Localitate = 'Bucuresti',
    @Adresa = 'Str. Exemplu nr. 123',
    @Asigurat = 1,
    @Casa_Asigurari = 'CNAS',
    @CreatDe = 'admin'
```

### Căutare Pacienți
```sql
EXEC sp_Pacienti_GetAll
    @PageNumber = 1,
    @PageSize = 50,
    @SearchText = 'Popescu',
    @Activ = 1,
    @SortColumn = 'Nume',
    @SortDirection = 'ASC'
```

### Actualizare Ultima Vizită
```sql
EXEC sp_Pacienti_UpdateUltimaVizita
    @Id = '...guid...',
    @DataVizita = '2025-01-23',
    @ModificatDe = 'admin'
```

### Verificare Unicitate
```sql
EXEC sp_Pacienti_CheckUnique
    @CNP = '1800515123456',
    @Cod_Pacient = 'PACIENT00000001'
```

## Reguli de Business

1. **Cod Pacient**: Se generează automat în format PACIENT00000001, incrementat secvențial
2. **CNP**: Opțional dar trebuie unic dacă este setat (pentru cazuri speciale fără CNP)
3. **Sex**: Obligatoriu, doar valorile 'M' sau 'F'
4. **Soft Delete**: Folosește flag-ul `Activ` pentru ștergere logică
5. **Audit Trail**: Toate modificările sunt înregistrate automat
6. **Nr_Total_Vizite**: Se actualizează automat prin `sp_Pacienti_UpdateUltimaVizita`

## Integrări

Tabela Pacienti se integrează cu:
- **Programari**: Link prin PacientId
- **Consultatii**: Link prin PacientId
- **IstoricMedical**: Link prin PacientId
- **RezultateTeste**: Link prin PacientId

## Securitate și Performanță

### Optimizări
- Folosește NEWSEQUENTIALID() pentru performanță optimă cu indexuri clustered
- Indexuri pe coloane frecvent căutate (Nume, CNP, Telefon, Email)
- Paginare implementată în stored procedures

### Securitate
- Nu expune direct tabela în aplicație
- Toate operațiile prin stored procedures
- Validare date la nivel de SP
- Audit trail complet

### Backup
- Include în backup-ul zilnic al bazei de date
- Date sensibile (CNP, date medicale) - respectă GDPR

## Changelog

- **2025-01-23**: Creare inițială tabel Pacienti
  - Structură completă conform specificații
  - 16 stored procedures
  - Indexuri de performanță
  - Trigger pentru audit automat
  - Documentație completă

## Suport

Pentru probleme sau întrebări:
1. Verificați scriptul `Pacienti_Verify.sql`
2. Consultați documentația stored procedures
3. Verificați log-urile SQL Server

---

**Ultima actualizare**: 2025-01-23  
**Versiune**: 1.0  
**Status**: Producție Ready
