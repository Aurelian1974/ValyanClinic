using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.Settings.Commands.UpdateSystemSetting;

public record UpdateSystemSettingCommand(
    string Categorie,
    string Cheie,
    string Valoare,
    string? Descriere,// ✅ ADĂUGAT - pentru editare descriere
    string ModificatDe
) : IRequest<Result>;
