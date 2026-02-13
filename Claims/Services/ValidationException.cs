namespace Claims.Services;

/// <summary>
/// Thrown when a business validation rule is violated.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Per-field validation errors. Key is the field name; value is the error message.
    /// </summary>
    public IDictionary<string, string> Errors { get; }

    public ValidationException(IDictionary<string, string> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string field, string message)
        : this(new Dictionary<string, string> { { field, message } })
    {
    }
}
