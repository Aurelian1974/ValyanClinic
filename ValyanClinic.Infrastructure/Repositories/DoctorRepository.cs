using Dapper;
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DoctorRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Doctor>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM Doctors 
            WHERE IsDeleted = 0 AND IsActive = 1
            ORDER BY LastName, FirstName";
        
        return await connection.QueryAsync<Doctor>(sql);
    }

    public async Task<Doctor?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM Doctors 
            WHERE Id = @Id AND IsDeleted = 0";
        
        return await connection.QueryFirstOrDefaultAsync<Doctor>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Doctor>> GetBySpecializationAsync(string specialization)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM Doctors 
            WHERE Specialization = @Specialization 
            AND IsDeleted = 0 AND IsActive = 1
            ORDER BY LastName, FirstName";
        
        return await connection.QueryAsync<Doctor>(sql, new { Specialization = specialization });
    }

    public async Task<Doctor> CreateAsync(Doctor doctor)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Doctors (FirstName, LastName, Email, PhoneNumber, Specialization, 
                               LicenseNumber, LicenseExpiryDate, Bio, ConsultationFee, IsActive,
                               CreatedAt, CreatedBy)
            VALUES (@FirstName, @LastName, @Email, @PhoneNumber, @Specialization, 
                   @LicenseNumber, @LicenseExpiryDate, @Bio, @ConsultationFee, @IsActive,
                   @CreatedAt, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        var id = await connection.QuerySingleAsync<int>(sql, doctor);
        doctor.Id = id;
        return doctor;
    }

    public async Task<Doctor> UpdateAsync(Doctor doctor)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Doctors SET 
                FirstName = @FirstName,
                LastName = @LastName,
                Email = @Email,
                PhoneNumber = @PhoneNumber,
                Specialization = @Specialization,
                LicenseNumber = @LicenseNumber,
                LicenseExpiryDate = @LicenseExpiryDate,
                Bio = @Bio,
                ConsultationFee = @ConsultationFee,
                IsActive = @IsActive,
                UpdatedAt = @UpdatedAt,
                UpdatedBy = @UpdatedBy
            WHERE Id = @Id";

        await connection.ExecuteAsync(sql, doctor);
        return doctor;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Doctors SET 
                IsDeleted = 1,
                DeletedAt = @DeletedAt,
                DeletedBy = @DeletedBy
            WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            Id = id, 
            DeletedAt = DateTime.UtcNow, 
            DeletedBy = "System"
        });

        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT COUNT(1) FROM Doctors 
            WHERE Id = @Id AND IsDeleted = 0";

        var count = await connection.QuerySingleAsync<int>(sql, new { Id = id });
        return count > 0;
    }
}