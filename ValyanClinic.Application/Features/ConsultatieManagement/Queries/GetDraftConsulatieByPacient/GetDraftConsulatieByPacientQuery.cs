using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Queries.GetDraftConsulatieByPacient;

/// <summary>
/// Query pentru a găsi o consultație draft (nefinalizată) existentă pentru un pacient
/// Folosit pentru a preveni crearea de consultații duplicate la reîntoarcerea în pagină
/// 
/// Caută consultația draft pe baza:
/// - PacientID (obligatoriu)
/// - MedicID (opțional - dacă specificat, caută doar pentru acel medic)
/// - DataConsultatie (opțional - implicit azi)
/// - Status = 'Draft' sau 'InProgres'
/// </summary>
public record GetDraftConsulatieByPacientQuery : IRequest<Result<ConsulatieDetailDto?>>
{
    /// <summary>
    /// ID-ul pacientului pentru care căutăm consultația draft
    /// </summary>
    public Guid PacientID { get; init; }

    /// <summary>
    /// ID-ul medicului (opțional - dacă specificat, caută doar consultațiile create de acest medic)
    /// </summary>
    public Guid? MedicID { get; init; }

    /// <summary>
    /// Data consultației (opțional - implicit DateTime.Today)
    /// </summary>
    public DateTime? DataConsultatie { get; init; }

    /// <summary>
    /// ID-ul programării (opțional - dacă specificat, caută și după programare)
    /// </summary>
    public Guid? ProgramareID { get; init; }

    public GetDraftConsulatieByPacientQuery(Guid pacientId, Guid? medicId = null, DateTime? dataConsultatie = null, Guid? programareId = null)
    {
        PacientID = pacientId;
        MedicID = medicId;
        DataConsultatie = dataConsultatie ?? DateTime.Today;
        ProgramareID = programareId;
    }
}
