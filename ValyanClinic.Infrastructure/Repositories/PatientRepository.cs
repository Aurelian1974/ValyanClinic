using Dapper;
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PatientRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Patient>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM Patients 
            WHERE IsDeleted = 0 
            ORDER BY LastName, FirstName";
        
        return await connection.QueryAsync<Patient>(sql);
    }

    public async Task<Patient?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM Patients 
            WHERE Id = @Id AND IsDeleted = 0";
        
        return await connection.QueryFirstOrDefaultAsync<Patient>(sql, new { Id = id });
    }

    public async Task<Patient?> GetByCNPAsync(string cnp)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM Patients 
            WHERE CNP = @CNP AND IsDeleted = 0";
        
        return await connection.QueryFirstOrDefaultAsync<Patient>(sql, new { CNP = cnp });
    }

    public async Task<IEnumerable<Patient>> SearchAsync(string searchTerm)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM Patients 
            WHERE IsDeleted = 0 
            AND (FirstName LIKE @SearchTerm 
                OR LastName LIKE @SearchTerm 
                OR Email LIKE @SearchTerm 
                OR PhoneNumber LIKE @SearchTerm
                OR CNP LIKE @SearchTerm)
            ORDER BY LastName, FirstName";
        
        var searchPattern = $"%{searchTerm}%";
        return await connection.QueryAsync<Patient>(sql, new { SearchTerm = searchPattern });
    }

    public async Task<Patient> CreateAsync(Patient patient)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Patients (FirstName, LastName, Email, PhoneNumber, DateOfBirth, Gender, 
                                Address, CNP, EmergencyContactName, EmergencyContactPhone, 
                                BloodType, Allergies, MedicalHistory, Notes, CreatedAt, CreatedBy)
            VALUES (@FirstName, @LastName, @Email, @PhoneNumber, @DateOfBirth, @Gender, 
                   @Address, @CNP, @EmergencyContactName, @EmergencyContactPhone, 
                   @BloodType, @Allergies, @MedicalHistory, @Notes, @CreatedAt, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        var id = await connection.QuerySingleAsync<int>(sql, patient);
        patient.Id = id;
        return patient;
    }

    public async Task<Patient> UpdateAsync(Patient patient)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Patients SET 
                FirstName = @FirstName,
                LastName = @LastName,
                Email = @Email,
                PhoneNumber = @PhoneNumber,
                DateOfBirth = @DateOfBirth,
                Gender = @Gender,
                Address = @Address,
                CNP = @CNP,
                EmergencyContactName = @EmergencyContactName,
                EmergencyContactPhone = @EmergencyContactPhone,
                BloodType = @BloodType,
                Allergies = @Allergies,
                MedicalHistory = @MedicalHistory,
                Notes = @Notes,
                UpdatedAt = @UpdatedAt,
                UpdatedBy = @UpdatedBy
            WHERE Id = @Id";

        await connection.ExecuteAsync(sql, patient);
        return patient;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Patients SET 
                IsDeleted = 1,
                DeletedAt = @DeletedAt,
                DeletedBy = @DeletedBy
            WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            Id = id, 
            DeletedAt = DateTime.UtcNow, 
            DeletedBy = "System" // TODO: Get from current user context
        });

        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT COUNT(1) FROM Patients 
            WHERE Id = @Id AND IsDeleted = 0";

        var count = await connection.QuerySingleAsync<int>(sql, new { Id = id });
        return count > 0;
    }
}