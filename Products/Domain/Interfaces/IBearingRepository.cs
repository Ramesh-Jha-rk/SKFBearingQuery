using Products.Domain.Entities;

namespace Products.Domain.Interfaces;

/// <summary>
/// Repository interface for bearing data access
/// </summary>
public interface IBearingRepository
{
    /// <summary>
    /// Get a bearing by its designation
  /// </summary>
    Bearing? GetByDesignation(string designation);

    /// <summary>
    /// Get all bearing designations
    /// </summary>
    IEnumerable<string> GetAllDesignations();

    /// <summary>
    /// Get all bearings
    /// </summary>
    IEnumerable<Bearing> GetAll();
}
