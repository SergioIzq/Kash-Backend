using AhorroLand.Application;
using AhorroLand.Infrastructure;
using AhorroLand.Infrastructure.Configuration;
using AhorroLand.Infrastructure.TypesHandlers;
using AhorroLand.Middleware;
using AhorroLand.Shared.Application;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

// 🔥 CONFIGURACIÓN SERILOG: Antes de crear el builder
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("🚀 Iniciando AhorroLand API...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 🔥 SERILOG: Configurar Serilog desde appsettings
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName));

    // 🔥 OPTIMIZACIÓN 1: Configurar Kestrel para máximo rendimiento
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxConcurrentConnections = 10000;
        options.Limits.MaxConcurrentUpgradedConnections = 10000;
        options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
        options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
        options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);

        options.ConfigureEndpointDefaults(listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
        });
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
       .AllowCredentials(); // ✅ IMPORTANTE: Necesario para cookies
        });
    });

    // 🔥 OPTIMIZACIÓN 2: Output Caching para respuestas repetidas
    builder.Services.AddOutputCache(options =>
    {
        options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(30)));

        options.AddPolicy("ReadEndpoints", builder =>
          builder.Expire(TimeSpan.FromMinutes(5))
           .SetVaryByQuery("page", "pageSize"));
    });

    builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 🔥 Configuración JSON FLEXIBLE para recibir cualquier formato
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; // Respuestas en camelCase
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // ✅ ACEPTA cualquier casing en requests
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.WriteIndented = false;
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString; // ✅ Acepta números como strings
        options.JsonSerializerOptions.AllowTrailingCommas = true; // ✅ Tolera comas finales
        options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip; // ✅ Ignora comentarios en JSON

        // Converters adicionales para mayor flexibilidad
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // Enums como strings

        // 🔥 Source Generators para mejor rendimiento
        options.JsonSerializerOptions.TypeInfoResolverChain.Add(AppJsonSerializerContext.Default);
    });

    // 🔧 Configurar comportamiento de validación de modelos (no devolver 400 automáticamente)
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        // Opción 1: Desactivar respuesta automática de validación (recomendado para flexibilidad)
        options.SuppressModelStateInvalidFilter = false; // Mantenemos validación pero sin 400 automático

        // Personalizar respuesta de validación
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

        // Configurar Swagger para mostrar ejemplos en camelCase
        options.DescribeAllParametersInCamelCase();
    });

    // 🔥 OPTIMIZACIÓN 4: Response Compression (Brotli + Gzip)
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();

        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
       new[] { "application/json", "text/json" });
    });

    builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
    {
        options.Level = builder.Environment.IsDevelopment()
     ? CompressionLevel.Fastest
      : CompressionLevel.Optimal;
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
        options.CheckConsentNeeded = context => false; // No requerir consentimiento en dev
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

    // 🔥 OPTIMIZACIÓN 5: Object Pooling para reducir GC pressure
    builder.Services.AddSingleton<Microsoft.Extensions.ObjectPool.ObjectPoolProvider,
        Microsoft.Extensions.ObjectPool.DefaultObjectPoolProvider>();

    // 🔥 OPTIMIZACIÓN 6: Redis Cache para paginación optimizada
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
              };
          });
    }
    else
    {
        builder.Services.AddDistributedMemoryCache();
    }

    // 🔥 Configuración de autenticación JWT
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
            ClockSkew = TimeSpan.Zero
        };

        options.SaveToken = false;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        // 🍪 Configuración para leer el JWT desde cookies
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Primero intenta leer del header Authorization
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                // Si no está en el header, intenta leer de la cookie
                if (string.IsNullOrEmpty(token))
                {
                    token = context.Request.Cookies["AccessToken"];
                }

                context.Token = token;
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();

    var app = builder.Build();

    // 🔥 SERILOG: Agregar request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        };
    });

    app.UseAhorroLandExceptionHandling();

    // 🌐 CORS: Aplicar política según entorno
    if (app.Environment.IsDevelopment())
    {
        app.UseCors("LocalhostPolicy");
    }
    else
    {
        app.UseCors("ProductionPolicy");
    }

    app.UseStaticFiles();

    // 🍪 Aplicar política de cookies
    app.UseCookiePolicy();

    // 🔥 OPTIMIZACIÓN 8: Output Caching middleware
    app.UseOutputCache();

    app.UseResponseCompression();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();

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
    Log.CloseAndFlush();
}

// 🔥 OPTIMIZACIÓN 9: Source Generator Context para JSON
[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(decimal))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(Guid))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
