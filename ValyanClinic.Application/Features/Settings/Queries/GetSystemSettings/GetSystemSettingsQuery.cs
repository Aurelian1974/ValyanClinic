using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.Settings.Queries.GetSystemSettings;

public record GetSystemSettingsQuery(string Categorie) : IRequest<Result<List<SystemSettingDto>>>;
