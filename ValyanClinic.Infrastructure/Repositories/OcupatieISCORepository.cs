using Dapper;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository pentru OcupatieISCO cu Dapper și Stored Procedures
/// Implementează accesul la datele ISCO-08 cu operațiuni complete CRUD
/// </summary>
public class OcupatieISCORepository : BaseRepository, IOcupatieISCORepository
{
    public OcupatieISCORepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<OcupatieISCO>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 50,
        string? searchText = null,
        byte? nivelIerarhic = null,
        string? grupaMajora = null,
        bool? esteActiv = true,
        string sortColumn = "Cod_ISCO",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchText = searchText,
            NivelIerarhic = nivelIerarhic,
            GrupaMajora = grupaMajora,
            EsteActiv = esteActiv,
            SortColumn = sortColumn,
            SortDirection = sortDirection
        };

        return await QueryAsync<OcupatieISCO>("sp_Ocupatii_ISCO08_GetAll", parameters, cancellationToken);
    }

    public async Task<int> GetCountAsync(
        string? searchText = null,
        byte? nivelIerarhic = null,
        string? grupaMajora = null,
        bool? esteActiv = true,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            SearchText = searchText,
            NivelIerarhic = nivelIerarhic,
            GrupaMajora = grupaMajora,
            EsteActiv = esteActiv
        };

        // Folosește stored procedure pentru count sau direct COUNT din GetAll
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Ocupatii_ISCO08 WHERE " +
            "(@SearchText IS NULL OR Denumire_Ocupatie LIKE '%' + @SearchText + '%' OR Cod_ISCO LIKE '%' + @SearchText + '%') " +
            "AND (@NivelIerarhic IS NULL OR Nivel_Ierarhic = @NivelIerarhic) " +
            "AND (@GrupaMajora IS NULL OR Grupa_Majora = @GrupaMajora) " +
            "AND (@EsteActiv IS NULL OR Este_Activ = @EsteActiv)",
            parameters);

        return result;
    }

    public async Task<OcupatieISCO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parameters = new { Id = id };
        return await QueryFirstOrDefaultAsync<OcupatieISCO>("sp_Ocupatii_ISCO08_GetById", parameters, cancellationToken);
    }

    public async Task<OcupatieISCO?> GetByCodISCOAsync(string codISCO, CancellationToken cancellationToken = default)
    {
        var parameters = new { CodISCO = codISCO };
        return await QueryFirstOrDefaultAsync<OcupatieISCO>("sp_Ocupatii_ISCO08_GetByCod", parameters, cancellationToken);
    }

    public async Task<IEnumerable<OcupatieISCO>> GetCopiiAsync(string codParinte, CancellationToken cancellationToken = default)
    {
        var parameters = new { CodParinte = codParinte };
        return await QueryAsync<OcupatieISCO>("sp_Ocupatii_ISCO08_GetCopii", parameters, cancellationToken);
    }

    public async Task<IEnumerable<OcupatieISCO>> GetGrupeMajoreAsync(CancellationToken cancellationToken = default)
    {
        return await QueryAsync<OcupatieISCO>("sp_Ocupatii_ISCO08_GetGrupeMajore", cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<(OcupatieISCO ocupatie, int scorRelevanta)>> SearchAsync(
        string searchText,
        byte? nivelIerarhic = null,
        int maxResults = 20,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            SearchText = searchText,
            NivelIerarhic = nivelIerarhic,
            MaxResults = maxResults
        };

        var results = await QueryAsync<SearchResultDto>("sp_Ocupatii_ISCO08_Search", parameters, cancellationToken);
        
        return results.Select(r => (
            ocupatie: new OcupatieISCO
            {
                Id = r.Id,
                CodISCO = r.Cod_ISCO,
                DenumireOcupatie = r.Denumire_Ocupatie,
                NivelIerarhic = r.Nivel_Ierarhic,
                GrupaMajoraDenumire = r.Grupa_Majora_Denumire
            },
            scorRelevanta: r.ScorRelevanta
        ));
    }

    public async Task<(string categorie, int numar, int active)[]> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var results = await QueryAsync<StatisticDto>("sp_Ocupatii_ISCO08_GetStatistics", cancellationToken: cancellationToken);
        return results.Select(r => (r.Categorie, r.Numar, r.Active)).ToArray();
    }

    public async Task<(string value, string text)[]> GetDropdownOptionsAsync(
        byte nivelIerarhic = 4,
        string? grupaMajora = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            NivelIerarhic = nivelIerarhic,
            GrupaMajora = grupaMajora
        };

        var results = await QueryAsync<DropdownOptionDto>("sp_Ocupatii_ISCO08_GetDropdownOptions", parameters, cancellationToken);
        return results.Select(r => (r.Value, r.Text)).ToArray();
    }

    public async Task<Guid> CreateAsync(OcupatieISCO ocupatie, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            ocupatie.CodISCO,
            ocupatie.DenumireOcupatie,
            ocupatie.DenumireOcupatieEN,
            ocupatie.NivelIerarhic,
            ocupatie.CodParinte,
            ocupatie.GrupaMajora,
            ocupatie.GrupaMajoraDenumire,
            ocupatie.Subgrupa,
            ocupatie.SubgrupaDenumire,
            ocupatie.GrupaMinora,
            ocupatie.GrupaMinoraDenumire,
            ocupatie.Descriere,
            ocupatie.Observatii,
            ocupatie.CreatDe
        };

        var result = await QueryFirstOrDefaultAsync<CreateResultDto>("sp_Ocupatii_ISCO08_Create", parameters, cancellationToken);
        return result?.Id ?? Guid.Empty;
    }

    public async Task<bool> UpdateAsync(OcupatieISCO ocupatie, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            ocupatie.Id,
            ocupatie.DenumireOcupatie,
            ocupatie.DenumireOcupatieEN,
            ocupatie.Descriere,
            ocupatie.Observatii,
            ocupatie.EsteActiv,
            ocupatie.ModificatDe
        };

        var result = await QueryFirstOrDefaultAsync<UpdateResultDto>("sp_Ocupatii_ISCO08_Update", parameters, cancellationToken);
        return result != null;
    }

    public async Task<bool> DeleteAsync(Guid id, string modificatDe, CancellationToken cancellationToken = default)
    {
        var parameters = new { Id = id, ModificatDe = modificatDe };
        
        var result = await QueryFirstOrDefaultAsync<DeleteResultDto>("sp_Ocupatii_ISCO08_Delete", parameters, cancellationToken);
        return result != null;
    }

    public async Task<bool> IsUniqueAsync(string codISCO, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = "SELECT COUNT(*) FROM Ocupatii_ISCO08 WHERE Cod_ISCO = @CodISCO" +
                   (excludeId.HasValue ? " AND Id != @ExcludeId" : "");
        
        object parameters;
        if (excludeId.HasValue)
        {
            parameters = new { CodISCO = codISCO, ExcludeId = excludeId.Value };
        }
        else
        {
            parameters = new { CodISCO = codISCO };
        }
        
        var count = await connection.ExecuteScalarAsync<int>(query, parameters);
        return count == 0;
    }

    #region DTOs pentru maparea rezultatelor

    private class SearchResultDto
    {
        public Guid Id { get; set; }
        public string Cod_ISCO { get; set; } = string.Empty;
        public string Denumire_Ocupatie { get; set; } = string.Empty;
        public byte Nivel_Ierarhic { get; set; }
        public string? Grupa_Majora_Denumire { get; set; }
        public int ScorRelevanta { get; set; }
    }

    private class StatisticDto
    {
        public string Categorie { get; set; } = string.Empty;
        public int Numar { get; set; }
        public int Active { get; set; }
    }

    private class DropdownOptionDto
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    private class CreateResultDto
    {
        public Guid Id { get; set; }
    }

    private class UpdateResultDto
    {
        public Guid Id { get; set; }
    }

    private class DeleteResultDto
    {
        public string Mesaj { get; set; } = string.Empty;
    }

    #endregion
}
