using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Inputs;
using ValyanClinic.Application.Features.Analize.Models;
using ValyanClinic.Application.Services.Analize;

namespace ValyanClinic.Components.Shared.Consultatie;

public partial class ImportAnalizePdfModal : ComponentBase, IDisposable
{
    [Inject] private IAnalizePdfParserService ParserService { get; set; } = default!;
    
    [Parameter] public Guid ConsultatieId { get; set; }
    [Parameter] public EventCallback<List<AnalizaImportDto>> OnImportComplete { get; set; }
    
    private bool _isVisible;
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible != value)
            {
                _isVisible = value;
                if (value)
                {
                    _ = LoadLaboratoareAsync();
                }
            }
        }
    }
    
    private SfUploader? _uploader;
    private List<LaboratorInfo> _laboratoare = new();
    private string? _selectedLaborator;
    private UploadFiles? _selectedFile;
    private ParsePdfResult? _parseResult;
    private bool _isParsing;
    
    private bool CanParse => !string.IsNullOrEmpty(_selectedLaborator) && 
                             _selectedFile != null && 
                             !_isParsing;
    
    private bool CanImport => _parseResult?.Success == true && 
                              _parseResult.Analize.Any();
    
    public void Show()
    {
        IsVisible = true;
        StateHasChanged();
    }
    
    public async Task ShowAsync()
    {
        IsVisible = true;
        await InvokeAsync(StateHasChanged);
    }
    
    public void Close()
    {
        IsVisible = false;
        Reset();
        StateHasChanged();
    }
    
    public async Task CloseAsync()
    {
        IsVisible = false;
        Reset();
        await InvokeAsync(StateHasChanged);
    }
    
    private void Reset()
    {
        _selectedLaborator = null;
        _selectedFile = null;
        _parseResult = null;
        _isParsing = false;
    }
    
    private async Task LoadLaboratoareAsync()
    {
        try
        {
            _laboratoare = await ParserService.GetLaboratoareAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Eroare încărcare laboratoare: {ex.Message}");
        }
    }
    
    private void OnFileSelected(UploadChangeEventArgs args)
    {
        _selectedFile = args.Files?.FirstOrDefault();
        _parseResult = null;
    }
    
    private async Task ParsePdf()
    {
        if (_selectedFile?.File == null || string.IsNullOrEmpty(_selectedLaborator))
            return;
        
        _isParsing = true;
        StateHasChanged();
        
        try
        {
            var stream = _selectedFile.File.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB max
            _parseResult = await ParserService.ParsePdfAsync(
                stream, 
                _selectedFile.FileInfo.Name, 
                _selectedLaborator
            );
        }
        catch (Exception ex)
        {
            _parseResult = new ParsePdfResult
            {
                Success = false,
                Warnings = new List<string> { $"Eroare: {ex.Message}" }
            };
        }
        finally
        {
            _isParsing = false;
            StateHasChanged();
        }
    }
    
    private async Task ImportAnalize()
    {
        if (_parseResult?.Analize == null || !_parseResult.Analize.Any())
            return;
        
        // Convertim în format import
        var importData = _parseResult.Analize.Select(a => new AnalizaImportDto
        {
            NumeAnaliza = a.NumeAnaliza,
            CodAnaliza = a.CodAnaliza,
            TipAnaliza = a.Categorie,
            Valoare = a.Rezultat,
            ValoareNumerica = a.RezultatNumeric,
            UnitatiMasura = a.UnitateMasura,
            ValoareNormalaMin = a.IntervalMin,
            ValoareNormalaMax = a.IntervalMax,
            ValoareNormalaText = a.IntervalText,
            EsteInAfaraLimitelor = a.EsteAnormal,
            DirectieAnormal = a.DirectieAnormal,
            DataRecoltare = _parseResult.DataRecoltare,
            Laborator = _parseResult.Laborator,
            NumarBuletin = _parseResult.NumarBuletin
        }).ToList();
        
        await OnImportComplete.InvokeAsync(importData);
        Close();
    }
    
    public void Dispose()
    {
        // Cleanup
    }
}
