FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore backend/src/PetMach.Api/PetMach.Api.csproj
RUN dotnet publish backend/src/PetMach.Api/PetMach.Api.csproj -c Release -o /app/publish --no-restore --no-self-contained

FROM build AS migrator
RUN dotnet tool restore
ENTRYPOINT ["dotnet", "ef", "database", "update", "--project", "backend/src/PetMach.Infrastructure/PetMach.Infrastructure.csproj", "--startup-project", "backend/src/PetMach.Api/PetMach.Api.csproj"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build --chown=$APP_UID:$APP_UID /app/publish .
USER $APP_UID
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "PetMach.Api.dll"]
