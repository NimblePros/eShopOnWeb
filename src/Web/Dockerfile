# RUN ALL CONTAINERS FROM ROOT (folder with .sln file):
# docker-compose build
# docker-compose up
#
# RUN JUST THIS CONTAINER FROM ROOT (folder with .sln file):
# docker build --pull -t web -f src/Web/Dockerfile .
#
# BUILD IN AZURE CONTAINER REGISTRY:
# az acr build --registry <acr-name> --image eshop-web:latest --file src/Web/Dockerfile .
#
# RUN COMMAND
#  docker run --name eshopweb --rm -it -p 8080:8080 web
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# Build and publish
WORKDIR /src/src/Web
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

# Configure for container
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Datadog serverless-init entrypoint
COPY --from=datadog/serverless-init:1 / /app/.

# Install Datadog tracer (supports both amd64 and arm64)
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

ENV DD_SERVICE=eshop-web
ENV DD_VERSION=1
ENV DD_SOURCE=csharp

ENTRYPOINT ["/app/datadog-init"]

CMD ["dotnet", "Web.dll"]
