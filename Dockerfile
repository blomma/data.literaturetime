FROM mcr.microsoft.com/dotnet/aspnet:7.0-jammy-arm64v8 AS base
WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/sdk:7.0-jammy AS build
WORKDIR "/"
COPY "data.literaturetime.sln" "./"
COPY ["src/Data.LiteratureTime.Worker/Data.LiteratureTime.Worker.csproj", "./src/Data.LiteratureTime.Worker/"]
COPY ["src/Data.LiteratureTime.Core/Data.LiteratureTime.Core.csproj", "./src/Data.LiteratureTime.Core/"]
COPY ["src/Data.LiteratureTime.Infrastructure/Data.LiteratureTime.Infrastructure.csproj", "./src/Data.LiteratureTime.Infrastructure/"]
COPY ["src/Irrbloss/Irrbloss.csproj", "./src/Irrbloss/"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/Data.LiteratureTime.Worker/."
RUN dotnet publish "Data.LiteratureTime.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Data.LiteratureTime.Worker.dll"]
