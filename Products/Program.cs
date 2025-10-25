


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Clean Architecture - Register layers from inside out

// Domain layer has no dependencies (pure business logic)

// Application layer - Use Cases
builder.Services.AddScoped<IProcessQueryUseCase, ProcessQueryUseCase>();

// Infrastructure layer - Implementations
builder.Services.AddSingleton<IBearingRepository, JsonBearingRepository>();
builder.Services.AddSingleton<IAiExtractionService, AzureOpenAiExtractionService>();
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
 options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SKF Bearing Query API",
    Version = "v1",
        Description = "Natural language query API for SKF bearing product datasheets using Azure OpenAI - Clean Architecture",
  Contact = new OpenApiContact
  {
        Name = "SKF Bearing Query System",
 Email = "support@example.com"
     }
  });

    // Include XML comments
 var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
   var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
  {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// ? Verify data loading on startup
var bearingRepository = app.Services.GetRequiredService<IBearingRepository>();
var allDesignations = bearingRepository.GetAllDesignations().ToList();
Console.WriteLine($"[STARTUP] Loaded {allDesignations.Count} bearings: {string.Join(", ", allDesignations)}");

// Test logistics for first bearing
if (allDesignations.Any())
{
    var firstBearing = bearingRepository.GetByDesignation(allDesignations.First());
    if (firstBearing != null)
    {
        Console.WriteLine($"[STARTUP] Test bearing '{firstBearing.Designation}' has {firstBearing.LogisticsData.Count} logistics items");
    if (firstBearing.LogisticsData.Any())
        {
    Console.WriteLine($"[STARTUP] First logistics item: '{firstBearing.LogisticsData.First().Name}'");
            
       // Test search for "height"
      var heightResult = firstBearing.FindAttribute("height");
            Console.WriteLine($"[STARTUP] Search 'height' result: {heightResult.AttributeName} = {heightResult.Value}");
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "SKF Bearing Query API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "SKF Bearing Query API - Clean Architecture";
    options.DefaultModelsExpandDepth(2);
  options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
});

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.MapControllers();
app.Run();
