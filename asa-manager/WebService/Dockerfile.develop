FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY asa-manager.sln ./
COPY DeviceGroupsAgent/DeviceGroupsAgent.csproj DeviceGroupsAgent/
COPY Services/Services.csproj Services/
COPY SetupAgent/SetupAgent.csproj SetupAgent/
COPY TelemetryRulesAgent/TelemetryRulesAgent.csproj TelemetryRulesAgent/
COPY WebService/WebService.csproj WebService/

RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/WebService
RUN dotnet build WebService.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish WebService.csproj -c Release -o /app

FROM base AS final
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Microsoft.Azure.IoTSolutions.AsaManager.WebService.dll"]
EXPOSE 9024
