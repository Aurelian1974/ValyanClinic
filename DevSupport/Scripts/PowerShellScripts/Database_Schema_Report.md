# RAPORT SCHEMA BAZA DE DATE
**Generat:** 2025-10-18 08:41:42
**Database:** 
**Server:** DESKTOP-9H54BCS\SQLSERVER

## Tabele Analizate

### Tabel: Departamente

| Coloana | Tip Date | Nullable | Primary Key |
|---------|----------|----------|-------------|
| IdDepartament | uniqueidentifier | NU | DA |
| IdTipDepartament | uniqueidentifier | DA |  |
| DenumireDepartament | varchar | NU |  |
| DescriereDepartament | varchar | DA |  |
### Tabel: Personal

| Coloana | Tip Date | Nullable | Primary Key |
|---------|----------|----------|-------------|
| Id_Personal | uniqueidentifier | NU | DA |
| Cod_Angajat | varchar | NU |  |
| CNP | varchar | NU |  |
| Nume | nvarchar | NU |  |
| Prenume | nvarchar | NU |  |
| Nume_Anterior | nvarchar | DA |  |
| Data_Nasterii | date | NU |  |
| Locul_Nasterii | nvarchar | DA |  |
| Nationalitate | nvarchar | DA |  |
| Cetatenie | nvarchar | DA |  |
| Telefon_Personal | varchar | DA |  |
| Telefon_Serviciu | varchar | DA |  |
| Email_Personal | varchar | DA |  |
| Email_Serviciu | varchar | DA |  |
| Adresa_Domiciliu | nvarchar | NU |  |
| Judet_Domiciliu | nvarchar | NU |  |
| Oras_Domiciliu | nvarchar | NU |  |
| Cod_Postal_Domiciliu | varchar | DA |  |
| Adresa_Resedinta | nvarchar | DA |  |
| Judet_Resedinta | nvarchar | DA |  |
| Oras_Resedinta | nvarchar | DA |  |
| Cod_Postal_Resedinta | varchar | DA |  |
| Stare_Civila | nvarchar | DA |  |
| Functia | nvarchar | NU |  |
| Departament | nvarchar | DA |  |
| Serie_CI | varchar | DA |  |
| Numar_CI | varchar | DA |  |
| Eliberat_CI_De | nvarchar | DA |  |
| Data_Eliberare_CI | date | DA |  |
| Valabil_CI_Pana | date | DA |  |
| Status_Angajat | nvarchar | DA |  |
| Observatii | nvarchar | DA |  |
| Data_Crearii | datetime2 | NU |  |
| Data_Ultimei_Modificari | datetime2 | NU |  |
| Creat_De | nvarchar | DA |  |
| Modificat_De | nvarchar | DA |  |
### Tabel: PersonalMedical

| Coloana | Tip Date | Nullable | Primary Key |
|---------|----------|----------|-------------|
| PersonalID | uniqueidentifier | NU | DA |
| Nume | nvarchar | NU |  |
| Prenume | nvarchar | NU |  |
| Specializare | nvarchar | DA |  |
| NumarLicenta | nvarchar | DA |  |
| Telefon | nvarchar | DA |  |
| Email | nvarchar | DA |  |
| Departament | nvarchar | DA |  |
| Pozitie | nvarchar | DA |  |
| EsteActiv | bit | DA |  |
| DataCreare | datetime2 | DA |  |
| CategorieID | uniqueidentifier | DA |  |
| SpecializareID | uniqueidentifier | DA |  |
| SubspecializareID | uniqueidentifier | DA |  |

## Stored Procedures Disponibile

| Procedura | Data Creare | Data Modificare |
|-----------|-------------|-----------------|
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |
|  |  |  |

## Recomandari pentru Cod

### Entity Classes
- Verificati ca proprietatile din Personal.cs, PersonalMedical.cs, Patient.cs, User.cs corespund cu coloanele din tabele
- Tipurile de date trebuie sa corespunda (ex: NVARCHAR -> string, INT -> int, BIT -> ool)
- Coloanele nullable din DB trebuie sa fie 
ullable si in C# (ex: string?, int?)

### Repository Methods
- Verificati ca stored procedures folosite in repositories existe in baza de date
- Parametrii SP trebuie sa corespunda cu proprietatile entitatilor

### Connection String
- Server actual: DESKTOP-9H54BCS\SQLSERVER
- Database actual: ValyanMed

---
*Generat automat de Database Schema Validation Script*
