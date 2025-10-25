using Microsoft.AspNetCore.Mvc;
using Products.Application.DTOs;
using Products.Application.Interfaces;
using Products.Domain.Interfaces;

namespace Products.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class QueryController : ControllerBase
{
    private readonly IProcessQueryUseCase _processQueryUseCase;
    private readonly ICacheService _cacheService;

    public QueryController(IProcessQueryUseCase processQueryUseCase, ICacheService cacheService)
    {
        _processQueryUseCase = processQueryUseCase;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Process a natural language query about bearing products
    /// </summary>
    /// <param name="q">The user query (e.g., "What is the width of 6205?" or "Tolerance class")</param>
    /// <returns>JSON response with answer and metadata</returns>
    /// <remarks>
    /// Sample requests:
    /// 
    ///     GET /api/query/ask?q=What is the width of 6205?
    ///     GET /api/query/ask?q=Bore diameter of 6205 N
    ///     GET /api/query/ask?q=Tolerance class
    ///   GET /api/query/ask?q=Limiting speed
    /// 
    /// Supported query types:
    /// - Product + Attribute: "What is the width of 6205?"
    /// - Attribute only: "Tolerance class" (searches all products)
    /// - Symbol queries: "What is B for 6205?"
    /// </remarks>
    /// <response code="200">Successfully processed query</response>
    /// <response code="400">Invalid query parameter</response>
    [HttpGet("ask")]
    [ProducesResponseType(typeof(QueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Ask([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { error = "Query parameter 'q' is required" });
        }

        var result = await _processQueryUseCase.ExecuteAsync(q);
      return Ok(result);
    }

    /// <summary>
    /// Process a natural language query (POST version for longer queries)
    /// </summary>
    /// <param name="request">Query request object</param>
    /// <returns>JSON response with answer and metadata</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/query/ask
    ///     {
    ///         "query": "What is the bore diameter of 6205 N?"
    ///     }
 /// 
    /// </remarks>
 /// <response code="200">Successfully processed query</response>
  /// <response code="400">Invalid request body</response>
    [HttpPost("ask")]
  [ProducesResponseType(typeof(QueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AskPost([FromBody] QueryRequest request)
    {
    if (string.IsNullOrWhiteSpace(request.Query))
        {
    return BadRequest(new { error = "Query is required" });
        }

        var result = await _processQueryUseCase.ExecuteAsync(request.Query);
        return Ok(result);
}

    /// <summary>
    /// Clear all cached query results
    /// </summary>
    /// <returns>Success message</returns>
    /// <remarks>
    /// This endpoint clears all cached query results from Redis.
    /// Use this when data has been updated or to force fresh results.
    /// </remarks>
    /// <response code="200">Cache cleared successfully</response>
    [HttpDelete("cache")]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearCache()
  {
        await _cacheService.ClearAsync("query:*");
        return Ok(new { message = "Cache cleared successfully" });
    }

    /// <summary>
    /// Clear a specific cached query
    /// </summary>
    /// <param name="product">Product designation (e.g., "6205")</param>
    /// <param name="attribute">Attribute name (e.g., "Width")</param>
/// <returns>Success message</returns>
    /// <response code="200">Cache entry cleared successfully</response>
    [HttpDelete("cache/{product}/{attribute}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearCacheEntry(string product, string attribute)
    {
   var cacheKey = $"query:{product}:{attribute}";
        await _cacheService.DeleteAsync(cacheKey);
    return Ok(new { message = $"Cache entry cleared for {product}/{attribute}" });
    }
}

/// <summary>
/// Request model for POST queries
/// </summary>
public class QueryRequest
{
    /// <summary>
    /// The natural language query about bearing products
    /// </summary>
    /// <example>What is the width of 6205?</example>
    public string Query { get; set; } = string.Empty;
}
