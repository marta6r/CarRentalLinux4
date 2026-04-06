# Базовый образ для ASP.NET Core 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Устанавливаем SQLite командную строку
RUN apt-get update && apt-get install -y sqlite3 curl && rm -rf /var/lib/apt/lists/*

# Образ для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CarRental.csproj", "."]
RUN dotnet restore "./CarRental.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "CarRental.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CarRental.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Финальный образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Создаем папки
RUN mkdir -p /app/wwwroot/images/cars
RUN mkdir -p /app/wwwroot/images/customers
RUN mkdir -p /app/data

# Запускаем приложение напрямую (без init-db.sh)
ENTRYPOINT ["dotnet", "CarRental.dll"]