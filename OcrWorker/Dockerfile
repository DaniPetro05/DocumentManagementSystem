FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY OcrWorker/OcrWorker.csproj OcrWorker/
COPY Shared/Shared.csproj Shared/
RUN dotnet restore OcrWorker/OcrWorker.csproj
COPY . .
RUN dotnet publish OcrWorker/OcrWorker.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    tesseract-ocr \
    tesseract-ocr-eng \
    ghostscript \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OcrWorker.dll"]