using ValyanClinic.Domain.DTOs;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository pentru gestionarea relațiilor dintre pacienti și personalul medical.
/// Oferă metode pentru accesarea și manipularea datelor din tabela de relații
/// PacientiPersonalMedical prin stored procedures dedicate.
/// </summary>
/// <remarks>
/// Acest repository abstractizează accesul la baza de date pentru operațiile
/// legate de relațiile pacient-doctor, permițând testare mai ușoară și 
/// separare clară a concerns-urilor conform Clean Architecture.
/// 
/// <b>Stored Procedures folosite:</b>
/// - <c>sp_PacientiPersonalMedical_GetDoctoriByPacient</c>
/// - <c>sp_PacientiPersonalMedical_GetPacientiByDoctor</c>
/// - <c>sp_PacientiPersonalMedical_RemoveRelatie</c>
/// </remarks>
public interface IPacientPersonalMedicalRepository
{
    /// <summary>
    /// Obține lista tuturor doctorilor asociați unui pacient specific.
    /// </summary>
    /// <param name="pacientId">
    /// ID-ul unic al pacientului pentru care se caută doctorii asociați.
    /// Nu poate fi <see cref="Guid.Empty"/>.
    /// </param>
    /// <param name="apenumereActivi">
    /// Determină dacă se returnează doar relațiile active (<c>true</c>) 
    /// sau toate relațiile, inclusiv cele inactive (<c>false</c>).
    /// </param>
    /// <param name="cancellationToken">
    /// Token pentru anularea operației asincrone (opțional).
    /// </param>
    /// <returns>
    /// Lista de <see cref="DoctorAsociatDto"/> care conține informații despre
    /// doctorii asociați. Lista va fi goală dacă nu există relații găsite.
    /// Nu returnează niciodată <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Aruncat când <paramref name="pacientId"/> este <see cref="Guid.Empty"/>.
    /// </exception>
    /// <exception cref="System.Data.SqlClient.SqlException">
    /// Aruncat când apare o eroare la executarea stored procedure-ului
    /// sau la conectarea la baza de date.
    /// </exception>
    /// <example>
    /// Utilizare tipică pentru a obține toți doctorii activi:
    /// <code>
    /// var doctori = await repository.GetDoctoriByPacientAsync(
    ///     pacientId: Guid.Parse("..."),
    ///     apenumereActivi: true,
    ///     cancellationToken: cancellationToken
    /// );
    /// 
    /// foreach (var doctor in doctori)
    /// {
    ///     Console.WriteLine($"{doctor.DoctorNumeComplet} - {doctor.DoctorSpecializare}");
    /// }
    /// </code>
    /// </example>
    Task<List<DoctorAsociatDto>> GetDoctoriByPacientAsync(
        Guid pacientId,
        bool apenumereActivi,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține lista tuturor pacienților asociați unui doctor specific.
    /// </summary>
    /// <param name="personalMedicalId">
    /// ID-ul unic al personalului medical (doctor) pentru care se caută pacienții asociați.
    /// Nu poate fi <see cref="Guid.Empty"/>.
    /// </param>
    /// <param name="apenumereActivi">
    /// Determină dacă se returnează doar relațiile active (<c>true</c>) 
    /// sau toate relațiile, inclusiv cele inactive (<c>false</c>).
    /// </param>
    /// <param name="tipRelatie">
    /// Tipul specific de relație pentru filtrare (opțional).
    /// Exemple: "MedicPrimar", "Specialist", "MedicConsultant".
    /// Dacă este <c>null</c>, se returnează toate tipurile de relații.
    /// </param>
    /// <param name="cancellationToken">
    /// Token pentru anularea operației asincrone (opțional).
    /// </param>
    /// <returns>
    /// Lista de <see cref="PacientAsociatDto"/> care conține informații despre
    /// pacienții asociați. Lista va fi goală dacă nu există relații găsite.
    /// Nu returnează niciodată <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Aruncat când <paramref name="personalMedicalId"/> este <see cref="Guid.Empty"/>.
    /// </exception>
    /// <exception cref="System.Data.SqlClient.SqlException">
    /// Aruncat când apare o eroare la executarea stored procedure-ului
    /// sau la conectarea la baza de date.
    /// </exception>
    /// <example>
    /// Utilizare pentru a obține toți pacienții activi ai unui doctor:
    /// <code>
    /// var pacienti = await repository.GetPacientiByDoctorAsync(
    ///     personalMedicalId: doctorId,
    ///     apenumereActivi: true,
    ///     tipRelatie: null,
    ///     cancellationToken: cancellationToken
    /// );
    /// </code>
    /// </example>
    Task<List<PacientAsociatDto>> GetPacientiByDoctorAsync(
        Guid personalMedicalId,
        bool apenumereActivi,
        string? tipRelatie = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Dezactivează (soft delete) o relație existentă dintre un pacient și un doctor.
    /// Această operație nu șterge fizic relația din baza de date, ci setează
    /// câmpul <c>EsteActiv = 0</c> și <c>DataDezactivarii = GETDATE()</c>.
    /// </summary>
    /// <param name="relatieId">
    /// ID-ul unic al relației care urmează să fie dezactivată.
    /// Nu poate fi <see cref="Guid.Empty"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// Token pentru anularea operației asincrone (opțional).
    /// </param>
    /// <returns>
    /// Task care se completează când operația de dezactivare este finalizată.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Aruncat când <paramref name="relatieId"/> este <see cref="Guid.Empty"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Aruncat când relația specificată nu există în baza de date
    /// sau este deja inactivă.
    /// </exception>
    /// <exception cref="System.Data.SqlClient.SqlException">
    /// Aruncat când apare o eroare la executarea stored procedure-ului
    /// sau la conectarea la baza de date.
    /// </exception>
    /// <remarks>
    /// <b>Important:</b> Această operație este ireversibilă din perspectiva UI-ului.
    /// Pentru a reactiva relația, trebuie apelată o operație dedicată de reactivare
    /// (dacă există) sau creată manual o nouă relație.
    /// 
    /// <b>Audit:</b> Stored procedure-ul actualizează automat câmpurile de audit:
    /// - <c>DataDezactivarii</c> - setat la data/ora curentă
    /// - <c>EsteActiv</c> - setat la 0
    /// </remarks>
    /// <example>
    /// Utilizare tipică cu confirmare:
    /// <code>
    /// try
    /// {
    ///     await repository.RemoveRelatieAsync(
    ///         relatieId: relatieId,
    ///         cancellationToken: cancellationToken
    ///     );
    ///     
    ///     Console.WriteLine("Relație dezactivată cu succes!");
    /// }
    /// catch (InvalidOperationException ex)
    /// {
    ///     Console.WriteLine($"Relația nu poate fi dezactivată: {ex.Message}");
    /// }
    /// </code>
    /// <summary>
        /// Deactivates an existing patient–medical-staff relationship (soft delete).
        /// </summary>
        /// <param name="relatieId">The relationship's unique identifier; must not be <see cref="Guid.Empty"/>.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="relatieId"/> is <see cref="Guid.Empty"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the relationship does not exist or is already inactive.</exception>
        /// <exception cref="System.Data.SqlClient.SqlException">Thrown on stored procedure execution or database connectivity errors.</exception>
        /// <remarks>
        /// The operation marks the relationship as inactive and records the deactivation date; it is not reversible from the UI perspective.
        /// Audit fields are updated by the underlying stored procedure.
        /// </remarks>
    Task RemoveRelatieAsync(
        Guid relatieId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adaugă o nouă relație între un pacient și personalul medical.
    /// </summary>
    /// <param name="pacientId">ID-ul pacientului</param>
    /// <param name="personalMedicalId">ID-ul personalului medical (doctor)</param>
    /// <param name="tipRelatie">Tipul relației (ex: MedicPrimar, Specialist)</param>
    /// <param name="observatii">Observații opționale</param>
    /// <param name="motiv">Motivul asocierii</param>
    /// <param name="creatDe">ID-ul utilizatorului care creează relația</param>
    /// <param name="cancellationToken">Token pentru anulare</param>
    /// <summary>
        /// Create a new relationship between a patient and a medical staff member.
        /// </summary>
        /// <param name="pacientId">The patient identifier; must not be Guid.Empty.</param>
        /// <param name="personalMedicalId">The medical staff identifier; must not be Guid.Empty.</param>
        /// <param name="tipRelatie">Optional relationship type (e.g., "MedicPrimar", "Specialist").</param>
        /// <param name="observatii">Optional notes about the relationship.</param>
        /// <param name="motiv">Optional reason for creating the relationship.</param>
        /// <param name="creatDe">Optional identifier of the user who creates the relationship.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The identifier (Guid) of the newly created relationship.</returns>
    Task<Guid> AddRelatieAsync(
        Guid pacientId,
        Guid personalMedicalId,
        string? tipRelatie,
        string? observatii,
        string? motiv,
        Guid? creatDe,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reactivează o relație dezactivată între un pacient și personalul medical.
    /// </summary>
    /// <param name="relatieId">ID-ul relației care urmează să fie reactivată</param>
    /// <param name="observatii">Observații despre reactivare</param>
    /// <param name="motiv">Motivul reactivării</param>
    /// <param name="modificatDe">ID-ul utilizatorului care face reactivarea</param>
    /// <summary>
        /// Reactivates a previously deactivated patient–medical-staff relationship.
        /// </summary>
        /// <param name="relatieId">The identifier of the relationship to reactivate; must not be Guid.Empty.</param>
        /// <param name="observatii">Optional notes about the reactivation.</param>
        /// <param name="motiv">Optional reason for the reactivation.</param>
        /// <param name="modificatDe">Optional identifier of the user performing the reactivation.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task ActivateRelatieAsync(
        Guid relatieId,
        string? observatii,
        string? motiv,
        Guid? modificatDe,
        CancellationToken cancellationToken = default);
}