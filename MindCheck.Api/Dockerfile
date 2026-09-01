# Build context must be the repo root (not this folder), since the Api
# project references sibling projects: docker build -f MindCheck.Api/Dockerfile .

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MindCheck.Domain/*.csproj MindCheck.Domain/
COPY MindCheck.Application/*.csproj MindCheck.Application/
COPY MindCheck.Infrastructure/*.csproj MindCheck.Infrastructure/
COPY MindCheck.Api/*.csproj MindCheck.Api/
RUN dotnet restore MindCheck.Api/MindCheck.Api.csproj

COPY MindCheck.Domain/ MindCheck.Domain/
COPY MindCheck.Application/ MindCheck.Application/
COPY MindCheck.Infrastructure/ MindCheck.Infrastructure/
COPY MindCheck.Api/ MindCheck.Api/
RUN dotnet publish MindCheck.Api/MindCheck.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

# Railway (and similar platforms) inject $PORT at runtime and expect the
# container to listen on it; default to 8080 for plain `docker run`/compose.
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet MindCheck.Api.dll"]
