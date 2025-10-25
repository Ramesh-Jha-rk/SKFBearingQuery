namespace Products.Domain.Interfaces;

/// <summary>
/// Service interface for AI-powered text extraction
/// </summary>
public interface IAiExtractionService
{
    /// <summary>
    /// Extract product designation from user query
    /// </summary>
    Task<string> ExtractProductDesignationAsync(string userQuery);

    /// <summary>
    /// Extract attribute name from user query
    /// </summary>
  Task<string> ExtractAttributeAsync(string userQuery);
}
