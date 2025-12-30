using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ServiceCenterAPI.Application.Services;
using ServiceCenterAPI.Infrastructure.Persistence;
using ServiceCenterAPI.Infrastructure.Persistence.DbContextActions;
using ServiceCenterAPI.Infrastructure.Repositories;
using ServiceCenterAPI.Repositories;

// ============ CREATE BUILDER ============
var builder = WebApplication.CreateBuilder(args);

// ============ DATABASE CONFIGURATION ============
// Add DbContext with SQL Server
builder.Services.AddDbContext<ServiceCenterDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.CommandTimeout(30)
    )
);

// ============ DEPENDENCY INJECTION SETUP ============

// Repository Pattern
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services - Employee Management
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IServiceOrderService, ServiceOrderService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IJobService, JobService>();

// ============ API CONFIGURATION ============

// Add Controllers
builder.Services.AddControllers();

// Add API Versioning (Optional - for future versions)
//builder.Services.AddApiVersioning(options =>
//{
//    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
//    options.AssumeDefaultVersionWhenUnspecified = true;
//    options.ReportApiVersions = true;
//});

// ============ SWAGGER/OPENAPI CONFIGURATION ============

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ServiceCenter API",
        Version = "v1.0",
        Description = "Complete REST API for Service Center Management System",
        Contact = new OpenApiContact
        {
            Name = "ServiceCenter Team",
            Email = "support@servicecenter.local"
        },
        License = new OpenApiLicense
        {
            Name = "Internal Use"
        }
    });

    // Add JWT Bearer Authentication to Swagger (Optional - uncomment when JWT is implemented)
    /*
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
    */

    // Include XML comments (Optional - if you add them to your project)
    // var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// ============ CORS CONFIGURATION ============

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
    );

    // Alternatively, for specific origins:
    /*
    options.AddPolicy("AllowLocalhost", builder =>
        builder
            .WithOrigins("http://localhost:3000", "http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
    );
    */
});

// ============ BUILD THE APPLICATION ============

var app = builder.Build();

// ============ MIDDLEWARE PIPELINE CONFIGURATION ============

// Enable HTTPS Redirection
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Enable Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ServiceCenter API v1.0");
        options.RoutePrefix = string.Empty; // Make Swagger the default page
        options.DefaultModelsExpandDepth(2);
        options.DefaultModelExpandDepth(2);
    });
}

// CORS Middleware (must be before UseRouting)
app.UseCors("AllowAll");
// OR use specific policy:
// app.UseCors("AllowLocalhost");

// Authentication & Authorization Middleware
// (Uncomment when implementing JWT authentication)
// app.UseAuthentication();
// app.UseAuthorization();

// Routing
app.UseRouting();

// Controllers
app.MapControllers();

// ============ DATABASE INITIALIZATION ============

// Optional: Ensure database is created
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ServiceCenterDbContext>();

        // Create database if it doesn't exist
        dbContext.Database.EnsureCreated();

        // Optional: Run migrations instead
        // dbContext.Database.Migrate();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error initializing database: {ex.Message}");
}

// ============ RUN THE APPLICATION ============

app.Run();