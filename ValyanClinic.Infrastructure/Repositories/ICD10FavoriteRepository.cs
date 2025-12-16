using Dapper;
using Microsoft.Data.SqlClient;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;
using System.Data;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository pentru ICD10 Favorites cu Dapper și Stored Procedures
/// Gestionează codurile ICD-10 favorite per medic
/// Tabel: ICD10_Favorites
/// </summary>
public class ICD10FavoriteRepository : BaseRepository, IICD10FavoriteRepository
{
    public ICD10FavoriteRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ICD10Code>> GetFavoritesAsync(
        Guid personalId,
        CancellationToken cancellationToken = default)
    {
        var parameters = new { PersonalID = personalId };
        
        return await QueryAsync<ICD10Code>(
            "sp_ICD10_GetFavorites",
            parameters,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Guid> AddFavoriteAsync(
        Guid personalId,
        Guid icd10Id,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("PersonalID", personalId);
        parameters.Add("ICD10_ID", icd10Id);
        parameters.Add("FavoriteId", dbType: DbType.Guid, direction: ParameterDirection.Output);
        
        await connection.ExecuteAsync(
            "sp_ICD10_AddFavorite",
            parameters,
            commandType: CommandType.StoredProcedure);
        
        return parameters.Get<Guid>("FavoriteId");
    }

    /// <inheritdoc />
    public async Task RemoveFavoriteAsync(
        Guid personalId,
        Guid icd10Id,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            PersonalID = personalId,
            ICD10_ID = icd10Id
        };
        
        await ExecuteAsync(
            "sp_ICD10_RemoveFavorite",
            parameters,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateSortOrderAsync(
        Guid personalId,
        Guid icd10Id,
        int newSortOrder,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            PersonalID = personalId,
            ICD10_ID = icd10Id,
            NewSortOrder = newSortOrder
        };
        
        await ExecuteAsync(
            "sp_ICD10_UpdateFavoriteOrder",
            parameters,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> IsFavoriteAsync(
        Guid personalId,
        Guid icd10Id,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var count = await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(1) FROM ICD10_Favorites 
              WHERE PersonalID = @PersonalID AND ICD10_ID = @ICD10_ID",
            new { PersonalID = personalId, ICD10_ID = icd10Id });
        
        return count > 0;
    }
}
