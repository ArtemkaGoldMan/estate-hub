# syntax=docker/dockerfile:1.4
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files first for better layer caching
COPY ["EstateHub.BackEnd/EstateHub.ListingService.API/EstateHub.ListingService.API.csproj", "EstateHub.ListingService.API/"]
COPY ["EstateHub.BackEnd/EstateHub.ListingService.Core/EstateHub.ListingService.Core.csproj", "EstateHub.ListingService.Core/"]
COPY ["EstateHub.BackEnd/EstateHub.ListingService.DataAccess.SqlServer/EstateHub.ListingService.DataAccess.SqlServer.csproj", "EstateHub.ListingService.DataAccess.SqlServer/"]
COPY ["EstateHub.BackEnd/EstateHub.ListingService.Domain/EstateHub.ListingService.Domain.csproj", "EstateHub.ListingService.Domain/"]
COPY ["EstateHub.BackEnd/EstateHub.ListingService.Infrastructure/EstateHub.ListingService.Infrastructure.csproj", "EstateHub.ListingService.Infrastructure/"]
COPY ["EstateHub.BackEnd/EstateHub.SharedKernel/EstateHub.SharedKernel.csproj", "EstateHub.SharedKernel/"]
COPY ["EstateHub.BackEnd/EstateHub.SharedKernel.API/EstateHub.SharedKernel.API.csproj", "EstateHub.SharedKernel.API/"]

# Restore with cache mount for faster builds
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore "EstateHub.ListingService.API/EstateHub.ListingService.API.csproj" --verbosity quiet

# Copy source code
COPY ["EstateHub.BackEnd/EstateHub.ListingService.API/", "EstateHub.ListingService.API/"]
COPY ["EstateHub.BackEnd/EstateHub.ListingService.Core/", "EstateHub.ListingService.Core/"]
COPY ["EstateHub.BackEnd/EstateHub.ListingService.DataAccess.SqlServer/", "EstateHub.ListingService.DataAccess.SqlServer/"]
COPY ["EstateHub.BackEnd/EstateHub.ListingService.Domain/", "EstateHub.ListingService.Domain/"]
COPY ["EstateHub.BackEnd/EstateHub.ListingService.Infrastructure/", "EstateHub.ListingService.Infrastructure/"]
COPY ["EstateHub.BackEnd/EstateHub.SharedKernel/", "EstateHub.SharedKernel/"]
COPY ["EstateHub.BackEnd/EstateHub.SharedKernel.API/", "EstateHub.SharedKernel.API/"]

WORKDIR "/src/EstateHub.ListingService.API"
# Restore and build together to ensure all dependencies are resolved
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet build "EstateHub.ListingService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish "EstateHub.ListingService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EstateHub.ListingService.API.dll"]
