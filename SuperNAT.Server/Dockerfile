FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build

RUN cd /usr/local/src 
RUN mkdir SuperNATServer 
WORKDIR /usr/local/src/SuperNATServer
COPY . .

ENTRYPOINT ["dotnet", "SuperNAT.Server.dll"]
