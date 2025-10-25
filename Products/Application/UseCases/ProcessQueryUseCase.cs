using Products.Application.DTOs;
using Products.Application.Interfaces;
using Products.Domain.Interfaces;
using System.Text.Json;

namespace Products.Application.UseCases;

/// <summary>
/// Use case for processing natural language queries about bearings
/// </summary>
public class ProcessQueryUseCase : IProcessQueryUseCase
{
    private readonly IBearingRepository _bearingRepository;
    private readonly IAiExtractionService _aiService;
    private readonly ICacheService _cacheService;

    // Synonym mapping for better attribute matching
    private readonly Dictionary<string, string> _attributeSynonyms = new(StringComparer.OrdinalIgnoreCase)
    {
        { "b", "Width" },
        { "width", "Width" },
    { "outside diameter", "Outside diameter" },
        { "outer diameter", "Outside diameter" },
        { "outside", "Outside diameter" },
        { "od", "Outside diameter" },
    { "d", "Bore diameter" },
        { "bore", "Bore diameter" },
        { "inner diameter", "Bore diameter" },
   { "id", "Bore diameter" },
        { "limiting speed", "Limiting speed" },
    { "nlim", "Limiting speed" },
      { "speed", "Limiting speed" },
        { "photo", "photo_url" },
        { "image", "photo_url" }
    };

    public ProcessQueryUseCase(
        IBearingRepository bearingRepository,
        IAiExtractionService aiService,
        ICacheService cacheService)
    {
        _bearingRepository = bearingRepository;
        _aiService = aiService;
   _cacheService = cacheService;
    }

    public async Task<QueryResponse> ExecuteAsync(string userQuery)
    {
        if (string.IsNullOrWhiteSpace(userQuery))
        {
   return new QueryResponse { Answer = "Please provide a query.", IsError = true };
        }

    // Sanitize input
    userQuery = SanitizeInput(userQuery);

    // Extract product designation using AI
    var product = await _aiService.ExtractProductDesignationAsync(userQuery);
    
    // Debug logging
    Console.WriteLine($"[DEBUG] Query: '{userQuery}'");
    Console.WriteLine($"[DEBUG] AI extracted product: '{product}'");

    // Normalize product result - treat error messages and invalid formats as empty
    if (!string.IsNullOrEmpty(product))
    {
        var lowerProduct = product.ToLowerInvariant();
   
        // Check if AI returned error text
        if (lowerProduct.Contains("not found") ||
       lowerProduct.Contains("no product") ||
            lowerProduct.Contains("none") ||
    lowerProduct.Contains("error") ||
 lowerProduct.Contains("n/a") ||
     lowerProduct.Contains("not specified"))
        {
        Console.WriteLine($"[DEBUG] Product contains error text, setting to empty");
            product = string.Empty;
        }
    // Validate product format - must start with digit and be reasonable length
        else if (product.Length < 2 || product.Length > 20)
        {
      Console.WriteLine($"[DEBUG] Product length invalid ({product.Length}), setting to empty");
            product = string.Empty;
        }
        // Must start with a digit (bearing designations start with numbers)
        else if (!char.IsDigit(product[0]))
 {
Console.WriteLine($"[DEBUG] Product doesn't start with digit, setting to empty");
      product = string.Empty;
        }
        // If product doesn't actually exist in the query text, treat as invalid
        // This catches cases where AI hallucinates a product number
        else if (!userQuery.Contains(product, StringComparison.OrdinalIgnoreCase))
        {
     Console.WriteLine($"[DEBUG] Product '{product}' not found in query '{userQuery}', setting to empty (hallucination detected)");
     product = string.Empty;
        }
        else
     {
            Console.WriteLine($"[DEBUG] Product validation passed: '{product}'");
   }
    }

    // Extract attribute using AI
    var attribute = await _aiService.ExtractAttributeAsync(userQuery);
    Console.WriteLine($"[DEBUG] AI extracted attribute: '{attribute}'");

    // Apply synonym mapping IMMEDIATELY after extraction
    if (!string.IsNullOrEmpty(attribute) && _attributeSynonyms.TryGetValue(attribute, out var preliminaryMappedAttribute))
    {
      Console.WriteLine($"[DEBUG] Preliminary synonym mapping: '{attribute}' -> '{preliminaryMappedAttribute}'");
        attribute = preliminaryMappedAttribute;
    }

    // If no product found but attribute is found, search across all products
    if (string.IsNullOrEmpty(product) && !string.IsNullOrEmpty(attribute))
    {
      Console.WriteLine($"[DEBUG] No product but attribute found, triggering multi-product search");
        return await SearchAttributeAcrossProductsAsync(attribute);
    }

        if (string.IsNullOrEmpty(product))
        {
       return new QueryResponse
       {
  Answer = "I'm sorry, I can't find that information. Please include a product designation like '6205' or '6205 N'.",
     IsError = true
       };
        }

   if (string.IsNullOrEmpty(attribute))
        {
            return new QueryResponse
 {
        Answer = "I'm sorry, I can't find that information.",
       IsError = true
};
        }

    // No need to map synonyms again since we did it above
    Console.WriteLine($"[DEBUG] Using attribute: '{attribute}' for query");

  // Check cache
        var cacheKey = $"query:{product}:{attribute}";
        var cached = await _cacheService.GetAsync(cacheKey);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<QueryResponse>(cached) ?? new QueryResponse { Answer = cached };
    }

        // Get bearing from repository
  var bearing = _bearingRepository.GetByDesignation(product);
  if (bearing == null)
   {
 return new QueryResponse
     {
   Answer = $"I'm sorry, I can't find that information.",
     IsError = true
         };
  }

        // Find attribute in bearing
   var result = FindAttributeInBearing(bearing, attribute);

   // Cache result
        await _cacheService.SetAsync(cacheKey, JsonSerializer.Serialize(result), TimeSpan.FromHours(1));

        return result;
    }

    private async Task<QueryResponse> SearchAttributeAcrossProductsAsync(string attribute)
    {
        Console.WriteLine($"[DEBUG] SearchAttributeAcrossProductsAsync called with attribute: '{attribute}'");
    
    // Map synonyms
    if (_attributeSynonyms.TryGetValue(attribute, out var mappedAttribute))
    {
        Console.WriteLine($"[DEBUG] Attribute '{attribute}' mapped to '{mappedAttribute}'");
        attribute = mappedAttribute;
    }

    var results = new List<(string designation, string attributeName, string value)>();
    var allDesignations = _bearingRepository.GetAllDesignations();
  
    Console.WriteLine($"[DEBUG] Found {allDesignations.Count()} designations to search");

    foreach (var designation in allDesignations)
    {
        var bearing = _bearingRepository.GetByDesignation(designation);
        if (bearing == null)
        {
  Console.WriteLine($"[DEBUG] Bearing '{designation}' returned null");
 continue;
  }

      Console.WriteLine($"[DEBUG] Searching for '{attribute}' in bearing '{designation}'");
        var (foundAttributeName, foundValue) = bearing.FindAttribute(attribute);
    
        if (foundValue != null)
        {
     var displayName = foundAttributeName ?? attribute;
        results.Add((designation, displayName, foundValue));
   Console.WriteLine($"[DEBUG] Found: {designation} - {displayName}: {foundValue}");
        }
    else
   {
            Console.WriteLine($"[DEBUG] Attribute '{attribute}' not found in bearing '{designation}'");
        }
    }

    Console.WriteLine($"[DEBUG] Total results found: {results.Count}");

    if (results.Count == 0)
    {
        return new QueryResponse
        {
       Answer = $"I'm sorry, I couldn't find '{attribute}' in any of the available products. Try asking 'What is the {attribute} of 6205?'",
         IsError = true
        };
    }

    // Format answer
    string answer;
    if (results.Count == 1)
    {
// Single product - natural language format
  var (designation, attributeName, value) = results[0];
        answer = $"The {attribute} of the {designation} bearing - {attributeName}: {value}";
    }
    else
    {
        // Multiple products - list format
        var formattedResults = results.Select(r => $"• {r.designation} bearing - {r.attributeName}: {r.value}");
        answer = $"Found '{attribute}' in {results.Count} product(s):\n\n" + string.Join("\n", formattedResults);
    }

    return new QueryResponse
    {
        Answer = answer,
        Attribute = attribute,
        Value = results.Count == 1 ? results[0].value : $"{results.Count} product(s)"
    };
}
    
    private QueryResponse FindAttributeInBearing(Domain.Entities.Bearing bearing, string attribute)
    {
      var (foundAttributeName, foundValue) = bearing.FindAttribute(attribute);

      if (foundValue == null)
      {
            var helpMessage = attribute.Equals("height", StringComparison.OrdinalIgnoreCase)
                ? $"I'm sorry, I can't find that information. Bearings typically have Width (axial dimension/B), Bore diameter (d), and Outside diameter (D). Did you mean one of these?"
             : $"I'm sorry, I can't find that information.";

        return new QueryResponse
      {
    Answer = helpMessage,
          IsError = true
       };
        }

   // Handle photo URLs differently
if (string.Equals(attribute, "photo_url", StringComparison.OrdinalIgnoreCase))
   {
            return new QueryResponse
     {
  Answer = $"Photo for {bearing.Designation}: {foundValue}",
       ImageUrl = foundValue,
 Product = bearing.Designation,
 Attribute = foundAttributeName ?? attribute,
    Value = foundValue
      };
     }

    // Use the found attribute name for display, fall back to requested attribute
    var displayAttribute = foundAttributeName ?? attribute;
 
        // Convert display attribute to lowercase for natural language response
        var attributeLowerCase = displayAttribute.ToLowerInvariant();

        return new QueryResponse
        {
    Answer = $"The {attributeLowerCase} of the {bearing.Designation} bearing is {foundValue}.",
 Product = bearing.Designation,
  Attribute = displayAttribute,
      Value = foundValue
        };
    }

    private string SanitizeInput(string input)
    {
        if (input.Length > 500) input = input.Substring(0, 500);
   return new string(input.Where(c => !char.IsControl(c) || char.IsWhiteSpace(c)).ToArray());
    }
}
