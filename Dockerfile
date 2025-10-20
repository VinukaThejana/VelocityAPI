# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["VelocityAPI.csproj", "./"]
RUN dotnet restore "VelocityAPI.csproj"

COPY . .
RUN dotnet build "VelocityAPI.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "VelocityAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN groupadd -r appuser && useradd -r -g appuser appuser

COPY --from=publish /app/publish .
COPY Templates ./Templates

RUN chown -R appuser:appuser /app

USER appuser

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "VelocityAPI.dll"]
