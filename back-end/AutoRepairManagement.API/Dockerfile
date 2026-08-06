FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
# Dedicated, volume-mountable directory for the SQLite database file, created
# and chowned while still root so the non-root $APP_UID can write to it.
RUN mkdir -p /app/data && chown -R $APP_UID:$APP_UID /app/data
EXPOSE 8080
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AutoRepairManagement.API.csproj", "./"]
RUN dotnet restore "AutoRepairManagement.API.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "./AutoRepairManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AutoRepairManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AutoRepairManagement.API.dll"]
