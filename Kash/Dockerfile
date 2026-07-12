# ETAPA 1: BUILD - Compilar la aplicación
# Actualizado a .NET 10.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar los archivos de solución y proyectos
COPY Kash.sln ./
COPY Kash.Api/Kash.Api.csproj Kash.Api/
COPY Kash.Application/Kash.Application.csproj Kash.Application/
COPY Kash.Domain/Kash.Domain.csproj Kash.Domain/
COPY Kash.Infrastructure/Kash.Infrastructure.csproj Kash.Infrastructure/
COPY Kash.Shared.Application/Kash.Shared.Application.csproj Kash.Shared.Application/
COPY Kash.Shared.Domain/Kash.Shared.Domain.csproj Kash.Shared.Domain/

# Restaurar dependencias
RUN dotnet restore

# Copiar el resto del código fuente
COPY . .

# Compilar y publicar Kash.Api
WORKDIR /src/Kash.Api
RUN dotnet build -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ETAPA 2: RUNTIME - Crear la imagen final
# Actualizado a .NET 10.0 ASP.NET Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Crear directorio de logs con permisos
RUN mkdir -p /app/logs && chmod 777 /app/logs

# Copiar los archivos publicados desde la etapa de build
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost/health || exit 1

# Punto de entrada
ENTRYPOINT ["dotnet", "Kash.Api.dll"]