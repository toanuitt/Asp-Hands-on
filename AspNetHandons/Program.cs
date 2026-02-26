using AspNetHandons.Entities;
using AspNetHandons.Exceptions;
using AspNetHandons.ExternalApis;
using AspNetHandons.Filters;
using AspNetHandons.Mappers;
using AspNetHandons.Services;
using AspNetHandons.Validation;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Refit;
using Serilog;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// Add Filter
builder.Services.AddControllers();
builder.Services.AddScoped<AuthorizationFilter>();

// Add validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ProductValidator>();

// Jwt authentication
builder.Services.Configure<Jwt>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<RsaKeyService>();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt").Get<Jwt>();
        var rsa = RsaKeyService.LoadRsaKey(jwt.RsaPublicKeyLocation);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new RsaSecurityKey(rsa),
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OrdersWritePolicy",
        policy => policy.RequireClaim("permission", "orders.write"));
});

// Exception Handling
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddProblemDetails();

// In memory caching
builder.Services.AddMemoryCache();

// mapper
builder.Services.AddScoped<ProductMapper>();

// background service
builder.Services.AddHostedService<BackgroundworkerService>();

// external api
builder.Services.AddRefitClient<IJsonTodo>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
    });


builder.Services.AddOpenApiDocument();
builder.Services.AddHealthChecks();

var app = builder.Build();



app.UseSerilogRequestLogging();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/healthz");
app.MapControllers();

app.Run();