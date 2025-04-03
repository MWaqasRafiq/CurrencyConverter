using Serilog;
using System.Text;
using FluentValidation;
using OpenTelemetry.Trace;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using FluentValidation.AspNetCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc.Versioning;
using CurrencyConverter.Application.Models;
using CurrencyConverter.Application.Interfaces;
using CurrencyConverter.Infrastructure.Services;
using CurrencyExchange.Infrastructure.Middleware;
using CurrencyExchange.Application.ModelValidator;
using CurrencyConverter.Infrastructure.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using CurrencyConverter.Infrastructure.SwaggerConfig;
using CurrencyConverter.Infrastructure.ResilienceProvider;



var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddJaegerExporter(opts =>
        {
            opts.AgentHost = configuration["OpenTelemetry:Jaeger:Host"];
            opts.AgentPort = Convert.ToInt32(configuration["OpenTelemetry:Jaeger:Port"]);
        }));


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();


// Add Swagger with Bearer Authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Currency Converter", Version = "v1" });

    var xmlFile = Path.Combine(AppContext.BaseDirectory, "CurrencyConverter.xml");
    c.IncludeXmlComments(xmlFile);

    // Add Bearer Token support
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
    c.OperationFilter<AddClientIdHeaderParameter>();

});


// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["JwtSettings:Issuer"],
            ValidAudience = configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"])),
        };
    });

//// Authorization
builder.Services.AddAuthorization();

// Caching (Redis)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration["CacheSettings:RedisConnection"];
});

// Dependency Injection
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddScoped<ICurrencyConverter, FrankFrutImplementation>();
builder.Services.AddScoped<ICurrencyProviderFactory, CurrencyProviderFactory>();
builder.Services.AddTransient<IValidator<CurrencyConvertReq>, CurrencyConvertReqValidator>();
builder.Services.AddTransient<IValidator<LoginReq>, LoginReqValidator>();

// HTTP client with Polly resilience
builder.Services.AddHttpClient("FrankfurterClient", client =>
{
    client.BaseAddress = new Uri(configuration["FrankfurterApi:BaseUrl"]);
})
.AddPolicyHandler(ResilienceProvider.GetRetryPolicy())
.AddPolicyHandler(ResilienceProvider.GetCircuitBreakerPolicy());

//api versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// API Rate Limiting
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(
        options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Currency Converter");
        });
}
app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<ValidateClientMiddleWare>();
app.UseMiddleware<RateLimitMiddleWare>();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
