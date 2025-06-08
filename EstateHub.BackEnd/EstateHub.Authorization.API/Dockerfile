FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["EstateHub.BackEnd/EstateHub.Authorization.API/EstateHub.Authorization.API.csproj", "EstateHub.Authorization.API/"]
COPY ["EstateHub.BackEnd/EstateHub.Authorization.Core/EstateHub.Authorization.Core.csproj", "EstateHub.Authorization.Core/"]
COPY ["EstateHub.BackEnd/EstateHub.Authorization.DataAccess.SqlServer/EstateHub.Authorization.DataAccess.SqlServer.csproj", "EstateHub.Authorization.DataAccess.SqlServer/"]
COPY ["EstateHub.BackEnd/EstateHub.Authorization.Domain/EstateHub.Authorization.Domain.csproj", "EstateHub.Authorization.Domain/"]
COPY ["EstateHub.BackEnd/EstateHub.Authorization.Infrastructure/EstateHub.Authorization.Infrastructure.csproj", "EstateHub.Authorization.Infrastructure/"]
COPY ["EstateHub.BackEnd/EstateHub.SharedKernel/EstateHub.SharedKernel.csproj", "EstateHub.SharedKernel/"]
COPY ["EstateHub.BackEnd/EstateHub.SharedKernel.API/EstateHub.SharedKernel.API.csproj", "EstateHub.SharedKernel.API/"]

RUN dotnet restore "EstateHub.Authorization.API/EstateHub.Authorization.API.csproj"

COPY ["EstateHub.BackEnd/EstateHub.Authorization.API/", "EstateHub.Authorization.API/"]
COPY ["EstateHub.BackEnd/EstateHub.Authorization.Core/", "EstateHub.Authorization.Core/"]
COPY ["EstateHub.BackEnd/EstateHub.Authorization.DataAccess.SqlServer/", "EstateHub.Authorization.DataAccess.SqlServer/"]
COPY ["EstateHub.BackEnd/EstateHub.Authorization.Domain/", "EstateHub.Authorization.Domain/"]
COPY ["EstateHub.BackEnd/EstateHub.Authorization.Infrastructure/", "EstateHub.Authorization.Infrastructure/"]
COPY ["EstateHub.BackEnd/EstateHub.SharedKernel/", "EstateHub.SharedKernel/"]
COPY ["EstateHub.BackEnd/EstateHub.SharedKernel.API/", "EstateHub.SharedKernel.API/"]

WORKDIR "/src/EstateHub.Authorization.API"
RUN dotnet build "EstateHub.Authorization.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG CERT_PASSWORD=defaultPassword

RUN dotnet publish "EstateHub.Authorization.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

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
    echo "DNS.2 = auth-service" >> /tmp/openssl.conf && \
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

ENTRYPOINT ["dotnet", "EstateHub.Authorization.API.dll"]
