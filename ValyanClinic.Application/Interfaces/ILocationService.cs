using ValyanClinic.Domain.Models;

namespace ValyanClinic.Application.Interfaces;

/// <summary>
/// Interface simplificată pentru serviciul de locații
/// </summary>
public interface ILocationService
{
    Task<IEnumerable<Judet>> GetAllJudeteAsync();
    Task<IEnumerable<Localitate>> GetLocalitatiByJudetIdAsync(int judetId);
    Task<Judet?> GetJudetByNameAsync(string nume);
    Task<Localitate?> GetLocalitateByNameAndJudetAsync(string nume, int judetId);
}

/// <summary>
/// Model simplificat pentru opțiuni dropdown
/// </summary>
public record LocationOption(int Id, string Name)
{
    public static implicit operator LocationOption((int Id, string Name) tuple) 
        => new(tuple.Id, tuple.Name);
    
    public override string ToString() => Name;
}

/// <summary>
/// Clasa pentru opțiunile dropdown - versiune simplificată
/// </summary>
public class DropdownOption<T>
{
    public T Value { get; set; } = default!;
    public string Text { get; set; } = string.Empty;
    public bool IsSelected { get; set; } = false;

    public DropdownOption() { }

    public DropdownOption(T value, string text, bool isSelected = false)
    {
        Value = value;
        Text = text;
        IsSelected = isSelected;
    }
}
