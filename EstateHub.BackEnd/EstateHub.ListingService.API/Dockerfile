FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["EstateHub.BackEnd/EstateHub.ListingService.API/EstateHub.ListingService.API.csproj", "EstateHub.ListingService.API/"]
COPY ["EstateHub.BackEnd/EstateHub.ListingService.Core/EstateHub.ListingService.Core.csproj", "EstateHub.ListingService.Core/"]
COPY ["EstateHub.BackEnd/EstateHub.ListingService.DataAccess.SqlServer/EstateHub.ListingService.DataAccess.SqlServer.csproj", "EstateHub.ListingService.DataAccess.SqlServer/"]
COPY ["EstateHub.BackEnd/EstateHub.ListingService.Domain/EstateHub.ListingService.Domain.csproj", "EstateHub.ListingService.Domain/"]
COPY ["EstateHub.BackEnd/EstateHub.ListingService.Infrastructure/EstateHub.ListingService.Infrastructure.csproj", "EstateHub.ListingService.Infrastructure/"]
COPY ["EstateHub.BackEnd/EstateHub.SharedKernel/EstateHub.SharedKernel.csproj", "EstateHub.SharedKernel/"]
COPY ["EstateHub.BackEnd/EstateHub.SharedKernel.API/EstateHub.SharedKernel.API.csproj", "EstateHub.SharedKernel.API/"]

RUN dotnet restore "EstateHub.ListingService.API/EstateHub.ListingService.API.csproj"

COPY ["EstateHub.BackEnd/EstateHub.ListingService.API/", "EstateHub.ListingService.API/"]
COPY ["EstateHub.BackEnd/EstateHub.ListingService.Core/", "EstateHub.ListingService.Core/"]
COPY ["EstateHub.BackEnd/EstateHub.ListingService.DataAccess.SqlServer/", "EstateHub.ListingService.DataAccess.SqlServer/"]
COPY ["EstateHub.BackEnd/EstateHub.ListingService.Domain/", "EstateHub.ListingService.Domain/"]
COPY ["EstateHub.BackEnd/EstateHub.ListingService.Infrastructure/", "EstateHub.ListingService.Infrastructure/"]
COPY ["EstateHub.BackEnd/EstateHub.SharedKernel/", "EstateHub.SharedKernel/"]
COPY ["EstateHub.BackEnd/EstateHub.SharedKernel.API/", "EstateHub.SharedKernel.API/"]

WORKDIR "/src/EstateHub.ListingService.API"
RUN dotnet build "EstateHub.ListingService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG CERT_PASSWORD=defaultPassword

RUN dotnet publish "EstateHub.ListingService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

RUN mkdir -p /https && apt-get update && apt-get install -y openssl

# Генеруємо openssl.conf покроково (як у KanBanan)
RUN echo "[req]" > /tmp/openssl.conf && \
    echo "default_bits = 2048" >> /tmp/openssl.conf && \
    echo "prompt = no" >> /tmp/openssl.conf && \
    echo "distinguished_name = req_distinguished_name" >> /tmp/openssl.conf && \
    echo "x509_extensions = v3_req" >> /tmp/openssl.conf && \
    echo "" >> /tmp/openssl.conf && \
    echo "[req_distinguished_name]" >> /tmp/openssl.conf && \
    echo "C = UA" >> /tmp/openssl.conf && \
    echo "ST = Kyiv" >> /tmp/openssl.conf && \
    echo "L = Kyiv" >> /tmp/openssl.conf && \
    echo "O = EstateHub" >> /tmp/openssl.conf && \
    echo "CN = localhost" >> /tmp/openssl.conf && \
    echo "" >> /tmp/openssl.conf && \
    echo "[v3_req]" >> /tmp/openssl.conf && \
    echo "basicConstraints = CA:FALSE" >> /tmp/openssl.conf && \
    echo "keyUsage = nonRepudiation, digitalSignature, keyEncipherment" >> /tmp/openssl.conf && \
    echo "subjectAltName = @alt_names" >> /tmp/openssl.conf && \
    echo "" >> /tmp/openssl.conf && \
    echo "[alt_names]" >> /tmp/openssl.conf && \
    echo "DNS.1 = localhost" >> /tmp/openssl.conf && \
    echo "DNS.2 = listing-service" >> /tmp/openssl.conf && \
    echo "DNS.3 = *.localhost" >> /tmp/openssl.conf && \
    echo "IP.1 = 127.0.0.1" >> /tmp/openssl.conf && \
    echo "IP.2 = ::1" >> /tmp/openssl.conf

RUN openssl genrsa -out /https/aspnetapp.key 2048 && \
    openssl req -new -x509 -key /https/aspnetapp.key -out /https/aspnetapp.crt -days 365 -config /tmp/openssl.conf -extensions v3_req && \
    openssl pkcs12 -export -out /https/aspnetapp.pfx -inkey /https/aspnetapp.key -in /https/aspnetapp.crt -passout pass:${CERT_PASSWORD}

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /https /https

RUN chmod 755 /https && chmod 644 /https/aspnetapp.pfx

ENTRYPOINT ["dotnet", "EstateHub.ListingService.API.dll"]