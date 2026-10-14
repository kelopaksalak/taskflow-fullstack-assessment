using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Npgsql.Replication;
using TaskFlow.Api.Configuration;
using TaskFlow.Api.Data;
using TaskFlow.Api.Middleware;
using TaskFlow.Api.Repositories;
using TaskFlow.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers, JSON enum conversion, and request validation.
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(
                allowIntegerValues: false));
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problem = new ValidationProblemDetails(
                context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Request validation failed.",
                Instance = context.HttpContext.Request.Path.Value
            };

            problem.Extensions["traceId"] =
                context.HttpContext.TraceIdentifier;

            var result = new BadRequestObjectResult(problem);

            result.ContentTypes.Add("application/problem+json");

            return result;
        };
    });

// Consistent JSON error responses.
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance =
            context.HttpContext.Request.Path.Value;

        context.ProblemDetails.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// PostgreSQL database.
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is missing.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Task repository and service.
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

// Read and validate JWT configuration.
var jwtOptions = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "JWT configuration is missing.");

Validator.ValidateObject(
    jwtOptions,
    new ValidationContext(jwtOptions),
    validateAllProperties: true);

builder.Services.AddSingleton(jwtOptions);

// Login service.
builder.Services.AddScoped<IAuthService, AuthService>();

// JWT authentication.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateLifetime = true,
                RequireExpirationTime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        jwtOptions.SigningKey)),

                ValidAlgorithms = new[]
                {
                    SecurityAlgorithms.HmacSha256
                },

                ClockSkew = TimeSpan.FromSeconds(30)
            };
    });

builder.Services.AddAuthorization();

// Swagger and enable authentication
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
   option.SwaggerDoc("v1", new OpenApiInfo
   {
       Title = "TaskFlow API",
       Version = "v1",
       Description = "Task Manager API with JWT authentication"
   });

   option.AddSecurityDefinition(
    "Bearer",
    new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste your JWT token here"
    });
    option.AddSecurityRequirement(document => 
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

var app = builder.Build();

// Apply pending migrations before accepting HTTP requests
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Handle unexpected exceptions and empty error responses.
app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
   options.SwaggerEndpoint(
    "/swagger/v1/swagger.json",
    "TaskFlow API v1"); 
});

// Identify the caller before checking access permissions.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();