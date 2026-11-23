
# Etapa 1: compilar el proyecto
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["Backend_custom_test.API.csproj", "./"]

RUN dotnet restore "Backend_custom_test.API.csproj"

COPY . .

RUN dotnet publish "Backend_custom_test.API.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Etapa 2: ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 10000

ENTRYPOINT ["sh", "-c", "dotnet Backend_custom_test.API.dll --urls http://0.0.0.0:${PORT:-10000}"]
