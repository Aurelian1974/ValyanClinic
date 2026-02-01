using ValyanClinic.Domain.Entities.Settings;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository pentru gestionarea programului de lucru al clinicii
/// </summary>
public interface IProgramLucruRepository
{
    /// <summary>
    /// Obține programul complet pentru toate zilele săptămânii
    /// </summary>
    Task<IEnumerable<ProgramLucru>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține programul pentru o anumită zi a săptămânii
    /// </summary>
    Task<ProgramLucru?> GetByDayAsync(DayOfWeek ziSaptamana, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualizează programul pentru o zi
    /// </summary>
    Task<bool> UpdateAsync(ProgramLucru programLucru, string modificatDe, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifică dacă clinica este deschisă la o anumită dată și oră
    /// </summary>
    Task<bool> EsteInProgramAsync(DateTime data, TimeSpan ora, CancellationToken cancellationToken = default);
}
