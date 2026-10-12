using Handicraft.Infrastructure.Persistence.Data;
using Handicraft.Application.Contracts.Persistence;
using Handicraft.Infrastructure.Repositories;
using Handicraft.Application.Services;
using Handicraft.Application.Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Handicraft.Infrastructure.BackgroundServices;
using Handicraft.Infrastructure.Services;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.Local.json",
    optional: true,
    reloadOnChange: true);

// ==========================
// Upload size limits
// ==========================

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 3_221_225_472;
    options.ValueLengthLimit = 16_384;
    options.MultipartHeadersLengthLimit = 32_768;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 3_221_225_472;
});


// ==========================
// JWT Settings
// ==========================

var jwtSection =
    builder.Configuration.GetSection("Jwt");

var jwtKey =
    jwtSection.GetValue<string>("Key")
    ?? throw new InvalidOperationException(
        "Jwt:Key is not configured");

if (jwtKey.Length < 32 ||
    jwtKey.StartsWith("ChangeThisSecret", StringComparison.Ordinal))
{
    throw new InvalidOperationException(
        "Jwt:Key must be a securely managed value of at least 32 characters.");
}

var jwtIssuer =
    jwtSection.GetValue<string>("Issuer")
    ?? "Handicraft";

var jwtAudience =
    jwtSection.GetValue<string>("Audience")
    ?? "HandicraftUsers";

var jwtExpiresMinutes =
    jwtSection.GetValue<int>("ExpiresMinutes");


// ==========================
// Database
// ==========================

builder.Services.AddDbContext<AppDbContext>(
    options =>
        options.UseNpgsql(
            builder.Configuration
                .GetConnectionString("DefaultConnection")));


// ==========================
// Repositories
// ==========================

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseEpisodeRepository, CourseEpisodeRepository>();
builder.Services.AddScoped<ICourseEnrollmentRepository, CourseEnrollmentRepository>();
builder.Services.AddScoped<ICourseLessonRepository, CourseLessonRepository>();
builder.Services.AddScoped<ICourseSectionRepository, CourseSectionRepository>();

builder.Services.AddScoped<IUserDeviceRepository, UserDeviceRepository>();

builder.Services.AddScoped<IPortfolioItemRepository, PortfolioItemRepository>();
builder.Services.AddScoped<IPortfolioImageRepository, PortfolioImageRepository>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICourseLessonProgressRepository, CourseLessonProgressRepository>();

// ==========================
// AutoMapper
// ==========================

builder.Services.AddAutoMapper(
    typeof(
        Handicraft.Application.Services.MappingProfile
    ).Assembly);


// ==========================
// Application Services
// ==========================

builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<IPortfolioService, PortfolioService>();

builder.Services.AddScoped<IPortfolioImageService, PortfolioImageService>();

builder.Services.AddScoped<IUserDeviceService, UserDeviceService>();

builder.Services.AddScoped<ICourseEnrollmentService, CourseEnrollmentService>();

builder.Services.AddScoped<ICourseEpisodeService, CourseEpisodeService>();

builder.Services.AddScoped<ICourseSectionService, CourseSectionService>();

builder.Services.AddScoped<ICourseLessonService, CourseLessonService>();
builder.Services.AddScoped< ICourseLessonProgressService, CourseLessonProgressService>();


// ==========================
// Video Processing
// ==========================

builder.Services.AddScoped<
    IVideoStorageService,
    LocalVideoStorageService>();

builder.Services.AddScoped<
    IVideoProcessor,
    FfmpegVideoProcessor>();

builder.Services.AddHostedService<
    VideoProcessingBackgroundService>();


// ==========================
// Controllers
// ==========================

builder.Services.AddControllers();

builder.Services.Configure<
    Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(
    options =>
    {
        options.InvalidModelStateResponseFactory =
            context =>
            {
                Console.WriteLine(
                    "========== AUTO MODELSTATE 400 ==========");

                foreach (var item in context.ModelState)
                {
                    foreach (var error in item.Value.Errors)
                    {
                        Console.WriteLine(
                            $"[AUTO 400] {item.Key}: {error.ErrorMessage}");

                        if (error.Exception != null)
                        {
                            Console.WriteLine(
                                $"[AUTO 400 EXCEPTION] {error.Exception.Message}");
                        }
                    }
                }

                return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
                    context.ModelState);
            };
    });


// ==========================
// CORS
// ==========================
//
// Client:
// http://localhost:5024
//
// API:
// http://localhost:5182
//
// HLS.js داخل Browser برای دریافت
// master.m3u8 و segmentها به CORS نیاز دارد.
//

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "ClientCors",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5024",
                    "http://localhost:5201")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


// ==========================
// Authentication - JWT Bearer
// ==========================

builder.Services.AddAuthentication(
    options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(
        options =>
        {
            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,

                    ValidateAudience = true,

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,

                    ValidIssuer =
                        jwtIssuer,

                    ValidAudience =
                        jwtAudience,

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                jwtKey)),

                    RoleClaimType =
                        System.Security.Claims.ClaimTypes.Role,

                    NameClaimType =
                        System.Security.Claims.ClaimTypes.Name,

                    ClockSkew =
                        TimeSpan.FromMinutes(2)
                };


            options.Events =
                new JwtBearerEvents
                {
                    OnAuthenticationFailed =
                        context =>
                        {
                            Console.WriteLine(
                                $"[JWT FAIL] {context.Exception.Message}");

                            return Task.CompletedTask;
                        },


                    OnChallenge =
                        context =>
                        {
                            Console.WriteLine(
                                $"[JWT CHALLENGE] " +
                                $"Error={context.Error}, " +
                                $"Desc={context.ErrorDescription}");

                            return Task.CompletedTask;
                        },


                    OnTokenValidated =
                        context =>
                        {
                            Console.WriteLine(
                                "[JWT OK] Token validated successfully");

                            return Task.CompletedTask;
                        }
                };
        });


// ==========================
// Swagger
// ==========================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(
    c =>
    {
        c.SwaggerDoc(
            "v1",
            new OpenApiInfo
            {
                Title = "Handicraft API",
                Version = "v1"
            });


        c.AddSecurityDefinition(
            "Bearer",
            new OpenApiSecurityScheme
            {
                Description =
                    "Enter 'Bearer' [space] and then your valid token " +
                    "in the text input below.\r\n\r\n" +
                    "Example: 'Bearer eyJhb...'",

                Name = "Authorization",

                In = ParameterLocation.Header,

                Type = SecuritySchemeType.Http,

                Scheme = "bearer",

                BearerFormat = "JWT"
            });


        c.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference =
                            new OpenApiReference
                            {
                                Type =
                                    ReferenceType.SecurityScheme,

                                Id = "Bearer"
                            }
                    },

                    Array.Empty<string>()
                }
            });
    });


// ==========================
// Build
// ==========================

var app = builder.Build();


// ==========================
// Database Migration / Seed
// ==========================

if (app.Environment.IsDevelopment())
{
    using var scope =
        app.Services.CreateScope();

    var services =
        scope.ServiceProvider;

    try
    {
        var db =
            services.GetRequiredService<AppDbContext>();

        db.Database.Migrate();

        await DataSeeder.SeedAsync(db);
    }
    catch (Exception ex)
    {
        var logger =
            services.GetRequiredService<
                ILogger<Program>>();

        logger.LogError(
            ex,
            "An error occurred while migrating or seeding the database.");

        throw;
    }
}


// ==========================
// Swagger
// ==========================

app.UseSwagger();

app.UseSwaggerUI(
    c =>
    {
        c.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Handicraft API V1");
    });


// ==========================
// HTTPS
// ==========================
//
// در Development فعلاً خاموش نگه می‌داریم.
// چون Client روی 5024 و API روی 5182 هستند.
//

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


// ==========================
// CORS
// ==========================
//
// باید قبل از Authentication و Authorization
// اجرا شود تا پاسخ HLS شامل:
//
// Access-Control-Allow-Origin
//
// باشد.
//

app.UseCors("ClientCors");


// ==========================
// Authentication
// ==========================

app.UseAuthentication();


// ==========================
// Authorization
// ==========================

app.UseAuthorization();


// ==========================
// Controllers
// ==========================

app.MapControllers();


// ==========================
// Run
// ==========================

app.Run();
