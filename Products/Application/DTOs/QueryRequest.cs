namespace Products.Application.DTOs;

/// <summary>
/// Request DTO for querying bearing information
/// </summary>
public class QueryRequest
{
    /// <summary>
    /// The natural language query
    /// </summary>
    public string Query { get; set; } = string.Empty;
}
