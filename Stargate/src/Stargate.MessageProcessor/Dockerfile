# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Stargate/src/Stargate.MessageProcessor/Stargate.MessageProcessor.csproj", "Stargate/src/Stargate.MessageProcessor/"]
COPY ["Common/src/Common.Infrastructure/Common.Infrastructure.csproj", "Common/src/Common.Infrastructure/"]
COPY ["Common/src/Common.LanguageExtensions/Common.LanguageExtensions.csproj", "Common/src/Common.LanguageExtensions/"]
COPY ["Stargate/src/Stargate.Infrastructure/Stargate.Infrastructure.csproj", "Stargate/src/Stargate.Infrastructure/"]
COPY ["Stargate/src/Stargate.Core/Stargate.Core.csproj", "Stargate/src/Stargate.Core/"]
COPY ["Stargate/src/Stargate.Core.Boundary/Stargate.Core.Boundary.csproj", "Stargate/src/Stargate.Core.Boundary/"]
COPY ["Stargate/src/Stargate.Core.Domain/Stargate.Core.Domain.csproj", "Stargate/src/Stargate.Core.Domain/"]
COPY ["Stargate/src/Stargate.Persistence/Stargate.Persistence.csproj", "Stargate/src/Stargate.Persistence/"]
RUN dotnet restore "./Stargate/src/Stargate.MessageProcessor/Stargate.MessageProcessor.csproj"
COPY . .
WORKDIR "/src/Stargate/src/Stargate.MessageProcessor"
RUN dotnet build "./Stargate.MessageProcessor.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Stargate.MessageProcessor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Stargate.MessageProcessor.dll"]