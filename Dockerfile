FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["CardDisputeAPI/CardDisputePortal.API.csproj", "CardDisputeAPI/"]
COPY ["CardDisputePortal.Core/CardDisputePortal.Core.csproj", "CardDisputePortal.Core/"]
COPY ["CardDisputePortal.Infrastructure/CardDisputePortal.Infrastructure.csproj", "CardDisputePortal.Infrastructure/"]
RUN dotnet restore "CardDisputeAPI/CardDisputePortal.API.csproj"
COPY CardDisputeAPI/ CardDisputeAPI/
COPY CardDisputePortal.Core/ CardDisputePortal.Core/
COPY CardDisputePortal.Infrastructure/ CardDisputePortal.Infrastructure/
WORKDIR "/src/CardDisputeAPI"
RUN dotnet build "CardDisputePortal.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CardDisputePortal.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CardDisputePortal.API.dll"]
