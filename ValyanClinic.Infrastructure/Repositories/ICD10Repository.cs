using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;  // ✅ ADD: Pentru DbType și ParameterDirection
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository pentru coduri ICD-10 - folosește stored procedures din ValyanMed
/// Implementează pattern Repository conform Clean Architecture (returnează Domain entities)
/// </summary>
public class ICD10Repository : IICD10Repository
{
    private readonly string _connectionString;

    public ICD10Repository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
         ?? throw new InvalidOperationException("DefaultConnection missing");
    }

    /// <summary>Caută coduri ICD-10 folosind sp_ICD10_Search</summary>
  public async Task<IEnumerable<ICD10Code>> SearchAsync(string searchTerm, int maxResults = 50)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var parameters = new
        {
    SearchTerm = searchTerm,
            MaxResults = maxResults
        };

        return await connection.QueryAsync<ICD10Code>(
            "sp_ICD10_Search",
     parameters,
            commandType: System.Data.CommandType.StoredProcedure
      );
    }

    /// <summary>Obține detalii cod ICD-10 după ID</summary>
    public async Task<ICD10Code?> GetByIdAsync(Guid icd10Id)
    {
using var connection = new SqlConnection(_connectionString);
        
        var parameters = new { ICD10_ID = icd10Id };

        return await connection.QueryFirstOrDefaultAsync<ICD10Code>(
      "sp_ICD10_GetById",
   parameters,
    commandType: System.Data.CommandType.StoredProcedure
);
  }

    /// <summary>Obține detalii cod ICD-10 după cod (ex: I10)</summary>
    public async Task<ICD10Code?> GetByCodeAsync(string code)
  {
        using var connection = new SqlConnection(_connectionString);
        
        var parameters = new { Code = code };

        return await connection.QueryFirstOrDefaultAsync<ICD10Code>(
  "sp_ICD10_GetByCode",
         parameters,
            commandType: System.Data.CommandType.StoredProcedure
        );
    }

    /// <summary>Obține coduri ICD-10 frecvente (IsCommon = true)</summary>
    public async Task<IEnumerable<ICD10Code>> GetCommonCodesAsync(int maxResults = 100)
    {
        using var connection = new SqlConnection(_connectionString);
   
        var parameters = new { MaxResults = maxResults };

        return await connection.QueryAsync<ICD10Code>(
            "sp_ICD10_GetCommonCodes",
  parameters,
  commandType: System.Data.CommandType.StoredProcedure
        );
    }

    /// <summary>Obține favorite ICD-10 ale utilizatorului</summary>
    public async Task<IEnumerable<ICD10Code>> GetFavoritesAsync(Guid userId)
    {
        using var connection = new SqlConnection(_connectionString);
  
        var parameters = new { PersonalID = userId };  // ✅ CORRECTED: PersonalID (matches DB schema)

        return await connection.QueryAsync<ICD10Code>(
  "sp_ICD10_GetFavorites",
   parameters,
     commandType: System.Data.CommandType.StoredProcedure
        );
    }

  /// <summary>Adaugă cod ICD-10 la favorite</summary>
   public async Task<bool> AddFavoriteAsync(Guid userId, Guid icd10Id)
    {
    using var connection = new SqlConnection(_connectionString);
      
        var parameters = new DynamicParameters();
        parameters.Add("PersonalID", userId, DbType.Guid);  // ✅ CORRECTED: PersonalID (matches DB schema)
  parameters.Add("ICD10_ID", icd10Id, DbType.Guid);

        var result = await connection.ExecuteAsync(
            "sp_ICD10_AddFavorite",
   parameters,
  commandType: System.Data.CommandType.StoredProcedure
        );

        // SP returns 1 on success, 0 if already favorite
   return result >= 0;
    }

    /// <summary>Elimină cod ICD-10 din favorite</summary>
    public async Task<bool> RemoveFavoriteAsync(Guid userId, Guid icd10Id)
    {
        using var connection = new SqlConnection(_connectionString);
    
   var parameters = new
        {
       PersonalID = userId,  // ✅ CORRECTED: PersonalID (matches DB schema)
     ICD10_ID = icd10Id
        };

        var result = await connection.ExecuteAsync(
  "sp_ICD10_RemoveFavorite",
            parameters,
commandType: System.Data.CommandType.StoredProcedure
        );

        return result > 0;
    }

    /// <summary>Obține statistici ICD-10 (tuple simplu)</summary>
    public async Task<(int TotalCodes, int TranslatedCodes, int CommonCodes, int LeafNodeCodes)> GetStatisticsAsync()
    {
    using var connection = new SqlConnection(_connectionString);

// sp_ICD10_GetStatistics returnează un singur rând cu statistici agregate
      var stats = await connection.QueryFirstAsync<dynamic>(
       "sp_ICD10_GetStatistics",
 commandType: System.Data.CommandType.StoredProcedure
 );

     return ((int)stats.TotalCodes, 
      (int)stats.TranslatedCodes, 
    (int)stats.CommonCodes, 
     (int)stats.LeafNodeCodes);
    }

    /// <summary>Validează cod ICD-10</summary>
    public async Task<(bool IsValid, string? ErrorMessage)> ValidateCodeAsync(string code)
    {
  using var connection = new SqlConnection(_connectionString);
        
    var parameters = new { Code = code };

        var result = await connection.QueryFirstAsync<dynamic>(
    "sp_ICD10_ValidateCode",
         parameters,
            commandType: System.Data.CommandType.StoredProcedure
        );

        return ((bool)result.IsValid, (string?)result.ErrorMessage);
    }
}
