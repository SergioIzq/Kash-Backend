using Kash.Application;
using Kash.Infrastructure;
using Kash.Infrastructure.Configuration;
using Kash.Infrastructure.TypesHandlers;
using Kash.Shared.Application;
using SergioIzq.AspNetCore.Kernel.DependencyInjection;
using SergioIzq.AspNetCore.Kernel.Middleware;
using SergioIzq.Logging.HtmlFile;
using Dapper;
using Serilog;
using System.Text.Json.Serialization;

// LOGGER DE ARRANQUE (consola; el definitivo HTML lo configura UseKernelSerilog)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("🚀 Iniciando Kash API en .NET 10...");

BootstrapExtensions.SetKernelCulture("es-ES");

// QuestPDF: licencia Community (gratuita para uso individual / <$1M facturación).
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

try
{
    var builder = WebApplication.CreateSlimBuilder(args);

    builder.UseKernelSerilog("Kash");
    builder.WebHost.ConfigureKernelKestrel();

    // Bootstrap del kernel: CORS, JSON (minimal APIs + MVC), validación de modelos,
    // Swagger con Bearer, compresión y política de cookies
    builder.Services.AddKernelCors("sergioizq.com");
    builder.Services.AddKernelJsonOptions(AppJsonSerializerContext.Default);
    builder.Services.AddKernelControllers(AppJsonSerializerContext.Default);
    builder.Services.AddKernelModelValidation();
    builder.Services.AddKernelSwagger();
    builder.Services.AddKernelResponseCompression();
    builder.Services.AddKernelCookiePolicy(builder.Environment);

    // Dapper + Mapster (específico de Kash: type handlers de sus Ids y mapeos de sus VOs)
    DefaultTypeMap.MatchNamesWithUnderscores = true;
    DapperTypeHandlerRegistration.RegisterGuidValueObjectHandlers();
    MapsterConfig.RegisterMapsterConfiguration(builder.Services);

    builder.Services.AddHtmlFileLogging(opts => builder.Configuration.GetSection("HtmlFileLog").Bind(opts));

    builder.Services.AddApplication();
    builder.Services.AddSharedApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    builder.Services.AddKernelHangfire(connectionString!);
    builder.Services.AddDistributedMemoryCache(); // Simplificado para que no falle si no hay Redis
    builder.Services.AddKernelJwtAuthentication(builder.Configuration);
    builder.Services.AddKernelHealthChecks(connectionString); // /health comprueba MySQL de verdad
    builder.Services.AddRateLimiter(_ => { }); // Simplificado

    // --------------------------------------------------------------------------------
    // PIPELINE DE LA APP
    // --------------------------------------------------------------------------------
    var app = builder.Build();

    app.UseSerilogRequestLogging();

    // Middleware de manejo de excepciones y resultados (PRIMERO)
    app.UseKernelExceptionHandling();

    app.UseRateLimiter();

    // CORS debe ir temprano
    app.UseCors(builder.Environment.IsDevelopment() ? "LocalhostPolicy" : "ProductionPolicy");

    app.UseStaticFiles(); // Importante para Swagger UI (CSS/JS)
    app.UseCookiePolicy();

    app.UseResponseCompression();

    app.UseKernelSwaggerUI("Kash API v1"); // solo en Development

    // Dashboard de Hangfire protegido por el filtro del kernel (libre en dev, 403 en prod)
    app.UseKernelHangfireDashboard();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    app.MapGet("/", () => Results.Redirect("/swagger"));

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


// .NET 10: Source Generator Context optimizado para JSON
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
