using Dapper;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories;

public class TipDepartamentRepository : BaseRepository, ITipDepartamentRepository
{
    public TipDepartamentRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<TipDepartament>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await QueryAsync<TipDepartament>("sp_TipDepartament_GetAll", cancellationToken: cancellationToken);
    }

    public async Task<TipDepartament?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parameters = new { IdTipDepartament = id };
        var all = await QueryAsync<TipDepartament>("sp_TipDepartament_GetAll", parameters, cancellationToken);
        return all.FirstOrDefault(t => t.IdTipDepartament == id);
    }
}
