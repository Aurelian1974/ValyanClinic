using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Infrastructure.Data;
using Dapper;
using System.Data;

namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalById;

public class GetPersonalMedicalByIdQueryHandler : IRequestHandler<GetPersonalMedicalByIdQuery, Result<PersonalMedicalDetailDto>>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<GetPersonalMedicalByIdQueryHandler> _logger;

    public GetPersonalMedicalByIdQueryHandler(
        IDbConnectionFactory connectionFactory,
        ILogger<GetPersonalMedicalByIdQueryHandler> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Result<PersonalMedicalDetailDto>> Handle(
        GetPersonalMedicalByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching PersonalMedical by ID: {PersonalID}", request.PersonalID);

            // CRITICAL: Query direct cu Dapper pentru a map-a TOATE coloanele din SP
            // inclusiv CategorieName, SpecializareName, SubspecializareName care vin din JOIN-uri
            using var connection = _connectionFactory.CreateConnection();
            
            var dto = await connection.QuerySingleOrDefaultAsync<PersonalMedicalDetailDto>(
                "sp_PersonalMedical_GetById",
                new { PersonalID = request.PersonalID },
                commandType: CommandType.StoredProcedure);

            if (dto == null)
            {
                _logger.LogWarning("PersonalMedical not found: {PersonalID}", request.PersonalID);
                return Result<PersonalMedicalDetailDto>.Failure("Personal medical not found");
            }

            _logger.LogInformation(
                "PersonalMedical found: {PersonalID} - {NumeComplet}, Categorie: {Categorie}, Specializare: {Specializare}", 
                dto.PersonalID, 
                dto.NumeComplet,
                dto.CategorieName ?? "N/A",
                dto.SpecializareName ?? "N/A");

            return Result<PersonalMedicalDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching PersonalMedical by ID: {PersonalID}", request.PersonalID);
            return Result<PersonalMedicalDetailDto>.Failure($"Error: {ex.Message}");
        }
    }
}
