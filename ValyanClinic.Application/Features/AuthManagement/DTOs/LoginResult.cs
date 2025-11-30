using System.Text.Json.Serialization;

namespace ValyanClinic.Application.Features.AuthManagement.DTOs;

/// <summary>
/// Response from JavaScript login API call.
/// Represents the result of an authentication attempt.
/// </summary>
/// <remarks>
/// JSON property names are lowercase to match JavaScript API response format.
/// This DTO is used for JavaScript interop between Blazor and the authentication API.
/// </remarks>
public class LoginResult
{
    /// <summary>
    /// Indicates if login was successful
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    /// <summary>
    /// Authenticated user data (null if login failed)
    /// </summary>
    [JsonPropertyName("data")]
    public LoginResponseData? Data { get; set; }
    
    /// <summary>
    /// Error message if login failed (null if successful)
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
