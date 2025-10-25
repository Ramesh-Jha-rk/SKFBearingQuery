namespace Products.Application.DTOs;

/// <summary>
/// Response DTO for query results
/// </summary>
public class QueryResponse
{
    /// <summary>
    /// The natural language answer
    /// </summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if an error occurred
    /// </summary>
    public bool IsError { get; set; }

    /// <summary>
    /// The product designation identified
    /// </summary>
    public string? Product { get; set; }

    /// <summary>
    /// The attribute requested
    /// </summary>
    public string? Attribute { get; set; }

    /// <summary>
    /// The value found
 /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// URL to product image if available
    /// </summary>
    public string? ImageUrl { get; set; }
}
