using Dapper;
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AppointmentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Appointment>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT a.*, p.FirstName as PatientFirstName, p.LastName as PatientLastName,
                   d.FirstName as DoctorFirstName, d.LastName as DoctorLastName
            FROM Appointments a
            INNER JOIN Patients p ON a.PatientId = p.Id
            INNER JOIN Doctors d ON a.DoctorId = d.Id
            WHERE a.IsDeleted = 0
            ORDER BY a.AppointmentDate, a.StartTime";
        
        return await connection.QueryAsync<Appointment>(sql);
    }

    public async Task<Appointment?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT a.*, p.FirstName as PatientFirstName, p.LastName as PatientLastName,
                   d.FirstName as DoctorFirstName, d.LastName as DoctorLastName
            FROM Appointments a
            INNER JOIN Patients p ON a.PatientId = p.Id
            INNER JOIN Doctors d ON a.DoctorId = d.Id
            WHERE a.Id = @Id AND a.IsDeleted = 0";
        
        return await connection.QueryFirstOrDefaultAsync<Appointment>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT a.*, p.FirstName as PatientFirstName, p.LastName as PatientLastName,
                   d.FirstName as DoctorFirstName, d.LastName as DoctorLastName
            FROM Appointments a
            INNER JOIN Patients p ON a.PatientId = p.Id
            INNER JOIN Doctors d ON a.DoctorId = d.Id
            WHERE a.PatientId = @PatientId AND a.IsDeleted = 0
            ORDER BY a.AppointmentDate DESC, a.StartTime";
        
        return await connection.QueryAsync<Appointment>(sql, new { PatientId = patientId });
    }

    public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT a.*, p.FirstName as PatientFirstName, p.LastName as PatientLastName,
                   d.FirstName as DoctorFirstName, d.LastName as DoctorLastName
            FROM Appointments a
            INNER JOIN Patients p ON a.PatientId = p.Id
            INNER JOIN Doctors d ON a.DoctorId = d.Id
            WHERE a.DoctorId = @DoctorId AND a.IsDeleted = 0
            ORDER BY a.AppointmentDate, a.StartTime";
        
        return await connection.QueryAsync<Appointment>(sql, new { DoctorId = doctorId });
    }

    public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT a.*, p.FirstName as PatientFirstName, p.LastName as PatientLastName,
                   d.FirstName as DoctorFirstName, d.LastName as DoctorLastName
            FROM Appointments a
            INNER JOIN Patients p ON a.PatientId = p.Id
            INNER JOIN Doctors d ON a.DoctorId = d.Id
            WHERE a.AppointmentDate >= @StartDate 
            AND a.AppointmentDate < @EndDate 
            AND a.IsDeleted = 0
            ORDER BY a.AppointmentDate, a.StartTime";
        
        return await connection.QueryAsync<Appointment>(sql, new { StartDate = startDate, EndDate = endDate });
    }

    public async Task<Appointment> CreateAsync(Appointment appointment)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Appointments (PatientId, DoctorId, AppointmentDate, StartTime, EndTime, 
                                    Type, Status, Reason, Notes, Cost, CreatedAt, CreatedBy)
            VALUES (@PatientId, @DoctorId, @AppointmentDate, @StartTime, @EndTime, 
                   @Type, @Status, @Reason, @Notes, @Cost, @CreatedAt, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        var id = await connection.QuerySingleAsync<int>(sql, appointment);
        appointment.Id = id;
        return appointment;
    }

    public async Task<Appointment> UpdateAsync(Appointment appointment)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Appointments SET 
                PatientId = @PatientId,
                DoctorId = @DoctorId,
                AppointmentDate = @AppointmentDate,
                StartTime = @StartTime,
                EndTime = @EndTime,
                Type = @Type,
                Status = @Status,
                Reason = @Reason,
                Notes = @Notes,
                Cost = @Cost,
                UpdatedAt = @UpdatedAt,
                UpdatedBy = @UpdatedBy
            WHERE Id = @Id";

        await connection.ExecuteAsync(sql, appointment);
        return appointment;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Appointments SET 
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

    public async Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeAppointmentId = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT COUNT(1) FROM Appointments 
            WHERE DoctorId = @DoctorId 
            AND AppointmentDate = @AppointmentDate
            AND IsDeleted = 0
            AND Status != 4 -- Not cancelled
            AND (
                (@StartTime >= StartTime AND @StartTime < EndTime) OR
                (@EndTime > StartTime AND @EndTime <= EndTime) OR
                (@StartTime <= StartTime AND @EndTime >= EndTime)
            )";

        if (excludeAppointmentId.HasValue)
        {
            sql += " AND Id != @ExcludeAppointmentId";
        }

        var parameters = new
        {
            DoctorId = doctorId,
            AppointmentDate = date.Date,
            StartTime = startTime,
            EndTime = endTime,
            ExcludeAppointmentId = excludeAppointmentId
        };

        var count = await connection.QuerySingleAsync<int>(sql, parameters);
        return count == 0;
    }
}