using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Products.Domain.Interfaces;
using System.Text.RegularExpressions;

namespace Products.Infrastructure.AI;

/// <summary>
/// Azure OpenAI implementation of AI extraction service
/// </summary>
public class AzureOpenAiExtractionService : IAiExtractionService
{
    private readonly OpenAIClient _client;
    private readonly string _deployment;

    public AzureOpenAiExtractionService(IConfiguration configuration)
    {
        var endpoint = configuration["AzureOpenAI:Endpoint"] ?? throw new InvalidOperationException("Missing Azure OpenAI endpoint");
  var key = configuration["AzureOpenAI:Key"] ?? throw new InvalidOperationException("Missing Azure OpenAI key");
     _deployment = configuration["AzureOpenAI:Deployment"] ?? "gpt-4o-mini";

 _client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
    }

    public async Task<string> ExtractProductDesignationAsync(string userQuery)
    {
        var systemPrompt = "You extract product designations from queries. Return only the designation (e.g., '6205' or '6205 N'), nothing else.";
        var userPrompt = $@"Extract the product designation from this query:
{userQuery}

Examples:
- 'What is the width of 6205?' -> 6205
- 'Height of 6205 N?' -> 6205 N

Return only the product designation:";

    try
        {
       var chatCompletionsOptions = new ChatCompletionsOptions()
       {
           DeploymentName = _deployment,
    Messages =
              {
      new ChatRequestSystemMessage(systemPrompt),
           new ChatRequestUserMessage(userPrompt)
     },
 MaxTokens = 10,
         Temperature = 0f
       };

  var response = await _client.GetChatCompletionsAsync(chatCompletionsOptions);
     var result = response.Value.Choices[0].Message.Content.Trim();

            // Regex fallback if AI returns empty
            if (string.IsNullOrEmpty(result))
            {
  var match = Regex.Match(userQuery, @"\b\d{3,5}(?:[\s-]?[A-Za-z0-9]+)?\b");
            if (match.Success) result = match.Value.Trim();
        }

  return result;
        }
        catch
  {
  // Fallback to regex if API fails
            var match = Regex.Match(userQuery, @"\b\d{3,5}(?:[\s-]?[A-Za-z0-9]+)?\b");
            return match.Success ? match.Value.Trim() : string.Empty;
      }
    }

    public async Task<string> ExtractAttributeAsync(string userQuery)
    {
     var systemPrompt = "You extract attribute names from bearing queries. Return only the attribute name exactly as it appears in bearing specifications (e.g., 'Width', 'Bore diameter', 'Outside diameter'). For 'height', determine if it means 'Width' (axial dimension/B) or if user wants literal height measurement.";
        var userPrompt = $@"Extract the attribute from this query:
{userQuery}

Examples:
- 'What is the width of 6205?' -> Width
- 'Show me the bore diameter' -> Bore diameter
- 'What is B for 6205?' -> Width
- 'Outside diameter of 6205?' -> Outside diameter

Return only the attribute name:";

        try
        {
      var chatCompletionsOptions = new ChatCompletionsOptions()
            {
       DeploymentName = _deployment,
          Messages =
                {
 new ChatRequestSystemMessage(systemPrompt),
              new ChatRequestUserMessage(userPrompt)
        },
              MaxTokens = 20,
           Temperature = 0f
            };

            var response = await _client.GetChatCompletionsAsync(chatCompletionsOptions);
     var result = response.Value.Choices[0].Message.Content.Trim();

            // Keyword fallback
          if (string.IsNullOrEmpty(result))
            {
    result = ExtractAttributeByKeywords(userQuery);
            }

     return result;
        }
  catch
   {
return ExtractAttributeByKeywords(userQuery);
        }
    }

    private string ExtractAttributeByKeywords(string query)
    {
        var lowered = query.ToLowerInvariant();

   var keywords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
     { "width", "Width" },
            { "b=", "Width" },
  { "b ", "Width" },
            { "outside diameter", "Outside diameter" },
  { "outer diameter", "Outside diameter" },
            { "od", "Outside diameter" },
{ "bore diameter", "Bore diameter" },
      { "bore", "Bore diameter" },
            { "d=", "Bore diameter" },
            { "inner diameter", "Bore diameter" },
            { "id", "Bore diameter" },
     { "limiting speed", "Limiting speed" },
       { "nlim", "Limiting speed" },
            { "speed", "Limiting speed" },
            { "reference speed", "Reference speed" },
    { "photo", "photo_url" },
          { "image", "photo_url" },
  { "picture", "photo_url" }
     };

        foreach (var kv in keywords)
        {
            if (lowered.Contains(kv.Key))
            {
         return kv.Value;
  }
        }

        // Symbol extraction
   var symbolMatch = Regex.Match(query, @"\b([BdD])\b");
        if (symbolMatch.Success)
   {
        var symbol = symbolMatch.Groups[1].Value.ToUpper();
            return symbol switch
            {
        "B" => "Width",
            "D" => "Outside diameter",
       _ => string.Empty
       };
        }

   return string.Empty;
    }
}
