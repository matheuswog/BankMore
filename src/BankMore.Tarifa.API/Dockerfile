FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/BankMore.Tarifa.API/BankMore.Tarifa.API.csproj", "src/BankMore.Tarifa.API/"]
COPY ["src/BankMore.Tarifa.Domain/BankMore.Tarifa.Domain.csproj", "src/BankMore.Tarifa.Domain/"]
COPY ["src/BankMore.Tarifa.Infrastructure/BankMore.Tarifa.Infrastructure.csproj", "src/BankMore.Tarifa.Infrastructure/"]
RUN dotnet restore "src/BankMore.Tarifa.API/BankMore.Tarifa.API.csproj"
COPY . .
WORKDIR "/src/src/BankMore.Tarifa.API"
RUN dotnet build "BankMore.Tarifa.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BankMore.Tarifa.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p /app/data
ENTRYPOINT ["dotnet", "BankMore.Tarifa.API.dll"]
