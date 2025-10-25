namespace Products.Domain.Entities;

/// <summary>
/// Domain entity representing a bearing product
/// </summary>
public class Bearing
{
    public string Designation { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<Dimension> Dimensions { get; set; } = new();
    public List<Property> Properties { get; set; } = new();
    public List<Performance> PerformanceData { get; set; } = new();
    public List<Specification> Specifications { get; set; } = new();
  public List<Logistics> LogisticsData { get; set; } = new();

    /// <summary>
    /// Find a dimension by name or symbol
    /// </summary>
    public Dimension? FindDimension(string nameOrSymbol)
    {
  return Dimensions.FirstOrDefault(d =>
            string.Equals(d.Name, nameOrSymbol, StringComparison.OrdinalIgnoreCase) ||
   string.Equals(d.Symbol, nameOrSymbol, StringComparison.OrdinalIgnoreCase) ||
            d.Name.Contains(nameOrSymbol, StringComparison.OrdinalIgnoreCase));
    }

  /// <summary>
    /// Find a property by name
    /// </summary>
    public Property? FindProperty(string name)
    {
        return Properties.FirstOrDefault(p =>
 string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase) ||
          p.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Find performance data by name
    /// </summary>
    public Performance? FindPerformance(string name)
    {
        return PerformanceData.FirstOrDefault(p =>
            string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase) ||
     p.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Find specification by name
 /// </summary>
    public Specification? FindSpecification(string name)
    {
        return Specifications.FirstOrDefault(s =>
          string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase) ||
         s.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Find logistics data by name
    /// </summary>
    public Logistics? FindLogistics(string name)
    {
     Console.WriteLine($"[DEBUG] FindLogistics searching for '{name}' in {LogisticsData.Count} items");
        foreach (var item in LogisticsData)
        {
            Console.WriteLine($"[DEBUG] Checking logistics item: '{item.Name}'");
  if (string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase))
            {
            Console.WriteLine($"[DEBUG] Exact match found!");
 return item;
         }
   if (item.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
        {
                Console.WriteLine($"[DEBUG] Contains match found!");
        return item;
            }
 }
        Console.WriteLine($"[DEBUG] No match found in logistics");
        return null;
    }

    /// <summary>
    /// Search for any attribute across all data
    /// </summary>
    public (string? AttributeName, string? Value) FindAttribute(string searchTerm)
    {
        Console.WriteLine($"[DEBUG] Bearing.FindAttribute called for '{Designation}' with search term: '{searchTerm}'");
  
  // Search dimensions
  var dimension = FindDimension(searchTerm);
        if (dimension != null)
        {
   Console.WriteLine($"[DEBUG] Found in Dimensions: {dimension.Name}");
            var value = dimension.Unit != null ? $"{dimension.Value}{dimension.Unit}" : dimension.Value.ToString();
        return (dimension.Name, value);
        }
        Console.WriteLine($"[DEBUG] Not found in Dimensions");

        // Search properties
 var property = FindProperty(searchTerm);
if (property != null)
        {
            Console.WriteLine($"[DEBUG] Found in Properties: {property.Name}");
          return (property.Name, property.Value);
        }
        Console.WriteLine($"[DEBUG] Not found in Properties");

        // Search performance
        var performance = FindPerformance(searchTerm);
     if (performance != null)
        {
          Console.WriteLine($"[DEBUG] Found in Performance: {performance.Name}");
    var value = performance.Unit != null ? $"{performance.Value}{performance.Unit}" : performance.Value.ToString();
       return (performance.Name, value);
        }
        Console.WriteLine($"[DEBUG] Not found in Performance");

        // Search specifications
        var specification = FindSpecification(searchTerm);
        if (specification != null)
        {
            Console.WriteLine($"[DEBUG] Found in Specifications: {specification.Name}");
  return (specification.Name, specification.Value);
     }
        Console.WriteLine($"[DEBUG] Not found in Specifications");

        // Search logistics
   Console.WriteLine($"[DEBUG] Searching Logistics, count: {LogisticsData.Count}");
        var logistics = FindLogistics(searchTerm);
        if (logistics != null)
    {
   Console.WriteLine($"[DEBUG] Found in Logistics: {logistics.Name}");
          var value = logistics.Unit != null ? $"{logistics.Value}{logistics.Unit}" : logistics.Value;
       return (logistics.Name, value);
        }
     Console.WriteLine($"[DEBUG] Not found in Logistics");

 Console.WriteLine($"[DEBUG] Attribute '{searchTerm}' not found in any section of '{Designation}'");
        return (null, null);
    }
}

public class Dimension
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; }
    public string? Unit { get; set; }
  public string? Symbol { get; set; }
}

public class Property
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class Performance
{
 public string Name { get; set; } = string.Empty;
    public string Value { get; set; }
    public string? Unit { get; set; }
    public string? Symbol { get; set; }
}

public class Specification
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class Logistics
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;  // Changed from decimal to string to support both
    public string? Unit { get; set; }
}
