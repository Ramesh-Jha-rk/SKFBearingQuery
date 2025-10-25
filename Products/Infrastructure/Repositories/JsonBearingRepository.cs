using Products.Domain.Entities;
using Products.Domain.Interfaces;
using System.Text.Json;

namespace Products.Infrastructure.Repositories;

/// <summary>
/// JSON file-based implementation of bearing repository
/// </summary>
public class JsonBearingRepository : IBearingRepository
{
    private readonly Dictionary<string, Bearing> _bearings = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _dataDirectory;

    public JsonBearingRepository()
    {
   // Use AppContext.BaseDirectory to get the application's base directory
        // Then look for data folder relative to it
        _dataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
  
     // Also try current directory as fallback
        if (!Directory.Exists(_dataDirectory))
        {
        _dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "data");
 }
 
        LoadBearingsFromJsonFiles();
    }

    public Bearing? GetByDesignation(string designation)
    {
        if (string.IsNullOrWhiteSpace(designation)) return null;

        if (_bearings.TryGetValue(designation.Trim(), out var bearing))
            return bearing;

        // Try alternative formats
        var alt = designation.Replace(" ", "").Replace("-", "");
        if (_bearings.TryGetValue(alt, out bearing))
 return bearing;

        return null;
    }

    public IEnumerable<string> GetAllDesignations()
    {
        return _bearings.Keys;
    }

    public IEnumerable<Bearing> GetAll()
    {
        return _bearings.Values;
    }

    private void LoadBearingsFromJsonFiles()
    {
        if (!Directory.Exists(_dataDirectory))
        {
  Console.WriteLine($"WARNING: Data directory not found: {_dataDirectory}");
      return;
        }

        var jsonFiles = Directory.GetFiles(_dataDirectory, "*.json");
        Console.WriteLine($"Loading bearings from {_dataDirectory}...");
 Console.WriteLine($"Found {jsonFiles.Length} JSON files");

  foreach (var filePath in jsonFiles)
   {
    try
            {
     Console.WriteLine($"\n[DEBUG] === Processing file: {Path.GetFileName(filePath)} ===");
   var jsonContent = File.ReadAllText(filePath);
       var jsonDoc = JsonDocument.Parse(jsonContent);
       var root = jsonDoc.RootElement;

            var bearing = MapJsonToBearing(root);
     if (!string.IsNullOrEmpty(bearing.Designation))
      {
        _bearings[bearing.Designation] = bearing;
        Console.WriteLine($"[DEBUG] Successfully loaded bearing: {bearing.Designation}");
      Console.WriteLine($"[DEBUG]   - Dimensions: {bearing.Dimensions.Count}");
 Console.WriteLine($"[DEBUG]   - Properties: {bearing.Properties.Count}");
 Console.WriteLine($"[DEBUG]   - Performance: {bearing.PerformanceData.Count}");
   Console.WriteLine($"[DEBUG]   - Specifications: {bearing.Specifications.Count}");
            Console.WriteLine($"[DEBUG]   - Logistics: {bearing.LogisticsData.Count}");
       }
            }
     catch (Exception ex)
         {
  // Log error but continue loading other files
           Console.WriteLine($"ERROR loading {Path.GetFileName(filePath)}: {ex.Message}");
      Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
        }

        Console.WriteLine($"\n[DEBUG] === Summary ===");
        Console.WriteLine($"Successfully loaded {_bearings.Count} bearings");
   foreach (var key in _bearings.Keys)
        {
       Console.WriteLine($"  - {key}: {_bearings[key].LogisticsData.Count} logistics items");
    }
    }

    private Bearing MapJsonToBearing(JsonElement json)
  {
        var bearing = new Bearing();

        if (json.TryGetProperty("designation", out var designation))
        bearing.Designation = designation.GetString() ?? string.Empty;

        if (json.TryGetProperty("short_description", out var shortDesc))
       bearing.ShortDescription = shortDesc.GetString() ?? string.Empty;

      if (json.TryGetProperty("description", out var desc))
            bearing.Description = desc.GetString() ?? string.Empty;

   if (json.TryGetProperty("category", out var category))
   bearing.Category = category.GetString() ?? string.Empty;

        // Map dimensions
        if (json.TryGetProperty("dimensions", out var dimensions) && dimensions.ValueKind == JsonValueKind.Array)
    {
 foreach (var dim in dimensions.EnumerateArray())
            {
       var dimension = new Dimension
     {
       Name = dim.GetProperty("name").GetString() ?? string.Empty,
     Unit = dim.TryGetProperty("unit", out var unit) ? unit.GetString() : null,
   Symbol = dim.TryGetProperty("symbol", out var symbol) ? symbol.GetString() : null
  };

   // Handle both string and numeric values
   if (dim.TryGetProperty("value", out var value))
  {
     if (value.ValueKind == JsonValueKind.String)
{
      dimension.Value = value.GetString() ?? string.Empty;
         }
    else if (value.ValueKind == JsonValueKind.Number)
   {
                dimension.Value = value.GetDecimal().ToString();
     }
    }

                bearing.Dimensions.Add(dimension);
            }
        }

        // Map properties
        if (json.TryGetProperty("properties", out var properties) && properties.ValueKind == JsonValueKind.Array)
        {
            foreach (var prop in properties.EnumerateArray())
            {
           bearing.Properties.Add(new Property
      {
           Name = prop.GetProperty("name").GetString() ?? string.Empty,
   Value = prop.GetProperty("value").GetString() ?? string.Empty
         });
      }
    }

        // Map performance
        if (json.TryGetProperty("performance", out var performance) && performance.ValueKind == JsonValueKind.Array)
        {
          foreach (var perf in performance.EnumerateArray())
     {
       var performanceItem = new Performance
       {
  Name = perf.GetProperty("name").GetString() ?? string.Empty,
      Unit = perf.TryGetProperty("unit", out var unit) ? unit.GetString() : null,
     Symbol = perf.TryGetProperty("symbol", out var symbol) ? symbol.GetString() : null
    };

 // Handle both string and numeric values
     if (perf.TryGetProperty("value", out var value))
     {
           if (value.ValueKind == JsonValueKind.String)
      {
      performanceItem.Value = value.GetString() ?? string.Empty;
            }
 else if (value.ValueKind == JsonValueKind.Number)
           {
                  performanceItem.Value = value.GetDecimal().ToString();
        }
    }

        bearing.PerformanceData.Add(performanceItem);
    }
}

        // Map specifications
        if (json.TryGetProperty("specifications", out var specifications) && specifications.ValueKind == JsonValueKind.Array)
        {
            foreach (var spec in specifications.EnumerateArray())
      {
       bearing.Specifications.Add(new Specification
      {
        Name = spec.GetProperty("name").GetString() ?? string.Empty,
         Value = spec.GetProperty("value").GetString() ?? string.Empty
                });
            }
        }

    // Map logistics
    if (json.TryGetProperty("logistics", out var logistics) && logistics.ValueKind == JsonValueKind.Array)
    {
 Console.WriteLine($"[DEBUG] Mapping logistics array for bearing...");
        int logisticsCount = 0;
        foreach (var log in logistics.EnumerateArray())
     {
          try
  {
           var logItem = new Logistics
         {
          Name = log.GetProperty("name").GetString() ?? string.Empty,
  Unit = log.TryGetProperty("unit", out var unit) ? unit.GetString() : null
     };

   // Handle both string and numeric values
       if (log.TryGetProperty("value", out var value))
   {
         if (value.ValueKind == JsonValueKind.String)
            {
              logItem.Value = value.GetString() ?? string.Empty;
        }
    else if (value.ValueKind == JsonValueKind.Number)
    {
         var numValue = value.GetDecimal();
   logItem.Value = numValue.ToString();
 }
                }

                bearing.LogisticsData.Add(logItem);
 logisticsCount++;
            Console.WriteLine($"[DEBUG] Loaded logistics: '{logItem.Name}' = '{logItem.Value}' {logItem.Unit}");
            }
  catch (Exception ex)
       {
    Console.WriteLine($"Error parsing logistics item: {ex.Message}");
}
      }
        Console.WriteLine($"[DEBUG] Total logistics items loaded: {logisticsCount}");
    }
    else
 {
        Console.WriteLine($"[DEBUG] No logistics section found in JSON");
    }

      return bearing;
    }
}
