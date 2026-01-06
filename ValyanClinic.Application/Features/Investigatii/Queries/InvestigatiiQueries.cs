using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.DTOs.Investigatii;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.Investigatii.Queries;

#region Nomenclatoare Queries

/// <summary>
/// Query pentru obținerea tuturor investigațiilor imagistice din nomenclator
/// </summary>
public record GetNomenclatorInvestigatiiImagisticeQuery : IRequest<Result<IEnumerable<NomenclatorInvestigatieImagisticaDto>>>;

public class GetNomenclatorInvestigatiiImagisticeQueryHandler 
    : IRequestHandler<GetNomenclatorInvestigatiiImagisticeQuery, Result<IEnumerable<NomenclatorInvestigatieImagisticaDto>>>
{
    private readonly INomenclatorInvestigatiiImagisticeRepository _repository;

    public GetNomenclatorInvestigatiiImagisticeQueryHandler(INomenclatorInvestigatiiImagisticeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<NomenclatorInvestigatieImagisticaDto>>> Handle(
        GetNomenclatorInvestigatiiImagisticeQuery request, 
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllActiveAsync(cancellationToken);
        
        var dtos = items.Select(i => new NomenclatorInvestigatieImagisticaDto
        {
            Id = i.Id,
            Cod = i.Cod,
            Denumire = i.Denumire,
            Categorie = i.Categorie,
            Descriere = i.Descriere
        });

        return Result<IEnumerable<NomenclatorInvestigatieImagisticaDto>>.Success(dtos);
    }
}

/// <summary>
/// Query pentru obținerea tuturor explorărilor funcționale din nomenclator
/// </summary>
public record GetNomenclatorExplorariFuncQuery : IRequest<Result<IEnumerable<NomenclatorExplorareFuncDto>>>;

public class GetNomenclatorExplorariFuncQueryHandler 
    : IRequestHandler<GetNomenclatorExplorariFuncQuery, Result<IEnumerable<NomenclatorExplorareFuncDto>>>
{
    private readonly INomenclatorExplorariFuncRepository _repository;

    public GetNomenclatorExplorariFuncQueryHandler(INomenclatorExplorariFuncRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<NomenclatorExplorareFuncDto>>> Handle(
        GetNomenclatorExplorariFuncQuery request, 
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllActiveAsync(cancellationToken);
        
        var dtos = items.Select(i => new NomenclatorExplorareFuncDto
        {
            Id = i.Id,
            Cod = i.Cod,
            Denumire = i.Denumire,
            Categorie = i.Categorie,
            Descriere = i.Descriere
        });

        return Result<IEnumerable<NomenclatorExplorareFuncDto>>.Success(dtos);
    }
}

/// <summary>
/// Query pentru obținerea tuturor endoscopiilor din nomenclator
/// </summary>
public record GetNomenclatorEndoscopiiQuery : IRequest<Result<IEnumerable<NomenclatorEndoscopieDto>>>;

public class GetNomenclatorEndoscopiiQueryHandler 
    : IRequestHandler<GetNomenclatorEndoscopiiQuery, Result<IEnumerable<NomenclatorEndoscopieDto>>>
{
    private readonly INomenclatorEndoscopiiRepository _repository;

    public GetNomenclatorEndoscopiiQueryHandler(INomenclatorEndoscopiiRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<NomenclatorEndoscopieDto>>> Handle(
        GetNomenclatorEndoscopiiQuery request, 
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllActiveAsync(cancellationToken);
        
        var dtos = items.Select(i => new NomenclatorEndoscopieDto
        {
            Id = i.Id,
            Cod = i.Cod,
            Denumire = i.Denumire,
            Categorie = i.Categorie,
            Descriere = i.Descriere
        });

        return Result<IEnumerable<NomenclatorEndoscopieDto>>.Success(dtos);
    }
}

#endregion

#region Recomandate Queries

/// <summary>
/// Query pentru obținerea investigațiilor imagistice recomandate pentru o consultație
/// </summary>
public record GetInvestigatiiImagisticeRecomandateByConsultatieQuery(Guid ConsultatieId) 
    : IRequest<Result<IEnumerable<InvestigatieImagisticaRecomandataDto>>>;

public class GetInvestigatiiImagisticeRecomandateByConsultatieQueryHandler 
    : IRequestHandler<GetInvestigatiiImagisticeRecomandateByConsultatieQuery, Result<IEnumerable<InvestigatieImagisticaRecomandataDto>>>
{
    private readonly IConsultatieInvestigatieImagisticaRecomandataRepository _repository;

    public GetInvestigatiiImagisticeRecomandateByConsultatieQueryHandler(IConsultatieInvestigatieImagisticaRecomandataRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<InvestigatieImagisticaRecomandataDto>>> Handle(
        GetInvestigatiiImagisticeRecomandateByConsultatieQuery request, 
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetByConsultatieIdAsync(request.ConsultatieId, cancellationToken);
        
        var dtos = items.Select(i => new InvestigatieImagisticaRecomandataDto
        {
            Id = i.Id,
            ConsultatieID = i.ConsultatieID,
            InvestigatieNomenclatorID = i.InvestigatieNomenclatorID,
            DenumireInvestigatie = i.DenumireInvestigatie,
            CodInvestigatie = i.CodInvestigatie,
            RegiuneAnatomica = i.RegiuneAnatomica,
            DataRecomandare = i.DataRecomandare,
            Prioritate = i.Prioritate,
            EsteCito = i.EsteCito,
            IndicatiiClinice = i.IndicatiiClinice,
            ObservatiiMedic = i.ObservatiiMedic,
            Status = i.Status
        });

        return Result<IEnumerable<InvestigatieImagisticaRecomandataDto>>.Success(dtos);
    }
}

/// <summary>
/// Query pentru obținerea explorărilor funcționale recomandate pentru o consultație
/// </summary>
public record GetExplorariRecomandateByConsultatieQuery(Guid ConsultatieId) 
    : IRequest<Result<IEnumerable<ExplorareRecomandataDto>>>;

public class GetExplorariRecomandateByConsultatieQueryHandler 
    : IRequestHandler<GetExplorariRecomandateByConsultatieQuery, Result<IEnumerable<ExplorareRecomandataDto>>>
{
    private readonly IConsultatieExplorareRecomandataRepository _repository;

    public GetExplorariRecomandateByConsultatieQueryHandler(IConsultatieExplorareRecomandataRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<ExplorareRecomandataDto>>> Handle(
        GetExplorariRecomandateByConsultatieQuery request, 
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetByConsultatieIdAsync(request.ConsultatieId, cancellationToken);
        
        var dtos = items.Select(i => new ExplorareRecomandataDto
        {
            Id = i.Id,
            ConsultatieID = i.ConsultatieID,
            ExplorareNomenclatorID = i.ExplorareNomenclatorID,
            DenumireExplorare = i.DenumireExplorare,
            CodExplorare = i.CodExplorare,
            DataRecomandare = i.DataRecomandare,
            Prioritate = i.Prioritate,
            EsteCito = i.EsteCito,
            IndicatiiClinice = i.IndicatiiClinice,
            ObservatiiMedic = i.ObservatiiMedic,
            Status = i.Status
        });

        return Result<IEnumerable<ExplorareRecomandataDto>>.Success(dtos);
    }
}

/// <summary>
/// Query pentru obținerea endoscopiilor recomandate pentru o consultație
/// </summary>
public record GetEndoscopiiRecomandateByConsultatieQuery(Guid ConsultatieId) 
    : IRequest<Result<IEnumerable<EndoscopieRecomandataDto>>>;

public class GetEndoscopiiRecomandateByConsultatieQueryHandler 
    : IRequestHandler<GetEndoscopiiRecomandateByConsultatieQuery, Result<IEnumerable<EndoscopieRecomandataDto>>>
{
    private readonly IConsultatieEndoscopieRecomandataRepository _repository;

    public GetEndoscopiiRecomandateByConsultatieQueryHandler(IConsultatieEndoscopieRecomandataRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<EndoscopieRecomandataDto>>> Handle(
        GetEndoscopiiRecomandateByConsultatieQuery request, 
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetByConsultatieIdAsync(request.ConsultatieId, cancellationToken);
        
        var dtos = items.Select(i => new EndoscopieRecomandataDto
        {
            Id = i.Id,
            ConsultatieID = i.ConsultatieID,
            EndoscopieNomenclatorID = i.EndoscopieNomenclatorID,
            DenumireEndoscopie = i.DenumireEndoscopie,
            CodEndoscopie = i.CodEndoscopie,
            DataRecomandare = i.DataRecomandare,
            Prioritate = i.Prioritate,
            EsteCito = i.EsteCito,
            IndicatiiClinice = i.IndicatiiClinice,
            ObservatiiMedic = i.ObservatiiMedic,
            Status = i.Status
        });

        return Result<IEnumerable<EndoscopieRecomandataDto>>.Success(dtos);
    }
}

#endregion

#region Efectuate Queries

/// <summary>
/// Query pentru obținerea investigațiilor imagistice efectuate pentru un pacient
/// </summary>
public record GetInvestigatiiImagisticeEfectuateByPacientQuery(Guid PacientId) 
    : IRequest<Result<IEnumerable<InvestigatieImagisticaEfectuataDto>>>;

public class GetInvestigatiiImagisticeEfectuateByPacientQueryHandler 
    : IRequestHandler<GetInvestigatiiImagisticeEfectuateByPacientQuery, Result<IEnumerable<InvestigatieImagisticaEfectuataDto>>>
{
    private readonly IConsultatieInvestigatieImagisticaEfectuataRepository _repository;

    public GetInvestigatiiImagisticeEfectuateByPacientQueryHandler(IConsultatieInvestigatieImagisticaEfectuataRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<InvestigatieImagisticaEfectuataDto>>> Handle(
        GetInvestigatiiImagisticeEfectuateByPacientQuery request, 
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetByPacientIdAsync(request.PacientId, cancellationToken);
        
        var dtos = items.Select(i => new InvestigatieImagisticaEfectuataDto
        {
            Id = i.Id,
            RecomandareID = i.RecomandareID,
            ConsultatieID = i.ConsultatieID,
            PacientID = i.PacientID,
            InvestigatieNomenclatorID = i.InvestigatieNomenclatorID,
            DenumireInvestigatie = i.DenumireInvestigatie,
            CodInvestigatie = i.CodInvestigatie,
            RegiuneAnatomica = i.RegiuneAnatomica,
            DataEfectuare = i.DataEfectuare,
            CentrulMedical = i.CentrulMedical,
            MedicExecutant = i.MedicExecutant,
            Rezultat = i.Rezultat,
            Concluzii = i.Concluzii,
            CaleFisierRezultat = i.CaleFisierRezultat
        });

        return Result<IEnumerable<InvestigatieImagisticaEfectuataDto>>.Success(dtos);
    }
}

/// <summary>
/// Query pentru obținerea explorărilor funcționale efectuate pentru un pacient
/// </summary>
public record GetExplorariEfectuateByPacientQuery(Guid PacientId) 
    : IRequest<Result<IEnumerable<ExplorareEfectuataDto>>>;

public class GetExplorariEfectuateByPacientQueryHandler 
    : IRequestHandler<GetExplorariEfectuateByPacientQuery, Result<IEnumerable<ExplorareEfectuataDto>>>
{
    private readonly IConsultatieExplorareEfectuataRepository _repository;

    public GetExplorariEfectuateByPacientQueryHandler(IConsultatieExplorareEfectuataRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<ExplorareEfectuataDto>>> Handle(
        GetExplorariEfectuateByPacientQuery request, 
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetByPacientIdAsync(request.PacientId, cancellationToken);
        
        var dtos = items.Select(i => new ExplorareEfectuataDto
        {
            Id = i.Id,
            RecomandareID = i.RecomandareID,
            ConsultatieID = i.ConsultatieID,
            PacientID = i.PacientID,
            ExplorareNomenclatorID = i.ExplorareNomenclatorID,
            DenumireExplorare = i.DenumireExplorare,
            CodExplorare = i.CodExplorare,
            DataEfectuare = i.DataEfectuare,
            CentrulMedical = i.CentrulMedical,
            MedicExecutant = i.MedicExecutant,
            Rezultat = i.Rezultat,
            Concluzii = i.Concluzii,
            ParametriMasurati = i.ParametriMasurati,
            CaleFisierRezultat = i.CaleFisierRezultat
        });

        return Result<IEnumerable<ExplorareEfectuataDto>>.Success(dtos);
    }
}

/// <summary>
/// Query pentru obținerea endoscopiilor efectuate pentru un pacient
/// </summary>
public record GetEndoscopiiEfectuateByPacientQuery(Guid PacientId) 
    : IRequest<Result<IEnumerable<EndoscopieEfectuataDto>>>;

public class GetEndoscopiiEfectuateByPacientQueryHandler 
    : IRequestHandler<GetEndoscopiiEfectuateByPacientQuery, Result<IEnumerable<EndoscopieEfectuataDto>>>
{
    private readonly IConsultatieEndoscopieEfectuataRepository _repository;

    public GetEndoscopiiEfectuateByPacientQueryHandler(IConsultatieEndoscopieEfectuataRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<EndoscopieEfectuataDto>>> Handle(
        GetEndoscopiiEfectuateByPacientQuery request, 
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetByPacientIdAsync(request.PacientId, cancellationToken);
        
        var dtos = items.Select(i => new EndoscopieEfectuataDto
        {
            Id = i.Id,
            RecomandareID = i.RecomandareID,
            ConsultatieID = i.ConsultatieID,
            PacientID = i.PacientID,
            EndoscopieNomenclatorID = i.EndoscopieNomenclatorID,
            DenumireEndoscopie = i.DenumireEndoscopie,
            CodEndoscopie = i.CodEndoscopie,
            DataEfectuare = i.DataEfectuare,
            CentrulMedical = i.CentrulMedical,
            MedicExecutant = i.MedicExecutant,
            Rezultat = i.Rezultat,
            Concluzii = i.Concluzii,
            BiopsiiPrelevate = i.BiopsiiPrelevate,
            RezultatHistopatologic = i.RezultatHistopatologic,
            CaleFisierRezultat = i.CaleFisierRezultat
        });

        return Result<IEnumerable<EndoscopieEfectuataDto>>.Success(dtos);
    }
}

#endregion
