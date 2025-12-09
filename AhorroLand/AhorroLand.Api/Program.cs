using AhorroLand.Application;
using AhorroLand.Infrastructure;
using AhorroLand.Infrastructure.Configuration;
using AhorroLand.Infrastructure.TypesHandlers;
using AhorroLand.Middleware;
using AhorroLand.NuevaApi;
using AhorroLand.Shared.Application;
using Dapper;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting; // 🆕 .NET 10
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting; // 🆕 .NET 10

// 🔥 CONFIGURACIÓN SERILOG: Antes de crear el builder
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("🚀 Iniciando AhorroLand API en .NET 10...");

try
{
    var builder = WebApplication.CreateSlimBuilder(args); // 🆕 .NET 10: SlimBuilder para menor footprint

    // 🔥 SERILOG: Configurar Serilog desde appsettings
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .Enrich.WithProperty("MachineName", Environment.MachineName)
        .Enrich.WithProperty("ProcessId", Environment.ProcessId));

    // 🔥 OPTIMIZACIÓN 1: Configurar Kestrel para máximo rendimiento con HTTP/3
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxConcurrentConnections = 10000;
        options.Limits.MaxConcurrentUpgradedConnections = 10000;
        options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
        options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
        options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);

        // 🆕 .NET 10: HTTP/3 habilitado por defecto con mejor rendimiento
        options.ConfigureEndpointDefaults(listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
        });

        // 🆕 .NET 10: Configuración mejorada de límites
        options.Limits.Http2.MaxStreamsPerConnection = 100;
        options.Limits.Http2.HeaderTableSize = 4096;
        options.Limits.Http2.MaxFrameSize = 16384;
        options.Limits.Http2.InitialConnectionWindowSize = 131072;
        options.Limits.Http2.InitialStreamWindowSize = 98304;
    });

    // 🌐 CORS: Configuración para desarrollo con localhost
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("LocalhostPolicy", policy =>
        {
            policy.WithOrigins(
                    "http://localhost:4200",  // Angular default
                    "http://localhost:3000",  // React default
                    "http://localhost:5173",  // Vite default
                    "http://localhost:8080",  // Vue default
                    "http://localhost:8081"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials() // ✅ IMPORTANTE: Necesario para cookies
                .WithExposedHeaders("Content-Disposition");
        });

        // Política adicional para producción (configurar según necesidades)
        options.AddPolicy("ProductionPolicy", policy =>
        {
            policy.WithOrigins(
                    builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                    ?? Array.Empty<string>()
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    // 🆕 .NET 10: Configuración JSON optimizada con mejoras de rendimiento
    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.SerializerOptions.PropertyNameCaseInsensitive = true;
        options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.SerializerOptions.WriteIndented = false;
        options.SerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        options.SerializerOptions.AllowTrailingCommas = true;
        options.SerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        
        // 🆕 .NET 10: Source Generators mejorados
        options.SerializerOptions.TypeInfoResolverChain.Add(AppJsonSerializerContext.Default);
        
        // 🆕 .NET 10: Nuevas opciones de rendimiento
        options.SerializerOptions.PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate;
        options.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip;
    });

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            // Aplicar misma configuración para controllers
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.WriteIndented = false;
            options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
            options.JsonSerializerOptions.AllowTrailingCommas = true;
            options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.TypeInfoResolverChain.Add(AppJsonSerializerContext.Default);
            
            // 🆕 .NET 10
            options.JsonSerializerOptions.PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate;
            options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip;
        });

    // 🔧 Configurar comportamiento de validación de modelos
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = false;

        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            var result = new
            {
                mensaje = "Error de validación en los datos enviados",
                errores = errors,
                ayuda = "Verifica el formato de los campos enviados. La API acepta camelCase, PascalCase y snake_case."
            };

            return new BadRequestObjectResult(result);
        };
    });

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
            Description = "Ingrese el token JWT en el formato: Bearer {token}"
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
                Array.Empty<string>()
            }
        });

        options.DescribeAllParametersInCamelCase();
    });

    // 🔥 OPTIMIZACIÓN 4: Response Compression mejorada con Brotli optimizado
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();

        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
            new[] { "application/json", "text/json", "application/xml", "text/xml" });
    });

    builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
    {
        // 🆕 .NET 10: Brotli optimizado con mejor rendimiento
        options.Level = builder.Environment.IsDevelopment()
            ? CompressionLevel.Fastest
            : CompressionLevel.SmallestSize; // Mejor compresión en producción
    });

    builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = builder.Environment.IsDevelopment()
            ? CompressionLevel.Fastest
            : CompressionLevel.Optimal;
    });

    // 🍪 Configuración de Cookies para la aplicación
    builder.Services.Configure<CookiePolicyOptions>(options =>
    {
        options.CheckConsentNeeded = context => false;
        options.MinimumSameSitePolicy = builder.Environment.IsDevelopment()
            ? SameSiteMode.Lax
            : SameSiteMode.Strict;
        options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
        options.Secure = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
    });

    DefaultTypeMap.MatchNamesWithUnderscores = true;

    DapperTypeHandlerRegistration.RegisterGuidValueObjectHandlers();

    MapsterConfig.RegisterMapsterConfiguration(builder.Services);

    builder.Services.AddApplication();
    builder.Services.AddSharedApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // 🔥 Configurar Hangfire para trabajos programados
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionString is not configured");

    builder.Services.AddHangfire(config =>
    {
        config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseStorage(
                new Hangfire.MySql.MySqlStorage(
                    connectionString,
                    new Hangfire.MySql.MySqlStorageOptions
                    {
                        QueuePollInterval = TimeSpan.FromSeconds(15),
                        JobExpirationCheckInterval = TimeSpan.FromHours(1),
                        CountersAggregateInterval = TimeSpan.FromMinutes(5),
                        PrepareSchemaIfNecessary = true,
                        DashboardJobListLimit = 50000,
                        TransactionTimeout = TimeSpan.FromMinutes(1),
                        TablesPrefix = "hangfire",
                        TransactionIsolationLevel = System.Transactions.IsolationLevel.ReadCommitted
                    }));
    });

    // Agregar el servidor de Hangfire con configuración optimizada
    builder.Services.AddHangfireServer(options =>
    {
        // 🆕 .NET 10: Mejor utilización de CPU cores
        options.WorkerCount = Math.Max(Environment.ProcessorCount * 2, 4);
        options.ServerName = $"AhorroLand-{Environment.MachineName}-{Environment.ProcessId}";
        options.Queues = new[] { "critical", "default", "low" };
        options.SchedulePollingInterval = TimeSpan.FromSeconds(15);
        options.ServerTimeout = TimeSpan.FromMinutes(5);
        options.ServerCheckInterval = TimeSpan.FromMinutes(1);
        options.CancellationCheckInterval = TimeSpan.FromSeconds(5);
    });

    // 🔥 OPTIMIZACIÓN 5: Object Pooling mejorado
    builder.Services.AddSingleton<Microsoft.Extensions.ObjectPool.ObjectPoolProvider,
        Microsoft.Extensions.ObjectPool.DefaultObjectPoolProvider>();

    // 🔥 OPTIMIZACIÓN 6: Redis Cache mejorado para .NET 10
    var redisConnection = builder.Configuration.GetConnectionString("Redis");
    if (!string.IsNullOrEmpty(redisConnection))
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "AhorroLand:";

            options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
            {
                EndPoints = { redisConnection },
                AbortOnConnectFail = false,
                ConnectTimeout = 5000,
                SyncTimeout = 5000,
                AsyncTimeout = 5000,
                KeepAlive = 60,
                ConnectRetry = 3,
                DefaultDatabase = 0,
                // 🆕 .NET 10: Configuraciones optimizadas
                AllowAdmin = false,
                Ssl = !builder.Environment.IsDevelopment(),
                ReconnectRetryPolicy = new StackExchange.Redis.ExponentialRetry(5000),
            };
        });

        Log.Information("✅ Redis Cache configurado correctamente");
    }
    else
    {
        builder.Services.AddDistributedMemoryCache();
        Log.Warning("⚠️ Redis no configurado, usando memoria caché en memoria");
    }

    // 🔥 Configuración de autenticación JWT optimizada
    var jwtKey = builder.Configuration["JwtSettings:SecretKey"]
        ?? throw new InvalidOperationException("JwtSettings:SecretKey no está configurada.");
    var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "AhorroLand";
    var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "AhorroLand";

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
            ClockSkew = TimeSpan.Zero,
            // 🆕 .NET 10: Validaciones adicionales de seguridad
            RequireExpirationTime = true,
            RequireSignedTokens = true,
        };

        options.SaveToken = false;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        // 🍪 Configuración para leer el JWT desde cookies
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Primero intenta leer del header Authorization
                var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

                // Si no está en el header, intenta leer de la cookie
                token ??= context.Request.Cookies["AccessToken"];

                context.Token = token;
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();

    // 🆕 .NET 10: Health Checks mejorados
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
        .AddMySql(connectionString, name: "mysql", timeout: TimeSpan.FromSeconds(3));

    // 🆕 .NET 10: Rate Limiting mejorado
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var userIdentifier = context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
            
            return RateLimitPartition.GetFixedWindowLimiter(userIdentifier, _ =>
                new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 10
                });
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    var app = builder.Build();

    // 🔥 SERILOG: Agregar request logging mejorado
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? string.Empty);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString() ?? string.Empty);
            diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            diagnosticContext.Set("Protocol", httpContext.Request.Protocol);
        };
        
        // 🆕 .NET 10: Filtrar logs innecesarios para mejor rendimiento
        options.GetLevel = (httpContext, elapsed, ex) =>
        {
            if (ex != null) return Serilog.Events.LogEventLevel.Error;
            if (httpContext.Response.StatusCode > 499) return Serilog.Events.LogEventLevel.Error;
            if (elapsed > 1000) return Serilog.Events.LogEventLevel.Warning;
            return Serilog.Events.LogEventLevel.Information;
        };
    });

    app.UseAhorroLandExceptionHandling();

    // 🆕 .NET 10: Rate Limiting
    app.UseRateLimiter();

    // 🌐 CORS: Aplicar política según entorno
    app.UseCors(builder.Environment.IsDevelopment() ? "LocalhostPolicy" : "ProductionPolicy");

    app.UseStaticFiles();

    // 🍪 Aplicar política de cookies
    app.UseCookiePolicy();

    app.UseResponseCompression();

    // 🔥 Hangfire Dashboard (solo en desarrollo)
    if (app.Environment.IsDevelopment())
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter() },
            StatsPollingInterval = 5000
        });
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "AhorroLand API v1");
            options.RoutePrefix = string.Empty; // Swagger en la raíz
        });
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // 🆕 .NET 10: Health Checks endpoints
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready");
    app.MapHealthChecks("/health/live");

    // 🆕 .NET 10: Información de la aplicación
    app.MapGet("/info", () => new
    {
        version = "1.0.0",
        environment = app.Environment.EnvironmentName,
        framework = Environment.Version.ToString(),
        runtime = ".NET 10",
        machineName = Environment.MachineName,
        processId = Environment.ProcessId,
        processorCount = Environment.ProcessorCount
    }).WithName("AppInfo");

    Log.Information("🎯 Configuración completada. Iniciando servidor...");
    Log.Information("📊 Procesadores disponibles: {ProcessorCount}", Environment.ProcessorCount);
    Log.Information("💾 Memoria total: {Memory} MB", GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024 / 1024);

    await app.RunAsync();

    Log.Information("✅ AhorroLand API se detuvo correctamente");
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ La aplicación falló al iniciar");
    throw;
}
finally
{
    Log.Information("🛑 Cerrando sistema de logging...");
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
