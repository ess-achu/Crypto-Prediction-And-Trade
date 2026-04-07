# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy csproj and restore dependencies
COPY ["src/CryptoTrader.App/CryptoTrader.App.csproj", "src/CryptoTrader.App/"]
RUN dotnet restore "src/CryptoTrader.App/CryptoTrader.App.csproj"

# Copy the rest of the code
COPY . .
WORKDIR "/source/src/CryptoTrader.App"
RUN dotnet publish -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render dynamically assigns ports, but defaults to 8080 for web services often
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "CryptoTrader.App.dll"]
