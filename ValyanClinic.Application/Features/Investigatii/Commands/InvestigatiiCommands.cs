using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities.Investigatii;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.Investigatii.Commands;

#region Investigatii Imagistice Recomandate

/// <summary>
/// Comandă pentru adăugarea unei investigații imagistice recomandate
/// </summary>
public record AddInvestigatieImagisticaRecomandataCommand : IRequest<Result<Guid>>
{
    public Guid ConsultatieID { get; init; }
    public Guid? InvestigatieNomenclatorID { get; init; }
    public string DenumireInvestigatie { get; init; } = string.Empty;
    public string? CodInvestigatie { get; init; }
    public string? RegiuneAnatomica { get; init; }
    public string? Prioritate { get; init; }
    public bool EsteCito { get; init; }
    public string? IndicatiiClinice { get; init; }
    public string? ObservatiiMedic { get; init; }
    public Guid CreatDe { get; init; }
}

public class AddInvestigatieImagisticaRecomandataCommandHandler 
    : IRequestHandler<AddInvestigatieImagisticaRecomandataCommand, Result<Guid>>
{
    private readonly IConsultatieInvestigatieImagisticaRecomandataRepository _repository;

    public AddInvestigatieImagisticaRecomandataCommandHandler(IConsultatieInvestigatieImagisticaRecomandataRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(AddInvestigatieImagisticaRecomandataCommand request, CancellationToken cancellationToken)
    {
        var entity = new ConsultatieInvestigatieImagisticaRecomandata
        {
            ConsultatieID = request.ConsultatieID,
            InvestigatieNomenclatorID = request.InvestigatieNomenclatorID,
            DenumireInvestigatie = request.DenumireInvestigatie,
            CodInvestigatie = request.CodInvestigatie,
            RegiuneAnatomica = request.RegiuneAnatomica,
            DataRecomandare = DateTime.Now,
            Prioritate = request.Prioritate ?? "Normala",
            EsteCito = request.EsteCito,
            IndicatiiClinice = request.IndicatiiClinice,
            ObservatiiMedic = request.ObservatiiMedic,
            Status = "Recomandata",
            CreatDe = request.CreatDe
        };

        var id = await _repository.CreateAsync(entity, cancellationToken);
        return Result<Guid>.Success(id);
    }
}

/// <summary>
/// Comandă pentru ștergerea unei investigații imagistice recomandate
/// </summary>
public record DeleteInvestigatieImagisticaRecomandataCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteInvestigatieImagisticaRecomandataCommandHandler 
    : IRequestHandler<DeleteInvestigatieImagisticaRecomandataCommand, Result<bool>>
{
    private readonly IConsultatieInvestigatieImagisticaRecomandataRepository _repository;

    public DeleteInvestigatieImagisticaRecomandataCommandHandler(IConsultatieInvestigatieImagisticaRecomandataRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(DeleteInvestigatieImagisticaRecomandataCommand request, CancellationToken cancellationToken)
    {
        var success = await _repository.DeleteAsync(request.Id, cancellationToken);
        return success 
            ? Result<bool>.Success(true) 
            : Result<bool>.Failure("Investigația nu a fost găsită.");
    }
}

#endregion

#region Explorări Funcționale Recomandate

/// <summary>
/// Comandă pentru adăugarea unei explorări funcționale recomandate
/// </summary>
public record AddExplorareRecomandataCommand : IRequest<Result<Guid>>
{
    public Guid ConsultatieID { get; init; }
    public Guid? ExplorareNomenclatorID { get; init; }
    public string DenumireExplorare { get; init; } = string.Empty;
    public string? CodExplorare { get; init; }
    public string? Prioritate { get; init; }
    public bool EsteCito { get; init; }
    public string? IndicatiiClinice { get; init; }
    public string? ObservatiiMedic { get; init; }
    public Guid CreatDe { get; init; }
}

public class AddExplorareRecomandataCommandHandler 
    : IRequestHandler<AddExplorareRecomandataCommand, Result<Guid>>
{
    private readonly IConsultatieExplorareRecomandataRepository _repository;

    public AddExplorareRecomandataCommandHandler(IConsultatieExplorareRecomandataRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(AddExplorareRecomandataCommand request, CancellationToken cancellationToken)
    {
        var entity = new ConsultatieExplorareRecomandata
        {
            ConsultatieID = request.ConsultatieID,
            ExplorareNomenclatorID = request.ExplorareNomenclatorID,
            DenumireExplorare = request.DenumireExplorare,
            CodExplorare = request.CodExplorare,
            DataRecomandare = DateTime.Now,
            Prioritate = request.Prioritate ?? "Normala",
            EsteCito = request.EsteCito,
            IndicatiiClinice = request.IndicatiiClinice,
            ObservatiiMedic = request.ObservatiiMedic,
            Status = "Recomandata",
            CreatDe = request.CreatDe
        };

        var id = await _repository.CreateAsync(entity, cancellationToken);
        return Result<Guid>.Success(id);
    }
}

/// <summary>
/// Comandă pentru ștergerea unei explorări funcționale recomandate
/// </summary>
public record DeleteExplorareRecomandataCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteExplorareRecomandataCommandHandler 
    : IRequestHandler<DeleteExplorareRecomandataCommand, Result<bool>>
{
    private readonly IConsultatieExplorareRecomandataRepository _repository;

    public DeleteExplorareRecomandataCommandHandler(IConsultatieExplorareRecomandataRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(DeleteExplorareRecomandataCommand request, CancellationToken cancellationToken)
    {
        var success = await _repository.DeleteAsync(request.Id, cancellationToken);
        return success 
            ? Result<bool>.Success(true) 
            : Result<bool>.Failure("Explorarea nu a fost găsită.");
    }
}

#endregion

#region Endoscopii Recomandate

/// <summary>
/// Comandă pentru adăugarea unei endoscopii recomandate
/// </summary>
public record AddEndoscopieRecomandataCommand : IRequest<Result<Guid>>
{
    public Guid ConsultatieID { get; init; }
    public Guid? EndoscopieNomenclatorID { get; init; }
    public string DenumireEndoscopie { get; init; } = string.Empty;
    public string? CodEndoscopie { get; init; }
    public string? Prioritate { get; init; }
    public bool EsteCito { get; init; }
    public string? IndicatiiClinice { get; init; }
    public string? ObservatiiMedic { get; init; }
    public Guid CreatDe { get; init; }
}

public class AddEndoscopieRecomandataCommandHandler 
    : IRequestHandler<AddEndoscopieRecomandataCommand, Result<Guid>>
{
    private readonly IConsultatieEndoscopieRecomandataRepository _repository;

    public AddEndoscopieRecomandataCommandHandler(IConsultatieEndoscopieRecomandataRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(AddEndoscopieRecomandataCommand request, CancellationToken cancellationToken)
    {
        var entity = new ConsultatieEndoscopieRecomandata
        {
            ConsultatieID = request.ConsultatieID,
            EndoscopieNomenclatorID = request.EndoscopieNomenclatorID,
            DenumireEndoscopie = request.DenumireEndoscopie,
            CodEndoscopie = request.CodEndoscopie,
            DataRecomandare = DateTime.Now,
            Prioritate = request.Prioritate ?? "Normala",
            EsteCito = request.EsteCito,
            IndicatiiClinice = request.IndicatiiClinice,
            ObservatiiMedic = request.ObservatiiMedic,
            Status = "Recomandata",
            CreatDe = request.CreatDe
        };

        var id = await _repository.CreateAsync(entity, cancellationToken);
        return Result<Guid>.Success(id);
    }
}

/// <summary>
/// Comandă pentru ștergerea unei endoscopii recomandate
/// </summary>
public record DeleteEndoscopieRecomandataCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteEndoscopieRecomandataCommandHandler 
    : IRequestHandler<DeleteEndoscopieRecomandataCommand, Result<bool>>
{
    private readonly IConsultatieEndoscopieRecomandataRepository _repository;

    public DeleteEndoscopieRecomandataCommandHandler(IConsultatieEndoscopieRecomandataRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(DeleteEndoscopieRecomandataCommand request, CancellationToken cancellationToken)
    {
        var success = await _repository.DeleteAsync(request.Id, cancellationToken);
        return success 
            ? Result<bool>.Success(true) 
            : Result<bool>.Failure("Endoscopia nu a fost găsită.");
    }
}

#endregion

#region Investigatii Imagistice Efectuate

/// <summary>
/// Comandă pentru adăugarea unei investigații imagistice efectuate
/// </summary>
public record AddInvestigatieImagisticaEfectuataCommand : IRequest<Result<Guid>>
{
    public Guid? RecomandareID { get; init; }
    public Guid? ConsultatieID { get; init; }
    public Guid PacientID { get; init; }
    public Guid? InvestigatieNomenclatorID { get; init; }
    public string DenumireInvestigatie { get; init; } = string.Empty;
    public string? CodInvestigatie { get; init; }
    public string? RegiuneAnatomica { get; init; }
    public DateTime DataEfectuare { get; init; }
    public string? CentrulMedical { get; init; }
    public string? MedicExecutant { get; init; }
    public string? Rezultat { get; init; }
    public string? Concluzii { get; init; }
    public string? CaleFisierRezultat { get; init; }
    public Guid CreatDe { get; init; }
}

public class AddInvestigatieImagisticaEfectuataCommandHandler 
    : IRequestHandler<AddInvestigatieImagisticaEfectuataCommand, Result<Guid>>
{
    private readonly IConsultatieInvestigatieImagisticaEfectuataRepository _repository;
    private readonly IConsultatieInvestigatieImagisticaRecomandataRepository _recomandataRepository;

    public AddInvestigatieImagisticaEfectuataCommandHandler(
        IConsultatieInvestigatieImagisticaEfectuataRepository repository,
        IConsultatieInvestigatieImagisticaRecomandataRepository recomandataRepository)
    {
        _repository = repository;
        _recomandataRepository = recomandataRepository;
    }

    public async Task<Result<Guid>> Handle(AddInvestigatieImagisticaEfectuataCommand request, CancellationToken cancellationToken)
    {
        var entity = new ConsultatieInvestigatieImagisticaEfectuata
        {
            RecomandareID = request.RecomandareID,
            ConsultatieID = request.ConsultatieID,
            PacientID = request.PacientID,
            InvestigatieNomenclatorID = request.InvestigatieNomenclatorID,
            DenumireInvestigatie = request.DenumireInvestigatie,
            CodInvestigatie = request.CodInvestigatie,
            RegiuneAnatomica = request.RegiuneAnatomica,
            DataEfectuare = request.DataEfectuare,
            CentrulMedical = request.CentrulMedical,
            MedicExecutant = request.MedicExecutant,
            Rezultat = request.Rezultat,
            Concluzii = request.Concluzii,
            CaleFisierRezultat = request.CaleFisierRezultat,
            CreatDe = request.CreatDe
        };

        var id = await _repository.CreateAsync(entity, cancellationToken);

        // Actualizează statusul recomandării dacă există
        if (request.RecomandareID.HasValue)
        {
            await _recomandataRepository.UpdateStatusAsync(
                request.RecomandareID.Value, 
                "Efectuata", 
                request.CreatDe, 
                cancellationToken);
        }

        return Result<Guid>.Success(id);
    }
}

#endregion

#region Explorări Funcționale Efectuate

/// <summary>
/// Comandă pentru adăugarea unei explorări funcționale efectuate
/// </summary>
public record AddExplorareEfectuataCommand : IRequest<Result<Guid>>
{
    public Guid? RecomandareID { get; init; }
    public Guid? ConsultatieID { get; init; }
    public Guid PacientID { get; init; }
    public Guid? ExplorareNomenclatorID { get; init; }
    public string DenumireExplorare { get; init; } = string.Empty;
    public string? CodExplorare { get; init; }
    public DateTime DataEfectuare { get; init; }
    public string? CentrulMedical { get; init; }
    public string? MedicExecutant { get; init; }
    public string? Rezultat { get; init; }
    public string? Concluzii { get; init; }
    public string? ParametriMasurati { get; init; }
    public string? CaleFisierRezultat { get; init; }
    public Guid CreatDe { get; init; }
}

public class AddExplorareEfectuataCommandHandler 
    : IRequestHandler<AddExplorareEfectuataCommand, Result<Guid>>
{
    private readonly IConsultatieExplorareEfectuataRepository _repository;
    private readonly IConsultatieExplorareRecomandataRepository _recomandataRepository;

    public AddExplorareEfectuataCommandHandler(
        IConsultatieExplorareEfectuataRepository repository,
        IConsultatieExplorareRecomandataRepository recomandataRepository)
    {
        _repository = repository;
        _recomandataRepository = recomandataRepository;
    }

    public async Task<Result<Guid>> Handle(AddExplorareEfectuataCommand request, CancellationToken cancellationToken)
    {
        var entity = new ConsultatieExplorareEfectuata
        {
            RecomandareID = request.RecomandareID,
            ConsultatieID = request.ConsultatieID,
            PacientID = request.PacientID,
            ExplorareNomenclatorID = request.ExplorareNomenclatorID,
            DenumireExplorare = request.DenumireExplorare,
            CodExplorare = request.CodExplorare,
            DataEfectuare = request.DataEfectuare,
            CentrulMedical = request.CentrulMedical,
            MedicExecutant = request.MedicExecutant,
            Rezultat = request.Rezultat,
            Concluzii = request.Concluzii,
            ParametriMasurati = request.ParametriMasurati,
            CaleFisierRezultat = request.CaleFisierRezultat,
            CreatDe = request.CreatDe
        };

        var id = await _repository.CreateAsync(entity, cancellationToken);

        // Actualizează statusul recomandării dacă există
        if (request.RecomandareID.HasValue)
        {
            await _recomandataRepository.UpdateStatusAsync(
                request.RecomandareID.Value, 
                "Efectuata", 
                request.CreatDe, 
                cancellationToken);
        }

        return Result<Guid>.Success(id);
    }
}

#endregion

#region Endoscopii Efectuate

/// <summary>
/// Comandă pentru adăugarea unei endoscopii efectuate
/// </summary>
public record AddEndoscopieEfectuataCommand : IRequest<Result<Guid>>
{
    public Guid? RecomandareID { get; init; }
    public Guid? ConsultatieID { get; init; }
    public Guid PacientID { get; init; }
    public Guid? EndoscopieNomenclatorID { get; init; }
    public string DenumireEndoscopie { get; init; } = string.Empty;
    public string? CodEndoscopie { get; init; }
    public DateTime DataEfectuare { get; init; }
    public string? CentrulMedical { get; init; }
    public string? MedicExecutant { get; init; }
    public string? Rezultat { get; init; }
    public string? Concluzii { get; init; }
    public string? BiopsiiPrelevate { get; init; }
    public string? RezultatHistopatologic { get; init; }
    public string? CaleFisierRezultat { get; init; }
    public Guid CreatDe { get; init; }
}

public class AddEndoscopieEfectuataCommandHandler 
    : IRequestHandler<AddEndoscopieEfectuataCommand, Result<Guid>>
{
    private readonly IConsultatieEndoscopieEfectuataRepository _repository;
    private readonly IConsultatieEndoscopieRecomandataRepository _recomandataRepository;

    public AddEndoscopieEfectuataCommandHandler(
        IConsultatieEndoscopieEfectuataRepository repository,
        IConsultatieEndoscopieRecomandataRepository recomandataRepository)
    {
        _repository = repository;
        _recomandataRepository = recomandataRepository;
    }

    public async Task<Result<Guid>> Handle(AddEndoscopieEfectuataCommand request, CancellationToken cancellationToken)
    {
        var entity = new ConsultatieEndoscopieEfectuata
        {
            RecomandareID = request.RecomandareID,
            ConsultatieID = request.ConsultatieID,
            PacientID = request.PacientID,
            EndoscopieNomenclatorID = request.EndoscopieNomenclatorID,
            DenumireEndoscopie = request.DenumireEndoscopie,
            CodEndoscopie = request.CodEndoscopie,
            DataEfectuare = request.DataEfectuare,
            CentrulMedical = request.CentrulMedical,
            MedicExecutant = request.MedicExecutant,
            Rezultat = request.Rezultat,
            Concluzii = request.Concluzii,
            BiopsiiPrelevate = request.BiopsiiPrelevate,
            RezultatHistopatologic = request.RezultatHistopatologic,
            CaleFisierRezultat = request.CaleFisierRezultat,
            CreatDe = request.CreatDe
        };

        var id = await _repository.CreateAsync(entity, cancellationToken);

        // Actualizează statusul recomandării dacă există
        if (request.RecomandareID.HasValue)
        {
            await _recomandataRepository.UpdateStatusAsync(
                request.RecomandareID.Value, 
                "Efectuata", 
                request.CreatDe, 
                cancellationToken);
        }

        return Result<Guid>.Success(id);
    }
}

#endregion
