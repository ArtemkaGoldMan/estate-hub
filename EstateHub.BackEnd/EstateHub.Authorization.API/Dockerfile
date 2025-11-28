# syntax=docker/dockerfile:1.4
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files first for better layer caching
COPY ["EstateHub.BackEnd/EstateHub.Authorization.API/EstateHub.Authorization.API.csproj", "EstateHub.Authorization.API/"]
COPY ["EstateHub.BackEnd/EstateHub.Authorization.Core/EstateHub.Authorization.Core.csproj", "EstateHub.Authorization.Core/"]
COPY ["EstateHub.BackEnd/EstateHub.Authorization.DataAccess.SqlServer/EstateHub.Authorization.DataAccess.SqlServer.csproj", "EstateHub.Authorization.DataAccess.SqlServer/"]
COPY ["EstateHub.BackEnd/EstateHub.Authorization.Domain/EstateHub.Authorization.Domain.csproj", "EstateHub.Authorization.Domain/"]
COPY ["EstateHub.BackEnd/EstateHub.Authorization.Infrastructure/EstateHub.Authorization.Infrastructure.csproj", "EstateHub.Authorization.Infrastructure/"]
COPY ["EstateHub.BackEnd/EstateHub.SharedKernel/EstateHub.SharedKernel.csproj", "EstateHub.SharedKernel/"]
COPY ["EstateHub.BackEnd/EstateHub.SharedKernel.API/EstateHub.SharedKernel.API.csproj", "EstateHub.SharedKernel.API/"]

# Restore with cache mount for faster builds
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore "EstateHub.Authorization.API/EstateHub.Authorization.API.csproj" --verbosity quiet

# Copy source code
COPY ["EstateHub.BackEnd/EstateHub.Authorization.API/", "EstateHub.Authorization.API/"]
COPY ["EstateHub.BackEnd/EstateHub.Authorization.Core/", "EstateHub.Authorization.Core/"]
COPY ["EstateHub.BackEnd/EstateHub.Authorization.DataAccess.SqlServer/", "EstateHub.Authorization.DataAccess.SqlServer/"]
COPY ["EstateHub.BackEnd/EstateHub.Authorization.Domain/", "EstateHub.Authorization.Domain/"]
COPY ["EstateHub.BackEnd/EstateHub.Authorization.Infrastructure/", "EstateHub.Authorization.Infrastructure/"]
COPY ["EstateHub.BackEnd/EstateHub.SharedKernel/", "EstateHub.SharedKernel/"]
COPY ["EstateHub.BackEnd/EstateHub.SharedKernel.API/", "EstateHub.SharedKernel.API/"]

WORKDIR "/src/EstateHub.Authorization.API"
# Restore and build together to ensure all dependencies are resolved
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet build "EstateHub.Authorization.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish "EstateHub.Authorization.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EstateHub.Authorization.API.dll"]
