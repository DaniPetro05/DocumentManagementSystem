FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY IndexingWorker/IndexingWorker.csproj IndexingWorker/
COPY Shared/Shared.csproj Shared/
RUN dotnet restore IndexingWorker/IndexingWorker.csproj
COPY . .
RUN dotnet publish IndexingWorker/IndexingWorker.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IndexingWorker.dll"]