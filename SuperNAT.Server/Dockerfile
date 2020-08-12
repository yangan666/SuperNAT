#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["SuperNAT.Server/SuperNAT.Server.csproj", "SuperNAT.Server/"]
COPY ["SuperNAT.Common/SuperNAT.Common.csproj", "SuperNAT.Common/"]
COPY ["SuperNAT.AsyncSocket/SuperNAT.AsyncSocket.csproj", "SuperNAT.AsyncSocket/"]
COPY ["SuperNAT.Core/SuperNAT.Core.csproj", "SuperNAT.Core/"]
COPY ["SuperNAT.Model/SuperNAT.Model.csproj", "SuperNAT.Model/"]
COPY ["SuperNAT.Bll/SuperNAT.Bll.csproj", "SuperNAT.Bll/"]
COPY ["SuperNAT.Dal/SuperNAT.Dal.csproj", "SuperNAT.Dal/"]
RUN dotnet restore "SuperNAT.Server/SuperNAT.Server.csproj"
COPY . .
WORKDIR "/src/SuperNAT.Server"
RUN dotnet build "SuperNAT.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SuperNAT.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SuperNAT.Server.dll"]