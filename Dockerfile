# ============================================
# STAGE 1: BUILD
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution file
COPY src/GreenSpace.sln ./src/

# Copy project files (layer caching)
COPY src/GreenSpace.Domain/GreenSpace.Domain.csproj ./src/GreenSpace.Domain/
COPY src/GreenSpace.Application/GreenSpace.Application.csproj ./src/GreenSpace.Application/
COPY src/GreenSpace.Infrastructure/GreenSpace.Infrastructure.csproj ./src/GreenSpace.Infrastructure/
COPY src/GreenSpace.WebAPI/GreenSpace.WebAPI.csproj ./src/GreenSpace.WebAPI/

# Restore dependencies
RUN dotnet restore src/GreenSpace.sln

# Copy all source files
COPY src/ ./src/

# Publish the WebAPI project
RUN dotnet publish src/GreenSpace.WebAPI/GreenSpace.WebAPI.csproj -c Release -o /app/publish

# ============================================
# STAGE 2: RUNTIME
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Expose port
EXPOSE 5020

# Environment variables
ENV ASPNETCORE_URLS=http://+:5020

# Entry point
ENTRYPOINT ["dotnet", "GreenSpace.WebAPI.dll"]
	