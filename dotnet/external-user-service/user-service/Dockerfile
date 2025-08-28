# ASP.NET Core JWT Authentication Dockerfile

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["AspNetJwtAuth.csproj", "AspNetJwtAuth/"]
RUN dotnet restore "AspNetJwtAuth/AspNetJwtAuth.csproj"

# Copy source code and build
COPY . AspNetJwtAuth/
WORKDIR "/src/AspNetJwtAuth"
RUN dotnet build "AspNetJwtAuth.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "AspNetJwtAuth.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final

# Create non-root user for security
RUN addgroup --system --gid 1001 dotnet && \
    adduser --system --uid 1001 --gid 1001 dotnet

WORKDIR /app

# Create logs directory with proper permissions
RUN mkdir -p logs && chown -R dotnet:dotnet logs

# Copy published application
COPY --from=publish /app/publish .

# Set proper ownership
RUN chown -R dotnet:dotnet /app

# Switch to non-root user
USER dotnet

# Expose ports
EXPOSE 80
EXPOSE 443

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Entry point
ENTRYPOINT ["dotnet", "AspNetJwtAuth.dll"]
