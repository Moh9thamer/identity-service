# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Identity.Api/Identity.Api.csproj Identity.Api/
COPY Identity.Application/Identity.Application.csproj Identity.Application/
COPY Identity.Infrastructure/Identity.Infrastructure.csproj Identity.Infrastructure/
COPY Identity.Domain/Identity.Domain.csproj Identity.Domain/

RUN dotnet restore Identity.Api/Identity.Api.csproj

COPY . .
RUN dotnet publish Identity.Api/Identity.Api.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Identity.Api.dll"]
