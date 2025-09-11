FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/BankMore.ContaCorrente.API/BankMore.ContaCorrente.API.csproj", "src/BankMore.ContaCorrente.API/"]
COPY ["src/BankMore.ContaCorrente.Domain/BankMore.ContaCorrente.Domain.csproj", "src/BankMore.ContaCorrente.Domain/"]
COPY ["src/BankMore.ContaCorrente.Infrastructure/BankMore.ContaCorrente.Infrastructure.csproj", "src/BankMore.ContaCorrente.Infrastructure/"]
RUN dotnet restore "src/BankMore.ContaCorrente.API/BankMore.ContaCorrente.API.csproj"
COPY . .
WORKDIR "/src/src/BankMore.ContaCorrente.API"
RUN dotnet build "BankMore.ContaCorrente.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BankMore.ContaCorrente.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p /app/data
ENTRYPOINT ["dotnet", "BankMore.ContaCorrente.API.dll"]
