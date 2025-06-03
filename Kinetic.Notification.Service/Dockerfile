FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Kinetic.Common/Kinetic.Common.csproj Kinetic.Common/
COPY Kinetic.Notification.Service/Kinetic.Notification.Service.csproj Kinetic.Notification.Service/
RUN dotnet restore Kinetic.Notification.Service/Kinetic.Notification.Service.csproj

COPY . .
WORKDIR /src/Kinetic.Notification.Service
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Kinetic.Notification.Service.dll"]