FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /source

COPY *.sln .
COPY src/Ttc.WebApi/*.csproj ./src/Ttc.WebApi/
COPY src/Frenoy.Api/*.csproj ./src/Frenoy.Api/
COPY src/Ttc.DataAccess/*.csproj ./src/Ttc.DataAccess/
COPY src/Ttc.DataEntities/*.csproj ./src/Ttc.DataEntities/
COPY src/Ttc.Model/*.csproj ./src/Ttc.Model/
COPY src/Ttc.UnitTests/*.csproj ./src/Ttc.UnitTests/

RUN dotnet restore

COPY src/Ttc.WebApi/. ./src/Ttc.WebApi/
COPY src/Frenoy.Api/. ./src/Frenoy.Api/
COPY src/Ttc.DataAccess/. ./src/Ttc.DataAccess/
COPY src/Ttc.DataEntities/. ./src/Ttc.DataEntities/
COPY src/Ttc.Model/. ./src/Ttc.Model/
COPY src/Ttc.UnitTests/. ./src/Ttc.UnitTests/

WORKDIR /source/src/Ttc.WebApi
RUN dotnet publish -c Release -o /app



FROM mcr.microsoft.com/dotnet/aspnet:8.0 as runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT [ "dotnet", "/app/Ttc.WebApi.dll" ]
