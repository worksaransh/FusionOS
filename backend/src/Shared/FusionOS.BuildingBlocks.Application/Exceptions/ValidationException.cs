using FluentValidation.Results;

namespace FusionOS.BuildingBlocks.Application.Exceptions;

/// <summary>
/// Thrown by ValidationBehavior; mapped to an RFC 7807 problem response at the API
/// boundary per 08_API_STANDARDS.md §6. Never caught and swallowed inside handlers.
/// </summary>
public sealed class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures) : base("One or more validation failures occurred.")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());
    }
}
