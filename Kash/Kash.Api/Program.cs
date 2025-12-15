using Kash.Application;
using Kash.FicheroLog;
using Kash.FicheroLog.Formatters;
using Kash.Infrastructure;
using Kash.Infrastructure.Configuration;
using Kash.Infrastructure.TypesHandlers;
using Kash.Middleware;
using Kash.Shared.Application;
using Dapper;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

// 🔥 CONFIGURACIÓN SERILOG
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("🚀 Iniciando Kash API en .NET 10...");

var cultureInfo = new CultureInfo("es-ES");
// Forzar el formato de números y fechas
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
// Forzar el formato de la interfaz (textos)
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

try
{
    var builder = WebApplication.CreateSlimBuilder(args);

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.File(
            // 1. Usamos TU formateador (para pintar cada log como HTML)
            formatter: new HtmlLogFormatter(),

            path: "logs/log.html",

            // 2. Importante: RollingInterval para que cree un archivo nuevo cada día/hora
            rollingInterval: RollingInterval.Day,

            // 3. Usamos TUS hooks (para pintar la cabecera, estilos y scripts al crear archivo)
            hooks: new HtmlLogHooks("Kash Logs")
        )
        .CreateLogger();

    builder.Host.UseSerilog();

    // 🔥 KESTREL (HTTP/3)
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxConcurrentConnections = 10000;
        options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
        options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);

        // Configuración para evitar los warnings de HTTP/2 sin TLS en local
        options.ConfigureEndpointDefaults(listenOptions =>
        {
            // Usamos HTTP1 y HTTP2 (HTTP3 requiere HTTPS obligatorio)
            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
        });
    });

    // 🌐 CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("LocalhostPolicy", policy =>
        {
            policy.WithOrigins(
                    "http://localhost:4200",
                    "http://localhost:3000",
                    "http://localhost:5173",
                    "http://localhost:8080"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .WithExposedHeaders("Content-Disposition");
        });

        options.AddPolicy("ProductionPolicy", policy =>
        {
            policy.SetIsOriginAllowed(origin =>
            {
                // Validamos que el origen sea una URL válida
                if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    var host = uri.Host;

                    return host.Equals("sergioizq.com", StringComparison.OrdinalIgnoreCase) ||
                           host.EndsWith(".sergioizq.com", StringComparison.OrdinalIgnoreCase);
                }
                return false;
            })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
        });
    });

    // 🆕 JSON Options
    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.SerializerOptions.PropertyNameCaseInsensitive = true;
        options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.SerializerOptions.TypeInfoResolverChain.Add(AppJsonSerializerContext.Default);
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.TypeInfoResolverChain.Add(AppJsonSerializerContext.Default);
        });

    // 🔧 Validación de Modelos
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(k => k.Key, v => v.Value?.Errors.Select(e => e.ErrorMessage).ToArray());

            return new BadRequestObjectResult(new
            {
                mensaje = "Error de validación",
                errores = errors
            });
        };
    });

    // 📄 SWAGGER
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Token JWT"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
        options.DescribeAllParametersInCamelCase();
    });

    // 🔥 COMPRESIÓN
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
    });

    // ... (Configuraciones de Brotli y Gzip igual que tenías)

    // 🍪 Cookies
    builder.Services.Configure<CookiePolicyOptions>(options =>
    {
        options.CheckConsentNeeded = context => false;
        options.MinimumSameSitePolicy = SameSiteMode.Lax; // Lax suele ir mejor en dev local
        options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
        options.Secure = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
    });

    DefaultTypeMap.MatchNamesWithUnderscores = true;
    DapperTypeHandlerRegistration.RegisterGuidValueObjectHandlers();
    MapsterConfig.RegisterMapsterConfiguration(builder.Services);

    builder.Services.AddHtmlFileLogging(opts => builder.Configuration.GetSection("HtmlFileLog").Bind(opts));

    builder.Services.AddApplication();
    builder.Services.AddSharedApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // 🔥 HANGFIRE
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseStorage(new Hangfire.MySql.MySqlStorage(connectionString, new Hangfire.MySql.MySqlStorageOptions
        {
            QueuePollInterval = TimeSpan.FromSeconds(15),
            PrepareSchemaIfNecessary = true,
            TablesPrefix = "hangfire",
        })));

    builder.Services.AddHangfireServer(options => options.WorkerCount = 2); // Reducido para dev local

    // ... (Redis y ObjectPool igual que tenías) ...
    builder.Services.AddDistributedMemoryCache(); // Simplificado para que no falle si no hay Redis

    // 🔐 JWT AUTH
    var jwtKey = builder.Configuration["JwtSettings:SecretKey"] ?? "CLAVE_DEFAULT_INSEGURA_PARA_DEV_CAMBIAME";
    var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "Kash";
    var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "Kash";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
        options.RequireHttpsMetadata = false; // Permitir http en dev

        // 🔥 FIX: Leer token desde Cookie O Header Authorization
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // 1. Primero intentar leer del header Authorization (comportamiento estándar)
                var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

                // 2. Si no está en el header, intentar leer de la cookie "AccessToken"
                token ??= context.Request.Cookies["AccessToken"];

                // 3. Asignar el token al contexto para que JWT Bearer lo valide
                context.Token = token;

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                // Agregar header si el token expiró
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();
    builder.Services.AddHealthChecks();
    builder.Services.AddRateLimiter(_ => { }); // Simplificado

    // --------------------------------------------------------------------------------
    // PIPELINE DE LA APP
    // --------------------------------------------------------------------------------
    var app = builder.Build();

    app.UseSerilogRequestLogging();
    
    // 🔥 Middleware de manejo de excepciones y resultados (PRIMERO)
    app.UseKashExceptionHandling();
    
    app.UseRateLimiter();

    // CORS debe ir temprano
    app.UseCors(builder.Environment.IsDevelopment() ? "LocalhostPolicy" : "ProductionPolicy");

    app.UseStaticFiles(); // Importante para Swagger UI (CSS/JS)
    app.UseCookiePolicy();
    
    // 🔥 Compresión después del middleware de no-caché
    app.UseResponseCompression();

    // 🔥 SWAGGER (Corregido)
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Kash API v1");
            // ❌ COMENTADO: Esto hacía que Swagger saliera en la raíz "/" y daba 404 en "/swagger"
            // options.RoutePrefix = string.Empty; 

            // Si comentas la línea de arriba, Swagger estará en: http://localhost:5131/swagger
        });
    }

    // Hangfire Dashboard
    if (app.Environment.IsDevelopment())
    {
        app.UseHangfireDashboard("/hangfire");
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    app.MapGet("/", () => Results.Redirect("/swagger")); // Redirigir raíz a swagger opcionalmente

    Log.Information("🎯 Iniciando servidor...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Error fatal al iniciar");
}
finally
{
    await Log.CloseAndFlushAsync();
}


// 🆕 .NET 10: Source Generator Context optimizado para JSON
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false,
    GenerationMode = JsonSourceGenerationMode.Metadata | JsonSourceGenerationMode.Serialization,
    PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
)]
[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(decimal))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(float))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(DateTimeOffset))]
[JsonSerializable(typeof(Guid))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(List<object>))]
[JsonSerializable(typeof(string[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
