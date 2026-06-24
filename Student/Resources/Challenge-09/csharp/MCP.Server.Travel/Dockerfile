# Use the official .NET 9.0 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the official .NET 9.0 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["MCP.Server.Travel.csproj", "./"]
RUN dotnet restore "MCP.Server.Travel.csproj"
COPY . .
RUN dotnet build "MCP.Server.Travel.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MCP.Server.Travel.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Bind to all interfaces inside the container so Container Apps ingress can reach it
ENV HttpHost=0.0.0.0
ENV HttpPort=8080

# Create a non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "MCP.Server.Travel.dll"]
