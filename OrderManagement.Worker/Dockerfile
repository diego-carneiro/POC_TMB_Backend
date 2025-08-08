FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copiar arquivos de projeto
COPY ["OrderManagement.Worker/OrderManagement.Worker.csproj", "OrderManagement.Worker/"]
COPY ["OrderManagement.Core/OrderManagement.Core.csproj", "OrderManagement.Core/"]
COPY ["OrderManagement.Infrastructure/OrderManagement.Infrastructure.csproj", "OrderManagement.Infrastructure/"]

# Restaurar dependências
RUN dotnet restore "OrderManagement.Worker/OrderManagement.Worker.csproj"

# Copiar código fonte
COPY . .

# Build da aplicação
WORKDIR "/src/OrderManagement.Worker"
RUN dotnet build "OrderManagement.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderManagement.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderManagement.Worker.dll"]