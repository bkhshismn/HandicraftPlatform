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
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.FileProviders;
using System.Net;
using System.Security.Claims;
using Handicraft.Api.Notifications;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// The default Windows Event Log provider can throw when the process lacks
// permission to write to the machine event log. Keep logging in process
// outputs so a logging failure can never terminate an API request.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddFilter("System.Net.Http.HttpClient.TelegramBotApi", LogLevel.Warning);

builder.Configuration.AddJsonFile(
    "appsettings.Local.json",
    optional: true,
    reloadOnChange: true);

// Keep environment variables at the highest precedence so container-provided
// connection strings and secrets override machine-local development settings.
builder.Configuration.AddEnvironmentVariables();

// ==========================
// Upload size limits
// ==========================

builder.Services.Configure<FormOptions>(options =>
{
    // Large uploads are enabled only on the dedicated video endpoint below.
    options.MultipartBodyLengthLimit = 32 * 1024 * 1024;
    options.ValueLengthLimit = 16_384;
    options.MultipartHeadersLengthLimit = 32_768;
});

builder.WebHost.ConfigureKestrel(options =>
{
    // Keep the default request limit small; video upload opts into its own limit.
    options.Limits.MaxRequestBodySize = 32 * 1024 * 1024;
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
builder.Services.AddScoped<IUserCourseCategoryRepository, UserCourseCategoryRepository>();
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
    _ => { },
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
// System status
// ==========================

builder.Services.AddSingleton<
    ISystemStatusService,
    SystemStatusService>();


// ==========================
// Controllers
// ==========================

builder.Services.AddControllers();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext =>
    {
        // Nginx appends the address of its immediate peer to X-Forwarded-For.
        // The API is bound to localhost and only reached through that proxy, so
        // use the last address it appended instead of the shared Docker gateway IP.
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].ToString();
        var clientIp = forwardedFor
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .LastOrDefault();

        if (!IPAddress.TryParse(clientIp, out _))
            clientIp = httpContext.Connection.RemoteIpAddress?.ToString();

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: clientIp ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(2),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});

builder.Services.Configure<TelegramBotOptions>(builder.Configuration.GetSection("Telegram"));
builder.Services.AddHttpClient("TelegramBotApi", client =>
{
    client.BaseAddress = new Uri("https://api.telegram.org/");
    client.Timeout = TimeSpan.FromSeconds(40);
});
builder.Services.AddSingleton<TelegramBotApiClient>();
builder.Services.AddSingleton<TelegramSubscriberStore>();
builder.Services.AddSingleton<TelegramEventQueue>();
builder.Services.AddHostedService<TelegramBotPollingService>();
builder.Services.AddHostedService<TelegramEventDispatcher>();

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

var corsOrigins = new[]
    {
        "http://localhost:5024",
        "http://localhost:5201",
        "http://localhost:8081",
        "http://127.0.0.1:8081"
    }
    .Concat(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "ClientCors",
        policy =>
        {
            policy
                .WithOrigins(corsOrigins)
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
            options.MapInboundClaims = false;

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

                    RoleClaimType = "role",

                    NameClaimType = "unique_name",

                    ClockSkew =
                        TimeSpan.FromMinutes(2)
                };


            options.Events =
                new JwtBearerEvents
                {
                    // سافاری/iOS برای HLS بومی هدر Authorization نمی‌فرستد؛
                    // توکن را از query هم می‌پذیریم.
                    OnMessageReceived =
                        context =>
                        {
                            if (string.IsNullOrWhiteSpace(context.Token))
                            {
                                var queryToken =
                                    context.Request.Query["access_token"].ToString();

                                if (!string.IsNullOrWhiteSpace(queryToken))
                                    context.Token = queryToken;
                            }

                            return Task.CompletedTask;
                        },

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
                        async context =>
                        {
                            var userIdText = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                ?? context.Principal?.FindFirst("sub")?.Value;
                            var isActive = Guid.TryParse(userIdText, out var userId)
                                && await context.HttpContext.RequestServices
                                    .GetRequiredService<AppDbContext>()
                                    .Users.AsNoTracking()
                                    .AnyAsync(x => x.Id == userId && x.IsActive);

                            if (!isActive)
                            {
                                context.Fail("User account is inactive.");
                                return;
                            }

                            Console.WriteLine(
                                "[JWT OK] Token validated successfully");
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

var imageStorageRoot = Path.Combine(
    app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot"),
    "uploads",
    "images");
Directory.CreateDirectory(imageStorageRoot);


// ==========================
// Database Migration / Seed
// ==========================

if (app.Environment.IsDevelopment() ||
    app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
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

app.UseRouting();
app.UseCors("ClientCors");

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imageStorageRoot),
    RequestPath = "/uploads/images"
});


// ==========================
// Authentication
// ==========================

app.UseAuthentication();

app.UseRateLimiter();


// ==========================
// Authorization
// ==========================

app.UseAuthorization();

app.UseMiddleware<TelegramEventMiddleware>();


// ==========================
// Controllers
// ==========================

app.MapControllers();


// ==========================
// Run
// ==========================

app.Run();
