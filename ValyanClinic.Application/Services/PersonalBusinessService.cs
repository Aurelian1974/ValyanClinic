using Microsoft.Extensions.Logging;

namespace ValyanClinic.Application.Services;

/// <summary>
/// Implementare serviciu de business logic pentru Personal
/// Contine logica de calcul varsta, validare CNP si verificari expirare documente
/// </summary>
public class PersonalBusinessService : IPersonalBusinessService
{
    private readonly ILogger<PersonalBusinessService> _logger;

    // Constante pentru threshold-uri expirare
    private const int ZILE_EXPIRARE_CRITICA = 30;     // < 1 luna - rosu
    private const int ZILE_EXPIRARE_ATENTIE = 365;    // < 1 an - portocaliu

    public PersonalBusinessService(ILogger<PersonalBusinessService> logger)
    {
        _logger = logger;
    }

    public string CalculeazaVarsta(DateTime dataNasterii)
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

    public string CalculeazaVarstaFromCNP(string cnp)
    {
        var dataNasterii = ExtractDataNasteriiFromCNP(cnp);
        
        if (dataNasterii == null)
        {
            _logger.LogDebug("CNP invalid pentru calculul varstei: {CNP}", cnp);
            return "CNP invalid";
        }
        
        return CalculeazaVarsta(dataNasterii.Value);
    }

    public (bool IsValid, string ErrorMessage) ValidateCNP(string cnp)
    {
        // Validare format
        if (string.IsNullOrWhiteSpace(cnp))
        {
            return (false, "CNP lipseste");
        }

        if (cnp.Length != 13 || !cnp.All(char.IsDigit))
        {
            return (false, "CNP trebuie sa contina exact 13 cifre");
        }

        // Parse componente
        if (!int.TryParse(cnp.Substring(0, 1), out int sex) ||
            !int.TryParse(cnp.Substring(1, 2), out int an) ||
            !int.TryParse(cnp.Substring(3, 2), out int luna) ||
            !int.TryParse(cnp.Substring(5, 2), out int zi))
        {
            return (false, "Format numeric CNP invalid");
        }

        // Validare cifra sex
        if (sex < 1 || sex > 9)
        {
            return (false, "Cifra sex invalida");
        }

        // Validare luna
        if (luna < 1 || luna > 12)
        {
            return (false, "Luna invalida in CNP");
        }

        // Validare zi
        int maxZileLuna = DateTime.DaysInMonth(2000, luna);
        if (zi < 1 || zi > maxZileLuna)
        {
            return (false, "Zi invalida in CNP");
        }

        // Determina secolul
        int secol = sex switch
        {
            1 or 2 => 1900,
            3 or 4 => 1800,
            5 or 6 => 2000,
            7 or 8 => 2000,
            9 => 1900,
            _ => 0
        };

        if (secol == 0)
        {
            return (false, "Cifra sex necunoscuta");
        }

        int anComplet = secol + an;

        // Validare an rezonabil
        if (anComplet < 1850 || anComplet > 2099)
        {
            return (false, "An nastere nerezonabil");
        }

        // Validare data poate fi construita
        try
        {
            var dataNasterii = new DateTime(anComplet, luna, zi);
            
            if (dataNasterii > DateTime.Today)
            {
                return (false, "Data nasterii in viitor");
            }

            var varstaAni = DateTime.Today.Year - dataNasterii.Year;
            if (varstaAni < 0 || varstaAni > 150)
            {
                return (false, "Varsta nerezonabila");
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            return (false, "Data invalida in CNP");
        }

        return (true, string.Empty);
    }

    public DateTime? ExtractDataNasteriiFromCNP(string cnp)
    {
        var validationResult = ValidateCNP(cnp);
        if (!validationResult.IsValid)
        {
            return null;
        }

        // Parse componente (validated above)
        int sex = int.Parse(cnp.Substring(0, 1));
        int an = int.Parse(cnp.Substring(1, 2));
        int luna = int.Parse(cnp.Substring(3, 2));
        int zi = int.Parse(cnp.Substring(5, 2));

        // Determina secolul
        int secol = sex switch
        {
            1 or 2 => 1900,
            3 or 4 => 1800,
            5 or 6 or 7 or 8 => 2000,
            9 => 1900,
            _ => 0
        };

        int anComplet = secol + an;

        try
        {
            return new DateTime(anComplet, luna, zi);
        }
        catch
        {
            return null;
        }
    }

    public string CalculeazaExpiraIn(DateTime? dataExpirare)
    {
        if (!dataExpirare.HasValue)
        {
            return "Fara data";
        }

        var expirare = dataExpirare.Value;
        var today = DateTime.Today;

        // Verificare daca a expirat
        if (expirare < today)
        {
            var timpTrecut = today - expirare;
            int zileTrecute = (int)timpTrecut.TotalDays;

            _logger.LogDebug("Document expirat: DataExpirare={DataExpirare}, ZileTrecute={ZileTrecute}", 
                expirare, zileTrecute);

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

        // Calculeaza timp ramas
        int aniRamasi = expirare.Year - today.Year;
        int luniRamase = expirare.Month - today.Month;
        int zileRamase = expirare.Day - today.Day;

        // Ajustare calcule
        if (zileRamase < 0)
        {
            luniRamase--;
            var previousMonth = expirare.AddMonths(-1);
            zileRamase += DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
        }

        if (luniRamase < 0)
        {
            aniRamasi--;
            luniRamase += 12;
        }

        // Logging pentru monitoring
        int totalZileRamase = (int)(expirare - today).TotalDays;
        if (totalZileRamase < ZILE_EXPIRARE_CRITICA)
        {
            _logger.LogWarning("Document expira in curand: DataExpirare={DataExpirare}, ZileRamase={ZileRamase}", 
                expirare, totalZileRamase);
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

    public string GetExpiraCssClass(DateTime? dataExpirare)
    {
        if (!dataExpirare.HasValue)
        {
            return "badge-expira-neutru";
        }

        var expirare = dataExpirare.Value;
        var today = DateTime.Today;

        // Expirat - rosu intens
        if (expirare < today)
        {
            return "badge-expira-rosu";
        }

        // Calculeaza zile ramase
        int zileRamase = (int)(expirare - today).TotalDays;

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
}
