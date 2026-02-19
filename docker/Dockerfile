# build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ServiceStarter.VS.sln .
COPY src/ServiceStarter.Api/ServiceStarter.Api.csproj src/ServiceStarter.Api/
COPY tests/ServiceStarter.UnitTests/ServiceStarter.UnitTests.csproj tests/ServiceStarter.UnitTests/
COPY tests/ServiceStarter.IntegrationTests/ServiceStarter.IntegrationTests.csproj tests/ServiceStarter.IntegrationTests/
RUN dotnet restore ServiceStarter.VS.sln

COPY . .
RUN dotnet publish src/ServiceStarter.Api/ServiceStarter.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ServiceStarter.Api.dll"]
