using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.PersonalManagement.Queries.GetPersonalById;

namespace ValyanClinic.Components.Pages.Administrare.Personal.Modals;

/// <summary>
/// Code-behind pentru PersonalViewModal - Modal read-only pentru vizualizare detalii personal
/// </summary>
public partial class PersonalViewModal : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<PersonalViewModal> Logger { get; set; } = default!;
    #endregion

    #region Parameters
    /// <summary>
    /// Event callback cand se solicita editare din modal
    /// </summary>
    [Parameter] public EventCallback<Guid> OnEditRequested { get; set; }

    /// <summary>
    /// Event callback cand se solicita stergere din modal
    /// </summary>
    [Parameter] public EventCallback<Guid> OnDeleteRequested { get; set; }

    /// <summary>
    /// Event callback cand modalul se inchide
    /// </summary>
    [Parameter] public EventCallback OnClosed { get; set; }
    #endregion

    #region State Properties
    /// <summary>
    /// Vizibilitatea modalului
    /// </summary>
    private bool IsVisible { get; set; }

    /// <summary>
    /// Indicator de incarcare date
    /// </summary>
    private bool IsLoading { get; set; }

    /// <summary>
    /// Indicator de eroare
    /// </summary>
    private bool HasError { get; set; }

    /// <summary>
    /// Mesaj de eroare
    /// </summary>
    private string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Datele personalului incarcate
    /// </summary>
    private PersonalDetailDto? PersonalData { get; set; }

    /// <summary>
    /// Tab-ul activ din modal
    /// </summary>
    private string ActiveTab { get; set; } = "personal";

    /// <summary>
    /// ID-ul personalului curent vizualizat
    /// </summary>
    private Guid CurrentPersonalId { get; set; }
    #endregion

    #region Public Methods
    /// <summary>
    /// Deschide modalul pentru vizualizare personal
    /// </summary>
    /// <param name="personalId">ID-ul personalului de vizualizat</param>
    public async Task Open(Guid personalId)
    {
        try
        {
            CurrentPersonalId = personalId;
            IsVisible = true;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;
            PersonalData = null;
            ActiveTab = "personal";

            await InvokeAsync(StateHasChanged);

            await LoadPersonalData(personalId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la deschiderea modalului pentru personalul {PersonalId}", personalId);
            HasError = true;
            ErrorMessage = $"Eroare la deschiderea modalului: {ex.Message}";
            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Inchide modalul
    /// </summary>
    public async Task Close()
    {
        IsVisible = false;
        await InvokeAsync(StateHasChanged);

        // Notify parent
        if (OnClosed.HasDelegate)
        {
            await OnClosed.InvokeAsync();
        }

        // Reset state after animation
        await Task.Delay(300);
        PersonalData = null;
        IsLoading = false;
        HasError = false;
        ErrorMessage = string.Empty;
        CurrentPersonalId = Guid.Empty;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Incarca datele personalului din backend
    /// </summary>
    private async Task LoadPersonalData(Guid personalId)
    {
        try
        {
            Logger.LogInformation("Incarca datele personalului {PersonalId} pentru vizualizare", personalId);

            var query = new GetPersonalByIdQuery(personalId);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                PersonalData = result.Value;
                HasError = false;
                Logger.LogInformation("Date personal incarcate cu succes pentru {PersonalId}", personalId);
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Errors?.FirstOrDefault() ?? "Nu s-au putut incarca datele personalului";
                Logger.LogWarning("Eroare la incarcarea datelor personalului {PersonalId}: {Error}", 
                    personalId, ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Eroare la incarcarea datelor: {ex.Message}";
            Logger.LogError(ex, "Exceptie la incarcarea datelor personalului {PersonalId}", personalId);
        }
        finally
        {
            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Seteaza tab-ul activ
    /// </summary>
    private void SetActiveTab(string tabName)
    {
        ActiveTab = tabName;
        Logger.LogDebug("Tab activ schimbat in: {TabName}", tabName);
    }

    /// <summary>
    /// Handler pentru click pe overlay (inchide modalul)
    /// </summary>
    private async Task HandleOverlayClick()
    {
        await Close();
    }

    /// <summary>
    /// Handler pentru butonul de editare
    /// </summary>
    private async Task HandleEdit()
    {
        if (PersonalData == null) return;

        Logger.LogInformation("Solicitare editare pentru personalul {PersonalId}", CurrentPersonalId);

        await Close();

        if (OnEditRequested.HasDelegate)
        {
            await OnEditRequested.InvokeAsync(CurrentPersonalId);
        }
    }

    /// <summary>
    /// Handler pentru butonul de stergere
    /// </summary>
    private async Task HandleDelete()
    {
        if (PersonalData == null) return;

        Logger.LogInformation("Solicitare stergere pentru personalul {PersonalId}", CurrentPersonalId);

        await Close();

        if (OnDeleteRequested.HasDelegate)
        {
            await OnDeleteRequested.InvokeAsync(CurrentPersonalId);
        }
    }

    /// <summary>
    /// Calculeaza varsta detaliata (ani, luni, zile) din data nasterii
    /// </summary>
    /// <param name="dataNasterii">Data nasterii persoanei</param>
    /// <returns>String formatat: "XX ani, XX luni, XX zile"</returns>
    private string CalculeazaVarstaDetaliata(DateTime dataNasterii)
    {
        var today = DateTime.Today;
        
        // Calculeaza ani
        int ani = today.Year - dataNasterii.Year;
        if (today.Month < dataNasterii.Month || 
            (today.Month == dataNasterii.Month && today.Day < dataNasterii.Day))
        {
            ani--;
        }
        
        // Calculeaza luni
        int luni = today.Month - dataNasterii.Month;
        if (luni < 0)
        {
            luni += 12;
        }
        if (today.Day < dataNasterii.Day)
        {
            luni--;
            if (luni < 0)
            {
                luni += 12;
            }
        }
        
        // Calculeaza zile
        int zile = today.Day - dataNasterii.Day;
        if (zile < 0)
        {
            // Luna anterioara
            var previousMonth = today.AddMonths(-1);
            zile += DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
        }
        
        // Formatare rezultat
        var parts = new List<string>();
        
        if (ani > 0)
        {
            parts.Add($"{ani} {(ani == 1 ? "an" : "ani")}");
        }
        
        if (luni > 0)
        {
            parts.Add($"{luni} {(luni == 1 ? "luna" : "luni")}");
        }
        
        if (zile > 0 || parts.Count == 0)
        {
            parts.Add($"{zile} {(zile == 1 ? "zi" : "zile")}");
        }
        
        return string.Join(", ", parts);
    }

    /// <summary>
    /// Calculeaza varsta detaliata (ani, luni, zile) din CNP
    /// </summary>
    /// <param name="cnp">CNP-ul persoanei (format: SAALLZZJJCCNNN)</param>
    /// <returns>String formatat: "XX ani, XX luni, XX zile" sau mesaj de eroare</returns>
    private string CalculeazaVarstaDetaliataFromCNP(string cnp)
    {
        // Validare format CNP
        if (string.IsNullOrWhiteSpace(cnp))
        {
            return "-";
        }

        if (cnp.Length != 13 || !cnp.All(char.IsDigit))
        {
            Logger.LogDebug("CNP invalid (lungime sau caractere): {CNP}", cnp);
            return "CNP invalid";
        }

        try
        {
            // Parse componente CNP (format: SAALLZZJJCCNNN)
            if (!int.TryParse(cnp.Substring(0, 1), out int sex) ||
                !int.TryParse(cnp.Substring(1, 2), out int an) ||
                !int.TryParse(cnp.Substring(3, 2), out int luna) ||
                !int.TryParse(cnp.Substring(5, 2), out int zi))
            {
                Logger.LogDebug("CNP cu format numeric invalid: {CNP}", cnp);
                return "CNP invalid";
            }

            // Validare cifra sex (1-9)
            if (sex < 1 || sex > 9)
            {
                Logger.LogDebug("Cifra sex invalida in CNP: {CNP}", cnp);
                return "CNP invalid";
            }

            // Validare luna (1-12)
            if (luna < 1 || luna > 12)
            {
                Logger.LogDebug("Luna invalida in CNP: {CNP} (luna={Luna})", cnp, luna);
                return "Data invalida";
            }

            // Validare zi (1-31, verificare per luna)
            int maxZileLuna = DateTime.DaysInMonth(2000, luna); // An bisect pentru max zile
            if (zi < 1 || zi > maxZileLuna)
            {
                Logger.LogDebug("Zi invalida in CNP: {CNP} (zi={Zi}, luna={Luna})", cnp, zi, luna);
                return "Data invalida";
            }

            // Determina secolul din cifra sex
            int secol = sex switch
            {
                1 or 2 => 1900,      // Nascuti 1900-1999
                3 or 4 => 1800,      // Nascuti 1800-1899
                5 or 6 => 2000,      // Nascuti 2000-2099
                7 or 8 => 2000,      // Rezidenti nascuti 2000-2099
                9 => 1900,           // Cetateni straini (conventie: 1900-1999)
                _ => 0
            };

            if (secol == 0)
            {
                Logger.LogWarning("Cifra sex necunoscuta in CNP: {CNP} (sex={Sex})", cnp, sex);
                return "CNP invalid";
            }

            // Construieste anul complet si data nasterii
            int anComplet = secol + an;
            
            // Validare an rezonabil (1850-2099)
            if (anComplet < 1850 || anComplet > 2099)
            {
                Logger.LogDebug("An nastere nerezonabil in CNP: {CNP} (an={An})", cnp, anComplet);
                return "Data invalida";
            }

            // Creeaza data nasterii cu validare
            DateTime dataNasterii;
            try
            {
                dataNasterii = new DateTime(anComplet, luna, zi);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Logger.LogDebug(ex, "Data invalida in CNP: {CNP} ({An}-{Luna}-{Zi})", 
                    cnp, anComplet, luna, zi);
                return "Data invalida";
            }

            // Validare data nu este in viitor
            if (dataNasterii > DateTime.Today)
            {
                Logger.LogDebug("Data nasterii in viitor in CNP: {CNP} ({DataNasterii})", 
                    cnp, dataNasterii.ToString("yyyy-MM-dd"));
                return "Data viitoare";
            }

            // Validare varsta rezonabila (0-150 ani)
            var varstaAni = DateTime.Today.Year - dataNasterii.Year;
            if (varstaAni < 0 || varstaAni > 150)
            {
                Logger.LogDebug("Varsta nerezonabila in CNP: {CNP} (varsta={Varsta})", cnp, varstaAni);
                return "Data invalida";
            }

            // Calcul varsta detaliata folosind metoda existenta
            return CalculeazaVarstaDetaliata(dataNasterii);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare neasteptata la calculul varstei din CNP: {CNP}", cnp);
            return "Eroare calcul";
        }
    }

    #region Expira In - Calcule si Stilizare

    // Constante pentru threshold-uri expirare
    private const int ZILE_EXPIRARE_CRITICA = 30;     // < 1 luna - rosu
    private const int ZILE_EXPIRARE_ATENTIE = 365;    // < 1 an - portocaliu
    // >= 365 zile - verde

    /// <summary>
    /// Calculeaza timpul ramas pana la expirare (ani, luni, zile)
    /// </summary>
    /// <param name="validabilPana">Data de expirare a documentului</param>
    /// <returns>String formatat: "XX ani, XX luni, XX zile" sau mesaj special</returns>
    private string CalculeazaExpiraIn(DateTime? validabilPana)
    {
        if (!validabilPana.HasValue)
        {
            return "Fara data";
        }

        var dataExpirare = validabilPana.Value;
        var today = DateTime.Today;

        // Verificare daca a expirat deja
        if (dataExpirare < today)
        {
            var timpTrecut = today - dataExpirare;
            int zileTrecute = (int)timpTrecut.TotalDays;

            Logger.LogDebug("Document expirat: Valabil_CI_Pana={DataExpirare}, Zile trecute={ZileTrecute}", 
                dataExpirare, zileTrecute);

            // Mesaje descriptive pentru documente expirate
            if (zileTrecute <= 7)
            {
                return $"Expirat acum {zileTrecute} {(zileTrecute == 1 ? "zi" : "zile")}";
            }
            else if (zileTrecute <= 30)
            {
                int saptamani = zileTrecute / 7;
                return $"Expirat acum {saptamani} {(saptamani == 1 ? "saptamana" : "saptamani")}";
            }
            else if (zileTrecute <= 365)
            {
                int luni = zileTrecute / 30;
                return $"Expirat acum {luni} {(luni == 1 ? "luna" : "luni")}";
            }
            else
            {
                int ani = zileTrecute / 365;
                return $"Expirat acum {ani} {(ani == 1 ? "an" : "ani")}";
            }
        }

        // Calculeaza timp ramas precis
        int aniRamasi = dataExpirare.Year - today.Year;
        int luniRamase = dataExpirare.Month - today.Month;
        int zileRamase = dataExpirare.Day - today.Day;

        // Ajustare calcule pentru valori negative
        if (zileRamase < 0)
        {
            luniRamase--;
            var previousMonth = dataExpirare.AddMonths(-1);
            zileRamase += DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
        }

        if (luniRamase < 0)
        {
            aniRamasi--;
            luniRamase += 12;
        }

        // Logging pentru monitoring
        int totalZileRamase = (int)(dataExpirare - today).TotalDays;
        if (totalZileRamase < ZILE_EXPIRARE_CRITICA)
        {
            Logger.LogWarning("Document expira in curand: Valabil_CI_Pana={DataExpirare}, Zile ramase={ZileRamase}", 
                dataExpirare, totalZileRamase);
        }

        // Formatare rezultat
        var parts = new List<string>();

        if (aniRamasi > 0)
        {
            parts.Add($"{aniRamasi} {(aniRamasi == 1 ? "an" : "ani")}");
        }

        if (luniRamase > 0)
        {
            parts.Add($"{luniRamase} {(luniRamase == 1 ? "luna" : "luni")}");
        }

        if (zileRamase > 0 || parts.Count == 0)
        {
            parts.Add($"{zileRamase} {(zileRamase == 1 ? "zi" : "zile")}");
        }

        return string.Join(", ", parts);
    }

    /// <summary>
    /// Determina clasa CSS pentru badge-ul Expira in functie de timpul ramas
    /// </summary>
    /// <param name="validabilPana">Data de expirare a documentului</param>
    /// <returns>Nume clasa CSS: badge-expira-verde, badge-expira-portocaliu, badge-expira-rosu, sau badge-expira-neutru</returns>
    private string GetExpiraInCssClass(DateTime? validabilPana)
    {
        if (!validabilPana.HasValue)
        {
            return "badge-expira-neutru";
        }

        var dataExpirare = validabilPana.Value;
        var today = DateTime.Today;

        // Expirat - rosu intens
        if (dataExpirare < today)
        {
            return "badge-expira-rosu";
        }

        // Calculeaza zile ramase
        int zileRamase = (int)(dataExpirare - today).TotalDays;

        // Rosu: < 30 zile (1 luna)
        if (zileRamase < ZILE_EXPIRARE_CRITICA)
        {
            return "badge-expira-rosu";
        }

        // Portocaliu: < 365 zile (1 an) dar > 30 zile
        if (zileRamase < ZILE_EXPIRARE_ATENTIE)
        {
            return "badge-expira-portocaliu";
        }

        // Verde: >= 365 zile (peste 1 an)
        return "badge-expira-verde";
    }

    #endregion
    #endregion
}
