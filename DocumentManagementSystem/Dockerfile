FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY DocumentManagementSystem/DocumentManagementSystem.csproj DocumentManagementSystem/
COPY Shared/Shared.csproj Shared/
RUN dotnet restore DocumentManagementSystem/DocumentManagementSystem.csproj
COPY . .
RUN dotnet publish DocumentManagementSystem/DocumentManagementSystem.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DocumentManagementSystem.dll"]