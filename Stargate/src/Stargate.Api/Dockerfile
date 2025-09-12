# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Stargate/src/Stargate.Api/Stargate.Api.csproj", "Stargate/src/Stargate.Api/"]
COPY ["Stargate/src/Stargate.Persistence/Stargate.Persistence.csproj", "Stargate/src/Stargate.Persistence/"]
COPY ["Stargate/src/Stargate.Core/Stargate.Core.csproj", "Stargate/src/Stargate.Core/"]
COPY ["Stargate/src/Stargate.Core.Domain/Stargate.Core.Domain.csproj", "Stargate/src/Stargate.Core.Domain/"]
RUN dotnet restore "./Stargate/src/Stargate.Api/Stargate.Api.csproj"
COPY . .
WORKDIR "/src/Stargate/src/Stargate.Api"
RUN dotnet build "./Stargate.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Stargate.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Stargate.Api.dll"]