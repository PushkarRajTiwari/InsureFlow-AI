FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY InsureFlow.sln ./
COPY src/InsureFlow.Api/InsureFlow.Api.csproj src/InsureFlow.Api/
COPY src/InsureFlow.Application/InsureFlow.Application.csproj src/InsureFlow.Application/
COPY src/InsureFlow.Domain/InsureFlow.Domain.csproj src/InsureFlow.Domain/
COPY src/InsureFlow.Infrastructure/InsureFlow.Infrastructure.csproj src/InsureFlow.Infrastructure/
RUN dotnet restore src/InsureFlow.Api/InsureFlow.Api.csproj

COPY . .
RUN dotnet publish src/InsureFlow.Api/InsureFlow.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "InsureFlow.Api.dll"]
