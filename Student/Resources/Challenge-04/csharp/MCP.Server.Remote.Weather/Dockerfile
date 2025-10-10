# Use the official .NET 9.0 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the official .NET 9.0 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["MCP.Server.Remote.Weather.csproj", "./"]
RUN dotnet restore "MCP.Server.Remote.Weather.csproj"
COPY . .
RUN dotnet build "MCP.Server.Remote.Weather.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MCP.Server.Remote.Weather.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "MCP.Server.Remote.Weather.dll"]
