using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PacientManagement.Commands.DeletePacient;

/// <summary>
/// Command pentru stergerea (soft delete) unui pacient
/// </summary>
public record DeletePacientCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string ModificatDe { get; init; } = string.Empty;
    public bool HardDelete { get; init; } = false; // Daca true, stergere fizica

    public DeletePacientCommand(Guid id, string modificatDe, bool hardDelete = false)
    {
        Id = id;
        ModificatDe = modificatDe;
        HardDelete = hardDelete;
    }
}
