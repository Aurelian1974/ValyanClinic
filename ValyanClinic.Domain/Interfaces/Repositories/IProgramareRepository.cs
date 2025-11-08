using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface pentru gestionarea programărilor medicale.
/// Definește toate operațiunile de acces la date pentru entitatea Programare.
/// </summary>
public interface IProgramareRepository
{
    // ==================== READ OPERATIONS - SINGLE ====================

    /// <summary>
    /// Obține o programare după ID-ul său unic.
    /// Include informații detaliate despre pacient, doctor și utilizatorul care a creat programarea.
    /// </summary>
  /// <param name="id">ID-ul programării.</param>
    /// <param name="cancellationToken">Token pentru anularea operațiunii.</param>
    /// <returns>Programarea găsită sau null dacă nu există.</returns>
    Task<Programare?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // ==================== READ OPERATIONS - COLLECTION (PAGINARE & FILTRARE) ====================

    /// <summary>
    /// Obține o listă paginată de programări cu filtrare complexă și sortare.
    /// </summary>
    /// <param name="pageNumber">Numărul paginii (începe de la 1).</param>
    /// <param name="pageSize">Numărul de elemente per pagină.</param>
    /// <param name="searchText">Text pentru căutare globală (pacient, doctor, observații).</param>
    /// <param name="doctorID">Filtru după ID-ul medicului.</param>
    /// <param name="pacientID">Filtru după ID-ul pacientului.</param>
    /// <param name="dataStart">Data de început pentru interval (implicit: azi).</param>
    /// <param name="dataEnd">Data de sfârșit pentru interval (implicit: azi + 30 zile).</param>
    /// <param name="status">Filtru după status programare.</param>
    /// <param name="tipProgramare">Filtru după tipul programării.</param>
    /// <param name="sortColumn">Coloana pentru sortare (DataProgramare, OraInceput, Status, etc.).</param>
    /// <param name="sortDirection">Direcția sortării (ASC sau DESC).</param>
    /// <param name="cancellationToken">Token pentru anularea operațiunii.</param>
    /// <returns>Lista de programări cu informații detaliate.</returns>
    Task<IEnumerable<Programare>> GetAllAsync(
        int pageNumber = 1,
   int pageSize = 50,
     string? searchText = null,
        Guid? doctorID = null,
        Guid? pacientID = null,
        DateTime? dataStart = null,
        DateTime? dataEnd = null,
    string? status = null,
        string? tipProgramare = null,
  string sortColumn = "DataProgramare",
        string sortDirection = "ASC",
 CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține numărul total de programări pentru parametrii de filtrare dați (necesar pentru paginare).
    /// </summary>
    /// <param name="searchText">Text pentru căutare globală.</param>
    /// <param name="doctorID">Filtru după ID-ul medicului.</param>
    /// <param name="pacientID">Filtru după ID-ul pacientului.</param>
    /// <param name="dataStart">Data de început pentru interval.</param>
    /// <param name="dataEnd">Data de sfârșit pentru interval.</param>
    /// <param name="status">Filtru după status programare.</param>
    /// <param name="tipProgramare">Filtru după tipul programării.</param>
    /// <param name="cancellationToken">Token pentru anularea operațiunii.</param>
    /// <returns>Numărul total de programări care corespund criteriilor.</returns>
    Task<int> GetCountAsync(
        string? searchText = null,
  Guid? doctorID = null,
        Guid? pacientID = null,
        DateTime? dataStart = null,
  DateTime? dataEnd = null,
        string? status = null,
        string? tipProgramare = null,
  CancellationToken cancellationToken = default);

    // ==================== SPECIALIZED QUERIES (BUSINESS LOGIC) ====================

    /// <summary>
    /// Obține toate programările pentru un medic specific într-un interval de date.
    /// </summary>
    /// <param name="doctorID">ID-ul medicului.</param>
    /// <param name="dataStart">Data de început (implicit: azi).</param>
    /// <param name="dataEnd">Data de sfârșit (implicit: azi + 7 zile).</param>
    /// <param name="cancellationToken">Token pentru anularea operațiunii.</param>
    /// <returns>Lista programărilor medicului în intervalul specificat.</returns>
    Task<IEnumerable<Programare>> GetByDoctorAsync(
     Guid doctorID,
        DateTime? dataStart = null,
        DateTime? dataEnd = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține istoricul complet al programărilor unui pacient.
    /// </summary>
    /// <param name="pacientID">ID-ul pacientului.</param>
  /// <param name="cancellationToken">Token pentru anularea operațiunii.</param>
    /// <returns>Lista tuturor programărilor pacientului (ordonate descrescător după dată).</returns>
    Task<IEnumerable<Programare>> GetByPacientAsync(
  Guid pacientID,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține toate programările pentru o dată specifică (cu filtru opțional după medic).
    /// </summary>
    /// <param name="date">Data pentru care se caută programările.</param>
  /// <param name="doctorID">ID-ul medicului (opțional, null = toți medicii).</param>
    /// <param name="cancellationToken">Token pentru anularea operațiunii.</param>
    /// <returns>Lista programărilor din ziua respectivă.</returns>
    Task<IEnumerable<Programare>> GetByDateAsync(
        DateTime date,
        Guid? doctorID = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține programările viitoare (următoarele N zile) cu filtru opțional după medic.
    /// Util pentru dashboard-uri și reminder-uri.
    /// </summary>
    /// <param name="days">Numărul de zile în viitor (implicit: 7).</param>
  /// <param name="doctorID">ID-ul medicului (opțional, null = toți medicii).</param>
    /// <param name="cancellationToken">Token pentru anularea operațiunii.</param>
    /// <returns>Lista programărilor viitoare.</returns>
    Task<IEnumerable<Programare>> GetUpcomingAsync(
 int days = 7,
        Guid? doctorID = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține programul complet al unui medic pentru un interval de date (calendar view).
    /// </summary>
    /// <param name="doctorID">ID-ul medicului.</param>
    /// <param name="dataStart">Data de început a intervalului.</param>
    /// <param name="dataEnd">Data de sfârșit a intervalului.</param>
    /// <param name="cancellationToken">Token pentru anularea operațiunii.</param>
    /// <returns>Lista programărilor medicului în intervalul specificat (ordonate după dată și oră).</returns>
    Task<IEnumerable<Programare>> GetDoctorScheduleAsync(
        Guid doctorID,
        DateTime dataStart,
        DateTime dataEnd,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține istoricul programărilor unui pacient pentru un număr specificat de zile în urmă.
    /// </summary>
    /// <param name="pacientID">ID-ul pacientului.</param>
    /// <param name="ultimeleZile">Numărul de zile în urmă pentru care se obține istoricul (implicit: 365).</param>
    /// <param name="cancellationToken">Token pentru anularea operațiunii.</param>
    /// <returns>Lista programărilor pacientului din ultimele N zile.</returns>
    Task<IEnumerable<Programare>> GetPacientHistoryAsync(
     Guid pacientID,
        int ultimeleZile = 365,
  CancellationToken cancellationToken = default);

    // ==================== WRITE OPERATIONS (CRUD) ====================

    /// <summary>
    /// Creează o programare nouă în baza de date.
    /// </summary>
    /// <param name="programare">Obiectul programare de creat (ProgramareID este generat de DB).</param>
    /// <param name="cancellationToken">Token pentru anularea operațiunii.</param>
  /// <returns>Programarea creată cu ID-ul generat și toate câmpurile populate.</returns>
    Task<Programare> CreateAsync(Programare programare, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualizează o programare existentă în baza de date.
    /// </summary>
    /// <param name="programare">Obiectul programare cu modificările (trebuie să includă ProgramareID valid).</param>
    /// <param name="cancellationToken">Token pentru anularea operațiunii.</param>
    /// <returns>Programarea actualizată cu toate câmpurile populate.</returns>
    Task<Programare> UpdateAsync(Programare programare, CancellationToken cancellationToken = default);

    /// <summary>
    /// Șterge o programare (soft delete - marchează ca anulată).
    /// </summary>
    /// <param name="id">ID-ul programării de șters.</param>
    /// <param name="modificatDe">ID-ul utilizatorului care efectuează ștergerea.</param>
    /// <param name="cancellationToken">Token pentru anularea operațiunii.</param>
    /// <returns>True dacă ștergerea a reușit, false în caz contrar.</returns>
    Task<bool> DeleteAsync(Guid id, Guid modificatDe, CancellationToken cancellationToken = default);

    // ==================== BUSINESS OPERATIONS (VALIDĂRI) ====================

    /// <summary>
    /// Verifică dacă există un conflict de programare (suprapunere de interval orar) pentru un medic.
    /// Folosit pentru validare înainte de creare/editare programare.
    /// </summary>
    /// <param name="doctorID">ID-ul medicului.</param>
/// <param name="dataProgramare">Data programării.</param>
    /// <param name="oraInceput">Ora de început a programării.</param>
    /// <param name="oraSfarsit">Ora de sfârșit a programării.</param>
    /// <param name="excludeProgramareID">ID-ul programării de exclus din verificare (pentru editare).</param>
    /// <param name="cancellationToken">Token pentru anularea operațiunii.</param>
    /// <returns>True dacă există conflict (suprapunere), false dacă intervalul este liber.</returns>
    Task<bool> CheckConflictAsync(
  Guid doctorID,
        DateTime dataProgramare,
        TimeSpan oraInceput,
  TimeSpan oraSfarsit,
      Guid? excludeProgramareID = null,
        CancellationToken cancellationToken = default);

    // ==================== STATISTICS (RAPORTARE) ====================

    /// <summary>
    /// Obține statistici globale despre programări pentru un interval de date.
    /// Returnează: total programări, programări per status, programări per tip, medici activi, etc.
    /// </summary>
    /// <param name="dataStart">Data de început pentru statistici (implicit: prima zi a lunii curente).</param>
    /// <param name="dataEnd">Data de sfârșit pentru statistici (implicit: ultima zi a lunii curente).</param>
    /// <param name="cancellationToken">Token pentru anularea operațiunii.</param>
    /// <returns>Dicționar cu statistici (cheie-valoare).</returns>
    Task<Dictionary<string, object>> GetStatisticsAsync(
        DateTime? dataStart = null,
  DateTime? dataEnd = null,
  CancellationToken cancellationToken = default);

  /// <summary>
    /// Obține statistici despre programările unui medic specific pentru un interval de date.
    /// Returnează: total programări, programări per status, rata no-show, etc.
    /// </summary>
    /// <param name="doctorID">ID-ul medicului.</param>
    /// <param name="dataStart">Data de început pentru statistici (implicit: prima zi a lunii curente).</param>
 /// <param name="dataEnd">Data de sfârșit pentru statistici (implicit: ultima zi a lunii curente).</param>
    /// <param name="cancellationToken">Token pentru anularea operațiunii.</param>
/// <returns>Dicționar cu statistici specifice medicului.</returns>
    Task<Dictionary<string, object>> GetDoctorStatisticsAsync(
        Guid doctorID,
        DateTime? dataStart = null,
        DateTime? dataEnd = null,
    CancellationToken cancellationToken = default);
}
