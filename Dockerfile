FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal-arm64v8 AS base
WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR "/"
COPY "data.literaturetime.sln" "./"
COPY ["src/Data.LiteratureTime.API/Data.LiteratureTime.API.csproj", "./src/Data.LiteratureTime.API/"]
COPY ["src/Data.LiteratureTime.Core/Data.LiteratureTime.Core.csproj", "./src/Data.LiteratureTime.Core/"]
COPY ["src/Data.LiteratureTime.Infrastructure/Data.LiteratureTime.Infrastructure.csproj", "./src/Data.LiteratureTime.Infrastructure/"]
COPY ["src/Irrbloss/Irrbloss.csproj", "./src/Irrbloss/"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/Data.LiteratureTime.API/."
RUN dotnet publish "Data.LiteratureTime.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Data.LiteratureTime.API.dll"]
