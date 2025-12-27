using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Services.Location;

/// <summary>
/// Serviciu centralizat pentru încărcarea și cache-uirea județelor și localităților.
/// Toate componentele care au nevoie de listă de județe trebuie să folosească acest serviciu.
/// </summary>
/// <remarks>
/// <b>Pattern:</b> Singleton service cu caching intern
/// <b>Cache Duration:</b> 1 oră (județele se schimbă foarte rar)
/// <b>Source:</b> Baza de date via ILocationRepository
/// </remarks>
public interface IJudeteService
{
    /// <summary>
    /// Returnează lista de județe din baza de date (cache-uită pentru performanță).
    /// </summary>
    /// <param name="cancellationToken">Token pentru anulare</param>
    /// <returns>Lista de județe sortată alfabetic</returns>
    Task<Result<IReadOnlyList<JudetDto>>> GetJudeteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returnează lista de localități pentru un județ specific.
    /// </summary>
    /// <param name="judetId">ID-ul județului</param>
    /// <param name="cancellationToken">Token pentru anulare</param>
    /// <returns>Lista de localități din județ</returns>
    Task<Result<IReadOnlyList<LocalitateDto>>> GetLocalitatiByJudetAsync(int judetId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returnează lista de localități pentru un județ după nume.
    /// </summary>
    /// <param name="judetNume">Numele județului</param>
    /// <param name="cancellationToken">Token pentru anulare</param>
    /// <returns>Lista de localități din județ</returns>
    Task<Result<IReadOnlyList<LocalitateDto>>> GetLocalitatiByJudetNameAsync(string judetNume, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidează cache-ul pentru județe (util după modificări în baza de date).
    /// </summary>
    void InvalidateCache();
}

/// <summary>
/// DTO pentru județ.
/// </summary>
public record JudetDto
{
    public int Id { get; init; }
    public string Nume { get; init; } = string.Empty;
}

/// <summary>
/// DTO pentru localitate.
/// </summary>
public record LocalitateDto
{
    public int Id { get; init; }
    public string Nume { get; init; } = string.Empty;
    public int JudetId { get; init; }
}
