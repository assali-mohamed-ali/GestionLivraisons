namespace Livreur.API.Factories;

// SRP: separate concern — only validation result
public class ValidationResult
{
    public bool IsValid { get; private init; }
    public IReadOnlyList<string> Errors { get; private init; } = [];

    public static ValidationResult Success() => new() { IsValid = true, Errors = [] };
    public static ValidationResult Failure(params string[] errors) => new() { IsValid = false, Errors = errors };
}
