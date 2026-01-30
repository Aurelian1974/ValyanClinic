using System;
using System.Collections.Generic;

namespace ValyanClinic.Domain.Models;

public class DraftResult<T> where T : class
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public DateTime? SavedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DraftErrorType ErrorType { get; set; }

    public static DraftResult<T> Success(T data, DateTime savedAt) => new() { IsSuccess = true, Data = data, SavedAt = savedAt, ErrorType = DraftErrorType.None };
    public static DraftResult<T> NotFound => new() { IsSuccess = false, ErrorMessage = "Draft not found", ErrorType = DraftErrorType.NotFound };
    public static DraftResult<T> Invalid => new() { IsSuccess = false, ErrorMessage = "Invalid draft data", ErrorType = DraftErrorType.InvalidData };
    public static DraftResult<T> Expired => new() { IsSuccess = false, ErrorMessage = "Draft has expired", ErrorType = DraftErrorType.Expired };
    public static DraftResult<T> Error(string message) => new() { IsSuccess = false, ErrorMessage = message, ErrorType = DraftErrorType.Unknown };
}

public enum DraftErrorType
{
    None,
    NotFound,
    InvalidData,
    Expired,
    StorageFull,
    Unknown
}
