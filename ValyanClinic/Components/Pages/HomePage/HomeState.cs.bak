namespace ValyanClinic.Components.Pages.HomePage;

/// <summary>
/// State Management pentru Home page - ORGANIZAT ÎN FOLDER HomePage
/// </summary>
public class HomeState
{
    public bool ShowWelcome { get; set; } = true;
    public bool IsLoading { get; set; } = true;
    public string? LoadingError { get; set; }
    public string? LastError { get; set; }
    
    public bool HasError => !string.IsNullOrEmpty(LoadingError) || !string.IsNullOrEmpty(LastError);
    public string? CurrentError => LoadingError ?? LastError;
    
    public void ClearErrors()
    {
        LoadingError = null;
        LastError = null;
    }
    
    public void SetError(string error)
    {
        LastError = error;
    }
    
    public void SetLoadingError(string error)
    {
        LoadingError = error;
    }
}