FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Kinetic.Common/Kinetic.Common.csproj Kinetic.Common/
COPY Kinetic.Inventory.API/Kinetic.Inventory.API.csproj Kinetic.Inventory.API/
RUN dotnet restore Kinetic.Inventory.API/Kinetic.Inventory.API.csproj

COPY . .
WORKDIR /src/Kinetic.Inventory.API
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Kinetic.Inventory.API.dll"]