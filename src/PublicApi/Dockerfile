#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY . .
#COPY ["src/PublicApi/PublicApi.csproj", "./PublicApi/"]
#RUN dotnet restore "./PublicApi/PublicApi.csproj"
#COPY . .
WORKDIR "/app/src/PublicApi"
RUN dotnet restore

RUN dotnet build "./PublicApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./PublicApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configure for container
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Datadog serverless-init entrypoint
COPY --from=datadog/serverless-init:1 / /app/.

ARG TRACER_VERSION=2.57.0
ARG TARGETARCH
RUN if [ "$TARGETARCH" = "arm64" ]; then \
      # For Mac ARM64: install curl and manually download arm64 tracer
      apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/* && \
      curl -Lo /tmp/datadog-dotnet-apm.tar.gz \
      https://github.com/DataDog/dd-trace-dotnet/releases/download/v${TRACER_VERSION}/datadog-dotnet-apm-${TRACER_VERSION}.arm64.tar.gz && \
      mkdir -p /dd_tracer/dotnet/ && tar -xzf /tmp/datadog-dotnet-apm.tar.gz -C /dd_tracer/dotnet/ && rm /tmp/datadog-dotnet-apm.tar.gz; \
    else \
      # For Azure amd64: use official install script
      /app/datadog-init/install_ddtrace.sh; \
    fi

ENV DD_SERVICE=eshop-publicapi
ENV DD_VERSION=1
ENV DD_SOURCE=csharp

ENTRYPOINT ["/app/datadog-init"]

CMD ["dotnet", "PublicApi.dll"]