using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using Products.Application.Interfaces;
using Products.Application.DTOs;

namespace BearingFunctions;

public class QueryFunction
{
    private readonly ILogger _logger;
    private readonly IProcessQueryUseCase _processQueryUseCase;

    public QueryFunction(ILoggerFactory loggerFactory, IProcessQueryUseCase processQueryUseCase)
    {
        _logger = loggerFactory.CreateLogger<QueryFunction>();
        _processQueryUseCase = processQueryUseCase;
    }

    /// <summary>
    /// HTTP-triggered Azure Function that processes natural language queries about SKF bearing products
    /// </summary>
    /// <param name="req">HTTP request with query parameter 'q'</param>
    /// <returns>JSON response with answer and metadata</returns>
    [Function("QueryBearing")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "query/ask")] HttpRequestData req)
    {
        _logger.LogInformation("QueryBearing function processing request");

        string query = string.Empty;

        try
        {
            // Handle GET request with query parameter
            if (req.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                var queryParams = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
                query = queryParams["q"] ?? string.Empty;
            }
            // Handle POST request with JSON body
            else if (req.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var requestData = JsonSerializer.Deserialize<QueryRequest>(requestBody);
                query = requestData?.Query ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("Empty query received");
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Query parameter 'q' is required (GET) or 'query' in body (POST)" });
                return badResponse;
            }

            _logger.LogInformation($"Processing query: {query}");

            // Use Clean Architecture Use Case
            var result = await _processQueryUseCase.ExecuteAsync(query);

            // Return success response
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = "An error occurred processing your query", details = ex.Message });
            return errorResponse;
        }
    }
}

public class QueryRequest
{
    public string Query { get; set; } = string.Empty;
}

public class QueryResult
{
    public string Answer { get; set; } = string.Empty;
    public bool IsError { get; set; }
    public string? Product { get; set; }
    public string? Attribute { get; set; }
    public string? Value { get; set; }
    public string? ImageUrl { get; set; }
}
