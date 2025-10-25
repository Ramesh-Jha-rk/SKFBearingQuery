using Products.Application.DTOs;

namespace Products.Application.Interfaces;

/// <summary>
/// Use case interface for processing bearing queries
/// </summary>
public interface IProcessQueryUseCase
{
    /// <summary>
    /// Process a natural language query about bearings
    /// </summary>
  Task<QueryResponse> ExecuteAsync(string userQuery);
}
