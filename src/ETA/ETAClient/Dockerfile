FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["ETA/ETAClient/ETAClient.csproj", "ETA/ETAClient/"]
COPY ["ETA/ETASupport/ETASupport.csproj", "ETA/ETASupport/"]
COPY ["DTOs/DTOs.csproj", "DTOs/"]
RUN dotnet restore "ETA/ETAClient/ETAClient.csproj"
COPY . .
WORKDIR "/src/ETA/ETAClient"
RUN dotnet build "ETAClient.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "ETAClient.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ETAClient.dll"]