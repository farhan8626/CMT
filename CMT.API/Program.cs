// ============================================================
//  CMT.API – Program.cs
//  ASP.NET Core 8 minimal hosting
// ============================================================
using System.Text;
using CMT.Core.Interfaces;
using CMT.Infrastructure.Data;
using CMT.Infrastructure.Helpers;
using CMT.Infrastructure.Repositories;
using CMT.Infrastructure.Services;
using static CMT.Infrastructure.Services.InvoiceService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------------
// QuestPDF Community License (free for non-commercial / open use)
// ----------------------------------------------------------------
QuestPDF.Settings.License = LicenseType.Community;

// ----------------------------------------------------------------
// Database – EF Core + SQL Server
// ----------------------------------------------------------------
builder.Services.AddDbContext<CmtDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(3)));

// ----------------------------------------------------------------
// JWT Authentication
// ----------------------------------------------------------------
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var secretKey  = jwtSection["SecretKey"]
    ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSection["Issuer"],
        ValidAudience            = jwtSection["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew                = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddAuthorization();

// ----------------------------------------------------------------
// CORS – allow React dev server
// ----------------------------------------------------------------
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.AddCors(opt =>
    opt.AddPolicy("CmtCorsPolicy", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

// ----------------------------------------------------------------
// Repositories (mix of EF Core + Dapper)
// ----------------------------------------------------------------
builder.Services.AddScoped<IUserRepository,          UserRepository>();
builder.Services.AddScoped<ICustomerRepository,      CustomerRepository>();
builder.Services.AddScoped<IProductRepository,       ProductRepository>();
builder.Services.AddScoped<IRepairServiceRepository, RepairServiceRepository>();
builder.Services.AddScoped<IRepairTicketRepository,  RepairTicketRepository>();
builder.Services.AddScoped<IInvoiceRepository,       InvoiceRepository>();
builder.Services.AddScoped<IDashboardRepository,     DashboardRepository>();

// ----------------------------------------------------------------
// Services
// ----------------------------------------------------------------
builder.Services.AddScoped<IAuthService,          AuthService>();
builder.Services.AddScoped<ICustomerService,      CustomerService>();
builder.Services.AddScoped<IProductService,       ProductService>();
builder.Services.AddScoped<IRepairServiceService, RepairServiceService>();
builder.Services.AddScoped<IRepairTicketService,  RepairTicketService>();
builder.Services.AddScoped<IInvoiceService,       InvoiceService>();
builder.Services.AddScoped<IDashboardService,     DashboardService>();
builder.Services.AddScoped<IEmailService,         EmailService>();
builder.Services.AddScoped<IPdfService,           PdfService>();

// ----------------------------------------------------------------
// Helpers
// ----------------------------------------------------------------
builder.Services.AddSingleton<JwtTokenHelper>();

// ----------------------------------------------------------------
// Controllers + Swagger
// ----------------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title   = "CMT Electrical Shop API",
        Version = "v1",
        Description = "Non-GST Shop Management, Inventory, Repair Tickets & Billing API"
    });

    // JWT Bearer support in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In          = ParameterLocation.Header,
        Description = "Enter: Bearer {your-token}",
        Name        = "Authorization",
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ----------------------------------------------------------------
// Build & Configure Pipeline
// ----------------------------------------------------------------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CMT API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("CmtCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

// Global error handling middleware
app.UseMiddleware<CMT.API.Middleware.GlobalExceptionMiddleware>();

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", utc = DateTime.UtcNow }))
    .AllowAnonymous();

app.MapControllers();

app.Run();
