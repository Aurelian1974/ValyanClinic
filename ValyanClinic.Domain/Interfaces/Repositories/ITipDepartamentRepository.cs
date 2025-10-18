using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

public interface ITipDepartamentRepository
{
    Task<IEnumerable<TipDepartament>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TipDepartament?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
