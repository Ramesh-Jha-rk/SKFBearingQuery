# SKF Bearing Query System

A Razor Pages application that allows users to ask natural-language questions about SKF bearing products using Azure OpenAI for intelligent query processing.

## Features

- **Natural Language Processing**: Ask questions like "What is the width of 6205?" and get accurate answers
- **Azure OpenAI Integration**: Uses GPT models to extract product designations and attributes
- **Redis Caching**: Speeds up repeated queries with optional Redis cache
- **Hallucination Reduction**: Validates AI outputs against actual datasheet data
- **Attractive Dashboard**: Clean, responsive UI with Bootstrap 5
- **REST API**: HTTP endpoints for programmatic access

## Architecture

The solution follows clean architecture principles with separated concerns:

```
Products/
??? Services/
?   ??? DataService.cs        # Loads and manages JSON datasheets
?   ??? OpenAiService.cs       # Azure OpenAI integration with fallbacks
?   ??? CacheService.cs        # Redis caching layer
?   ??? QueryService.cs        # Main query processing logic
??? Controllers/
?   ??? QueryController.cs     # REST API endpoints
??? Pages/
?   ??? Index.cshtml           # Interactive dashboard UI
??? data/
    ??? 6205.json              # Product datasheets
    ??? 6205 N.json
```

## Configuration

The application uses `appsettings.json` for configuration. Azure OpenAI and Redis credentials are already configured.

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://developer-evals-foundry.cognitiveservices.azure.com/",
    "Key": "***",
    "Deployment": "Ramesh_Kumar_Jha"
  },
  "Redis": {
    "Connection": "skf-developer-eval.redis.cache.windows.net:6380,password=***"
  }
}
```

## Running the Application

### Prerequisites
- .NET 9 SDK
- Visual Studio 2022 or VS Code

### Steps

1. **Restore packages**:
   ```bash
   cd Products
   dotnet restore
   ```

2. **Run the application**:
   ```bash
   dotnet run
   ```

3. **Access the dashboard**:
   - Open browser to `https://localhost:5001` or `http://localhost:5000`
   - The interactive query dashboard will be displayed

## API Usage

### GET Endpoint

```bash
curl "http://localhost:5000/api/query/ask?q=What%20is%20the%20width%20of%206205%3F"
```

**Response:**
```json
{
  "answer": "The Width of the 6205 bearing is 15mm.",
  "isError": false,
  "product": "6205",
  "attribute": "Width",
  "value": "15mm"
}
```

### POST Endpoint

```bash
curl -X POST "http://localhost:5000/api/query/ask" \
  -H "Content-Type: application/json" \
  -d '{"query": "What is the bore diameter of 6205 N?"}'
```

## Example Queries

- "What is the width of 6205?"
- "Bore diameter of 6205 N"
- "What is the limiting speed for 6205?"
- "Show me the outside diameter of 6205"
- "Height of 6205 N" (maps to Width/B dimension)

## Security Features

1. **Input Sanitization**: Queries are sanitized to prevent injection attacks
2. **Length Limits**: Input limited to 500 characters
3. **Configuration-based Secrets**: Credentials in config files (move to Key Vault for production)
4. **Error Handling**: Graceful fallbacks when services unavailable

## Hallucination Reduction Strategy

1. **Constrained Prompts**: AI only extracts product/attribute, never generates facts
2. **Validation**: All values come from actual JSON datasheets
3. **Fallback Logic**: Regex patterns catch common cases if AI fails
4. **Synonym Mapping**: Maps user terms to actual datasheet attribute names
5. **Temperature 0**: Deterministic AI responses for extraction tasks

## Caching Strategy

- **Cache Key Format**: `query:{product}:{attribute}`
- **TTL**: 1 hour
- **Graceful Degradation**: Works without Redis (no-op)
- **Cache Hit Response**: Includes `source: "cache"` metadata

## Testing

### Manual Testing via Dashboard
1. Navigate to the homepage
2. Enter a query in the input field
3. View formatted results with product details

### API Testing via curl
```bash
# Test successful query
curl "http://localhost:5000/api/query/ask?q=width%20of%206205"

# Test missing product
curl "http://localhost:5000/api/query/ask?q=width%20of%209999"

# Test missing attribute
curl "http://localhost:5000/api/query/ask?q=color%20of%206205"
```

## Data Files

The application reads from `data/*.json` files:

- **6205.json**: Full product specification for bearing 6205
- **6205 N.json**: Specification for bearing 6205 N

Each file contains:
- `dimensions`: Width, bore diameter, outside diameter
- `properties`: Material, coating, sealing, etc.
- `performance`: Limiting speed, load ratings
- `specifications`: Photo URL, codes

## Error Handling

The system handles errors gracefully:

- **Product not found**: "I'm sorry, I can't find product '{product}' in the datasheets."
- **Attribute not found**: "I'm sorry, I can't find information about '{attribute}' for product '{product}'."
- **No product identified**: "I'm sorry, I can't identify a product in your question."
- **Service failures**: Falls back to regex extraction

## Future Enhancements

- Add unit tests with mocked Azure OpenAI
- Move secrets to Azure Key Vault
- Add authentication/authorization
- Support bulk queries
- Add conversation history
- Expand product catalog
- Add telemetry and logging

## Development Notes

- **Target Framework**: .NET 9
- **UI Framework**: Razor Pages (per workspace requirements)
- **AI Model**: gpt-4o-mini via Azure OpenAI
- **Cache**: Redis (optional, graceful fallback)
- **Code Style**: Clean architecture, dependency injection, async/await

## License

Internal SKF evaluation project.
